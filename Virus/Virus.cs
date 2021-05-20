using System;

namespace Virus
{
    /// <summary>
    /// The virus representation.
    /// </summary>
    public class Virus
    {
        // TODO: rates for demographics & mutations
        public double Infectivity { get; }
        public double Fatality { get; }
        public double Reinfectivity { get; }
        public double Symptomaticity { get; }

        public Virus(double infectivity, double fatality, double reinfectivity, double symptomaticity)
        {
            this.Infectivity = infectivity;
            this.Fatality = fatality;
            this.Reinfectivity = reinfectivity;
            this.Symptomaticity = symptomaticity;
        }
    }
}
