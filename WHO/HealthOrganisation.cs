using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Models;
using Models.Parameters;
using Serilog;
using WHO.Tracking;
using Interface.Client;
using WHO.Extensions;
using System.Linq;
using Serilog.Debugging;
using System.Runtime.CompilerServices;

namespace WHO
{
    public class HealthOrganisation : IAsyncDisposable
    {

        public static HealthOrganisation Instance { get; private set; }

        internal static void SetInstanceForTestingOnly(HealthOrganisation org)
        {
            Instance = org;
        }

        /// <summary>
        /// Constant for referring to the global tracker
        /// </summary>
        private const string ALL_LOCATION_ID = "_all";

        [AllowNull]
        private IClient _client;

        private const int _statusPingDelayInMs = 100;

        [AllowNull]
        private SimulationSettings _simulationSettings;

        /// <summary>
        /// The list of trackers for each location, also contains one with the 'ALL_LOCATION_ID' which keeps track of the global situation
        /// </summary>
        private Dictionary<string, LocationTracker> _locationTrackers = new();

        /// <summary>
        /// Keeps track of which actions have been applied to the locations as well as the location list\<string\> form
        /// </summary>
        private Dictionary<string, LocationStatus> _locationStatuses = new();

        public Dictionary<string, LocationTracker> LocationTrackers => this._locationTrackers;
        public Dictionary<string, LocationStatus> LocationStatuses => this._locationStatuses;

        /// <summary>
        /// The budget for the current turn
        /// </summary>
        private int _budget = 0;

        /// <summary>
        /// Keeps track of if the client is running, set to false to end the program
        /// </summary>
        private bool _running = false;

        /// <summary>
        /// List of tasks to execute
        /// </summary>
        private readonly List<WhoAction> _tasksToExecute = new();

        /// <summary>
        /// Local triggers on ran on each location and sub location in a depth first pattern
        /// </summary>
        private readonly List<ITrigger> _triggersForLocalLocations = new();

        /// <summary>
        /// Global triggers on ran on the sum of all the top level locations
        /// </summary>
        private readonly List<ITrigger> _triggersForGlobal = new();

        public int TriggerCount => this._triggersForGlobal.Count + this._triggersForLocalLocations.Count;

        /// <summary>
        /// Keeps track of the current action id 
        /// </summary>
        private int _currentActionId = 0;

        public HealthOrganisation(string uri) : this(new WebSocket(uri))
        {
        }

        public HealthOrganisation(IClient client)
        {
            if (Instance != null)
            {
                throw new InvalidOperationException("Cannot instantiate two copies of the health organisation");
            }
            Instance = this;
            this._client = client;
        }

        public void SetLocationTrackerFromTest(Dictionary<string, LocationTracker> tracker)
        {
            this._locationTrackers = tracker;
        }

        public void CreateTriggers()
        {
            // Example triggers

            // This trigger tries to calculate what the best actions are
            ITrigger bestAction = new CustomTrigger(TrackingValue.SeriousInfection, p => p.CurrentParameterCount > 100, (loc) => this.calculateBestAction(this._budget, loc), 7);
            this._triggersForLocalLocations.Add(bestAction);

            ITrigger deployVaccines = new CustomTrigger(TrackingValue.SeriousInfection, p => p.CurrentParameterCount > 100, (loc) => this.StartTestAndIsolation(0, 0, 0, loc, false), 7);
            this._triggersForGlobal.Add(deployVaccines);

            ITrigger basicIncreaseOfInfections = new BasicTrigger(TrackingValue.SeriousInfection, TrackingFunction.GREATER_THAN, 1.2f, (_) => Console.WriteLine("Increase"), 7);
            ITrigger complexIncreaseOfInfections = new CustomTrigger(TrackingValue.SeriousInfection, p => p.CurrentParameterCount > 1000 && p.Change > 1.2f, (_) => Console.WriteLine("Custom Increase"), 7);
            this._triggersForLocalLocations.Add(basicIncreaseOfInfections);
            this._triggersForLocalLocations.Add(complexIncreaseOfInfections);
        }

        public async Task PopulateInitialInformation()
        {
            // Get a list of locations
            this._simulationSettings = await this._client.GetSettings();

            // Get the initial information for the trackers
            this.InitialiseLocationInformation(this._simulationSettings.Locations);
            await this.GetTrackingInformation();
        }

        public void Stop()
        {
            this._running = false;
        }

        public async Task Run()
        {
            if (this._running)
            {
                throw new InvalidOperationException("Health Organisation is already running");
            }
            this.CreateTriggers();

            this._running = true;
            bool firstTurn = true;


            while (this._running)
            {
                this._budget = await this.WaitForOurTurn();
                if (firstTurn)
                {
                    // First turn we won't be able to see any trends to just 
                    firstTurn = false;
                    await this.PopulateInitialInformation();
                }
                else
                {
                    // Get the latest tracking information and run the triggers
                    await this.GetTrackingInformation();
                    this.RunTriggerChecks();
                }

                // Executes all tasks that were queued
                if (this._tasksToExecute.Count > 0)
                {
                    await this.ExecuteTasks();
                    this._tasksToExecute.Clear();
                }
                await (this._client as WebSocket).EndTurn();
            }

        }

        private void RunTriggerChecks()
        {
            // Run both local and global triggers
            this._simulationSettings.Locations.ForEach(loc => this.RunLocalTriggerChecks(loc));
            this.RunGlobalTriggerChecks();
        }

        private void RunLocalTriggerChecks(LocationDefinition location, int depth = 0)
        {
            // Loop through all the triggers and if they should be applied then apply them
            foreach (var trigger in this._triggersForLocalLocations)
            {
                if (ITrigger.IsValidDepth(depth, trigger.DepthRange))
                {
                    trigger.Apply(this._locationTrackers[location.Coord]);
                }
            }
            // Recurse for the sub locations
            if (location.SubLocations != null)
            {
                location.SubLocations.ForEach((l) => this.RunLocalTriggerChecks(l, depth + 1));
            }
        }

        private void RunGlobalTriggerChecks()
        {
            // Check all triggers against the global tracker and apply them
            LocationTracker globalTracker = this._locationTrackers[ALL_LOCATION_ID];
            foreach (var trigger in this._triggersForGlobal)
            {
                if (ITrigger.IsValidDepth(0, trigger.DepthRange))
                {
                    trigger.Apply(globalTracker);
                }
            }
        }

        private async Task<int> WaitForOurTurn()
        {
            // Request the status every _statusPingDelayInMs milliseconds until it is the who turn and return the budget
            do
            {
                var status = await this._client.GetStatus();
                if (status.IsWhoTurn)
                {
                    return this._budget + status.Budget;
                }
                await Task.Delay(_statusPingDelayInMs);
            }
            while (true);
        }

        public async ValueTask DisposeAsync()
        {
            if (this._client is WebSocket socket)
            {
                await socket.DisposeAsync();
            }
        }

        private async Task ExecuteTasks()
        {
            Dictionary<int, WhoAction> dict = this._tasksToExecute.ToDictionary(action => action.Id, action => action);
            var results = await this._client.ApplyActions(this._tasksToExecute);
            foreach (var result in results)
            {
                if (result.Code != 200)
                {
                    Log.Error($"Failed to execute action {result.Id} of type {dict[result.Id].Action}. Error: {result.Code} - {result.Message}");
                }
                else
                {
                    var action = dict[result.Id];
                    if (action.Parameters?.Location != null)
                    {
                        this._locationStatuses[string.Join("", action.Parameters.Location)].AddAction(dict[result.Id]);
                    }
                }
            }
        }

        private void InitialiseLocationInformation(List<LocationDefinition> locations, string locationKey="")
        {
            // Creates the global tracker and then creates a tracker for each location
            this._locationTrackers[ALL_LOCATION_ID] = new LocationTracker(ALL_LOCATION_ID, null);
            foreach (var location in locations)
            {
                string localLocationKey = locationKey + location.Coord;
                LocationStatus status;
                this._locationStatuses.Add(localLocationKey, status = new LocationStatus(localLocationKey));
                this._locationTrackers.Add(localLocationKey, new LocationTracker(localLocationKey, status));
                if (location.SubLocations != null)
                {
                    this.InitialiseLocationInformation(location.SubLocations, localLocationKey);
                }
            }
        }

        private async Task GetTrackingInformation()
        {
            // Apparently this blocks even though Visual Studio claims it doesn't
            Task.WaitAll(this._simulationSettings.Locations.Select(loc => this.GetLocationTrackingInformation(loc)).ToArray());
            this._locationTrackers[ALL_LOCATION_ID].Track(this.GetTotalsForAll());
        }

        private async Task GetLocationTrackingInformation(LocationDefinition location, string localLocationKey="")
        {
            // Create the tracker and populate it with the inital information
            localLocationKey += location.Coord;
            LocationTracker tracker = this._locationTrackers[localLocationKey];
            var trackingInformation = await this._client.GetInfoTotals(new SearchRequest(new() { this._locationStatuses[localLocationKey].Location }));
            tracker.Track(trackingInformation[0]);

            // Recursively populate sub locations
            if (location.SubLocations != null)
            {
                Task.WaitAll(location.SubLocations.Select(loc => this.GetLocationTrackingInformation(loc, localLocationKey)).ToArray());
            }
        }

        private InfectionTotals GetTotalsForAll()
        {
            // Sum over all the top level locations
            InfectionTotals totals = new(new() {}, 0, 0, 0, 0, 0, 0, 0);
            foreach (var location in this._simulationSettings.Locations)
            {
                InfectionTotals? latest = this._locationTrackers[location.Coord].Latest;
                if (latest == null)
                {
                    continue;
                }
                totals.Add(latest);
            }
            return totals;
        }

        private void StartTestAndIsolation(int testQuality, int quarantinePeriod, int quantity, List<string> location, bool symptomaticOnly)
        {
            // Example for how we can create an action and send it. It gets added to the list of tasks which are executed at the end of the turn.
            TestAndIsolation testAndIsolation = new(testQuality, quarantinePeriod, quantity, location, symptomaticOnly);
            WhoAction testAction = new(this._currentActionId++, testAndIsolation);
            this._tasksToExecute.Add(testAction);
        }

        private void calculateBestAction(int budgetAvailable, List<string> loc, float threshold = 1.2f)
        {
            string location = string.Join("", loc);

            // Calculate amount of people infected
            var asymptomaticInfectedInfectious = this._locationTrackers[location].Latest.GetParameterTotals(TrackingValue.AsymptomaticInfectedInfectious);
            var symptomaticInfected = this._locationTrackers[location].Latest.GetParameterTotals(TrackingValue.Symptomatic);

            int locationPopulation = this._locationTrackers[location].Latest.GetTotalPeople();

            // Calcualte infection rate
            float infectionRate = ((asymptomaticInfectedInfectious + symptomaticInfected) / locationPopulation);

            // Using the proportion of people who are infected calculate an appropriate budget for that location
            int totalInfections = this._locationTrackers[ALL_LOCATION_ID].Latest.GetTotalPeople() - this._locationTrackers[ALL_LOCATION_ID].Latest.GetParameterTotals(TrackingValue.Uninfected);
            int infectionsInArea = this._locationTrackers[location].Latest.GetTotalPeople() - this._locationTrackers[location].Latest.GetParameterTotals(TrackingValue.Uninfected);

            float percentageOfInfections = (infectionsInArea / totalInfections);

            // Limit the amount of money spent each term to a third of the budget
            budgetAvailable = budgetAvailable / 3;

            float budgetForLocation = budgetAvailable * percentageOfInfections;

            // Get all WhoActions
            List<Object> actions;

            if (infectionRate > threshold)
            {
                actions = getWhoActions(loc, ActionCostCalculator.ActionMode.Create, budgetForLocation);
            }
            else
            {
                actions = getWhoActions(loc, ActionCostCalculator.ActionMode.Delete, budgetForLocation);
            }

            // Actions which are collectively all available given the budget
            List<Object> actionsAvailable = new List<Object>();

            if (infectionRate > threshold)
            {
                foreach (Object action in actions)
                {
                    float actionCost = ActionCostCalculator.CalculateCost(action, ActionCostCalculator.ActionMode.Create);

                    if (actionCost < budgetForLocation)
                    {
                        actionsAvailable.Add(action);
                        budgetForLocation = budgetForLocation - actionCost;
                    }
                }
            }
            else if (infectionRate < (threshold * 0.75))
            {
                foreach (Object action in actions)
                {
                    float actionCost = ActionCostCalculator.CalculateCost(action, ActionCostCalculator.ActionMode.Delete);

                    if (actionCost < budgetForLocation)
                    {
                        actionsAvailable.Add(action);
                        budgetForLocation = budgetForLocation - actionCost;
                    }
                }
            }

            List<WhoAction> whoActionsAvailable = new List<WhoAction>();

            // Convert actions to WhoActions
            foreach (Object action in actionsAvailable)
            {
                WhoAction whoAction = new(this._currentActionId++, new(action.GetType().Name));
                whoActionsAvailable.Add(whoAction);
            }

            foreach (WhoAction whoAction in whoActionsAvailable)
            {
                this._tasksToExecute.Add(whoAction);
            }
        }

        private List<Object> getWhoActions(List<string> loc, ActionCostCalculator.ActionMode mode, float budgetForLocation)
        {
            List<Object> actions = new List<Object>();

            TestAndIsolation testAndIsolation = new(0, 0, 0, loc, false);
            actions.Add(testAndIsolation);

            CloseBorders closeBorders = new(loc);
            actions.Add(closeBorders);

            CloseRecreationalLocations closeRecreationalLocations = new(loc);
            actions.Add(closeRecreationalLocations);

            CloseSchools closeSchools = new(loc);
            actions.Add(closeSchools);

            Curfew curfew = new(loc);
            actions.Add(curfew);

            // Investment - if creating restrictions 15% of the budget will be allocated to investment
            // If deleting restrictions 25% of the budget will be allocated to investment
            switch (mode)
            {
                case ActionCostCalculator.ActionMode.Create:

                    // Using the action mode determine the level of the mask mandate
                    MaskMandate maskMandate = new(loc, 2);
                    actions.Add(maskMandate);

                    // Make the movement and social distancing measures harsh
                    MovementRestrictions movementRestrictions = new(loc, 2);
                    actions.Add(movementRestrictions);

                    SocialDistancingMandate socialDistancingMandate = new(loc, 2);
                    actions.Add(socialDistancingMandate);

                    // Calculate amoount of budget allocated for investment
                    double investmentBudget = budgetForLocation * 0.15;

                    // Each of the following have been given a proportion of the 15% according to their criticality
                    Furlough furlough = new((int)Math.Round((investmentBudget * 0.2), 0), loc);
                    actions.Add(furlough);

                    InformationPressRelease informationPressRelease = new((int)Math.Round((investmentBudget * 0.1), 0), loc);
                    actions.Add(informationPressRelease);

                    InvestInHealthServices investInHealthServices = new((int)Math.Round((investmentBudget * 0.3), 0));
                    actions.Add(investInHealthServices);

                    InvestInVaccine investInVaccine = new((int)Math.Round((investmentBudget * 0.3), 0));
                    actions.Add(investInVaccine);

                    Loan loan = new((int)Math.Round((investmentBudget * 0.1), 0));
                    actions.Add(loan);

                    break;
                case ActionCostCalculator.ActionMode.Delete:

                    // Using the action mode determine the level of the mask mandate
                    MaskMandate maskMandateDelete = new(loc, 0);
                    actions.Add(maskMandateDelete);

                    // Make the movement and social distancing measures harsh
                    MovementRestrictions movementRestrictionsDelete = new(loc, 0);
                    actions.Add(movementRestrictionsDelete);

                    SocialDistancingMandate socialDistancingMandateDelete = new(loc, 0);
                    actions.Add(socialDistancingMandateDelete);

                    // Calculate amoount of budget allocated for investment
                    double investmentBudgetDelete = budgetForLocation * 0.25;

                    // Each of the following have been given a proportion of the 25% according to their criticality
                    Furlough furloughDelete = new((int)Math.Round((investmentBudgetDelete * 0.2), 0), loc);
                    actions.Add(furloughDelete);

                    InformationPressRelease informationPressReleaseDelete = new((int)Math.Round((investmentBudgetDelete * 0.1), 0), loc);
                    actions.Add(informationPressReleaseDelete);

                    InvestInHealthServices investInHealthServicesDelete = new((int)Math.Round((investmentBudgetDelete * 0.3), 0));
                    actions.Add(investInHealthServicesDelete);

                    InvestInVaccine investInVaccineDelete = new((int)Math.Round((investmentBudgetDelete * 0.3), 0));
                    actions.Add(investInVaccineDelete);

                    Loan loanDelete = new((int)Math.Round((investmentBudgetDelete * 0.3), 0));
                    actions.Add(loanDelete);

                    break;
            }

            HealthDrive healthDrive = new(loc);
            actions.Add(healthDrive);

            StayAtHome stayAtHome = new(loc);
            actions.Add(stayAtHome);

            return actions;
        }

    }
}
