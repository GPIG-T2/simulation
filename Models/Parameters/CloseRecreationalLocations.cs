using System;
using System.Text;
using System.Collections.Generic;

namespace Models.Parameters
{

    /// <summary>
    /// The parameters for the &#x60;closeRecreationalLocations&#x60; action.  Action: Close all recreational facilities in the given location and prevent mixing of people and households in these locations.
    /// </summary>
    public class CloseRecreationalLocations
    {
        public const string ActionName = "closeRecreationalLocations";

        /// <summary>
        /// Gets or Sets Location
        /// </summary>
        public List<string> Location { get; set; }

        public CloseRecreationalLocations(List<string> location)
        {
            this.Location = location;
        }

        public static implicit operator CloseRecreationalLocations(ParamsContainer value)
        {
            if (value.Location == null)
            {
                throw new NullReferenceException(nameof(value.Location));
            }

            return new CloseRecreationalLocations(value.Location);
        }

        public static implicit operator ParamsContainer(CloseRecreationalLocations value) =>
            new(ActionName) { Location = value.Location };

        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class CloseRecreationalLocationsParameters {\n");
            sb.Append("  Location: ").Append(this.Location).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }
}
