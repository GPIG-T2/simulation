using System;
using System.Text;
using System.Collections.Generic;

namespace Models.Parameters
{

    /// <summary>
    /// The parameters for the &#x60;closeBorders&#x60; action.  Action: Stop all movement between one location and all others.
    /// </summary>
    public class CloseBorders
    {
        public const string ActionName = "closeBorders";

        /// <summary>
        /// Gets or Sets Location
        /// </summary>
        public List<string> Location { get; set; }

        public CloseBorders(List<string> location)
        {
            this.Location = location;
        }

        public static implicit operator CloseBorders(ParamsContainer value)
        {
            if (value.Location == null)
            {
                throw new NullReferenceException(nameof(value.Location));
            }

            return new CloseBorders(value.Location);
        }

        public static implicit operator ParamsContainer(CloseBorders value) =>
            new(ActionName) { Location = value.Location };

        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class CloseBordersParameters {\n");
            sb.Append("  Location: ").Append(this.Location).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }
}
