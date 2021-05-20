using System;

namespace VirusModel
{
    /** 
     * Representation of the world
     * Contains the Node Map + Edges
     * Contains virus
     * Update - all nodes and edges update simultaneously - allows for high degree parallelism
     * Contains the effects of various actions affecting interactivity rate 
     * Contains the current day in the simulation
     * TODO: Replace Node and Edge arrays with dictionaries - will probably work better with interface
     */
    public class World
    {
        //ATTRIBUTES
        public Node[] nodes;
        public Edge[] edges;
        public Virus virus;
        public int day;

        //CONSTRUCTORS
        /***
         * Takes in a list of nodes, edges must than be constructed utilizing these nodes
         * Edges represented as a composite of list of base interactivities, populations and the indices of the nodes it connects
         */
        public World(Node[] nodeList, Edge[] edgeList, Virus virus)
        {
            this.nodes = nodeList;
            this.edges = edgeList;
            this.virus = virus;
            this.day = 0;
        }

        //METHODS
        /***
         * Update goes through all edges to produce infections to add nodes
         * Goes through all nodes and updates them individually
         * Increments day
         */
        public void update()
        {
            int[] edgeInfections = new int[nodes.Length];
            Array.Clear(edgeInfections, 0, edgeInfections.Length);

            //goes through all edges and increases the number of new infections to add to the node
            foreach (Edge e in this.edges)
            {
                Tuple<int, int> adds = e.update(this.virus);
                edgeInfections[e.node1.index] += adds.Item1;
                edgeInfections[e.node2.index] += adds.Item2;
            }

            //goes through all nodes, updates each individual node and infects people based off the edges
            foreach (Node n in this.nodes)
            {
                n.update(this.virus);
                n.infect(edgeInfections[n.index]);
                n.incrementHead();
            }

            day++;
        }

    }


    /**
    * Contains representation of the virus
    * Currently 4 rates - Infection Rate, Fatality Rate, Reinfection Rate, Symptomatic Rate
    * Future changes - include rates for demographics, include mutation
    * 
    */
    public class Virus
    {
        //ATTRIBUTES
        public double infectivity;
        public double fatality;
        public double reinfectivity;
        public double symptomaticity;

        //CONSTRUCTORS
        public Virus(double inf, double fat, double reinf, double symp)
        {
            this.infectivity = inf;
            this.fatality = fat;
            this.reinfectivity = reinf;
            this.symptomaticity = symp;
        }

        //METHODS 


    }





    /***
     * Representation of a single node within the system - a geographic location
     * Contains the index of the node on the map
     * Contains starting population and current population
     * Contains populations for the 7 stages of infectivity 
     *  Uninfected -> Asymp Not Infectious -> Asymp Infectious -> Symp -> Serious -> Death/Recovered
     * Contains interactivity rate within a node - how many people a person interacts with in a spreadable way
     * Contains internal update method which changes population numbers
     * Contains history of asymptomatic infections
     * Contains infect method which takes in a given number of uninfected people and infects them 
     * Contains a histories for symptomatic and serious infections
     * 
     */
    public class Node
    {
        //ATTRIBUTES
        public int index;
        public int startPopulation;
        public int totalPopulation;
        public int uninfected;
        public int asympUninf;
        public int asympInf;
        public int symp;
        public int serious;
        public int dead;
        public int recovered;
        public double interactivity;
        public int[] asympHistory;
        public int historyHead;
        public int[] sympHistory;
        public int[] seriousHistory;

        //CONSTRUCTORS
        public Node(int index, int pop, double inter)
        {
            this.index = index;
            this.startPopulation = pop;
            this.totalPopulation = pop;
            this.uninfected = pop;
            this.asympInf = 0;
            this.asympUninf = 0;
            this.symp = 0;
            this.serious = 0;
            this.dead = 0;
            this.recovered = 0;
            this.interactivity = inter;
            this.historyHead = 0;
            this.asympHistory = new int[14]; //infections last 14 days
            this.sympHistory = new int[14];
            this.seriousHistory = new int[14];

        }


        //METHODS
        /***
         * Takes current populations and updates to form new set of populations based off given virus, interactivity within the node
         * Uses tracked history to determine how many people go from one state to another
         * TODO: Replace symp -> serious rate with a virus variable
         * TODO: Replace virus and state durations with virus variables
         */
        public void update(Virus virus)
        {
            Random random = new Random(); //should maybe be included outside this scope?
            //finds the total number of infectious - could be separated for different interactivities
            int totalInfectious = this.asympInf + this.symp + this.serious;

            //infectiousness (infectious interactions) decided by the number of infectious people, the portion of the population which can be infected, and the interactivity of the node
            //TODO: Unsure if uninfected/totalPopulation is appropriate for this utiliziation - may need specific statstical method
            double infectiousness = totalInfectious * ((double)this.uninfected / (double)this.totalPopulation) * this.interactivity;
            infectiousness = infectiousness * virus.infectivity; //multiplies interactions * infectivity to get total number of people infected
            //the infectiousness is the number of people infected + the chance of 1 more
            int infected = (int)Math.Floor(infectiousness);

            infectiousness -= infected;
            var chance = random.NextDouble();
            if (infectiousness > chance)
            {
                infected += 1;
            }

            //infects the population
            this.infect(infected);


            //based on the infection history, moves people through levels of infectiousness
            //asympUninf (2 days) -> asympInf (2 days)
            int aUninf2InfHead = (historyHead + 14 - 2) % 14;
            int aInf2SympHead = (historyHead + 14 - 4) % 14;

            //following 2 days, all asympUninf go to asympInf
            this.asympUninf -= asympHistory[aUninf2InfHead];
            this.asympInf += asympHistory[aUninf2InfHead];

            //follwing 4 days (2 days after asympUninf -> asympInf) a portion of the asympInf move to symp depdendent on symptomaticity
            int aInf2Symp = (int)Math.Floor((double)asympHistory[aInf2SympHead] * virus.symptomaticity); //rounds to 0 under 1
            this.asympInf -= aInf2Symp;
            this.symp += aInf2Symp;
            //moves histories
            this.sympHistory[aInf2SympHead] += aInf2Symp;
            this.asympHistory[aInf2SympHead] -= aInf2Symp;

            //symp (2 day min) -> serious (2 day min) 
            int symp2SeriousHead = (historyHead + 14 - 6) % 14;
            int serious2DeadHead = (historyHead + 14 - 8) % 14;

            //a portion (25% - should be variable) of symptomatic go to serious after 2 days
            //TODO - replace 0.5 with proper virus variable
            int symp2Serious = (int)Math.Floor((double)sympHistory[symp2SeriousHead] * 0.25);
            this.symp -= symp2Serious;
            this.serious += symp2Serious;
            this.sympHistory[symp2SeriousHead] -= symp2Serious;
            this.seriousHistory[symp2SeriousHead] += symp2Serious;

            //a portion of serious cases result in death - depdenent on virus fatality rate
            int serious2Dead = (int)Math.Floor((double)seriousHistory[serious2DeadHead] * virus.fatality);
            this.serious -= serious2Dead;
            this.dead += serious2Dead;
            this.totalPopulation -= serious2Dead; //removes dead people from the current population
            this.seriousHistory[serious2DeadHead] -= serious2Dead; //removed from history for recovery tracking

            //at the end of the 14 day period - everyone infected 14 days ago still alive recovers
            int historyEnd = (historyHead + 1) % 14;
            int asymp2Recovered = asympHistory[historyEnd];
            int symp2Recovered = sympHistory[historyEnd];
            int serious2Recovered = seriousHistory[historyEnd];
            this.asympInf -= asymp2Recovered;
            this.symp -= symp2Recovered;
            this.serious -= serious2Recovered;
            this.recovered += asymp2Recovered + symp2Recovered + serious2Recovered;
            //clears history at the end
            asympHistory[historyEnd] = 0;
            sympHistory[historyEnd] = 0;
            seriousHistory[historyEnd] = 0;

            //moves a proportion of recovered to uninfected based on virus reinfectivity
            int reinfections = (int)Math.Floor(this.recovered * virus.reinfectivity);
            this.recovered -= reinfections;
            this.uninfected += reinfections;

            //Increment head not included in update - has to be done by world
            //This is due to edges also needing to use the head for infections

        }

        /***
         * Infects a given number of people by taking them from uninfected to asympUninf
         */
        public void infect(int pop)
        {
            if (pop > this.uninfected)
            {
                pop = this.uninfected;
            }
            this.uninfected -= pop;
            this.asympUninf += pop;
            this.asympHistory[this.historyHead] += pop;
        }

        /***
         * Increments the head of the infection history (queue)
         */
        public void incrementHead()
        {
            this.historyHead = (historyHead + 1) % 14;
        }

    }


    /***
     *  Representation of the links between nodes - transport between different geographic locations + infection across nodes
     *  Contains a total population - "thickness" of the edge, how many people go back and forth between the two nodes per tick
     *  Contains an interactivity rate within an edge - both the base and the current stored (allows for adjustments)
     *  Contains reference to the two Nodes it connects
     *  Contains an update function, which produces the number of infected people to add two each node
     *
     */
    public class Edge
    {
        //ATTRIBUTES
        public int totalPopulation;
        public double baseInteractivity;
        public double currentInteractivity;
        public Node node1;
        public Node node2;

        //CONSTRUCTORS
        public Edge(Node n1, Node n2, int pop, double interactivity)
        {
            this.node1 = n1;
            this.node2 = n2;
            this.totalPopulation = pop;
            this.baseInteractivity = interactivity;
            this.currentInteractivity = interactivity;

        }

        //METHODS
        /***
         * Update method uses the current interactivity + a given virus
         * Looks at the number of symptomatically infectious populations in nodes to determine number of infectious people on the edge
         * Produces a number of infections in the edge
         * Then distributes those infections to nodes on either end of the edge.
         * 
         */
        public Tuple<int, int> update(Virus virus)
        {
            Tuple<int, int> output;
            Random random = new Random();

            //determining how many people from each node in the population
            int n1Pop = (int)(this.totalPopulation * ((double)this.node1.totalPopulation / (this.node1.totalPopulation + this.node2.totalPopulation)));

            int n2Pop = this.totalPopulation - n1Pop;

            //determining how many infectious people come from each node - the population from the node * the proportion of people in the node who are infectious
            //TODO: Replace with a proper statistical measure on how many infectious people there would be from a subset of the population
            //TODO: Maybe not indclude serious?
            int n1Inf = (int)Math.Floor(n1Pop * ((double)(node1.asympInf + node1.symp + node1.serious) / node1.totalPopulation));
            int n2Inf = (int)Math.Floor(n2Pop * ((double)(node2.asympInf + node2.symp + node2.serious) / node2.totalPopulation));

            //determines how many recovered come from each node - same as above
            //TODO: Replace with proper statsitical measure on how many recovered people there would be from a subset of the population
            int n1Rec = (int)Math.Floor(n1Pop * ((double)node1.recovered / node1.totalPopulation));
            int n2Rec = (int)Math.Floor(n2Pop * ((double)node2.recovered / node2.totalPopulation));

            //the rest of the population is uninfected
            int n1UnInf = n1Pop - n1Inf - n1Rec;
            int n2UnInf = n2Pop - n2Inf - n2Rec;


            //finds the totals along the edge, then infects a new amount of people following same method as node
            int totalInfectious = n1Inf + n2Inf;
            int totalUninfected = n1UnInf + n2UnInf;

            //infectiousness (infectious interactions) decided by the number of infectious people, the portion of the population which can be infected, and the interactivity of the node
            double infectiousness = totalInfectious * ((double)totalUninfected / (double)this.totalPopulation) * this.currentInteractivity;
            infectiousness = infectiousness * virus.infectivity; //multiplies interactions * infectivity to get total number of people infected

            //the infectiousness is the number of people infected + the chance of 1 more
            int infected = (int)Math.Floor(infectiousness);
            infectiousness -= infected;
            var chance = random.NextDouble();
            if (infectiousness > chance)
            {
                infected += 1;
            }

            //splits the infected people into infected going into node 1 and node 2
            int n1Out = (int)(infected * ((double)n1Pop / totalPopulation));
            int n2Out = infected - n1Out;

            output = Tuple.Create(n1Out, n2Out);

            return output;

        }

    }


}
