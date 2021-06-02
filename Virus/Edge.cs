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
        public const double PopulationFloor = 0.1;

        public Node Left { get; }
        public Node Right { get; }

        private readonly Random _random = new();
        private readonly double _baseInteractivity;

        private readonly int _basePopulation;
        private int _totalPopulation;
        private double _currentInteractivity;

        public int Distance { get; } // the physical distance the edge traverses

        public Edge(Node left, Node right, int population, double interactivity, int distance)
        {
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
        public (int leftInfections, int rightInfections) Update(Virus virus)
        {
            // determining how many people from each node in the population
            int n1Pop = (int)(this._totalPopulation * ((double)this.Left.TotalPopulation / (this.Left.TotalPopulation + this.Right.TotalPopulation)));

            int n2Pop = this._totalPopulation - n1Pop;

            // determining how many infectious people come from each node - the population from the node * the proportion of people in the node who are infectious
            // TODO: Replace with a proper statistical measure on how many infectious people there would be from a subset of the population
            // TODO: Maybe not indclude serious?
            int n1Inf = (int)Math.Floor(n1Pop
                * ((double)(this.Left.Totals.AsymptomaticInfectedInfectious
                    + this.Left.Totals.Symptomatic
                    + this.Left.Totals.SeriousInfection)
                / this.Left.TotalPopulation));
            int n2Inf = (int)Math.Floor(n2Pop
                * ((double)(this.Right.Totals.AsymptomaticInfectedInfectious
                    + this.Right.Totals.Symptomatic
                    + this.Right.Totals.SeriousInfection)
                / this.Right.TotalPopulation));

            // determines how many recovered come from each node - same as above
            // TODO: Replace with proper statsitical measure on how many recovered people there would be from a subset of the population
            int n1Rec = (int)Math.Floor(n1Pop * ((double)this.Left.Totals.RecoveredImmune / this.Left.TotalPopulation));
            int n2Rec = (int)Math.Floor(n2Pop * ((double)this.Right.Totals.RecoveredImmune / this.Right.TotalPopulation));

            // the rest of the population is uninfected
            int n1UnInf = n1Pop - n1Inf - n1Rec;
            int n2UnInf = n2Pop - n2Inf - n2Rec;


            // finds the totals along the edge, then infects a new amount of people following same method as node
            int totalInfectious = n1Inf + n2Inf;
            int totalUninfected = n1UnInf + n2UnInf;

            // infectiousness (infectious interactions) decided by the number of infectious people, the portion of the population which can be infected, and the interactivity of the node
            double infectiousness = totalInfectious * ((double)totalUninfected / (double)this._totalPopulation) * this._currentInteractivity;
            infectiousness *= virus.Infectivity; //multiplies interactions * infectivity to get total number of people infected

            // the infectiousness is the number of people infected + the chance of 1 more
            int infected = (int)Math.Floor(infectiousness);
            infectiousness -= infected;
            var chance = this._random.NextDouble();
            if (infectiousness > chance)
            {
                infected += 1;
            }

            // splits the infected people into infected going into node 1 and node 2
            int n1Out = (int)(infected * ((double)n1Pop / this._totalPopulation));
            int n2Out = infected - n1Out;

            return (n1Out, n2Out);
        }

        ///<summary>
        /// Closes edge - Reduces population on a node to 0 - restricts all movement
        /// </summary>
        public void CloseEdge()
        {
            this._totalPopulation = (int)Math.Round(this._basePopulation * PopulationFloor);
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
