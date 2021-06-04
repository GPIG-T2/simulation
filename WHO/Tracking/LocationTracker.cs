using System;
using System.Collections.Generic;
using Models;

namespace WHO.Tracking
{
    public class LocationTracker
    {

        private readonly List<InfectionTotals> _totals = new();
        private readonly string _location;
        private readonly LocationStatus? _status;

        public int Count { get { return this._totals.Count; } }
        public InfectionTotals? Latest => this.Count > 0 ? this._totals[this.Count - 1] : null;
        public string LocationKey => this._location;
        public LocationStatus? Status => this._status;

        public LocationTracker(string location, LocationStatus? status)
        {
            this._location = location;
            this._status = status;
        }

        public void Track(InfectionTotals total)
        {
            this._totals.Add(total);
        }

        /// <summary>
        /// Get the infection totals for a given timestep
        /// </summary>
        /// <param name="timeStep">The timestep that is requested</param>
        /// <returns>The InfectionTotals at that timestep</returns>
        public InfectionTotals Get(int timeStep)
        {
            if (timeStep < 0 || timeStep >= this.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(timeStep), timeStep, $"{nameof(timeStep)} must be between 0 and {this.Count}");
            }
            return this._totals[timeStep];
        }


        /// <summary>
        /// Gets the difference in InfectionTotals between two timesteps
        /// </summary>
        /// <param name="firstStep">The earlier timestep</param>
        /// <param name="secondStep">The later timestep</param>
        /// <returns>The InfectionTotal difference</returns>
        public InfectionTotals GetChange(int firstStep, int secondStep)
        {
            if (firstStep < 0 || firstStep >= this.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(firstStep), firstStep, $"{nameof(firstStep)} must be between 0 and {this.Count}");
            }

            if (secondStep < 0 || secondStep >= this.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(secondStep), secondStep, $"{nameof(secondStep)} must be between 0 and {this.Count}");
            }

            var first = this._totals[firstStep];
            var second = this._totals[secondStep];

            return new InfectionTotals(
                location: first.Location,
                asymptomaticInfectedInfectious: second.AsymptomaticInfectedInfectious - first.AsymptomaticInfectedInfectious,
                asymptomaticInfectedNotInfectious: second.AsymptomaticInfectedNotInfectious - first.AsymptomaticInfectedNotInfectious,
                dead: second.Dead - first.Dead,
                recoveredImmune: second.RecoveredImmune - first.RecoveredImmune,
                seriousInfection: second.SeriousInfection - first.SeriousInfection,
                symptomatic: second.Symptomatic - first.Symptomatic,
                uninfected: second.Uninfected - first.Uninfected
            );
        }

        /// <summary>
        /// Gets the sum of all the infection totals between two timesteps
        /// </summary>
        /// <param name="firstStep"></param>
        /// <param name="secondStep"></param>
        /// <returns></returns>
        public InfectionTotals GetSum(int firstStep, int secondStep)
        {
            if (firstStep < 0 || firstStep >= this.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(firstStep), firstStep, $"{nameof(firstStep)} must be between 0 and {this.Count}");
            }

            if (secondStep < 0 || secondStep >= this.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(secondStep), secondStep, $"{nameof(secondStep)} must be between 0 and {this.Count}");
            }

            if (secondStep < firstStep)
            {
                throw new ArgumentOutOfRangeException(nameof(secondStep), secondStep, $"{nameof(secondStep)} must be greater than or equal to {nameof(firstStep)}");
            }

            var first = this.Get(firstStep);

            var sum = new InfectionTotals(
                location: first.Location,
                asymptomaticInfectedInfectious: first.AsymptomaticInfectedInfectious,
                asymptomaticInfectedNotInfectious: first.AsymptomaticInfectedNotInfectious,
                dead: first.Dead,
                recoveredImmune: first.RecoveredImmune,
                seriousInfection: first.SeriousInfection,
                symptomatic: first.Symptomatic,
                uninfected: first.Uninfected);

            for (int i = firstStep + 1; i <= secondStep; i++)
            {
                var next = this.Get(i);
                sum.AsymptomaticInfectedInfectious += next.AsymptomaticInfectedInfectious;
                sum.AsymptomaticInfectedNotInfectious += next.AsymptomaticInfectedNotInfectious;
                sum.Dead += next.Dead;
                sum.RecoveredImmune += next.RecoveredImmune;
                sum.SeriousInfection += next.SeriousInfection;
                sum.Symptomatic += next.Symptomatic;
                sum.Uninfected += next.Uninfected;
            }

            return sum;
        }
    }
}
