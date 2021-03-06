using System;
using System.Collections.Generic;

namespace WHO.Tracking
{
    public interface ITrigger
    {
        /// <summary>The parameter that will be checked</summary>
        public TrackingValue Parameter { get; }

        /// <summary>The timespan over which the values are calculated in number of days</summary> 
        public int Timespan { get; }

        /// <summary>Action that will be called when the conditions are met</summary>
        public Action<List<string>?> ResultingAction { get; }

        /// <summary>The range of location depths that the function will be on. (-1, -1) will be used to represent all depths. First value inclusive second value is exclusive.</summary> 
        public (int, int) DepthRange { get; }

        /// <summary>
        /// Checks if the tracked location matches the conditions and then executes the ResultingAction
        /// </summary>
        /// <param name="tracker">The tracked location to check</param>
        public void Apply(LocationTracker tracker);

        /// <summary>
        /// Checks whether the depth is within the range given in the trigger definition
        /// </summary>
        /// <param name="depth">The depth that is being checked</param>
        /// <returns>true if the depth is within the range [a, b) otherwise false</returns>
        public static bool IsValidDepth(int depth, (int, int) depthRange)
        {
            // This means no depth checks
            if (depthRange == (-1, -1))
            {
                return true;
            }

            if (depthRange.Item1 == -1)
            {
                return depth < depthRange.Item2;
            }
            else if (depthRange.Item2 == -1)
            {
                return depth >= depthRange.Item1;
            }
            return depth >= depthRange.Item1 && depth < depthRange.Item2;
        }

    }
}
