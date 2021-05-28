using System;

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
        // TODO: nodes and edges should be dictionaries to improve compatability
        // with the interface
        private readonly Node[] _nodes;
        private readonly Edge[] _edges;
        private readonly Virus _virus;
        private int _day;
        public double budget { get;}
        private double budgetIncrease;
        public double vaccineProgress;
        public double loanMoney;
        public double totalLoanMoney;
        public List<int>[] nodeMap; //array of lists containing index of each node connected to a node

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
            this._day = 0;
            this.budget = 1000000;
            this.budgetIncrease = 500000;
            this.vaccineProgress = 0;
            this.loanMoney = 0;
            this.nodeMap = new List<int>[nodes.Length];
            foreach (Node n in nodes)
            {
                this.nodeMap[n.Index] = new List<int>();
            }
            int i = 0;
            foreach (Edge e in edges)
            {
                this.nodeMap[e.Left.Index].Add(i);
                this.nodeMap[e.Right.Index].Add(i);
                i++;
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
                n.IncrementHead();
            }

            this._day++;
            this._budget += this.budgetIncrease - this.loanMoney; //increases budget + undoes affect of loaned money on daily budget

            //if loaned money, set loanMoney to 0
            if (this.loanMoney > 0)
            {
                this.loanMoney = 0;
            }

        }
        
        /* Actions from WHO
         * ALL ACTIONS RETURN THEIR COST IN THE BUDGET
         */

        ///<summary>
        /// Applies test and isolate to a specific area
        /// </summary>
        public double TestAndIsolate(bool goodTest, int quarantinePeriod, int testQuantity, int locationIndex)
        {
            this._nodes[locationIndex].TestAndIsolate(goodTest, quarantinePeriod, testQuantity);
            int cost = 0;
            if (goodTest)
            {
                cost = 140 * testQuantity;
            } else
            {
                cost = 5.5 * testQuantity;
            }
            return cost;
        }

        ///<summary>
        /// Cancels test and isolate in a specific area - no cost
        /// </summary>
        public double CancelTestAndIsolate(bool goodTest, int quarantinePeriod, int testQuantity, int locationIndex)
        {
            this._nodes[locationIndex].CancelTestAndIsolate(goodTest, quarantinePeriod, testQuantity);
            return 0;
        }

        ///<summary>
        /// Implements stay at home order on a specific node, cost at 0.01 per person (press release)
        /// </summary>
        public double StayAtHomeOrder(int locationIndex)
        {
            this._nodes[locationIndex].StayAtHomeOrder();
            return 0.01 * this._nodes[locationIndex].TotalPopulation;
        }

        ///<summary>
        /// Cancels stay at home order on a specific node, cost at 0.01 per person (press release)
        /// </summary>
        public double CancelStayAtHomeOrder(int locationIndex)
        {
            this._nodes[locationIndex].CancelStayAtHomeOrder();
            return 0.01 * this._nodes[locationIndex].TotalPopulation;
        }

        ///<summary>
        /// Closes schools in a specific node - cost at 0.01 per person (press release) + learning from home recurrent costs removed from budget increase
        /// </summary>
        public double CloseSchools(int locationIndex)
        {
            this._nodes[locationIndex].CloseSchools();
            //(weekly cost/7) * number of schoolaged children
            this.budgetIncrease -= (0.165/7) * this._nodes[locationIndex].TotalPopulation * this._nodes[locationIndex].demographics[1];
            return 0.01 * this._nodes[locationIndex].TotalPopulation;
        }

        ///<summary>
        /// Cancels closes schools and readds the the cost from budget increase - press release cost
        /// </summary>
        public double CancelCloseSchools(int locationIndex)
        {
            this._nodes[locationIndex].CancelCloseSchools();
            this.budgetIncrease += (0.165/7) * this._nodes[locationIndex].TotalPopulation * this._nodes[locationIndex].demographics[1];
            return 0.01 * this._nodes[locationIndex].TotalPopulation;
        }

        ///<summary>
        /// Closes recreational areas in a specific node - press release cost
        /// </summary>
        public double CloseRecreationalAreas(int locationIndex)
        {
            this._nodes[locationIndex].CancelCloseRecreationalAreas();
            return 0.01 * this._nodes[locationIndex].TotalPopulation;
        }

        ///<summary>
        /// Cancels closed recreational areas in a specific node - press release cost
        /// </summary>
        public double CancelCloseRecreationalAreas(int locationIndex)
        {
            this._nodes[locationIndex].CancelCloseRecreationalAreas();
            return 0.01 * this._nodes[locationIndex].TotalPopulation;
        }

        ///<summary>
        /// Implements shielding in a specific node - press release cost    
        /// </summary>
        public double ShieldingProgram(int locationIndex)
        {
            this._nodes[locationIndex].ShieldingProgram();
            return 0.01 * this._nodes[locationIndex].TotalPopulation;
        }

        ///<summary>
        /// Cancels shielding in a specific node - press release cost
        /// </summary>
        public double CancelShieldingPorgram(int locationIndex)
        {
            this._nodes[locationIndex].CancelShieldingProgram();
            return 0.01 * this._nodes[locationIndex].TotalPopulation;
        }

        ///<summary>
        /// Closes edges over a distance connected to a specific node - press release cost
        /// </summary>
        public double MovementRestrictions(int locationIndex, int distance)
        {
            this._nodes[locationIndex].MovementRestrictions();
            foreach (int i in this.nodeMap[locationIndex])
            {
                if (this._edges[i].distance > distance)
                {
                    this._edges[i].CloseEdge();
                }
            }
            return 0.01 * this._nodes[locationIndex].TotalPopulation;
        }

        ///<summary>
        /// Cancels closed edges over a distance connected to a specific node - press release cost
        /// </summary>
        public double CancelMovementRestrictions(int locationIndex, int distance)
        {
            this._nodes[locationIndex].CancelMovementRestrictions();
            foreach (int i in this.nodeMap[locationIndex])
            {
                this._edges[i].OpenEdge();
            }
            return 0.01 * this._nodes[locationIndex].TotalPopulation;
        }

        ///<summary>
        /// Closes all edges connected to a specific node - press release cost
        /// </summary>
        public double CloseBorders(int locationIndex)
        {
            this._nodes[locationIndex].CloseBorders();
            foreach (int i in this.nodeMap[locationIndex])
            {
                this._edges[i].CloseEdge();
            }
            return 0.01 * this._nodes[locationIndex].TotalPopulation;
        }

        ///<summary>
        /// Cancels the effect of Close borders in a specific node
        /// </summary>
        public double CancelCloseBorders(int locationIndex)
        {
            this._nodes[locationIndex].CancelCloseBorders();
            foreach (int i in this.nodeMap[locationIndex])
            {
                this._edges[i].OpenEdge()
            }
            return 0.01 * this._nodes[locationIndex].TotalPopulation;
        }

        ///<summary>
        /// Invests into vaccine progress
        /// </summary>
        public double InvestInVaccine(double investment)
        {
            //uses 80 million cost from competition rules
            this.vaccineProgress += investment / 80000000;
            return investment;
        }

        ///<summary>
        /// Implement furlough schemes in a specific node - recurrent cost on the budget increase + initial cost
        /// </summary>
        public double FurloughScheme(int investment, int locationIndex)
        {
            this._nodes[locationIndex].FurloughScheme(investment);
            int cost = investment * this._nodes[locationIndex].TotalPopulation; //currently overestimates
            this.budgetIncrease -= cost;
            return cost;
        }

        ///<summary>
        /// Cancels the furlough scheme - cost press release
        /// </summary>
        public double CancelFurloughScheme(int investment, int locationIndex)
        {
            this._nodes[locationIndex].CancelFurloughScheme(investment);
            int cost = investment * this._nodes[locationIndex].TotalPopulation; //currently overestimates
            return 0.01 * this._nodes[locationIndex].TotalPopulation
        }

        ///<summary>
        /// Implements a single press release in a node - press release cost
        /// Amount currently does nothing
        /// </summary>
        public double InformationPressRelease(int amount, int locationIndex)
        {
            this._nodes[locationIndex].InformationPressRelease();
            return 0.01 * this._nodes[locationIndex].TotalPopulation;
        }

        ///<summary>
        /// Implements taking out a loan, which will be added to next turn as a lump sum and all loans tracked
        /// </summary>
        public void TakeLoan(int amount)
        {
            this.totalLoanMoney += amount;
            this.budget += amount;
            this.loanMoney += amount;
        }

        ///<summary>
        /// Implements mask mandate in a specific node - press release cost + cost per mask
        /// </summary>
        public double MaskMandate(int locationIndex, int masksProvided)
        {
            //values using competition rules
            int cost = 0.01 * this._nodes[locationIndex].TotalPopulation;
            switch (masksProvided)
            {
                case 1:
                    this._nodes[locationIndex].MaskMandate(false,0.5);
                    break;
                case 2:
                    this._nodes[locationIndex].MaskMandate(true,0.5);
                    cost += 1 * this._nodes[locationIndex].TotalPopulation;
                    break;
                case 3:
                    this._nodes[locationIndex].MaskMandate(true,0.7);
                    cost += 10 * this._nodes[locationIndex].TotalPopulation;
                    break;
                default:
                    Console.WriteLine("Invalid Mask Mandate option");
                    break;
            }
            return cost;
        }

        ///<summary>
        /// Cancels the mask mandate - cost of press release
        /// </summary>
        public double CancelMaskMandate(int locationIndex, int masksProvided)
        {
            //values using competition rules
            int cost = 0.01 * this._nodes[locationIndex].TotalPopulation;
            switch (masksProvided)
            {
                case 1:
                    this._nodes[locationIndex].CancelMaskMandate(false,0.5);
                    break;
                case 2:
                    this._nodes[locationIndex].CancelMaskMandate(true,0.5);
                    break;
                case 3:
                    this._nodes[locationIndex].CancelMaskMandate(true,0.7);
                    break;
                default:
                    Console.WriteLine("Invalid Mask Mandate option");
                    break;
            }
            return cost;
        }

        ///<summary>
        /// Implements health drive - cost of press release - needs a recurrent cost but unsure of what it is from competition rules
        /// </summary>
        public double HealthDrive(int locationIndex)
        {
            this._nodes[locationIndex].HealthDrive();
            return 0.01 * this._nodes[locationIndex].TotalPopulation;
        }

        ///<summary>
        /// Cancels health drive - cost of press release - currently no point to do this
        /// </summary>
        public double CancelHealthDrive(int locationIndex)
        {
            this._nodes[locationIndex].CancelHealthDrive();
            return 0.01 * this._nodes[locationIndex].TotalPopulation;
        }

        ///<summary>
        /// invests in the health services of a node - decreases lethality proportional to the cost of investment - needs recurrent cost
        /// </summary>
        public double InvestInHealthServices(int locationIndex, int investment)
        {
            this._nodes[locationIndex].InvestInHealthServices(investment);
            return investment;
        }

        ///<summmary>
        /// cancels investment in health services - needs a recurrent cost to have a purpose
        /// </summary>
        public double CancelInvestInHealthServices(int locationIndex, int investment)
        {
            this._nodes[locationIndex].CancelInvestInHealthServices(investment);
            return 0;
        }

        ///<summary>
        /// Implements social distancing of n meters in a specific node - cost of press release
        /// </summary>
        public double SocialDistancing(int n, int locationIndex)
        {
            this._nodes[locationIndex].SocialDistancing(n);
            return 0.01 * this._nodes[locationIndex].TotalPopulation;
        }

        ///<summary>
        /// Cancels social distancing - cost of press release
        /// </summary>
        public double CancelSocialDistancing(int n, int locationIndex)
        {
            this._nodes[locationIndex].CancelSocialDistancing(n);
            return 0.01 * this._nodes[locationIndex].TotalPopulation;
        }

        ///<summary>
        /// Implements curfew in a specific node at the cost of a press release
        /// </summary>
        public double Curfew(int locationIndex)
        {
            this._nodes[locationIndex].Curfew();
            return 0.01 * this._nodes[locationIndex].TotalPopulation;
        }

        ///<summary>
        /// Cancels a curfew in a specific node at the cost of a press release
        /// </summary>
        public double CancelCurfew(int locationIndex)
        {
            this._nodes[locationIndex].CancelCurfew();
            return 0.01 * this._nodes[locationIndex].TotalPopulation;
        }

        ///<summary> 
        /// Administers the vaccine to a target node and age range - age range is the index in the demographics array
        /// </summary>
        public double AdministerVaccine(int locationIndex, int ageRange)
        {
            this._nodes[locationIndex].AdministerVaccine(ageRange);
            //cost per dose set to arbitrary 15
            int cost = 15 * this._nodes[locationIndex].TotalPopulation * this._nodes[locationIndex].demographics[ageRange]
        }
        

    }
}
