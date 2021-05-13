using System;
using System.Text;
using System.Collections.Generic;

namespace Models.Parameters
{

    /// <summary>
    /// The parameters for the &#x60;investInHealthServices&#x60; action.  Action: Increase investment in health services. Improves health services response to contagion.
    /// </summary>
    public class InvestInHealthServices
    {
        public const string ActionName = "investInHealthServices";

        /// <summary>
        /// Amount invested in total. Higher amount will result in deminising returns.
        /// </summary>
        /// <value>Amount invested in total. Higher amount will result in deminising returns.</value>
        public int AmountInvested { get; set; }

        public InvestInHealthServices(int amountInvested)
        {
            this.AmountInvested = amountInvested;
        }

        public static implicit operator InvestInHealthServices(ParamsContainer value)
        {
            if (value.AmountInvested == null)
            {
                throw new NullReferenceException(nameof(value.AmountInvested));
            }

            return new InvestInHealthServices(value.AmountInvested.Value);
        }

        public static implicit operator ParamsContainer(InvestInHealthServices value) =>
            new(ActionName) { AmountInvested = value.AmountInvested };

        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class InvestInHealthServicesParameters {\n");
            sb.Append("  AmountInvested: ").Append(this.AmountInvested).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }
}
