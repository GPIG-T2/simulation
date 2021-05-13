using System;
using System.Text;
using System.Collections.Generic;

namespace Models.Parameters
{

    /// <summary>
    /// The parameters for the &#x60;maskMandate&#x60; action.  Action: Require people to wear masks while in public to reduce the spread of the contagion.
    /// </summary>
    public class MaskMandate
    {
        public const string ActionName = "maskMandate";

        /// <summary>
        /// Gets or Sets Location
        /// </summary>
        public List<string> Location { get; set; }

        /// <summary>
        /// The level at which to provision masks for public use. 0 is no provisioning at all, 1 is low-quality mask provisioning, and 2 is high-quality mask provisioning.
        /// </summary>
        /// <value>The level at which to provision masks for public use. 0 is no provisioning at all, 1 is low-quality mask provisioning, and 2 is high-quality mask provisioning.</value>
        public int MaskProvisionLevel { get; set; }

        public MaskMandate(List<string> location, int maskProvisionLevel)
        {
            this.Location = location;
            this.MaskProvisionLevel = maskProvisionLevel;
        }

        public static implicit operator MaskMandate(ParamsContainer value)
        {
            if (value.Location == null)
            {
                throw new NullReferenceException(nameof(value.Location));
            }

            if (value.MaskProvisionLevel == null)
            {
                throw new NullReferenceException(nameof(value.MaskProvisionLevel));
            }

            return new MaskMandate(value.Location, value.MaskProvisionLevel.Value);
        }

        public static implicit operator ParamsContainer(MaskMandate value) =>
            new(ActionName) { Location = value.Location, MaskProvisionLevel = value.MaskProvisionLevel };

        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class MaskMandateParameters {\n");
            sb.Append("  Location: ").Append(this.Location).Append("\n");
            sb.Append("  MaskProvisionLevel: ").Append(this.MaskProvisionLevel).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }
}
