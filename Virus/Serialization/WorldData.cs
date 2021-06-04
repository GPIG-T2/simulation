using System;
using System.Collections.Generic;
using System.Linq;

namespace Virus.Serialization
{
    public class WorldData
    {
        public List<NodeData> Nodes { get; set; }
        public List<EdgeData> Edges { get; set; }
        public Virus Virus { get; set; }
        public Models.SimulationSettings.SelectedMap Map { get; set; }

        public WorldData(List<NodeData> nodes,
            List<EdgeData> edges,
            Virus virus,
            Models.SimulationSettings.SelectedMap map)
        {
            this.Nodes = nodes;
            this.Edges = edges;
            this.Virus = virus;
            this.Map = map;
        }

        public static explicit operator World(WorldData data)
        {
            Node[] nodes = new Node[data.Nodes.Count];
            Edge[] edges = new Edge[data.Edges.Count];

            foreach ((NodeData node, int i) in data.Nodes.Select((n, i) => (n, i)))
            {
                nodes[i] = new Node(i, node.Population, node.Interactivity, node.Name,
                    node.Position, node.Demographics, node.Gdp, node.TestingCapacity);
            }

            foreach ((EdgeData edge, int i) in data.Edges.Select((e, i) => (e, i)))
            {
                edges[i] = new Edge(nodes[edge.Left], nodes[edge.Right], edge.Population, edge.Interactivity, edge.Distance);
            }

            return new World(nodes, edges, data.Virus);
        }
    }
}
