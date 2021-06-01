using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Models;
using Models.Parameters;
using Serilog;
using WHO.Tracking;
using WHO.Interface;
using WHO.Extensions;
using System.Linq;
using Serilog.Debugging;

namespace WHO
{
    class HealthOrganisation : IAsyncDisposable
    {

        public static HealthOrganisation Instance { get; private set; }

        /// <summary>
        /// Constant for referring to the global tracker
        /// </summary>
        private const string ALL_LOCATION_ID = "_all";

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

        /// <summary>
        /// Keeps track of the current action id 
        /// </summary>
        private int _currentActionId = 0;

        public HealthOrganisation(string uri)
        {
            if (Instance != null)
            {
                throw new InvalidOperationException("Cannot instantiate two copies of the health organisation");
            }
            Instance = this;
            this.CreateTriggers();
            this._client = new WebSocket(uri);
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

        public async Task Run()
        {
            if (this._running)
            {
                throw new InvalidOperationException("Health Organisation is already running");
            }

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
                if (trigger.IsValidDepth(depth))
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
                if (trigger.IsValidDepth(0))
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

        //ToDo Ask about budget endpoints
        // Will each location have its own particular budget?
        private void calculateBestAction(int budgetAvailable, List<string> loc, float threshold = 1.2f) 
        {
            string location = string.Join("", loc);

            // Calculate amount of people infected
            var asymptomaticInfectedInfectious = this._locationTrackers[location].Latest.Value.GetParameterTotals(TrackingValue.AsymptomaticInfectedInfectious);
            var symptomaticInfected = this._locationTrackers[location].Latest.Value.GetParameterTotals(TrackingValue.Symptomatic);

            int locationPopulation = this._locationTrackers[location].Latest.Value.GetTotalPeople();

            // Calcualte infection rate
            float infectionRate = (asymptomaticInfectedInfectious + symptomaticInfected)/locationPopulation;

            if (infectionRate > threshold) {
                List<WhoAction> actions = getWhoActions(loc);

                List<WhoAction> actionsAvailable = new List<WhoAction>();

                foreach (WhoAction action in actions)
                {
                    // Need to figure out how to get cost of each action
                    if (action.cost < budgetAvailable) {
                        actionsAvailable.Add(action);
                        budgetAvailable = budgetAvailable - action.cost;
                    }
                }

                // Randomly select an action based on actions available
                var random = new Random();
                int index = random.Next(actionsAvailable.Count);
                this._tasksToExecute.Add(actionsAvailable[index]);

            }
        }

        private List<WhoAction> getWhoActions(List<string> loc) {
            // ToDo write this function
            List<WhoAction> whoActions = new List<WhoAction>();
            
            TestAndIsolation testAndIsolation = new(0, 0, 0, loc, false);
            WhoAction testAndIsolationAction = new(this._currentActionId++, testAndIsolation);
            whoActions.Add(testAndIsolationAction);
            
            CloseBorders closeBorders = new(loc);
            WhoAction closeBordersAction = new(this._currentActionId++, closeBorders);
            whoActions.Add(closeBordersAction);

            CloseRecreationalLocations closeRecreationalLocations = new(loc);
            WhoAction closeRecreationalLocationsAction = new(this._currentActionId++, closeRecreationalLocations);
            whoActions.Add(closeRecreationalLocationsAction);

            CloseSchools closeSchools = new(loc);
            WhoAction closeSchoolsAction = new(this._currentActionId++, closeSchools);
            whoActions.Add(closeSchoolsAction);

            Curfew curfew = new(loc);
            WhoAction curfewAction = new(this._currentActionId++, curfew);
            whoActions.Add(curfewAction);

            // Need to think about investment logic
            Furlough furlough = new(1000, loc);
            WhoAction furloughAction = new(this._currentActionId++, furlough);
            whoActions.Add(furloughAction);

            HealthDrive healthDrive = new(loc);
            WhoAction healthDriveAction = new(this._currentActionId++, healthDrive);
            whoActions.Add(healthDriveAction);

            // Need to think about investment logic
            InformationPressRelease informationPressRelease = new(1000, loc);
            WhoAction informationPressReleaseAction = new(this._currentActionId++, informationPressRelease);
            whoActions.Add(informationPressReleaseAction);

            // Need to think about investment logic
            InvestInHealthServices investInHealthServices = new(1000);
            WhoAction investInHealthServicesAction = new(this._currentActionId++, investInHealthServices);
            whoActions.Add(investInHealthServicesAction);

            // Need to think about investment logic
            InvestInVaccine investInVaccine = new(1000);
            WhoAction investInVaccineAction = new(this._currentActionId++, investInVaccine);
            whoActions.Add(investInVaccineAction);

            // Need to think about amount loaned, I don't even know what the loan action does
            Loan loan = new(1000);
            WhoAction loanAction = new(this._currentActionId++, loan);
            whoActions.Add(loanAction);

            // Need to think about the MaskProvisionLevel
            MaskMandate maskMandate = new(loc, 0);
            WhoAction maskMandateAction = new(this._currentActionId++, maskMandate);
            whoActions.Add(maskMandateAction);

            // Need to think about distances
            MovementRestrictions movementRestrictions = new(loc, 50);
            WhoAction movementRestricitionsAction = new(this._currentActionId++, movementRestrictions);
            whoActions.Add(movementRestricitionsAction);

            // Need to think about distances
            SocialDistancingMandate socialDistancingMandate = new(loc, 50);
            WhoAction socialDistancingMandateAction = new(this._currentActionId++, socialDistancingMandate);
            whoActions.Add(socialDistancingMandateAction);

            StayAtHome stayAtHome = new(loc);
            WhoAction stayAtHomeAction = new(this._currentActionId++, stayAtHome);
            whoActions.Add(stayAtHomeAction);

            return whoActions;
        }

    }
}
