using System;
using System.Text;
using System.Collections.Generic;

namespace Models.Parameters
{

    /// <summary>
    /// The parameters for the &#x60;stayAtHome&#x60; action.  Action: Begin a stay-at-home order for the given location that prevents people from leaving their residence except for necessary reasons (exercise/shopping/etc).
    /// </summary>
    public class StayAtHome
    {
        public const string ActionName = "stayAtHome";

        /// <summary>
        /// Gets or Sets Location
        /// </summary>
        public List<string> Location { get; set; }

        public StayAtHome(List<string> location)
        {
            this.Location = location;
        }

        public static implicit operator StayAtHome(ParamsContainer value)
        {
            if (value.Location == null)
            {
                throw new NullReferenceException(nameof(value.Location));
            }

            return new StayAtHome(value.Location);
        }

        public static implicit operator ParamsContainer(StayAtHome value) =>
            new(ActionName) { Location = value.Location };

        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class StayAtHomeParameters {\n");
            sb.Append("  Location: ").Append(this.Location).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }
}
