using System;
using System.Collections.Generic;

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
        //Effect constants - dictate how big effect certain actions should have - should probably be in config file
        public const double PressReleaseCost = 0.01;
        public const double GoodTestCost = 150;
        public const double BadTestCost = 15;
        public const double VaccineCost = 80000000;
        public const double HomeMadeMask = 0.5;
        public const double LowLevelMask = 0.6;
        public const double HighLevelMask = 0.7;
        public const double LowLevelMaskCost = 1;
        public const double HighLevelMaskCost = 15;

        // TODO: nodes and edges should be dictionaries to improve compatability
        // with the interface
        private readonly Node[] _nodes;
        private readonly Edge[] _edges;
        private readonly Virus _virus;
        private int _day;
        public double Budget;
        private double _budgetIncrease;
        private double _vaccineProgress;
        private double _loanMoney;
        private double _totalLoanMoney;
        private List<int>[] _nodeMap; //array of lists containing index of each node connected to a node

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
            this.Budget = 1000000;
            this._budgetIncrease = 500000;
            this._vaccineProgress = 0;
            this._loanMoney = 0;
            this._nodeMap = new List<int>[nodes.Length];
            foreach (Node n in nodes)
            {
                this._nodeMap[n.Index] = new List<int>();
            }
            int i = 0;
            foreach (Edge e in edges)
            {
                this._nodeMap[e.Left.Index].Add(i);
                this._nodeMap[e.Right.Index].Add(i);
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
            this._budget += this._budgetIncrease - this._loanMoney; //increases Budget + undoes affect of loaned money on daily Budget

            //if loaned money, set _loanMoney to 0
            if (this._loanMoney > 0)
            {
                this._loanMoney = 0;
            }

        }
        
        ///<summary>
        /// Takes in string location and converts it to an integer index
        /// </summary>
        /// <param name="location></param>
        public int LocationToLocationIndex(string location)
        {
            return Int32.Parse(location.Substring(1));
        }
        
        /* Actions from WHO
         * ALL ACTIONS RETURN THEIR COST IN THE Budget
         */

        ///<summary>
        /// Applies test and isolate to a specific area
        /// </summary>
        public double TestAndIsolation(Models.WhoAction.Parameters parameters)
        {
            if (parameters.ActionName != "testAndIsolation")
            {
                throw //TODO THROW
            }
            int locationIndex = LocationToLocationIndex(parameters.Location[0]);
            bool goodTest = true;
            if (parameters.TestQuality == 0)
            {
                bool goodTest = false;
            }
            int quarantinePeriod = parameters.QuarantinePeriod;
            int testQuantity = parameters.Quantity;

            this._nodes[locationIndex].TestAndIsolate(goodTest, quarantinePeriod, testQuantity);
            int cost = 0;
            if (goodTest)
            {
                cost = GoodTestCost * testQuantity;
            } else
            {
                cost = BadTestCost * testQuantity;
            }
            return cost;
        }

        ///<summary>
        /// Cancels test and isolate in a specific area - no cost
        /// </summary>
        public double CancelTestAndIsolate(Models.WhoAction.Parameters parameters)
        {
            if (parameters.ActionName != "testAndIsolation")
            {
                throw //TODO THROW
            }
            int locationIndex = LocationToLocationIndex(parameters.Location[0]);
            bool goodTest = true;
            if (parameters.TestQuality == 0)
            {
                bool goodTest = false;
            }
            int quarantinePeriod = parameters.QuarantinePeriod;
            int testQuantity = parameters.Quantity;

            this._nodes[locationIndex].CancelTestAndIsolate(goodTest, quarantinePeriod, testQuantity);
            return 0;
        }

        ///<summary>
        /// Implements stay at home order on a specific node, cost at PressReleaseCost per person (press release)
        /// </summary>
        public double StayAtHomeOrder(Models.WhoAction.Parameters parameters)
        {
            if (parameters.ActionName != "stayAtHome")
            {
                throw //TODO THROW
            }
            int locationIndex = LocationToLocationIndex(parameters.Location[0]);

            this._nodes[locationIndex].StayAtHomeOrder();
            return PressReleaseCost * this._nodes[locationIndex].TotalPopulation;
        }

        ///<summary>
        /// Cancels stay at home order on a specific node, cost at 0.01 per person (press release)
        /// </summary>
        public double CancelStayAtHomeOrder(Models.WhoAction.Parameters parameters)
        {
            if (parameters.ActionName != "stayAtHome")
            {
                throw //TODO THROW
            }
            int locationIndex = LocationToLocationIndex(parameters.Location[0]); 

            this._nodes[locationIndex].CancelStayAtHomeOrder();
            return PressReleaseCost * this._nodes[locationIndex].TotalPopulation;
        }

        ///<summary>
        /// Closes schools in a specific node - cost at 0.01 per person (press release) + learning from home recurrent costs removed from Budget increase
        /// </summary>
        public double CloseSchools(Models.WhoAction.Parameters parameters)
        {
            if (parameters.ActionName != "closeSchools")
            {
                throw //TODO THROW
            }
            int locationIndex = LocationToLocationIndex(parameters.Location[0]); 

            this._nodes[locationIndex].CloseSchools();
            //(weekly cost/7) * number of schoolaged children
            this._budgetIncrease -= (0.165/7) * this._nodes[locationIndex].TotalPopulation * this._nodes[locationIndex].demographics[1];
            return PressReleaseCost * this._nodes[locationIndex].TotalPopulation;
        }

        ///<summary>
        /// Cancels closes schools and readds the the cost from Budget increase - press release cost
        /// </summary>
        public double CancelCloseSchools(Models.WhoAction.Parameters parameters)
        {
            if (parameters.ActionName != "closeSchools")
            {
                throw //TODO THROW
            }
            int locationIndex = LocationToLocationIndex(parameters.Location[0]); 

            this._nodes[locationIndex].CancelCloseSchools();
            this._budgetIncrease += (0.165/7) * this._nodes[locationIndex].TotalPopulation * this._nodes[locationIndex].demographics[1];
            return PressReleaseCost * this._nodes[locationIndex].TotalPopulation;
        }

        ///<summary>
        /// Closes recreational areas in a specific node - press release cost
        /// </summary>
        public double CloseRecreationalLocations(Models.WhoAction.Parameters parameters)
        {
            if (parameters.ActionName != "closeRecreationalLocations")
            {
                throw //TODO THROW
            }
            int locationIndex = LocationToLocationIndex(parameters.Location[0]); 

            this._nodes[locationIndex].CancelCloseRecreationalAreas();
            return PressReleaseCost * this._nodes[locationIndex].TotalPopulation;
        }

        ///<summary>
        /// Cancels closed recreational areas in a specific node - press release cost
        /// </summary>
        public double CancelCloseRecreationalLocations(Models.WhoAction.Parameters parameters)
        {
            if (parameters.ActionName != "closeRecreationalLocations")
            {
                throw //TODO THROW
            }
            int locationIndex = LocationToLocationIndex(parameters.Location[0]); 

            this._nodes[locationIndex].CancelCloseRecreationalAreas();
            return PressReleaseCost * this._nodes[locationIndex].TotalPopulation;
        }

        ///<summary>
        /// Implements shielding in a specific node - press release cost    
        /// </summary>
        public double ShieldingProgram(Models.WhoAction.Parameters parameters)
        {
            if (parameters.ActionName != "shieldingProgram")
            {
                throw //TODO THROW
            }
            int locationIndex = LocationToLocationIndex(parameters.Location[0]); 

            this._nodes[locationIndex].ShieldingProgram();
            return PressReleaseCost * this._nodes[locationIndex].TotalPopulation;
        }

        ///<summary>
        /// Cancels shielding in a specific node - press release cost
        /// </summary>
        public double CancelShieldingPorgram(Models.WhoAction.Parameters parameters)
        {
            if (parameters.ActionName != "shieldingProgram")
            {
                throw //TODO THROW
            }
            int locationIndex = LocationToLocationIndex(parameters.Location[0]); 

            this._nodes[locationIndex].CancelShieldingProgram();
            return PressReleaseCost * this._nodes[locationIndex].TotalPopulation;
        }

        ///<summary>
        /// Closes edges over a distance connected to a specific node - press release cost
        /// </summary>
        public double MovementRestrictions(Models.WhoAction.Parameters parameters)
        {
            if (parameters.ActionName != "movementRestrictions")
            {
                throw //TODO THROW
            }
            int locationIndex = LocationToLocationIndex(parameters.Location[0]); 
            int distance = parameters.Distance;

            this._nodes[locationIndex].MovementRestrictions();
            foreach (int i in this._nodeMap[locationIndex])
            {
                if (this._edges[i].distance > distance)
                {
                    this._edges[i].CloseEdge();
                }
            }
            return PressReleaseCost * this._nodes[locationIndex].TotalPopulation;
        }

        ///<summary>
        /// Cancels closed edges over a distance connected to a specific node - press release cost
        /// </summary>
        public double CancelMovementRestrictions(Models.WhoAction.Parameters parameters)
        {
            if (parameters.ActionName != "movementRestrictions")
            {
                throw //TODO THROW
            }
            int locationIndex = LocationToLocationIndex(parameters.Location[0]); 
            int distance = parameters.Distance;

            this._nodes[locationIndex].CancelMovementRestrictions();
            foreach (int i in this._nodeMap[locationIndex])
            {
                this._edges[i].OpenEdge();
            }
            return PressReleaseCost * this._nodes[locationIndex].TotalPopulation;
        }

        ///<summary>
        /// Closes all edges connected to a specific node - press release cost
        /// </summary>
        public double CloseBorders(Models.WhoAction.Parameters parameters)
        {
            if (parameters.ActionName != "closeBorders")
            {
                throw //TODO THROW
            }
            int locationIndex = LocationToLocationIndex(parameters.Location[0]); 

            this._nodes[locationIndex].CloseBorders();
            foreach (int i in this._nodeMap[locationIndex])
            {
                this._edges[i].CloseEdge();
            }
            return PressReleaseCost * this._nodes[locationIndex].TotalPopulation;
        }

        ///<summary>
        /// Cancels the effect of Close borders in a specific node
        /// </summary>
        public double CancelCloseBorders(Models.WhoAction.Parameters parameters)
        {
            if (parameters.ActionName != "closeBorders")
            {
                throw //TODO THROW
            }
            int locationIndex = LocationToLocationIndex(parameters.Location[0]); 

            this._nodes[locationIndex].CancelCloseBorders();
            foreach (int i in this._nodeMap[locationIndex])
            {
                this._edges[i].OpenEdge()
            }
            return PressReleaseCost * this._nodes[locationIndex].TotalPopulation;
        }

        ///<summary>
        /// Invests into vaccine progress
        /// </summary>
        public double InvestInVaccine(Models.WhoAction.Parameters parameters)
        {
            if (parameters.ActionName != "investInVaccine")
            {
                throw //TODO THROW
            }
            int investment = parameters.AmountInvested;
            //uses 80 million cost from competition rules
            this._vaccineProgress += (double) investment / VaccineCost;
            return investment;
        }

        ///<summary>
        /// Implement furlough schemes in a specific node - recurrent cost on the Budget increase + initial cost
        /// </summary>
        public double FurloughScheme(Models.WhoAction.Parameters parameters)
        {
            if (parameters.ActionName != "furlough")
            {
                throw //TODO THROW
            }
            int locationIndex = LocationToLocationIndex(parameters.Location[0]); 
            int investment = parameters.AmountInvested;

            this._nodes[locationIndex].FurloughScheme(investment);
            int cost = investment * this._nodes[locationIndex].TotalPopulation; //currently overestimates
            this._budgetIncrease -= cost;
            return cost;
        }

        ///<summary>
        /// Cancels the furlough scheme - cost press release
        /// </summary>
        public double CancelFurloughScheme(Models.WhoAction.Parameters parameters)
        {
            if (parameters.ActionName != "furlough")
            {
                throw //TODO THROW
            }
            int locationIndex = LocationToLocationIndex(parameters.Location[0]); 
            int investment = parameters.AmountInvested;

            this._nodes[locationIndex].CancelFurloughScheme(investment);
            int cost = investment * this._nodes[locationIndex].TotalPopulation; //currently overestimates
            return PressReleaseCost * this._nodes[locationIndex].TotalPopulation
        }

        ///<summary>
        /// Implements a single press release in a node - press release cost
        /// Amount currently does nothing
        /// </summary>
        public double InformationPressRelease(Models.WhoAction.Parameters parameters)
        {
            if (parameters.ActionName != "infoPressRelease")
            {
                throw //TODO THROW
            }
            int locationIndex = LocationToLocationIndex(parameters.Location[0]); 

            this._nodes[locationIndex].InformationPressRelease();
            return PressReleaseCost * this._nodes[locationIndex].TotalPopulation;
        }

        ///<summary>
        /// Implements taking out a loan, which will be added to next turn as a lump sum and all loans tracked
        /// </summary>
        public void TakeLoan(Models.WhoAction.Parameters parameters)
        {
            if (parameters.ActionName != "loan")
            {
                throw //TODO THROW
            }
            int amount = parameters.AmountLoaned;

            this._totalLoanMoney += amount;
            this.Budget += amount;
            this._loanMoney += amount;
        }

        ///<summary>
        /// Implements mask mandate in a specific node - press release cost + cost per mask
        /// </summary>
        public double MaskMandate(Models.WhoAction.Parameters parameters)
        {
            if (parameters.ActionName != "maskMandate")
            {
                throw //TODO THROW
            }
            int locationIndex = LocationToLocationIndex(parameters.Location[0]); 
            int masksProvided = parameters.MaskProvisionLevel;

            //values using competition rules
            int cost = PressReleaseCost * this._nodes[locationIndex].TotalPopulation;
            switch (masksProvided)
            {
                case 0:
                    this._nodes[locationIndex].MaskMandate(false,HomeMadeMask);
                    break;
                case 1:
                    this._nodes[locationIndex].MaskMandate(true,LowLevelMask);
                    cost += LowLevelMaskCost * this._nodes[locationIndex].TotalPopulation;
                    break;
                case 2:
                    this._nodes[locationIndex].MaskMandate(true,HighLevelMask);
                    cost += HighLevelMaskCost * this._nodes[locationIndex].TotalPopulation;
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
        public double CancelMaskMandate(Models.WhoAction.Parameters parameters)
        {
            if (parameters.ActionName != "maskMandate")
            {
                throw //TODO THROW
            }
            int locationIndex = LocationToLocationIndex(parameters.Location[0]); 
            int masksProvided = parameters.MaskProvisionLevel;

            //values using competition rules
            int cost = PressReleaseCost * this._nodes[locationIndex].TotalPopulation;
            switch (masksProvided)
            {
                case 0:
                    this._nodes[locationIndex].CancelMaskMandate(false,HomeMadeMask);
                    break;
                case 1:
                    this._nodes[locationIndex].CancelMaskMandate(true,LowLevelMask);
                    break;
                case 2:
                    this._nodes[locationIndex].CancelMaskMandate(true,HighLevelMask);
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
        public double HealthDrive(Models.WhoAction.Parameters parameters)
        {
            if (parameters.ActionName != "healthDrive")
            {
                throw //TODO THROW
            }
            int locationIndex = LocationToLocationIndex(parameters.Location[0]); 

            this._nodes[locationIndex].HealthDrive();
            return PressReleaseCost * this._nodes[locationIndex].TotalPopulation;
        }

        ///<summary>
        /// Cancels health drive - cost of press release - currently no point to do this
        /// </summary>
        public double CancelHealthDrive(Models.WhoAction.Parameters parameters)
        {
            if (parameters.ActionName != "healthDrive")
            {
                throw //TODO THROW
            }
            int locationIndex = LocationToLocationIndex(parameters.Location[0]); 

            this._nodes[locationIndex].CancelHealthDrive();
            return PressReleaseCost * this._nodes[locationIndex].TotalPopulation;
        }

        ///<summary>
        /// invests in the health services of a node - decreases lethality proportional to the cost of investment - needs recurrent cost
        /// </summary>
        public double InvestInHealthServices(Models.WhoAction.Parameters parameters)
        {
            if (parameters.ActionName != "healthDrive")
            {
                throw //TODO THROW
            }
            int investment = parameters.AmountInvested;

            foreach (Node n in _nodes)
            {
                n.InvestInHealthServices(investment);
            }

            return investment;
        }

        ///<summmary>
        /// cancels investment in health services - needs a recurrent cost to have a purpose
        /// </summary>
        public double CancelInvestInHealthServices(Models.WhoAction.Parameters parameters)
        {
            if (parameters.ActionName != "healthDrive")
            {
                throw //TODO THROW
            }
            int investment = parameters.AmountInvested/_nodes.Length;

            foreach (Node n in _nodes)
            {
                n.CancelInvestInHealthServices(investment);
            }
            return 0;
        }

        ///<summary>
        /// Implements social distancing of n meters in a specific node - cost of press release
        /// </summary>
        public double SocialDistancing(Models.WhoAction.Parameters parameters)
        {
            if (parameters.ActionName != "socialDistancingMandate")
            {
                throw //TODO THROW
            }
            int locationIndex = LocationToLocationIndex(parameters.Location[0]); 
            int n = parameters.Distance;

            this._nodes[locationIndex].SocialDistancing(n);
            return PressReleaseCost * this._nodes[locationIndex].TotalPopulation;
        }

        ///<summary>
        /// Cancels social distancing - cost of press release
        /// </summary>
        public double CancelSocialDistancing(Models.WhoAction.Parameters parameters)
        {
            if (parameters.ActionName != "socialDistancingMandate")
            {
                throw //TODO THROW
            }
            int locationIndex = LocationToLocationIndex(parameters.Location[0]); 
            int n = parameters.Distance;

            this._nodes[locationIndex].CancelSocialDistancing(n);
            return PressReleaseCost * this._nodes[locationIndex].TotalPopulation;
        }

        ///<summary>
        /// Implements curfew in a specific node at the cost of a press release
        /// </summary>
        public double Curfew(Models.WhoAction.Parameters parameters)
        {
            if (parameters.ActionName != "curfew")
            {
                throw //TODO THROW
            }
            int locationIndex = LocationToLocationIndex(parameters.Location[0]); 

            this._nodes[locationIndex].Curfew();
            return PressReleaseCost * this._nodes[locationIndex].TotalPopulation;
        }

        ///<summary>
        /// Cancels a curfew in a specific node at the cost of a press release
        /// </summary>
        public double CancelCurfew(Models.WhoAction.Parameters parameters)
        {
            if (parameters.ActionName != "curfew")
            {
                throw //TODO THROW
            }
            int locationIndex = LocationToLocationIndex(parameters.Location[0]); 

            this._nodes[locationIndex].CancelCurfew();
            return PressReleaseCost * this._nodes[locationIndex].TotalPopulation;
        }

        ///<summary> 
        /// Administers the vaccine to a target node and age range - age range is the index in the demographics array
        /// </summary>
        public double AdministerVaccine(Models.WhoAction.Parameters parameters)
        {
            if (parameters.ActionName != "administerVaccine")
            {
                throw //TODO THROW
            }
            int locationIndex = LocationToLocationIndex(parameters.Location[0]); 
            int ageRange = 5; //TODO replace age-range with actual interpretation

            this._nodes[locationIndex].AdministerVaccine(ageRange);
            //cost per dose set to arbitrary 15
            int cost = 15 * this._nodes[locationIndex].TotalPopulation * this._nodes[locationIndex].demographics[ageRange]
        }
        

    }
}
