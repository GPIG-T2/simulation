using System;
using System.Collections.Generic;

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
    }
}
