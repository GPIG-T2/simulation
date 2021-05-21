using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private readonly Action _resultingAction;
        private readonly (int, int) _depthRange;
        private readonly Func<CustomTrackingFunctionParameters, bool> _comparisonFunction;

        public TrackingValue Parameter => this._parameter;

        public int Timespan => this._timespan;

        public Action ResultingAction => this._resultingAction;

        public (int, int) DepthRange => this._depthRange;

        public Func<CustomTrackingFunctionParameters, bool> ComparisonFunction => this._comparisonFunction;

        public CustomTrigger(TrackingValue parameter, Func<CustomTrackingFunctionParameters, bool> comparisonFunction, Action resultingAction, int timespan, (int, int)? depthRange = null)
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

            int currParameterCount = current.GetParameterTotals(this.Parameter);
            int prevParameterCount = previous.GetParameterTotals(this.Parameter);
            int currTotal = current.GetTotalPeople();
            int prevTotal = previous.GetTotalPeople();
            float change = ((float)currParameterCount / currTotal) / ((float)prevParameterCount / prevTotal);

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
                this.ResultingAction.Invoke();
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
        public int CurrentParameterCount;
        /// <summary>
        /// The total number of people in the are for that time span in the current period
        /// </summary>
        public int CurrentTotalPopulation;
        /// <summary>
        /// The total amount of people that match the parameter of the trigger in the last period
        /// </summary>
        public int PreviousParameterCount;
        /// <summary>
        /// The total number of people in the are for that time span in the previous period
        /// </summary>
        public int PreviousTotalPopulation;
        /// <summary>
        /// The change calculated as (curr_count/curr_total) / (prev_count/prev_total)
        /// </summary>
        public float Change;
    }
}
