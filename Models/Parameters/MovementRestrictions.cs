using System;
using System.Text;
using System.Collections.Generic;

namespace Models.Parameters
{

    /// <summary>
    /// The parameters for the &#x60;movementRestrictions&#x60; action.  Action: Stop people from travelling a certain distance away from their household.
    /// </summary>
    public class MovementRestrictions
    {
        public const string ActionName = "movementRestrictions";

        /// <summary>
        /// Gets or Sets Location
        /// </summary>
        public List<string> Location { get; set; }

        /// <summary>
        /// The distance in terms of the lowest-level grid size.
        /// </summary>
        /// <value>The distance in terms of the lowest-level grid size.</value>
        public int Distance { get; set; }

        public MovementRestrictions(List<string> location, int distance)
        {
            this.Location = location;
            this.Distance = distance;
        }

        public static implicit operator MovementRestrictions(ParamsContainer value)
        {
            if (value.Location == null)
            {
                throw new NullReferenceException(nameof(value.Location));
            }

            if (value.Distance == null)
            {
                throw new NullReferenceException(nameof(value.Distance));
            }

            return new MovementRestrictions(value.Location, value.Distance.Value);
        }

        public static implicit operator ParamsContainer(MovementRestrictions value) =>
            new(ActionName) { Location = value.Location, Distance = value.Distance };

        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class MovementRestrictionsParameters {\n");
            sb.Append("  Location: ").Append(this.Location).Append("\n");
            sb.Append("  Distance: ").Append(this.Distance).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }
}
