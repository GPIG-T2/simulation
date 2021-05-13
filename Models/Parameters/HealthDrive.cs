using System;
using System.Text;
using System.Collections.Generic;

namespace Models.Parameters
{

    /// <summary>
    /// The parameters for the &#x60;healthDrive&#x60; action.  Action: Attempt to improve the health of people via things like exercise and healthy eating to try and improve the base well-being of the population.
    /// </summary>
    public class HealthDrive
    {
        public const string ActionName = "healthDrive";

        /// <summary>
        /// Gets or Sets Location
        /// </summary>
        public List<string> Location { get; set; }

        public HealthDrive(List<string> location)
        {
            this.Location = location;
        }

        public static implicit operator HealthDrive(ParamsContainer value)
        {
            if (value.Location == null)
            {
                throw new NullReferenceException(nameof(value.Location));
            }

            return new HealthDrive(value.Location);
        }

        public static implicit operator ParamsContainer(HealthDrive value) =>
            new(ActionName) { Location = value.Location };

        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class HealthDriveParameters {\n");
            sb.Append("  Location: ").Append(this.Location).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }
}
