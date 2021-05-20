using System;

namespace Virus
{
    /// <summary>
    /// Representation of the world.
    /// </summary>
    /// <remarks>
    /// This is the top-level container providing the world for the simulation to
    /// run in.
    ///
    /// Updates to the nodes and edges are done in parallel, enabling full
    /// parallelisation when processing these updates.
    /// </remarks>
    public class World
    {
        // TODO: nodes and edges should be dictionaries to improve compatability
        // with the interface
        private readonly Node[] _nodes;
        private readonly Edge[] _edges;
        private readonly Virus _virus;
        private int _day;

        /// <summary>
        /// Takes in a list of nodes, edges must than be constructed utilizing these nodes.
        /// Edges represented as a composite of list of base interactivities, populations
        /// and the indices of the nodes it connects.
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="edges"></param>
        /// <param name="virus"></param>
        public World(Node[] nodes, Edge[] edges, Virus virus)
        {
            this._nodes = nodes;
            this._edges = edges;
            this._virus = virus;
            this._day = 0;
        }

        /// <summary>
        /// Update goes through all edges to produce infections to add nodes.
        /// Each node and edge is updated individually.
        /// </summary>
        public void Update()
        {
            int[] edgeInfections = new int[this._nodes.Length];
            Array.Clear(edgeInfections, 0, edgeInfections.Length);

            // Goes through all edges and increases the number of new infections to add to the node.
            foreach (Edge e in this._edges)
            {
                (int left, int right) = e.Update(this._virus);
                edgeInfections[e.Left.Index] += left;
                edgeInfections[e.Right.Index] += right;
            }

            // Goes through all nodes, updates each individual node and infects people based off the edges.
            foreach (Node n in this._nodes)
            {
                n.Update(this._virus);
                n.Infect(edgeInfections[n.Index]);
                n.IncrementHead();
            }

            this._day++;
        }
    }
}
