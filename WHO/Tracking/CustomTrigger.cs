using System;
using System.Collections.Generic;
using Models;
using WHO.Extensions;

namespace WHO.Tracking
{
    /// <summary>
    /// The CustomTrigger applies a custom function so that more complex calculations can be
    /// carried out on the parameter, such as if the amount has increased by 10% and the 
    /// amount passed 200 cases.
    /// </summary>
    class CustomTrigger : ITrigger
    {
        private readonly TrackingValue _parameter;
        private readonly int _timespan;
        private readonly Action<List<string>?> _resultingAction;
        private readonly (int, int) _depthRange;
        private readonly Func<CustomTrackingFunctionParameters, bool> _comparisonFunction;

        public TrackingValue Parameter => this._parameter;

        public int Timespan => this._timespan;

        public Action<List<string>?> ResultingAction => this._resultingAction;

        public (int, int) DepthRange => this._depthRange;

        public Func<CustomTrackingFunctionParameters, bool> ComparisonFunction => this._comparisonFunction;

        public CustomTrigger(TrackingValue parameter, Func<CustomTrackingFunctionParameters, bool> comparisonFunction, Action<List<string>?> resultingAction, int timespan, (int, int)? depthRange = null)
        {
            this._parameter = parameter;
            this._comparisonFunction = comparisonFunction;
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

            long currParameterCount = current.GetParameterTotals(this.Parameter);
            long prevParameterCount = previous.GetParameterTotals(this.Parameter);
            long currTotal = current.GetTotalPeople();
            long prevTotal = previous.GetTotalPeople();
            double change = ((double)currParameterCount / currTotal) / ((double)prevParameterCount / prevTotal);

            CustomTrackingFunctionParameters customParams = new()
            {
                CurrentParameterCount = currParameterCount,
                PreviousParameterCount = prevParameterCount,
                CurrentTotalPopulation = currTotal,
                PreviousTotalPopulation = prevTotal,
                Change = change
            };

            if (this.ComparisonFunction.Invoke(customParams))
            {
                this.ResultingAction.Invoke(tracker.Status?.Location);
            }

        }
    }

    /// <summary>
    /// The parameters that are given to the custom function. 
    /// </summary>
    struct CustomTrackingFunctionParameters
    {
        /// <summary>
        /// The total amount of people that match the parameter of the trigger in the current period
        /// </summary>
        public long CurrentParameterCount;
        /// <summary>
        /// The total number of people in the are for that time span in the current period
        /// </summary>
        public long CurrentTotalPopulation;
        /// <summary>
        /// The total amount of people that match the parameter of the trigger in the last period
        /// </summary>
        public long PreviousParameterCount;
        /// <summary>
        /// The total number of people in the are for that time span in the previous period
        /// </summary>
        public long PreviousTotalPopulation;
        /// <summary>
        /// The change calculated as (curr_count/curr_total) / (prev_count/prev_total)
        /// </summary>
        public double Change;
    }
}
