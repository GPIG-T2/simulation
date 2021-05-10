using System;
using System.Text;
using System.Collections.Generic;

namespace Models.Parameters
{

    /// <summary>
    /// The parameters for the &#x60;testAndIsolation&#x60; action.  Action: Starts a test-and-isolation program in an area using the given budget that will test symptomatic people or any person who requests, and any who return positive are automatically recommended to self-isolate.
    /// </summary>
    public class TestAndIsolation
    {
        public const string ActionName = "testAndIsolation";

        /// <summary>
        /// The quality level of the tests used. 0 is low-quality, 1 is high-quality.
        /// </summary>
        /// <value>The quality level of the tests used. 0 is low-quality, 1 is high-quality.</value>
        public int TestQuality { get; set; }

        /// <summary>
        /// The period of time a person is asked to self-isolate in day after receiving a positive result.
        /// </summary>
        /// <value>The period of time a person is asked to self-isolate in day after receiving a positive result.</value>
        public int QuarantinePeriod { get; set; }

        /// <summary>
        /// The quantity of tests requested.
        /// </summary>
        /// <value>The quantity of tests requested.</value>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or Sets Location
        /// </summary>
        public List<string> Location { get; set; }

        /// <summary>
        /// Whether to test symptomatic people only.
        /// </summary>
        /// <value>Whether to test symptomatic people only.</value>
        public bool SymptomaticOnly { get; set; }

        public TestAndIsolation(
            int testQuality,
            int quarantinePeriod,
            int quantity,
            List<string> location,
            bool symptomaticOnly)
        {
            this.TestQuality = testQuality;
            this.QuarantinePeriod = quarantinePeriod;
            this.Quantity = quantity;
            this.Location = location;
            this.SymptomaticOnly = symptomaticOnly;
        }

        public static implicit operator TestAndIsolation(ParamsContainer value)
        {
            if (value.TestQuality == null)
            {
                throw new NullReferenceException(nameof(value.TestQuality));
            }

            if (value.QuarantinePeriod == null)
            {
                throw new NullReferenceException(nameof(value.QuarantinePeriod));
            }

            if (value.Quantity == null)
            {
                throw new NullReferenceException(nameof(value.Quantity));
            }

            if (value.Location == null)
            {
                throw new NullReferenceException(nameof(value.Location));
            }

            if (value.SymptomaticOnly == null)
            {
                throw new NullReferenceException(nameof(value.SymptomaticOnly));
            }

            return new TestAndIsolation(
                value.TestQuality.Value,
                value.QuarantinePeriod.Value,
                value.Quantity.Value,
                value.Location,
                value.SymptomaticOnly.Value
            );
        }

        public static implicit operator ParamsContainer(TestAndIsolation value) =>
            new(ActionName)
            {
                TestQuality = value.TestQuality,
                QuarantinePeriod = value.QuarantinePeriod,
                Quantity = value.Quantity,
                Location = value.Location,
                SymptomaticOnly = value.SymptomaticOnly,
            };

        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class TestAndIsolationParameters {\n");
            sb.Append("  TestQuality: ").Append(this.TestQuality).Append("\n");
            sb.Append("  QuarantinePeriod: ").Append(this.QuarantinePeriod).Append("\n");
            sb.Append("  Quantity: ").Append(this.Quantity).Append("\n");
            sb.Append("  Location: ").Append(this.Location).Append("\n");
            sb.Append("  SymptomaticOnly: ").Append(this.SymptomaticOnly).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }
}
