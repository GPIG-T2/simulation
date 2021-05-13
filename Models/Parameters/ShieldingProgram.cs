using System;
using System.Text;
using System.Collections.Generic;

namespace Models.Parameters
{

    /// <summary>
    /// The parameters for the &#x60;shieldingProgram&#x60; action.  Action: Instruct people to isolate to reduce chance of catching contagion.
    /// </summary>
    public class ShieldingProgram
    {
        public const string ActionName = "shieldingProgram";

        /// <summary>
        /// Gets or Sets Location
        /// </summary>
        public List<string> Location { get; set; }

        /// <summary>
        /// If true, clinically vulnerable people will be required to shield. If not given, defaults to false.
        /// </summary>
        /// <value>If true, clinically vulnerable people will be required to shield. If not given, defaults to false.</value>
        public bool? VulnerablePeople { get; set; }

        /// <summary>
        /// If given, people equal or over the threshold will be required to shield. If not given, or set to `null`, then age is ignored for shielding.
        /// </summary>
        /// <value>If given, people equal or over the threshold will be required to shield. If not given, or set to `null`, then age is ignored for shielding.</value>
        public int? AgeThreshold { get; set; }

        public ShieldingProgram(List<string> location)
        {
            this.Location = location;
        }

        public static implicit operator ShieldingProgram(ParamsContainer value)
        {
            if (value.Location == null)
            {
                throw new NullReferenceException(nameof(value.Location));
            }

            return new ShieldingProgram(value.Location)
            {
                VulnerablePeople = value.VulnerablePeople,
                AgeThreshold = value.AgeThreshold
            };
        }

        public static implicit operator ParamsContainer(ShieldingProgram value) =>
            new(ActionName)
            {
                Location = value.Location,
                VulnerablePeople = value.VulnerablePeople,
                AgeThreshold = value.AgeThreshold,
            };

        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class ShieldingProgramParameters {\n");
            sb.Append("  Location: ").Append(this.Location).Append("\n");
            sb.Append("  VulnerablePeople: ").Append(this.VulnerablePeople).Append("\n");
            sb.Append("  AgeThreshold: ").Append(this.AgeThreshold).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }
}
