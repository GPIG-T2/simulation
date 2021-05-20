using System;
using System.Collections.Generic;

namespace Virus
{
    /// <summary>
    /// Representation of a single node within the system, i.e a geographic location.
    /// </summary>
    public class Node
    {
        public int Index { get; }
        public List<string> Location { get; }
        public int TotalPopulation { get; set; }
        public Models.InfectionTotals Totals { get; }

        private readonly Random _random = new();
        private readonly int _startPopulation;
        private readonly double _interactivity;
        private readonly int[] _asympHistory;
        private int _historyHead;
        private readonly int[] _sympHistory;
        private readonly int[] _seriousHistory;

        public Node(int index, int population, double interactivity)
        {
            this.Index = index;
            this.Location = new List<string> { $"N{index}" };
            this._startPopulation = population;
            this.TotalPopulation = population;
            this.Totals = new Models.InfectionTotals(this.Location, population, 0, 0, 0, 0, 0, 0);

            this._interactivity = interactivity;
            this._historyHead = 0;
            this._asympHistory = new int[14]; // infections last 14 days
            this._sympHistory = new int[14];
            this._seriousHistory = new int[14];
        }

        /// <summary>
        /// Takes current populations and updates to form new set of populations
        /// based off given virus, interactivity within the node.
        /// </summary>
        /// <param name="virus"></param>
        public void Update(Virus virus)
        {
            // finds the total number of infectious - could be separated for different interactivities
            int totalInfectious = this.Totals.AsymptomaticInfectedInfectious
                + this.Totals.Symptomatic
                + this.Totals.SeriousInfection;

            // infectiousness (infectious interactions) decided by the number of infectious people, the portion of the population which can be infected, and the interactivity of the node
            // TODO: Unsure if uninfected/totalPopulation is appropriate for this utiliziation - may need specific statstical method
            double infectiousness = totalInfectious * ((double)this.Totals.Uninfected / (double)this.TotalPopulation) * this._interactivity;
            infectiousness *= virus.Infectivity; //multiplies interactions * infectivity to get total number of people infected
            // the infectiousness is the number of people infected + the chance of 1 more
            int infected = (int)Math.Floor(infectiousness);

            infectiousness -= infected;
            var chance = this._random.NextDouble();
            if (infectiousness > chance)
            {
                infected += 1;
            }

            // infects the population
            this.Infect(infected);


            // based on the infection history, moves people through levels of infectiousness
            // asympUninf (2 days) -> asympInf (2 days)
            int aUninf2InfHead = (this._historyHead + 14 - 2) % 14;
            int aInf2SympHead = (this._historyHead + 14 - 4) % 14;

            // following 2 days, all asympUninf go to asympInf
            this.Totals.AsymptomaticInfectedNotInfectious -= this._asympHistory[aUninf2InfHead];
            this.Totals.AsymptomaticInfectedInfectious += this._asympHistory[aUninf2InfHead];

            // follwing 4 days (2 days after asympUninf -> asympInf) a portion of the asympInf move to symp depdendent on symptomaticity
            int aInf2Symp = (int)Math.Floor((double)this._asympHistory[aInf2SympHead] * virus.Symptomaticity); //rounds to 0 under 1
            this.Totals.AsymptomaticInfectedInfectious -= aInf2Symp;
            this.Totals.Symptomatic += aInf2Symp;
            // moves histories
            this._sympHistory[aInf2SympHead] += aInf2Symp;
            this._asympHistory[aInf2SympHead] -= aInf2Symp;

            // symp (2 day min) -> serious (2 day min) 
            int symp2SeriousHead = (this._historyHead + 14 - 6) % 14;
            int serious2DeadHead = (this._historyHead + 14 - 8) % 14;

            // a portion (25% - should be variable) of symptomatic go to serious after 2 days
            // TODO - replace 0.5 with proper virus variable
            int symp2Serious = (int)Math.Floor((double)this._sympHistory[symp2SeriousHead] * 0.25);
            this.Totals.Symptomatic -= symp2Serious;
            this.Totals.SeriousInfection += symp2Serious;
            this._sympHistory[symp2SeriousHead] -= symp2Serious;
            this._seriousHistory[symp2SeriousHead] += symp2Serious;

            // a portion of serious cases result in death - depdenent on virus fatality rate
            int serious2Dead = (int)Math.Floor((double)this._seriousHistory[serious2DeadHead] * virus.Fatality);
            this.Totals.SeriousInfection -= serious2Dead;
            this.Totals.Dead += serious2Dead;
            this.TotalPopulation -= serious2Dead; //removes dead people from the current population
            this._seriousHistory[serious2DeadHead] -= serious2Dead; //removed from history for recovery tracking

            // at the end of the 14 day period - everyone infected 14 days ago still alive recovers
            int historyEnd = (_historyHead + 1) % 14;
            int asymp2Recovered = this._asympHistory[historyEnd];
            int symp2Recovered = this._sympHistory[historyEnd];
            int serious2Recovered = this._seriousHistory[historyEnd];
            this.Totals.AsymptomaticInfectedInfectious -= asymp2Recovered;
            this.Totals.Symptomatic -= symp2Recovered;
            this.Totals.SeriousInfection -= serious2Recovered;
            this.Totals.RecoveredImmune += asymp2Recovered + symp2Recovered + serious2Recovered;
            // clears history at the end
            this._asympHistory[historyEnd] = 0;
            this._sympHistory[historyEnd] = 0;
            this._seriousHistory[historyEnd] = 0;

            // moves a proportion of recovered to uninfected based on virus reinfectivity
            int reinfections = (int)Math.Floor(this.Totals.RecoveredImmune * virus.Reinfectivity);
            this.Totals.RecoveredImmune -= reinfections;
            this.Totals.Uninfected += reinfections;

            // Increment head not included in update - has to be done by world
            // This is due to edges also needing to use the head for infections
        }

        /// <summary>
        /// Infects a given number of people by taking them from uninfected to asympUninf
        /// </summary>
        /// <param name="pop"></param>
        public void Infect(int pop)
        {
            if (pop > this.Totals.Uninfected)
            {
                pop = this.Totals.Uninfected;
            }
            this.Totals.Uninfected -= pop;
            this.Totals.AsymptomaticInfectedNotInfectious += pop;
            this._asympHistory[this._historyHead] += pop;
        }

        /// <summary>
        /// Increments the head of the infection history (queue).
        /// </summary>
        public void IncrementHead()
        {
            this._historyHead = (this._historyHead + 1) % 14;
        }
    }
}
