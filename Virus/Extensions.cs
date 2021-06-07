using System;
using System.Collections.Generic;
using Models;

namespace Virus
{
    public static class Extensions
    {
        /// <summary>
        /// Takes in a location and converts it to an integer index.
        /// </summary>
        /// <param name="location"></param>
        public static int ToNodeIndex(this List<string> location) => int.Parse(location[0].Substring(1));

        /// <summary>
        /// Gets the node from the location string array.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <param name="location">The location of the target node.</param>
        /// <returns>The node at the given location.</returns>
        public static Node Get(this Node[] nodes, List<string> location) =>
            nodes[location.ToNodeIndex()];

        /// <summary>
        /// For a > 1, provides an exponential curve of values between 0 and 1, taking in values between 0 and 1.
        /// </summary>
        /// <param name="a">The exponential factor, a grows so does the rate of growth of the curve.</param>
        /// <param name="x">Input x between 0 and 1.</param>
        /// <returns>A value between 0 and 1.</returns>
        public static double Exponential0And1(double a, double x) => (Math.Pow(a, x) - 1) / (a - 1);

        /// <summary>
        /// Sigmoid function given values between 0 and 1 returns a scaled value between 0 and 1
        /// </summary>
        /// <param name="a">The exponential factor</param>
        /// <param name="b">The offset, values > 5 provide sigmoid shape</param>
        /// <param name="x">Input x between 0 and 1</param>
        /// <returns></returns>
        public static double Sigmoid(double a, double b, double x) => 1.0 / (1 + Math.Pow(a, (5 - (b * x))));

        /// <summary>
        /// Converts a set of infection totals to a CSV line.
        /// </summary>
        /// <param name="t">The infection totals to convert.</param>
        /// <returns>The string CSV line.</returns>
        public static string ToCsvLine(this InfectionTotals t)
            => string.Format("{0},{1},{2},{3},{4},{5},{6}", t.Uninfected,
                    t.AsymptomaticInfectedNotInfectious, t.AsymptomaticInfectedInfectious,
                    t.Symptomatic, t.SeriousInfection, t.Dead, t.RecoveredImmune
                );
    }
}
