using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    /// <summary>
    /// An edge in the node-graph.
    /// </summary>
    public struct Edge
    {
        /// <summary>
        /// The starting node of the edge.
        /// </summary>
        public List<string> From { get; set; }

        /// <summary>
        /// The connecting node of the edge.
        /// </summary>
        public List<string> To { get; set; }

        public Edge(List<string> from, List<string> to)
        {
            this.From = from;
            this.To = to;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class Edge {\n");
            sb.Append("  From: ").Append(this.From).Append("\n");
            sb.Append("  To: ").Append(this.To).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }
}
