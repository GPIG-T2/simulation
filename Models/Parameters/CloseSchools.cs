using System;
using System.Text;
using System.Collections.Generic;

namespace Models.Parameters
{

    /// <summary>
    /// The parameters for the &#x60;closeSchools&#x60; action.  Action: Close all schools in the given location and prevent school-age people from mixing at school.
    /// </summary>
    public class CloseSchools
    {
        public const string ActionName = "closeSchools";

        /// <summary>
        /// Gets or Sets Location
        /// </summary>
        public List<string> Location { get; set; }

        public CloseSchools(List<string> location)
        {
            this.Location = location;
        }

        public static implicit operator CloseSchools(ParamsContainer value)
        {
            if (value.Location == null)
            {
                throw new NullReferenceException(nameof(value.Location));
            }

            return new CloseSchools(value.Location);
        }

        public static implicit operator ParamsContainer(CloseSchools value) =>
            new(ActionName) { Location = value.Location };

        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class CloseSchoolsParameters {\n");
            sb.Append("  Location: ").Append(this.Location).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }
}
