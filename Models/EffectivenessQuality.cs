using System;
using System.Text;

namespace Models
{
    public class EffectivenessQuality
    {
        /// <summary>
        /// Effectiveness of the low-quality item.
        /// </summary>
        /// <value>Effectiveness of the low-quality item.</value>
        public double LowQuality { get; set; }

        /// <summary>
        /// Effectiveness of the high-quality item.
        /// </summary>
        /// <value>Effectiveness of the high-quality item.</value>
        public double HighQuality { get; set; }

        public EffectivenessQuality(double lowQuality, double highQuality)
        {
            this.LowQuality = lowQuality;
            this.HighQuality = highQuality;
        }

        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class EffectivenessQuality {\n");
            sb.Append("  LowQuality: ").Append(this.LowQuality).Append("\n");
            sb.Append("  HighQuality: ").Append(this.HighQuality).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }
}
