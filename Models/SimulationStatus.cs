using System;
using System.Text;
using System.Collections.Generic;

namespace Models
{
    /// <summary>
    /// The object containing the status of the simulation.
    /// </summary>
    public class SimulationStatus
    {
        /// <summary>
        /// Whether it is currently the turn of the WHO model.
        /// </summary>
        /// <value>Whether it is currently the turn of the WHO model.</value>
        public bool IsWhoTurn { get; set; }

        /// <summary>
        /// The amount of full turns (i.e. virus->who) that have completed. For the first full turn, this number is set to zero, and it should be incremented by 1 upon WHO turn end.
        /// </summary>
        /// <value>The amount of full turns (i.e. virus->who) that have completed. For the first full turn, this number is set to zero, and it should be incremented by 1 upon WHO turn end.</value>
        public int TurnCount { get; set; }

        /// <summary>
        /// The total budget the WHO has for the current turn. This can only be considered valid when `isWhoTurn` is `true`.
        /// </summary>
        /// <value>The total budget the WHO has for the current turn. This can only be considered valid when `isWhoTurn` is `true`.</value>
        public int Budget { get; set; }

        public SimulationStatus(bool isWhoTurn, int turnCount, int budget)
        {
            this.IsWhoTurn = isWhoTurn;
            this.TurnCount = turnCount;
            this.Budget = budget;
        }

        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class SimulationStatus {\n");
            sb.Append("  IsWhoTurn: ").Append(IsWhoTurn).Append("\n");
            sb.Append("  TurnCount: ").Append(TurnCount).Append("\n");
            sb.Append("  Budget: ").Append(Budget).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }
}
