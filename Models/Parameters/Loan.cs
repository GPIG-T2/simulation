using System;
using System.Text;
using System.Collections.Generic;

namespace Models.Parameters
{

    /// <summary>
    /// The parameters to the &#x60;loan&#x60; action.  Action: Allow the WHO to take additional Budget for situations in which they run out of money. Negative effects of taking a loan are considered too long term or otherwise out of scope  - Reduction in Score - Increase in Budget for the following turn (Lump sum) 
    /// </summary>
    public class Loan
    {
        public const string ActionName = "loan";

        /// <summary>
        /// Total amount to be loaned to the WHO.
        /// </summary>
        /// <value>Total amount to be loaned to the WHO.</value>
        public int AmountLoaned { get; set; }

        public Loan(int amountLoaded)
        {
            this.AmountLoaned = amountLoaded;
        }

        public static implicit operator Loan(ParamsContainer value)
        {
            if (value.AmountLoaned == null)
            {
                throw new NullReferenceException(nameof(value.AmountLoaned));
            }

            return new Loan(value.AmountLoaned.Value);
        }

        public static implicit operator ParamsContainer(Loan value) =>
            new(ActionName) { AmountLoaned = value.AmountLoaned };

        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class LoanParameters {\n");
            sb.Append("  AmountLoaned: ").Append(this.AmountLoaned).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }
}
