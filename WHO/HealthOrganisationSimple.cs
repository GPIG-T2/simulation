using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Interface.Client;
using Models;
using Serilog;

namespace WHO
{
    public class HealthOrganisationSimple : IHealthOrganisation
    {
        private const int _statusPingDelayInMs = 100;
        private const long _lockdownLevel = 10_000;
        private const long _openupLevel = 0;
        private const long _closeBordersLevel = 150_000;
        private const long _openBordersLevel = 50_000;

        private readonly IClient _client;
        private SimulationSettings? _settings;
        private bool _running = true;
        private Dictionary<string, NodeTracking> _nodeTracking = new();
        private int _actionId = 0;
        private int _day = 0;

        public HealthOrganisationSimple(string uri) : this(new WebSocket(uri))
        {
        }

        public HealthOrganisationSimple(IClient client)
        {
            this._client = client;
            this._client.Closed += this.Stop;
        }

        public async Task Run()
        {
            var settings = await this.GetSettings();
            var initialTotals = await this._client.GetInfoTotals(
                new(settings.Locations.Select(l => new List<string>() { l.Coord }).ToList())
            );

            this._nodeTracking.EnsureCapacity(settings.Locations.Count);
            foreach (var (l, t) in settings.Locations.Zip(initialTotals))
            {
                this._nodeTracking[l.Coord] = new(l, t);
            }

            await this.Loop();
        }

        public async ValueTask DisposeAsync()
        {
            await this._client.DisposeAsync();
        }

        private async Task Loop()
        {
            try
            {
                while (this._running)
                {
                    Log.Information("Waiting for our turn...");
                    await this.WaitForOurTurn();

                    await this.UpdateTracking();
                    await this.ApplyNecessaryActions();

                    await this._client.EndTurn();
                    this._day++;
                }
            }
            catch (TaskCanceledException) { }
        }

        private async Task UpdateTracking()
        {
            var totals = await this._client.GetInfoTotals(new(this._nodeTracking.Values.Select(nt => nt.Location).ToList()));

            foreach (var total in totals)
            {
                this._nodeTracking[total.Location[0]].Update(total);
            }

            Log.Information("Node {Name} now at {Infections} infections", "N0", this._nodeTracking["N0"].CumulativeInfections);
        }

        private async Task ApplyNecessaryActions()
        {
            var actions = Enumerable.Empty<WhoAction>();
            int created = 0;
            int deleted = 0;

            bool eliminated = !_nodeTracking.Values.Any(nt => nt.PreviousTotals.IsInfected);
            long totalInfections = _nodeTracking.Values.Select(nt => nt.CumulativeInfections).Sum();

            foreach (var tracking in this._nodeTracking.Values)
            {
                if (tracking.CumulativeInfections > _lockdownLevel && tracking.LockdownAction == null)
                {
                    // We need to take action.
                    tracking.LockdownAction = new(this._actionId++, new Models.Parameters.StayAtHome(tracking.Location));
                    actions = actions.Append(tracking.LockdownAction);
                    created++;
                }
                else if (tracking.LockdownAction != null && tracking.CumulativeInfections < _openupLevel)
                {
                    // Infection has gone.
                    actions = actions.Append(new(tracking.LockdownAction.Id));
                    tracking.LockdownAction = null;
                    deleted++;
                }

                if (totalInfections > _closeBordersLevel && tracking.CloseBordersAction == null)
                {
                    tracking.CloseBordersAction = new(this._actionId++, new Models.Parameters.CloseBorders(tracking.Location));
                    actions = actions.Append(tracking.CloseBordersAction);
                    created++;
                }
                else if (totalInfections < _openBordersLevel && tracking.CloseBordersAction != null)
                {
                    // Infection has been wiped out
                    actions = actions.Append(new(tracking.CloseBordersAction.Id));
                    tracking.CloseBordersAction = null;
                    deleted++;
                }
            }

            await this._client.ApplyActions(actions.ToList());
            Log.Information("Created {Created} and deleted {Deleted} actions on day {Day}", created, deleted, this._day);
        }

        private async Task WaitForOurTurn()
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
                    return;
                }
            }
            while (true);
        }

        private void Stop()
        {
            this._running = false;
        }

        private async ValueTask<SimulationSettings> GetSettings()
        {
            if (this._settings != null)
            {
                return this._settings;
            }

            var settings = await this._client.GetSettings();
            this._settings = settings;
            return settings;
        }

        private class NodeTracking
        {
            public List<string> Location { get; }
            public WhoAction? LockdownAction { get; set; }
            public WhoAction? CloseBordersAction { get; set; }
            public long CumulativeInfections { get; set; } = 0;
            public InfectionTotals PreviousTotals { get; set; }

            public bool NeedsAction => this.PreviousTotals.IsInfected && this.CumulativeInfections > _lockdownLevel;
            public bool HasCleared => !this.PreviousTotals.IsInfected && this.Location != null;

            public NodeTracking(LocationDefinition node, InfectionTotals totals)
            {
                this.Location = new() { node.Coord };
                this.PreviousTotals = totals;
            }

            public void Update(InfectionTotals totals)
            {
                var change = totals.AsymptomaticInfectedNotInfectious - this.PreviousTotals.AsymptomaticInfectedNotInfectious;

                if (change > 0)
                {
                    this.CumulativeInfections += change;
                }

                this.PreviousTotals = totals;
            }
        }
    }
}
