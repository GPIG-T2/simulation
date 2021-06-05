using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace Virus
{
    /// <summary>
    /// Representation of the world.
    /// </summary>
    /// <remarks>
    /// This is the top-level container providing the world for the simulation to
    /// run in.
    ///
    /// Updates to the nodes and edges are done in parallel, enabling full
    /// parallelisation when processing these updates.
    /// </remarks>
    public class World
    {
        // Effect constants - dictate how big effect certain actions should have - should probably be in config file
        public const double PressReleaseCost = 0.01;
        public const double GoodTestCost = 140;
        public const double BadTestCost = 5.5;
        public const double CloseSchoolsStudentCost = 96.5;
        public const double VaccineCost = 80000000;
        public const double HomeMadeMask = 0.5;
        public const double LowLevelMask = 0.6;
        public const double HighLevelMask = 0.7;
        public const double LowLevelMaskCost = 1;
        public const double HighLevelMaskCost = 10;

        // TODO: nodes and edges should be dictionaries to improve compatability
        // with the interface
        public double Budget { get; set; }

        public Node[] Nodes => this._nodes;
        public Edge[] Edges => this._edges;
        public int Day => this._day;

        private readonly Node[] _nodes;
        private readonly Edge[] _edges;
        private readonly Virus _virus;
        private readonly List<int>[] _nodeMap; //array of lists containing index of each node connected to a node

        private int _day = 0;
        private double _budgetIncrease;
        private double _vaccineProgress = 0;
        private double _loanMoney = 0;
        private double _totalLoanMoney = 0;

        /// <summary>
        /// Takes in a list of nodes, edges must than be constructed utilizing these nodes.
        /// Edges represented as a composite of list of base interactivities, populations
        /// and the indices of the nodes it connects.
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="edges"></param>
        /// <param name="virus"></param>
        public World(Node[] nodes, Edge[] edges, Virus virus)
        {
            this._nodes = nodes;
            this._edges = edges;
            this._virus = virus;

            this.Budget = 0;
            this._budgetIncrease = 0;

            this._nodeMap = new List<int>[nodes.Length];
            foreach (Node n in nodes)
            {
                this._nodeMap[n.Index] = new List<int>();

                //budget proportional to the population size
                this.Budget += 0.5 * n.TotalPopulation; //in a population of 7 billion, should be close to WHO's 2018-19 budget of 4.4 billion
            }

            foreach ((Edge e, int i) in edges.Select((e, i) => (e, i)))
            {
                this._nodeMap[e.Left.Index].Add(i);
                this._nodeMap[e.Right.Index].Add(i);
            }
        }

        /// <summary>
        /// Update goes through all edges to produce infections to add nodes.
        /// Each node and edge is updated individually.
        /// </summary>
        public void Update()
        {
            int[] edgeInfections = new int[this._nodes.Length];
            Array.Clear(edgeInfections, 0, edgeInfections.Length);
            this._budgetIncrease = 0;

            // Goes through all edges and increases the number of new infections to add to the node.
            foreach (Edge e in this._edges)
            {
                (int left, int right) = e.Update(this._virus);
                edgeInfections[e.Left.Index] += left;
                edgeInfections[e.Right.Index] += right;
            }

            // Goes through all nodes, updates each individual node and infects people based off the edges.
            foreach (Node n in this._nodes)
            {
                n.Update(this._virus);
                n.Infect(edgeInfections[n.Index]);
                this._budgetIncrease = 10 * n.Totals.SeriousInfection + n.Totals.Dead;
            }

            this._day++;
            this.Budget += this._budgetIncrease - this._loanMoney; //increases Budget + undoes affect of loaned money on daily Budget

            //if loaned money, set _loanMoney to 0
            if (this._loanMoney > 0)
            {
                this._loanMoney = 0;
            }
        }

        /* Actions from WHO
         * ALL ACTIONS RETURN THEIR COST IN THE Budget
         */

        /// <summary>
        /// Applies test and isolate to a specific area
        /// </summary>
        public double TestAndIsolation(Models.Parameters.TestAndIsolation p)
        {
            this._nodes.Get(p.Location).TestAndIsolate(p.TestQuality == 1, p.QuarantinePeriod, p.Quantity, p.SymptomaticOnly);

            // TODO: rework cost calculations elsewhere
            return (p.TestQuality == 1 ? GoodTestCost : BadTestCost) * p.Quantity;
        }

        /// <summary>
        /// Cancels test and isolate in a specific area - no cost
        /// </summary>
        public void CancelTestAndIsolate(Models.Parameters.TestAndIsolation p)
        {
            this._nodes.Get(p.Location).CancelTestAndIsolate(p.TestQuality == 1, p.QuarantinePeriod, p.Quantity, p.SymptomaticOnly);
        }

        /// <summary>
        /// Implements stay at home order on a specific node, cost at PressReleaseCost per person (press release)
        /// </summary>
        public double StayAtHomeOrder(Models.Parameters.StayAtHome p)
        {
            int i = p.Location.ToNodeIndex();
            this._nodes[i].StayAtHomeOrder();

            // TODO: rework cost calculations elsewhere
            return PressReleaseCost * this._nodes[i].TotalPopulation;
        }

        /// <summary>
        /// Cancels stay at home order on a specific node, cost at 0.01 per person (press release)
        /// </summary>
        public void CancelStayAtHomeOrder(Models.Parameters.StayAtHome p)
        {
            this._nodes.Get(p.Location).CancelStayAtHomeOrder();
        }

        /// <summary>
        /// Closes schools in a specific node - cost at 0.01 per person (press release)
        /// + learning from home recurrent costs removed from Budget increase
        /// </summary>
        public double CloseSchools(Models.Parameters.CloseSchools p)
        {
            int i = p.Location.ToNodeIndex();
            this._nodes[i].CloseSchools();

            // TODO: rework cost calculations elsewhere
            //(weekly cost/7) * number of schoolaged children
            this.Budget -= (CloseSchoolsStudentCost / 7) * this._nodes[i].TotalPopulation * this._nodes[i].NodeDemographics.FiveToSeventeen;
            return PressReleaseCost * this._nodes[i].TotalPopulation;
        }

        /// <summary>
        /// Cancels closes schools and readds the the cost from Budget increase - press release cost
        /// </summary>
        public void CancelCloseSchools(Models.Parameters.CloseSchools p)
        {
            int i = p.Location.ToNodeIndex();
            this._nodes.Get(p.Location).CancelCloseSchools();
            this.Budget += (CloseSchoolsStudentCost / 7) * this._nodes[i].TotalPopulation * this._nodes[i].NodeDemographics.FiveToSeventeen;
        }

        /// <summary>
        /// Closes recreational areas in a specific node - press release cost
        /// </summary>
        public double CloseRecreationalLocations(Models.Parameters.CloseRecreationalLocations p)
        {
            int i = p.Location.ToNodeIndex();
            this._nodes[i].CancelCloseRecreationalAreas();

            // TODO: rework cost calculations elsewhere
            return PressReleaseCost * this._nodes[i].TotalPopulation;
        }

        /// <summary>
        /// Cancels closed recreational areas in a specific node - press release cost
        /// </summary>
        public void CancelCloseRecreationalLocations(Models.Parameters.CloseRecreationalLocations p)
        {
            this._nodes.Get(p.Location).CancelCloseRecreationalAreas();
        }

        /// <summary>
        /// Implements shielding in a specific node - press release cost    
        /// </summary>
        public double ShieldingProgram(Models.Parameters.ShieldingProgram p)
        {
            int i = p.Location.ToNodeIndex();
            this._nodes[i].ShieldingProgram();

            // TODO: rework cost calculations elsewhere
            return PressReleaseCost * this._nodes[i].TotalPopulation;
        }

        /// <summary>
        /// Cancels shielding in a specific node - press release cost
        /// </summary>
        public void CancelShieldingPorgram(Models.Parameters.ShieldingProgram p)
        {
            this._nodes.Get(p.Location).CancelShieldingProgram();
        }

        /// <summary>
        /// Closes edges over a distance connected to a specific node - press release cost
        /// </summary>
        public double MovementRestrictions(Models.Parameters.MovementRestrictions p)
        {
            int i = p.Location.ToNodeIndex();
            this._nodes[i].MovementRestrictions();

            foreach (int j in this._nodeMap[i])
            {
                if (this._edges[j].Distance > p.Distance)
                {
                    this._edges[j].CloseEdge();
                }
            }

            // TODO: rework cost calculations elsewhere
            return PressReleaseCost * this._nodes[i].TotalPopulation;
        }

        /// <summary>
        /// Cancels closed edges over a distance connected to a specific node - press release cost
        /// </summary>
        public void CancelMovementRestrictions(Models.Parameters.MovementRestrictions p)
        {
            int i = p.Location.ToNodeIndex();
            this._nodes[i].CancelMovementRestrictions();

            foreach (int j in this._nodeMap[i])
            {
                this._edges[j].OpenEdge();
            }
        }

        /// <summary>
        /// Closes all edges connected to a specific node - press release cost
        /// </summary>
        public double CloseBorders(Models.Parameters.CloseBorders p)
        {
            int i = p.Location.ToNodeIndex();
            this._nodes[i].CloseBorders();

            foreach (int j in this._nodeMap[i])
            {
                this._edges[j].CloseEdge();
            }

            // TODO: rework cost calculations elsewhere
            return PressReleaseCost * this._nodes[i].TotalPopulation;
        }

        /// <summary>
        /// Cancels the effect of Close borders in a specific node
        /// </summary>
        public void CancelCloseBorders(Models.Parameters.CloseBorders p)
        {
            int i = p.Location.ToNodeIndex();
            this._nodes[i].CancelCloseBorders();

            foreach (int j in this._nodeMap[i])
            {
                this._edges[j].OpenEdge();
            }
        }

        /// <summary>
        /// Invests into vaccine progress
        /// </summary>
        public void InvestInVaccine(Models.Parameters.InvestInVaccine p)
        {
            this._vaccineProgress += p.AmountInvested / VaccineCost;
        }

        /// <summary>
        /// Implement furlough schemes in a specific node - recurrent cost on the Budget increase + initial cost
        /// </summary>
        public double FurloughScheme(Models.Parameters.Furlough p)
        {
            int i = p.Location.ToNodeIndex();
            this._nodes[i].FurloughScheme(p.AmountInvested);

            // TODO: rework cost calculations elsewhere
            int cost = p.AmountInvested * this._nodes[i].TotalPopulation; //currently overestimates
            this.Budget -= cost;
            return cost;
        }

        /// <summary>
        /// Cancels the furlough scheme - cost press release
        /// </summary>
        public void CancelFurloughScheme(Models.Parameters.Furlough p)
        {
            int i = p.Location.ToNodeIndex();
            this._nodes.Get(p.Location).CancelFurloughScheme(p.AmountInvested);
            this.Budget += p.AmountInvested * this._nodes[i].TotalPopulation;
        }

        /// <summary>
        /// Implements a single press release in a node - press release cost
        /// Amount currently does nothing
        /// </summary>
        public double InformationPressRelease(Models.Parameters.InformationPressRelease p)
        {
            int i = p.Location.ToNodeIndex();
            this._nodes[i].InformationPressRelease();

            // TODO: rework cost calculations elsewhere
            return PressReleaseCost * this._nodes[i].TotalPopulation;
        }

        /// <summary>
        /// Implements taking out a loan, which will be added to next turn as a lump sum and all loans tracked
        /// </summary>
        public void TakeLoan(Models.Parameters.Loan p)
        {
            this._totalLoanMoney += p.AmountLoaned;
            this.Budget += p.AmountLoaned;
            this._loanMoney += p.AmountLoaned;
        }

        /// <summary>
        /// Implements mask mandate in a specific node - press release cost + cost per mask
        /// </summary>
        public double MaskMandate(Models.Parameters.MaskMandate p)
        {
            int i = p.Location.ToNodeIndex();

            // TODO: rework cost calculations elsewhere
            //values using competition rules
            double cost = PressReleaseCost * this._nodes[i].TotalPopulation;
            switch (p.MaskProvisionLevel)
            {
                case 0:
                    this._nodes[i].MaskMandate(false, HomeMadeMask);
                    break;
                case 1:
                    this._nodes[i].MaskMandate(true, LowLevelMask);
                    cost += LowLevelMaskCost * this._nodes[i].TotalPopulation;
                    break;
                case 2:
                    this._nodes[i].MaskMandate(true, HighLevelMask);
                    cost += HighLevelMaskCost * this._nodes[i].TotalPopulation;
                    break;
                default:
                    Log.Error("Invalid Mask Mandate option");
                    break;
            }
            return cost;
        }

        /// <summary>
        /// Cancels the mask mandate - cost of press release
        /// </summary>
        public void CancelMaskMandate(Models.Parameters.MaskMandate p)
        {
            int i = p.Location.ToNodeIndex();

            //values using competition rules
            switch (p.MaskProvisionLevel)
            {
                case 0:
                    this._nodes[i].CancelMaskMandate(false, HomeMadeMask);
                    break;
                case 1:
                    this._nodes[i].CancelMaskMandate(true, LowLevelMask);
                    break;
                case 2:
                    this._nodes[i].CancelMaskMandate(true, HighLevelMask);
                    break;
                default:
                    Log.Error("Invalid Mask Mandate option");
                    break;
            }
        }

        /// <summary>
        /// Implements health drive - cost of press release - needs a recurrent cost but unsure of what it is from competition rules
        /// </summary>
        public double HealthDrive(Models.Parameters.HealthDrive p)
        {
            int i = p.Location.ToNodeIndex();
            this._nodes[i].HealthDrive();

            // TODO: rework cost calculations elsewhere
            return PressReleaseCost * this._nodes[i].TotalPopulation;
        }

        /// <summary>
        /// Cancels health drive - cost of press release - currently no point to do this
        /// </summary>
        public void CancelHealthDrive(Models.Parameters.HealthDrive p)
        {
            this._nodes.Get(p.Location).CancelHealthDrive();
        }

        /// <summary>
        /// Invests in the health services of a node - decreases lethality proportional
        /// to the cost of investment - needs recurrent cost
        /// </summary>
        public void InvestInHealthServices(Models.Parameters.InvestInHealthServices p)
        {
            int investment = p.AmountInvested / this._nodes.Length;

            foreach (Node n in _nodes)
            {
                n.InvestInHealthServices(investment);
            }
        }

        /// <summmary>
        /// cancels investment in health services - needs a recurrent cost to have a purpose
        /// </summary>
        public void CancelInvestInHealthServices(Models.Parameters.InvestInHealthServices p)
        {
            int investment = p.AmountInvested / this._nodes.Length;

            foreach (Node n in _nodes)
            {
                n.CancelInvestInHealthServices(investment);
            }
        }

        /// <summary>
        /// Implements social distancing of n meters in a specific node - cost of press release
        /// </summary>
        public double SocialDistancing(Models.Parameters.SocialDistancingMandate p)
        {
            int i = p.Location.ToNodeIndex();
            this._nodes[i].SocialDistancing(p.Distance);

            // TODO: rework cost calculations elsewhere
            return PressReleaseCost * this._nodes[i].TotalPopulation;
        }

        /// <summary>
        /// Cancels social distancing - cost of press release
        /// </summary>
        public void CancelSocialDistancing(Models.Parameters.SocialDistancingMandate p)
        {
            this._nodes.Get(p.Location).CancelSocialDistancing(p.Distance);
        }

        /// <summary>
        /// Implements curfew in a specific node at the cost of a press release
        /// </summary>
        public double Curfew(Models.Parameters.Curfew p)
        {
            int i = p.Location.ToNodeIndex();
            this._nodes[i].Curfew();

            // TODO: rework cost calculations elsewhere
            return PressReleaseCost * this._nodes[i].TotalPopulation;
        }

        /// <summary>
        /// Cancels a curfew in a specific node at the cost of a press release
        /// </summary>
        public void CancelCurfew(Models.Parameters.Curfew p)
        {
            this._nodes.Get(p.Location).CancelCurfew();
        }

        ///<summary> 
        /// Administers the vaccine to a target node and age range - age range is the index in the demographics array
        /// </summary>
        /* TODO: Implement after administration actions are added
        public void AdministerVaccine(Models.Parameters.? parameters)
        {
            int i = p.Location.ToNodeIndex();
            int ageRange = 5; //TODO replace age-range with actual interpretation

            this._nodes[i].AdministerVaccine(ageRange);

            // TODO: rework cost calculations elsewhere
            //cost per dose set to arbitrary 15
            int cost = 15 * this._nodes[i].TotalPopulation * this._nodes[i].Demographics[ageRange]
        }
        */
    }
}
