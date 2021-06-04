using System;

namespace Virus.Serialization
{
    public class NodeData
    {
        public int Population { get; set; }
        public Demographics Interactivity { get; set; }
        public string Name { get; set; }
        public Models.Coordinate Position { get; set; }
        public Demographics Demographics { get; set; }
        public double Gdp { get; set; }
        public int TestingCapacity { get; set; }

        public NodeData(int population,
            Demographics interactivity,
            string name,
            Models.Coordinate position,
            Demographics demographics,
            double gdp,
            int testingCapacity)
        {
            this.Population = population;
            this.Interactivity = interactivity;
            this.Name = name;
            this.Position = position;
            this.Demographics = demographics;
            this.Gdp = gdp;
            this.TestingCapacity = testingCapacity;
        }
    }
}
