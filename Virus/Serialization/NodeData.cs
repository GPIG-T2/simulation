using System;

namespace Virus.Serialization
{
    public class NodeData
    {
        public long Population { get; set; }
        public Demographics Interactivity { get; set; }
        public string Name { get; set; }
        public Models.Coordinate Position { get; set; }
        public Demographics Demographics { get; set; }
        public long Gdp { get; set; }
        public long TestingCapacity { get; set; }

        public NodeData(long population,
            Demographics interactivity,
            string name,
            Models.Coordinate position,
            Demographics demographics,
            long gdp,
            long testingCapacity)
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
