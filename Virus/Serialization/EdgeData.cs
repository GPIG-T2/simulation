using System;

namespace Virus.Serialization
{
    public class EdgeData
    {
        public int Left { get; set; }
        public int Right { get; set; }
        public int Population { get; set; }
        public double Interactivity { get; set; }
        public int Distance { get; set; }

        public EdgeData(int left,
            int right,
            int population,
            double interactivity,
            int distance)
        {
            this.Left = left;
            this.Right = right;
            this.Population = population;
            this.Interactivity = interactivity;
            this.Distance = distance;
        }
    }
}
