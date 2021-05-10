using System;
using System.Text;
using System.Collections.Generic;

namespace Models.Parameters
{

    /// <summary>
    /// The parameters to the &#x60;infoPressRelease&#x60; action.  Action: Press-releases to improve morale/disseminate information, which could have a negative effect if it goes wrong and are produced every set unit of time.  - Increase awareness - Probably increase public opinion - Implementation can change the amount this increases (minimum of 0 increase) - Implementation can alter how much the improvement is based on number of previous press releases
    /// </summary>
    public class InformationPressRelease
    {
        public const string ActionName = "infoPressRelease";

        /// <summary>
        /// Amount invested in total.
        /// </summary>
        /// <value>Amount invested in total.</value>
        public int AmountInvested { get; set; }

        /// <summary>
        /// Gets or Sets Location
        /// </summary>
        public List<string> Location { get; set; }

        public InformationPressRelease(int amountInvested, List<string> location)
        {
            this.AmountInvested = amountInvested;
            this.Location = location;
        }

        public static implicit operator InformationPressRelease(ParamsContainer value)
        {
            if (value.AmountInvested == null)
            {
                throw new NullReferenceException(nameof(value.AmountInvested));
            }

            if (value.Location == null)
            {
                throw new NullReferenceException(nameof(value.Location));
            }

            return new InformationPressRelease(value.AmountInvested.Value, value.Location);
        }

        public static implicit operator ParamsContainer(InformationPressRelease value) =>
            new(ActionName) { AmountInvested = value.AmountInvested, Location = value.Location };

        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class InformationPressReleaseParameters {\n");
            sb.Append("  AmountInvested: ").Append(this.AmountInvested).Append("\n");
            sb.Append("  Location: ").Append(this.Location).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }
}
