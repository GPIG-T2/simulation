using System;
using System.Text;
using System.Collections.Generic;

namespace Models.Parameters
{

    /// <summary>
    /// The parameters for the &#x60;socialDistancingMadate&#x60; action.  Action: Introduce social distancing rules to reduce the spread caused by proximity to others in public places. Subject to disobedience.
    /// </summary>
    public class SocialDistancingMandate
    {
        public const string ActionName = "socialDistancingMadate";

        /// <summary>
        /// Gets or Sets Location
        /// </summary>
        public List<string> Location { get; set; }

        /// <summary>
        /// The distance in terms of the lowest-level grid size.
        /// </summary>
        /// <value>The distance in terms of the lowest-level grid size.</value>
        public int Distance { get; set; }

        public SocialDistancingMandate(List<string> location, int distance)
        {
            this.Location = location;
            this.Distance = distance;
        }

        public static implicit operator SocialDistancingMandate(ParamsContainer value)
        {
            if (value.Location == null)
            {
                throw new NullReferenceException(nameof(value.Location));
            }

            if (value.Distance == null)
            {
                throw new NullReferenceException(nameof(value.Distance));
            }

            return new SocialDistancingMandate(value.Location, value.Distance.Value);
        }

        public static implicit operator ParamsContainer(SocialDistancingMandate value) =>
            new(ActionName) { Location = value.Location };

        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class SocialDistancingMandateParameters {\n");
            sb.Append("  Location: ").Append(this.Location).Append("\n");
            sb.Append("  Distance: ").Append(this.Distance).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }
}
