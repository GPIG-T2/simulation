using System;
using System.Text;
using System.Collections.Generic;

namespace Models
{
    /// <summary>
    /// The object containing the status of the simulation.
    /// </summary>
    public class SimulationStatusUpdate
    {
        /// <summary>
        /// Required property to notify that the client has ended its turn. Has to be set to `false`, other values (and properties) are ignored, and do not perform any changes.
        /// </summary>
        /// <value>Required property to notify that the client has ended its turn. Has to be set to `false`, other values (and properties) are ignored, and do not perform any changes.</value>
        public bool IsWhoTurn { get; set; } = false;

        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class SimulationStatusUpdate {\n");
            sb.Append("  IsWhoTurn: ").Append(IsWhoTurn).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }
}
