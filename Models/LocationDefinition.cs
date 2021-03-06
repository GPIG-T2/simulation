using System;
using System.Text;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Models
{
    /// <summary>
    /// A definition for a given location that is available.
    /// </summary>
    public class LocationDefinition
    {
        /// <summary>
        /// Gets or Sets Coord
        /// </summary>
        public string Coord { get; set; }

        /// <summary>
        /// An array of sub-locations that are under this location. If not given, or empty, then there are no sub-locations.  Each item in this array is another LocationDefinition object.
        /// </summary>
        /// <value>An array of sub-locations that are under this location. If not given, or empty, then there are no sub-locations.  Each item in this array is another LocationDefinition object.</value>
        public List<LocationDefinition>? SubLocations { get; set; }

        /// <summary>
        /// The position of the node in the graph.
        /// </summary>
        /// <remarks>
        /// This property is indended to be used by the visualisation to allow
        /// the position to be shown.
        /// </remarks>
        [JsonPropertyName("_position")]
        public Coordinate Position { get; set; }

        /// <summary>
        /// The name of the node.
        /// </summary>
        /// <remarks>
        /// This property is indended to be used by the visualisation to allow
        /// the position to be shown.
        /// </remarks>
        [JsonPropertyName("_name")]
        public string Name { get; set; }

        public LocationDefinition(string coord, Coordinate position, string name)
        {
            this.Coord = coord;
            this.Position = position;
            this.Name = name;
        }

        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class LocationDefinition {\n");
            sb.Append("  Coord: ").Append(this.Coord).Append("\n");
            sb.Append("  SubLocations: ").Append(this.SubLocations).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }
}
