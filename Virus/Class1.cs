using System;
using System.Linq;

namespace Virus
{
    /** 
     * Representation of the world
     * Contains the Node Map + Edges
     * Contains virus
     * Update - all nodes and edges update simultaneously - allows for high degree parallelism
     * Contains the effects of various actions affecting interactivity rate 
     * Contains the current day in the simulation
     *
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
        public World(Node[] nodeList, double[] interactivities, int[] populations, (int,int)[] connections, Virus virus)
        {
            this.nodes = nodeList;

            //goes through all edges and creates them
            for (int i = 0; i<interactivities.Length; i++)
            {
                edges.add(new Edge(nodeList[connections[i].Item2],nodeList[connections[i].Item2],populations[i],interactivities[i]));
            }

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
            Array.Clear(edgeInfections,0,edgeInfections.Length);
            
            //goes through all edges and increases the number of new infections to add to the node
            foreach (var e in this.edges)
            {
                (int,int) adds = e.update(this.virus);
                edgeInfections[e.node1.index] += adds.Item1;
                edgeInfections[e.node2.index] += adds.Item2;
            }

            //goes through all nodes, updates each individual node and infects people based off the edges
            foreach(var n in this.nodes)
            {
                n.update(this.virus);
                n.infect(edgeInfections[n.index]);
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
     * Contains starting population
     * Contains populations for the 7 stages of infectivity 
     *  Uninfected -> Asymp Not Infectious -> Asymp Infectious -> Symp -> Serious -> Death/Recovered
     * Contains interactivity rate within a node - how many people a person interacts with in a spreadable way
     * Contains internal update method which changes population numbers
     * Contains history of number of infections - tracks number of infections over time to affect  
     * Contains infect method which takes in a given number of uninfected people and infects them 
     * 
     */
    public class Node
    {
        //ATTRIBUTES
        public int index;
        public int totalPopulation;
        public int uninfected;
        public int asympUninf;
        public int asympInf;
        public int symp;
        public int serious;
        public int dead;
        public int recovered;
        public double interactivity;
        public int[] infectionHistory;

        //CONSTRUCTORS
        public Node(int pop, int index)
        {
            this.index = index;
            this.totalPopulation = pop;
            this.uninfected = pop;
            this.asympInf = 0;
            this.asympUninf = 0;
            this.symp = 0;
            this.serious = 0;
            this.dead = 0;
            this.recovered = 0;

        }


        //METHODS
        /***
         * Takes current populations and updates to form new set of populations based off given virus, interactivity within the node
         * Uses tracked history to determine how many people go from one state to another
         * 
         */
        public void update(Virus virus)
        {

        }

        /***
         * Infects a given number of people by taking them from uninfected to asympUninf
         */
        public void infect(int pop)
        {
            this.uninfected -= pop;
            this.asympUninf += pop;
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
        public Tuple<int,int> update(Virus virus)
        {


        }

    }


}

