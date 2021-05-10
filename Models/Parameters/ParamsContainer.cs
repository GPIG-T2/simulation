using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Models.Parameters
{
    /// <summary>
    /// This class contains every possible property of the action parameters
    /// in order to have a single serialise-deserialise class.
    ///
    /// Every parameter class, however, has implicit convertions to and from
    /// this class, allowing for more natural use.
    /// </summary>
    public class ParamsContainer
    {
        [JsonIgnore]
        public string ActionName { get; set; }

        public List<string>? Location { get; set; }
        public int? AmountInvested { get; set; }
        public int? AmountLoaned { get; set; }
        public int? MaskProvisionLevel { get; set; }
        public int? Distance { get; set; }
        public bool? VulnerablePeople { get; set; }
        public int? AgeThreshold { get; set; }
        public int? TestQuality { get; set; }
        public int? QuarantinePeriod { get; set; }
        public int? Quantity { get; set; }
        public bool? SymptomaticOnly { get; set; }

        internal ParamsContainer(string actionName)
        {
            this.ActionName = actionName;
        }
    }
}
