using System;
using System.Collections.Generic;
using Models;
using WHO.Extensions;

namespace WHO.Tracking
{
    /// <summary>
    /// Used to call the ResultingAction when the ComparisonFunction returns true, in the case of the
    /// BasicTrigger it only accepts either LESS_THAN or GREATER_THAN to check whether the percentage
    /// change of a parameter over the given timespan has changed by more or less than the threshold.
    /// </summary>
    public class BasicTrigger : ITrigger
    {

        private readonly TrackingValue _parameter;
        private readonly int _timespan;
        private readonly Action<List<string>?> _resultingAction;
        private readonly (int, int) _depthRange;
        private readonly TrackingFunction _comparisonFunction;
        private readonly double _threshold;

        public TrackingValue Parameter => this._parameter;

        public int Timespan => this._timespan;

        public Action<List<string>?> ResultingAction => this._resultingAction;

        public (int, int) DepthRange => this._depthRange;

        public TrackingFunction ComparisonFunction => this._comparisonFunction;

        public double Threshold => this._threshold;

        public BasicTrigger(TrackingValue parameter, TrackingFunction comparisonFunction, double threshold, Action<List<string>?> resultingAction, int timespan, (int, int)? depthRange = null)
        {
            this._parameter = parameter;
            this._comparisonFunction = comparisonFunction;
            this._threshold = threshold;
            this._resultingAction = resultingAction;
            this._timespan = timespan;
            this._depthRange = depthRange ?? (-1, -1);
        }

        public void Apply(LocationTracker tracker)
        {
            int currentLastestTimestamp = tracker.Count - 1;
            int currentEarliestTimestamp = currentLastestTimestamp - this.Timespan;

            int previousLatestTimestamp = currentEarliestTimestamp - 1;
            int previousEarliestTimestamp = previousLatestTimestamp - this.Timespan;

            if (previousEarliestTimestamp < 0)
            {
                return;
            }

            InfectionTotals current;
            InfectionTotals previous;

            if (this.Timespan == 0)
            {
                current = tracker.Get(currentLastestTimestamp);
                previous = tracker.Get(previousLatestTimestamp);
            }
            else
            {
                current = tracker.GetSum(currentEarliestTimestamp, currentLastestTimestamp);
                previous = tracker.GetSum(previousEarliestTimestamp, previousLatestTimestamp);
            }

            double currentPercentage = this.GetPercentageForInfectionTotals(current);
            double previousPercentage = this.GetPercentageForInfectionTotals(previous);

            double change = currentPercentage / previousPercentage;
            bool evaluation = false;
            switch (this.ComparisonFunction)
            {
                case TrackingFunction.GREATER_THAN:
                    evaluation = change > this.Threshold;
                    break;
                case TrackingFunction.LESS_THAN:
                    evaluation = change < this.Threshold;
                    break;
            }

            if (evaluation)
            {
                this.ResultingAction.Invoke(tracker.Status?.Location);
            }

        }

        protected double GetPercentageForInfectionTotals(InfectionTotals totals)
        {
            long total = totals.GetTotalPeople();

            long parameter = totals.GetParameterTotals(this.Parameter);

            return (double)parameter / total;
        }

    }
}
