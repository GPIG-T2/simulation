using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    /// <summary>
    /// Provides the test results collected by the test-and-isolation action for a location.
    /// </summary>
    public class TestResults
    {
        /// <summary>
        /// The total number of tests performed.
        /// </summary>
        /// <value>The total number of tests performed.</value>
        public long Total { get; set; }

        /// <summary>
        /// The number of positive test results.
        /// </summary>
        /// <value>The number of positive test results.</value>
        public long Positive { get; set; }

        /// <summary>
        /// The location that was asked for.
        /// </summary>
        public List<string> Location { get; set; }

        public TestResults(long total, long positive, List<string> location)
        {
            this.Total = total;
            this.Positive = positive;
            this.Location = location;
        }

        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class TestResults {\n");
            sb.Append("  Total: ").Append(Total).Append("\n");
            sb.Append("  Positive: ").Append(Positive).Append("\n");
            sb.Append("  Location: ").Append(Location).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }
}
