using System;
using System.Text;
using System.Collections.Generic;

namespace Models.Parameters
{

    /// <summary>
    /// The parameters for the &#x60;curfew&#x60; action.  Action: Introduce a curfew to require the public to not go into public places past a certain time, reducing socialisation and use of recreational businesses.
    /// </summary>
    public class Curfew
    {
        public const string ActionName = "curfew";

        /// <summary>
        /// Gets or Sets Location
        /// </summary>
        public List<string> Location { get; set; }

        public Curfew(List<string> location)
        {
            this.Location = location;
        }

        public static implicit operator Curfew(ParamsContainer value)
        {
            if (value.Location == null)
            {
                throw new NullReferenceException(nameof(value.Location));
            }

            return new Curfew(value.Location);
        }

        public static implicit operator ParamsContainer(Curfew value) =>
            new(ActionName) { Location = value.Location };

        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class CurfewParameters {\n");
            sb.Append("  Location: ").Append(this.Location).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }
}
