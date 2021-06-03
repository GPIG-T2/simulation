using System;

namespace Virus
{
    /// <summary>
    /// The virus representation.
    /// </summary>
    public class Virus
    {
        // TODO: rates for demographics & mutations
        public Demographics Infectivity { get; }
        public Demographics Fatality { get; }
        public Demographics Reinfectivity { get; }
        public Demographics Symptomaticity { get; }
        public Demographics SeriousRate { get; }

        public Virus(Demographics infectivity, Demographics fatality, Demographics reinfectivity, Demographics symptomaticity, Demographics serious)
        {
            this.Infectivity = infectivity;
            this.Fatality = fatality;
            this.Reinfectivity = reinfectivity;
            this.Symptomaticity = symptomaticity;
            this.SeriousRate = serious;
        }
    }
}
