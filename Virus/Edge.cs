using System;

namespace Virus
{
    /// <summary>
    /// Representation of the links between nodes - transport between different
    /// geographic locations & infection across nodes.
    /// </summary>
    public class Edge
    {
        // Effect constants - dictate how big effect certain actions should have - should probably be in config file
        public const double PopulationFloor = 0.0;

        public string Name { get; }
        public Node Left { get; }
        public Node Right { get; }

        private readonly Random _random = new();
        private readonly double _baseInteractivity;

        private readonly long _basePopulation;
        private long _totalPopulation;
        private double _currentInteractivity;

        public int Distance { get; } // the physical distance the edge traverses - measured in km

        public Edge(string name, Node left, Node right, long population, double interactivity, int distance)
        {
            this.Name = name;
            this.Left = left;
            this.Right = right;
            this._basePopulation = population;
            this._totalPopulation = population;
            this._baseInteractivity = interactivity;
            this._currentInteractivity = interactivity;
            this.Distance = distance;
        }

        /// <summary>
        /// Looks at the number of symptomatically infectious populations in nodes
        /// to determine number of infectious people on the edge.
        /// </summary>
        /// <param name="virus"></param>
        /// <returns>
        /// The infections for each node connected to this edge.
        /// </returns>
        public (long leftInfections, long rightInfections) Update(Virus virus)
        {
            //if the population along a node is 0, returns 0
            if (this._totalPopulation == 0) { return (0, 0); }

            // determining how many people from each node in the population
            long n1Pop = (long)(this._totalPopulation * ((double)this.Left.TotalPopulation / (this.Left.TotalPopulation + this.Right.TotalPopulation)));

            long n2Pop = this._totalPopulation - n1Pop;

            //ensures that the edge is two way
            n1Pop = Math.Max(1, n1Pop);
            n2Pop = Math.Max(1, n2Pop);

            // determining how many infectious people come from each node - the population from the node * the proportion of people in the node who are infectious
            double n1Infec = (n1Pop
                * ((double)(this.Left.Totals.AsymptomaticInfectedInfectious
                    + this.Left.Totals.Symptomatic
                    + this.Left.Totals.SeriousInfection)
                / this.Left.TotalPopulation));

            long n1Inf = (long)Math.Floor(n1Infec);
            
            double n2Infec = (n2Pop
                * ((double)(this.Right.Totals.AsymptomaticInfectedInfectious
                    + this.Right.Totals.Symptomatic
                    + this.Right.Totals.SeriousInfection)
                / this.Right.TotalPopulation));
            long n2Inf = (long)Math.Floor(n2Infec);

            n1Infec -= n1Inf;
            n2Infec -= n2Inf;

            var chance = this._random.NextDouble();
            if (n1Infec > chance)
            {
                n1Inf++;
            }
            chance = this._random.NextDouble();
            if (n2Infec > chance)
            {
                n2Inf++;
            }


            // determines how many recovered come from each node - same as above
            // TODO: Replace with proper statsitical measure on how many recovered people there would be from a subset of the population
            long n1Rec = (long)Math.Floor(n1Pop * ((double)this.Left.Totals.RecoveredImmune / this.Left.TotalPopulation));
            long n2Rec = (long)Math.Floor(n2Pop * ((double)this.Right.Totals.RecoveredImmune / this.Right.TotalPopulation));

            // the rest of the population is uninfected
            long n1UnInf = Math.Max(n1Pop - n1Inf - n1Rec,0);
            long n2UnInf = Math.Max(n2Pop - n2Inf - n2Rec,0);


            // finds the totals along the edge, then infects a new amount of people following same method as node
            long totalInfectious = n1Inf + n2Inf;
            long totalUninfected = n1UnInf + n2UnInf;

            // infectiousness (infectious interactions) decided by the number of infectious people, the portion of the population which can be infected, and the interactivity of the node
            double infectiousness = totalInfectious * ((double)totalUninfected / (double)this._totalPopulation) * this._currentInteractivity;
            infectiousness *= virus.Infectivity.EighteenToTwentyNine; //multiplies interactions * infectivity to get total number of people infected

            // the infectiousness is the number of people infected + the chance of 1 more
            long infected = (long)Math.Floor(infectiousness);
            infectiousness -= infected;
            chance = this._random.NextDouble();
            if (infectiousness > chance)
            {
                infected += 1;
            }

            // splits the infected people into infected going into node 1 and node 2
            long n1Out = (long)(infected * ((double)n1Pop / this._totalPopulation));
            long n2Out = infected - n1Out;

            //ensures that there are infections going both ways when there are more than 2 infections
            if ((n2Out > 1) & (n1Out == 0)) { n1Out = 1; n2Out -= 1; }
            if ((n1Out > 1) & (n2Out == 0)) { n2Out = 1; n1Out -= 1; }


            return (n1Out, n2Out);
        }

        ///<summary>
        /// Closes edge - Reduces population on a node to 0 - restricts all movement
        /// </summary>
        public void CloseEdge()
        {
            this._totalPopulation = (long)Math.Round(this._basePopulation * PopulationFloor);
        }

        ///<summary>
        /// Reopens the edge
        /// </summary>
        public void OpenEdge()
        {
            this._totalPopulation = this._basePopulation;
        }

        //TODO: Add more variable changes - maybe based off compliance in left/right nodes?

        public static explicit operator Models.Edge(Edge edge)
            => new(edge.Left.Location, edge.Right.Location);
    }
}
