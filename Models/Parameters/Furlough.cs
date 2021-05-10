using System;
using System.Text;
using System.Collections.Generic;

namespace Models.Parameters
{

    /// <summary>
    /// The parameters to the &#x60;furlough&#x60; action.  Action: Provide monetary support to those who are not able to work due to the contagion, propping up businesses that are not viable during the pandemic.  - Reduces the number of people going to work - Higher amount results in higher budget cost - If used in combination with action 2 or 4, that action&#x27;s GDP cost is reduced 
    /// </summary>
    public class Furlough
    {
        public const string ActionName = "furlough";

        /// <summary>
        /// Amount invested per working person.
        /// </summary>
        /// <value>Amount invested per working person.</value>
        public int AmountInvested { get; set; }

        /// <summary>
        /// Gets or Sets Location
        /// </summary>
        public List<string> Location { get; set; }

        public Furlough(int amountInvested, List<string> location)
        {
            this.AmountInvested = amountInvested;
            this.Location = location;
        }

        public static implicit operator Furlough(ParamsContainer value)
        {
            if (value.AmountInvested == null)
            {
                throw new NullReferenceException(nameof(value.AmountInvested));
            }

            if (value.Location == null)
            {
                throw new NullReferenceException(nameof(value.Location));
            }

            return new Furlough(value.AmountInvested.Value, value.Location);
        }

        public static implicit operator ParamsContainer(Furlough value) =>
            new(ActionName) { AmountInvested = value.AmountInvested, Location = value.Location };

        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class FurloughParameters {\n");
            sb.Append("  AmountInvested: ").Append(this.AmountInvested).Append("\n");
            sb.Append("  Location: ").Append(this.Location).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }
}
