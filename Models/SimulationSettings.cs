using System;
using System.Text;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Models
{
    /// <summary>
    /// The object containing the settings for the simulation.
    /// </summary>
    public class SimulationSettings
    {
        /// <summary>
        /// An object stating how much in-simulation time will pass for each full turn.
        /// </summary>
        /// <value>An object stating how much in-simulation time will pass for each full turn.</value>
        public SimulationTurnLength TurnLength { get; set; }

        /// <summary>
        /// An array of all top-level locations that are available on the map.
        /// </summary>
        /// <value>An array of all top-level locations that are available on the map.</value>
        public List<LocationDefinition> Locations { get; set; }

        /// <summary>
        /// Provides the effectivenessess of various tools the WHO can use.
        /// </summary>
        /// <value>Provides the effectivenessess of various tools the WHO can use.</value>
        public SimulationEffectivenesses Effectivenesses { get; set; }

        /// <summary>
        /// An edge in the graph.
        /// </summary>
        /// <remarks>
        /// This property is indended to be used by the visualisation to allow
        /// the position to be shown.
        /// </remarks>
        [JsonPropertyName("_edges")]
        public List<Edge> Edges { get; set; }

        public SimulationSettings(
            SimulationTurnLength turnLength,
            List<LocationDefinition> locations,
            SimulationEffectivenesses effectivenesses,
            List<Edge> edges)
        {
            this.TurnLength = turnLength;
            this.Locations = locations;
            this.Effectivenesses = effectivenesses;
            this.Edges = edges;
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

        /// <summary>
        /// An object stating how much in-simulation time will pass for each full turn.
        /// </summary>
        public class SimulationEffectivenesses
        {
            /// <summary>
            /// Provides the effectiveness of the low- and high-quality items of a given tool.
            /// </summary>
            /// <value>Provides the effectiveness of the low- and high-quality items of a given tool.</value>
            public EffectivenessQuality Masks { get; set; }

            /// <summary>
            /// An array of effectiveness values, one for each vaccine.
            /// </summary>
            /// <value>An array of effectiveness values, one for each vaccine.</value>
            public List<float> Vaccines { get; set; }

            /// <summary>
            /// Provides the effectiveness of the low- and high-quality items of a given tool.
            /// </summary>
            /// <value>Provides the effectiveness of the low- and high-quality items of a given tool.</value>
            public EffectivenessQuality Tests { get; set; }

            public SimulationEffectivenesses(EffectivenessQuality masks, List<float> vaccines, EffectivenessQuality tests)
            {
                this.Masks = masks;
                this.Vaccines = vaccines;
                this.Tests = tests;
            }

            /// <summary>
            /// Get the string presentation of the object
            /// </summary>
            /// <returns>String presentation of the object</returns>
            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.Append("class SimulationSettingsTurnLength {\n");
                sb.Append("  Masks: ").Append(this.Masks).Append("\n");
                sb.Append("  Vaccines: ").Append(this.Vaccines).Append("\n");
                sb.Append("  Tests: ").Append(this.Tests).Append("\n");
                sb.Append("}\n");
                return sb.ToString();
            }
        }
    }
}
