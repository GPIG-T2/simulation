using System;

namespace Virus.Serialization
{
    public class EdgeData
    {
        public string Name { get; set; }
        public int Left { get; set; }
        public int Right { get; set; }
        public long Population { get; set; }
        public double Interactivity { get; set; }
        public int Distance { get; set; }

        public EdgeData(string name,
            int left,
            int right,
            long population,
            double interactivity,
            int distance)
        {
            this.Name = name;
            this.Left = left;
            this.Right = right;
            this.Population = population;
            this.Interactivity = interactivity;
            this.Distance = distance;
        }
    }
}
