using System;
using System.Text;
using System.Collections.Generic;

namespace Models.Parameters
{

    /// <summary>
    /// The parameters to the &#x60;investInVaccine&#x60; action.  Action: Work towards producing a vaccine for the contagion. A vaccine is produced once US$80 million has been invested in the production of a vaccine, at this point further investment will lead to subsequent vaccines being produced via the same process. The produced vaccine will be as described in section 8 of the GPIG Cross-Team Competition Rules.
    /// </summary>
    public class InvestInVaccine
    {
        public const string ActionName = "investInVaccine";

        /// <summary>
        /// Amount invested for this turn.
        /// </summary>
        /// <value>Amount invested for this turn.</value>
        public int AmountInvested { get; set; }

        public InvestInVaccine(int amountInvested)
        {
            this.AmountInvested = amountInvested;
        }

        public static implicit operator InvestInVaccine(ParamsContainer value)
        {
            if (value.AmountInvested == null)
            {
                throw new NullReferenceException(nameof(value.AmountInvested));
            }

            return new InvestInVaccine(value.AmountInvested.Value);
        }

        public static implicit operator ParamsContainer(InvestInVaccine value) =>
            new(ActionName) { AmountInvested = value.AmountInvested };

        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class InvestInVaccineParameters {\n");
            sb.Append("  AmountInvested: ").Append(this.AmountInvested).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }
}
