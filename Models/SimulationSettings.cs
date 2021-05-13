using System;
using System.Text;
using System.Collections.Generic;

namespace Models
{
    /// <summary>
    /// The object containing the settings for the simulation.
    /// </summary>
    public class SimulationSettings
    {
        /// <summary>
        /// Gets or Sets TurnLength
        /// </summary>
        public SimulationTurnLength TurnLength { get; set; }

        /// <summary>
        /// An array of all top-level locations that are available on the map.
        /// </summary>
        /// <value>An array of all top-level locations that are available on the map.</value>
        public List<LocationDefinition> Locations { get; set; }

        public SimulationSettings(SimulationTurnLength turnLength, List<LocationDefinition> locations)
        {
            this.TurnLength = turnLength;
            this.Locations = locations;
        }

        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class SimulationSettings {\n");
            sb.Append("  TurnLength: ").Append(this.TurnLength).Append("\n");
            sb.Append("  Locations: ").Append(this.Locations).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// An object stating how much in-simulation time will pass for each full turn.
        /// </summary>
        public class SimulationTurnLength
        {
            /// <summary>
            /// The unit of time to measure in.
            /// </summary>
            /// <value>The unit of time to measure in.</value>
            public string Unit { get; set; }

            /// <summary>
            /// The amount of unit of time each turn consists of.
            /// </summary>
            /// <value>The amount of unit of time each turn consists of.</value>
            public int Count { get; set; }

            public SimulationTurnLength(string unit, int count)
            {
                this.Unit = unit;
                this.Count = count;
            }

            /// <summary>
            /// Get the string presentation of the object
            /// </summary>
            /// <returns>String presentation of the object</returns>
            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.Append("class SimulationSettingsTurnLength {\n");
                sb.Append("  Unit: ").Append(this.Unit).Append("\n");
                sb.Append("  Count: ").Append(this.Count).Append("\n");
                sb.Append("}\n");
                return sb.ToString();
            }
        }
    }
}
