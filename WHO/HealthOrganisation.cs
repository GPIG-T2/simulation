using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Interface.Client;
using Models;
using Models.Parameters;
using Serilog;
using WHO.Extensions;
using WHO.Tracking;

namespace WHO
{
    public class HealthOrganisation : IHealthOrganisation
    {

        public static HealthOrganisation? Instance { get; private set; }

        internal static void SetInstanceForTestingOnly(HealthOrganisation? org)
        {
            Instance = org;
        }

        /// <summary>
        /// Constant for referring to the global tracker
        /// </summary>
        internal const string ALL_LOCATION_ID = "_all";

        private const int VaccineCost = 80000000;
        private int _vaccineInvestment = 0;

        private readonly IClient _client;

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
        private readonly Dictionary<string, LocationStatus> _locationStatuses = new();

        public Dictionary<string, LocationTracker> LocationTrackers => this._locationTrackers;
        public Dictionary<string, LocationStatus> LocationStatuses => this._locationStatuses;

        /// <summary>
        /// The budget for the current turn
        /// </summary>
        private long _budget = 0;

        /// <summary>
        /// Keeps track of if the client is running, set to false to end the program
        /// </summary>
        private bool _running = false;

        public bool Running
        {
            get => this._running;
            set => this._running = value;
        }

        /// <summary>
        /// List of tasks to execute
        /// </summary>
        private readonly List<WhoAction> _tasksToExecute = new();

        public List<WhoAction> TasksToExecute => this._tasksToExecute;

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
            this._client.Closed += this.Stop;
        }

        public void SetLocationTrackerFromTest(Dictionary<string, LocationTracker> tracker)
        {
            this._locationTrackers = tracker;
        }

        public void AddLocalTrigger(ITrigger trigger)
        {
            this._triggersForLocalLocations.Add(trigger);
        }

        public void AddGlobalTrigger(ITrigger trigger)
        {
            this._triggersForGlobal.Add(trigger);
        }

        public void CreateTriggers()
        {
            // Example triggers

            // This trigger tries to calculate what the best actions are
            //ITrigger bestAction = new CustomTrigger(
            //    TrackingValue.SeriousInfection,
            //    p => p.CurrentParameterCount > 100,
            //    (loc) =>
            //    {
            //        // If location is null, then most of the actions this handles
            //        // will not work, as they need a location.
            //        if (loc != null)
            //        {
            //            this.CalculateBestAction(this._budget, loc);
            //        }
            //    },
            //    7
            //);
            //this._triggersForLocalLocations.Add(bestAction);

            // Testing
            this._triggersForLocalLocations.Add(new CustomTrigger(TrackingValue.SeriousInfection, (p) => p.Change > 1.05f,
                (loc) => this.CreateAction(loc, new TestAndIsolation(1, 14, 1000, loc, true), true), 0));

            // Close Borders
            this._triggersForLocalLocations.Add(new CustomTrigger(TrackingValue.Symptomatic, (p) => ((float)p.CurrentParameterCount / p.CurrentTotalPopulation) > 0.25f,
                (loc) => this.CreateAction(loc, new CloseBorders(loc)), 0));
            this._triggersForLocalLocations.Add(new CustomTrigger(TrackingValue.Symptomatic, (p) => ((float)p.CurrentParameterCount / p.CurrentTotalPopulation) < 0.05f,
                (loc) => this.DeleteAction(loc, new CloseBorders(loc)), 0));

            // Social Distancing Mandate
            this._triggersForLocalLocations.Add(new CustomTrigger(TrackingValue.Symptomatic, (p) => ((float)p.CurrentParameterCount / p.CurrentTotalPopulation) > 0.10f,
                (loc) => this.CreateAction(loc, new SocialDistancingMandate(loc, 2)), 0));
            this._triggersForLocalLocations.Add(new CustomTrigger(TrackingValue.Symptomatic, (p) => ((float)p.CurrentParameterCount / p.CurrentTotalPopulation) < 0.01f,
                (loc) => this.DeleteAction(loc, new SocialDistancingMandate(loc, 2)), 0));

            // Health Drive
            this._triggersForLocalLocations.Add(new CustomTrigger(TrackingValue.Symptomatic, (p) => p.CurrentParameterCount > 0,
                (loc) => this.CreateAction(loc, new HealthDrive(loc)), 0));

            // Stay At Home 
            this._triggersForLocalLocations.Add(new CustomTrigger(TrackingValue.Symptomatic, (p) => ((float)p.CurrentParameterCount / p.CurrentTotalPopulation) > 0.10f,
                (loc) => this.CreateAction(loc, new SocialDistancingMandate(loc, 2)), 0));
            this._triggersForLocalLocations.Add(new CustomTrigger(TrackingValue.Symptomatic, (p) => ((float)p.CurrentParameterCount / p.CurrentTotalPopulation) < 0.01f,
                (loc) => this.DeleteAction(loc, new SocialDistancingMandate(loc, 2)), 0));

            // Vaccine investment
            this._triggersForGlobal.Add(new CustomTrigger(TrackingValue.Symptomatic, (p) => ((float)p.CurrentParameterCount / p.CurrentTotalPopulation) > 0.5f, (_) => this._tasksToExecute.Add(new WhoAction(this._currentActionId++, new InvestInVaccine((int)Math.Min(this._budget, Math.Min(VaccineCost / 100, VaccineCost - this._vaccineInvestment))))), 0));
            

            //ITrigger basicIncreaseOfInfections = new BasicTrigger(TrackingValue.SeriousInfection, TrackingFunction.GREATER_THAN, 1.2f, (_) => Log.Information("Increase"), 7);
            //ITrigger complexIncreaseOfInfections = new CustomTrigger(TrackingValue.SeriousInfection, p => p.CurrentParameterCount > 1000 && p.Change > 1.2f, (_) => Log.Information("Custom Increase"), 7);
            //this._triggersForLocalLocations.Add(basicIncreaseOfInfections);
            //this._triggersForLocalLocations.Add(complexIncreaseOfInfections);
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
            Log.Information("Beginning run");
            if (this._running)
            {
                throw new InvalidOperationException("Health Organisation is already running");
            }
            this.CreateTriggers();

            this._running = true;
            bool firstTurn = true;

            try
            {
                while (this._running)
                {
                    Log.Information("Waiting for our turn...");
                    this._budget = await this.WaitForOurTurn();
                    if (firstTurn)
                    {
                        // First turn we won't be able to see any trends to just 
                        firstTurn = false;
                        await this.PopulateInitialInformation();
                        Log.Information("First-time setup complete");
                    }
                    else
                    {
                        // Get the latest tracking information and run the triggers
                        await this.GetTrackingInformation();
                        this.RunTriggerChecks();
                        Log.Information("Updated to latest tracking data");
                    }

                    // Executes all tasks that were queued
                    if (this._tasksToExecute.Count > 0)
                    {
                        await this.ExecuteTasks();
                        Log.Information("Executed {Count} tasks", this._tasksToExecute.Count);
                        this._tasksToExecute.Clear();
                    }

                    await this._client.EndTurn();
                    Log.Information("Ended our turn");
                }
            }
            catch (TaskCanceledException) { }
        }

        private void CreateAction(List<string> location, ParamsContainer parameters, bool overwrite=false)
        {
            LocationStatus status = this._locationStatuses[location.ToKey()];
            var statusActions = new List<int>(status.GetActionsOfType(parameters.ActionName));
            if (statusActions.Count > 0)
            {
                if (overwrite)
                {
                    this.DeleteAction(location, parameters);
                }
                else
                {
                    return;
                }
            }
            if (this._budget < ActionCostCalculator.CalculateCost(parameters, ActionCostCalculator.ActionMode.Create))
            {
                return;
            }
            this._tasksToExecute.Add(new(this._currentActionId++, parameters));
        }

        private void DeleteAction(List<string> location, ParamsContainer parameters)
        {
            LocationStatus status = this._locationStatuses[location.ToKey()];
            var statusActions = new List<int>(status.GetActionsOfType(parameters.ActionName));
            if (this._budget < ActionCostCalculator.CalculateCost(parameters, ActionCostCalculator.ActionMode.Delete) * statusActions.Count)
            {
                return;
            }
            while (statusActions.Count > 0)
            {
                this._tasksToExecute.Add(new(statusActions[0]));
                status.RemoveAction(statusActions[0]);
                statusActions.RemoveAt(0);
            }
        }

        private void RunTriggerChecks()
        {
            // Run both local and global triggers
            this._simulationSettings.Locations.ForEach(loc => this.RunLocalTriggerChecks(loc));
            this.RunGlobalTriggerChecks();
        }

        private void RunLocalTriggerChecks(LocationDefinition location, int depth = 0, List<string>? parent = null)
        {
            // Loop through all the triggers and if they should be applied then apply them
            List<string> current = Generic.BuildLocation(parent, location.Coord);
            string key = current.ToKey();

            foreach (var trigger in this._triggersForLocalLocations)
            {
                if (ITrigger.IsValidDepth(depth, trigger.DepthRange))
                {
                    trigger.Apply(this._locationTrackers[key]);
                }
            }
            // Recurse for the sub locations
            if (location.SubLocations != null)
            {
                location.SubLocations.ForEach((l) => this.RunLocalTriggerChecks(l, depth + 1, current));
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

        private async Task<long> WaitForOurTurn()
        {
            // Request the status every _statusPingDelayInMs milliseconds until it is the who turn and return the budget
            do
            {
                await Task.Delay(_statusPingDelayInMs);

                if (!this._running)
                {
                    throw new TaskCanceledException();
                }

                var status = await this._client.GetStatus();

                if (status.Budget == -1)
                {
                    if (status.IsWhoTurn)
                    {
                        await this._client.EndTurn();
                    }

                    this.Stop();
                    throw new TaskCanceledException();
                }

                if (status.IsWhoTurn)
                {
                    if (status.Budget < 0)
                    {
                        Log.Error("Budget below 0: {Budget}", status.Budget);
                    }
                    return this._budget + status.Budget;
                }
            }
            while (true);
        }

        public async ValueTask DisposeAsync()
        {
            await this._client.DisposeAsync();
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
                        if (action.Mode == "create")
                        {
                            this._locationStatuses[action.Parameters.Location.ToKey()].AddAction(dict[result.Id]);
                        }
                        else
                        {
                            this._locationStatuses[action.Parameters.Location.ToKey()].RemoveAction(result.Id);
                        }
                    }
                }
            }
        }

        private void InitialiseLocationInformation(List<LocationDefinition> locations, List<string>? parent = null)
        {
            // Creates the global tracker and then creates a tracker for each location
            this._locationTrackers[ALL_LOCATION_ID] = new LocationTracker(ALL_LOCATION_ID, null);
            foreach (var location in locations)
            {
                List<string> loc = Generic.BuildLocation(parent, location.Coord);
                string localLocationKey = loc.ToKey();
                LocationStatus status = new(loc);

                this._locationStatuses.Add(localLocationKey, status);
                this._locationTrackers.Add(localLocationKey, new LocationTracker(localLocationKey, status));
                if (location.SubLocations != null)
                {
                    this.InitialiseLocationInformation(location.SubLocations, loc);
                }
            }
        }

        private async Task GetTrackingInformation()
        {
            await Task.WhenAll(this._simulationSettings.Locations.Select(loc => this.GetLocationTrackingInformation(loc)).ToArray());
            this._locationTrackers[ALL_LOCATION_ID].Track(this.GetTotalsForAll());
        }

        private async Task GetLocationTrackingInformation(LocationDefinition location, List<string>? parent = null)
        {
            // Create the tracker and populate it with the inital information
            var current = Generic.BuildLocation(parent, location.Coord);
            string localLocationKey = current.ToKey();
            LocationTracker tracker = this._locationTrackers[localLocationKey];
            var trackingInformation = await this._client.GetInfoTotals(new SearchRequest(new() { this._locationStatuses[localLocationKey].Location }));
            tracker.Track(trackingInformation[0]);

            // Recursively populate sub locations
            if (location.SubLocations != null)
            {
                Task.WaitAll(location.SubLocations.Select(loc => this.GetLocationTrackingInformation(loc, current)).ToArray());
            }
        }

        private InfectionTotals GetTotalsForAll()
        {
            // Sum over all the top level locations
            InfectionTotals totals = new(new() { }, 0, 0, 0, 0, 0, 0, 0);
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

        public void CalculateBestAction(long budgetAvailable, List<string> loc, double threshold = 0.1)
        {
            string location = loc.ToKey();

            // Calculate amount of people infected
            var asymptomaticInfectedInfectious = this._locationTrackers[location].Latest?.GetParameterTotals(TrackingValue.AsymptomaticInfectedInfectious) ?? 0;
            var symptomaticInfected = this._locationTrackers[location].Latest?.GetParameterTotals(TrackingValue.Symptomatic) ?? 0;

            long locationPopulation = this._locationTrackers[location].Latest?.GetTotalPeople() ?? 0;

            // Calcualte infection rate
            double infectionRate = (asymptomaticInfectedInfectious + symptomaticInfected) / (double)locationPopulation;

            // Using the proportion of people who are infected calculate an appropriate budget for that location
            long totalInfections = this._locationTrackers[ALL_LOCATION_ID].Latest?.GetTotalPeople() ?? 0 - this._locationTrackers[ALL_LOCATION_ID].Latest?.GetParameterTotals(TrackingValue.Uninfected) ?? 0;
            long infectionsInArea = this._locationTrackers[location].Latest?.GetTotalPeople() ?? 0 - this._locationTrackers[location].Latest?.GetParameterTotals(TrackingValue.Uninfected) ?? 0;

            double percentageOfInfections = (double)infectionsInArea / totalInfections;

            // Limit the amount of money spent each term to a third of the budget
            budgetAvailable = (long)Math.Round(budgetAvailable / 3.0, 0);

            double budgetForLocation = (long)Math.Round(budgetAvailable * percentageOfInfections, 0);

            // Get all WhoActions
            List<ParamsContainer> actions;

            if (infectionRate > threshold)
            {
                actions = this.GetWhoActions(loc, ActionCostCalculator.ActionMode.Create, budgetForLocation);
            }
            else
            {
                actions = this.GetWhoActions(loc, ActionCostCalculator.ActionMode.Delete, budgetForLocation);
            }

            // Actions which are collectively all available given the budget
            List<ParamsContainer> actionsAvailable = new();
            var existingActions = this._locationStatuses[location].AppliedActions;

            //Log.Information("Infection rate {InfectionRate}", infectionRate);
            if (infectionRate > threshold)
            {
                foreach (var action in actions)
                {
                    if (!existingActions.Contains(action.ActionName))
                    {
                        double actionCost = ActionCostCalculator.CalculateCost(action, ActionCostCalculator.ActionMode.Create);

                        if (actionCost < budgetForLocation)
                        {
                            actionsAvailable.Add(action);
                            budgetForLocation -= actionCost;
                        }
                    }
                }
            }
            else if (infectionRate <= (threshold * 0.75))
            {
                foreach (var action in actions)
                {
                    if (existingActions.Contains(action.ActionName))
                    {
                        double actionCost = ActionCostCalculator.CalculateCost(action, ActionCostCalculator.ActionMode.Delete);
                        var actionList = this._locationStatuses[location].GetActionsOfType(action.ActionName);
                        if (actionList.Count > 0)
                        {
                            foreach (var appliedAction in actionList)
                            {
                                if (actionCost < budgetForLocation)
                                {
                                    this._tasksToExecute.Add(new WhoAction(appliedAction));
                                    budgetForLocation -= actionCost;
                                }
                            }
                        }
                    }
                }
            }

            // Convert actions to WhoActions and add to tasks which need to be executed
            foreach (var action in actionsAvailable)
            {
                this._tasksToExecute.Add(new(this._currentActionId++, action));
            }
        }

        public List<ParamsContainer> GetWhoActions(List<string> loc, ActionCostCalculator.ActionMode mode, double budgetForLocation)
        {
            List<ParamsContainer> actions = new();

            string location = loc.ToKey();
            long locationPopulation = this._locationTrackers[location].Latest?.GetTotalPeople() ?? 0;

            TestAndIsolation testAndIsolation = new(1, 14, (long)Math.Round((locationPopulation * 0.5), 0), loc, false);
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

                    ShieldingProgram shieldingProgram = new(loc);
                    actions.Add(shieldingProgram);

                    InvestInHealthServices investInHealthServices = new((int)Math.Round((investmentBudget * 0.3), 0));
                    actions.Add(investInHealthServices);

                    if (this._vaccineInvestment < VaccineCost)
                    {
                        int amountToInvest = (int)Math.Round(investmentBudget * 0.3);
                        int amountNeeded = VaccineCost - this._vaccineInvestment;
                        int actualAmountToInvest = Math.Min(amountToInvest, amountNeeded);
                        InvestInVaccine investInVaccine = new(actualAmountToInvest);
                        this._vaccineInvestment += actualAmountToInvest;
                        actions.Add(investInVaccine);
                    }

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

                    ShieldingProgram shieldingProgramDelete = new(loc);
                    actions.Add(shieldingProgramDelete);

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
