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

        //variables for WHO effects and effectiveness
        public double base_compliance; 
        public double compliance; //between 0-1 as a modifier on WHO actions
        public double complianceModifiers; //used to track drops in compliance over time
        public double GDP;
        public double[] demographics; //list of proportions following {<5, 5-17, 18-29, 30-9, 40-9, 50-64, 65-74, 75-84, 85+} 
        public double localLethality { get; set; }
        public double publicOpinion;
        public int numPressReleases;

        //compliances used to remember compliances for specific policies - needed to undo
        public double TestAndIsolateCompliance;
        public double StayAtHomeCompliance;
        public double CloseRecreationalAreasCompliance;
        public double ShieldingProgramCompliance;
        public double MaskMandateCompliance;
        public double HealthDriveCompliance;
        public double SocialDistancingCompliance;
        public double CurfewCompliance;

        //used to see if certain schemes are active
        public bool StayAtHomeBool;
        public bool CloseRecAreaBool;

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

            //all presets to avoid messing with constructors in other parts
            this.base_compliance = 0.8
            this.compliance = 0.8;
            this.GDP = 50;
            this.complianceModifiers = 1;
            this.demographics = { 0.2, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1 }
            this.localLethality = 1; //modifier on virus fatality for the local node
            this.publicOpinion = 1;
            this.numPressReleases = 0;

            this.TestAndIsolateCompliance = 0.8;
            this.StayAtHomeCompliance = 0.8;
            this.CloseRecreationalAreasCompliance = 0.8;
            this.MaskMandateCompliance = 0.8;
            this.HealthDriveCompliance = 0.8;
            this.SocialDistancingCompliance = 0.8;
            this.CurfewCompliance = 0.8;

            this.StayAtHomeBool = false;
            this.CloseRecAreaBool = false;

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
            int serious2Dead = (int)Math.Floor((double)this._seriousHistory[serious2DeadHead] * virus.Fatality * localLethality);
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

            //changes complaince by the compliance modifier
            this.compliance = this.base_compliance * this.complianceModifiers;

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

        /// <summary>
        /// Changes compliance modifier without going out of bounds
        /// TODO: change to a less scuffed system with bounds on 0 and 1 - tanh?
        /// </summary>
        public void ChangeComplianceModifier(int change)
        {
            this.complianceModifiers += change;
            if (this.complianceModifiers > 1)
            {
                this.complianceModifiers = 1;
            } else if (this.complianceModifiers < 0) {
                this.complianceModifiers = 0;
            }

        }

        /// <summary>
        /// Does test and isolate within a node
        /// </summary>
        public void TestAndIsolate(bool goodTest, int quarantinePeriod, int testQuantity)
        {
            //TODO: implementation of testing/quarantine - unsure on how to implement quarantine period with current model
            this.GDP *= 0.95; //arbitrary GDP cost
            
            //interactivity determined by the test quality and how many tests there are 
            if (goodTest)
            {
                this._interactivity *= this.compliance * 0.1 * 0.7 * ((double)testQuantity / this.TotalPopulation);
            } else
            {
                this._interactivity *= this.compliance * 0.1 * 0.4 * ((double)testQuantity / this.TotalPopulation);
            }

            this.TestAndIsolateCompliance = this.compliance;
            

        }

        /// <summary>
        /// Undoes test and isolate - inputs need to match the original test and isolate
        /// </summary>
        public void CancelTestAndIsolate(bool goodTest, int quarantinePeriod, int testQuantity)
        {
            this.GDP /= 0.95;

            //interactivity determined by the test quality and how many tests there are 
            if (goodTest)
            {
                this._interactivity /= this.TestAndIsolateCompliance * 0.1 * 0.7 * ((double)testQuantity / this.TotalPopulation);
            } else
            {
                this._interactivity /= this.TestAndIsolateCompliance * 0.1 * 0.4 * ((double)testQuantity / this.TotalPopulation);
            }

        }

        ///<summary>
        /// Stay at home order - lowers interactivity within a node by a preset amount, contains a cost to compliance
        /// </summary>
        public void StayAtHomeOrder()
        {
            this._interactivity *= this.compliance * 0.9; //preset to 0.9, make it changeable?
            ChangeComplianceModifier(-0.1);
            this.GDP *= 0.95; //arbitrary GDP cost chosen
            this.StayAtHomeCompliance = this.compliance;
            this.StayAtHomeBool = true;
        }

        ///<summary>
        /// Undoes stay at home order
        /// </summary>
        public void CancelStayAtHomeOrder()
        {
            this._interactivity /= this.StayAtHomeCompliance * 0.9;
            ChangeComplianceModifier(0.1);
            this.GDP /= 0.95;
            this.StayAtHomeBool = false;
        }

        ///<summary>
        /// Closes schools - lowers interactivity based on how much of the population is school age (5-17) - full compliance
        /// </summary>
        public void CloseSchools()
        {
            this._interactivity *= (this.demographics[1]) * 0.1) + (1 - this.demographics);
            this.GDP *= 0.95;
            ChangeComplianceModifier(-0.1);
            this.publicOpinion *= 0.9;
        }

        ///<summary>
        /// Cancels close schools
        /// </summary>
        public void CancelCloseSchools()
        {
            this._interactivity /= (this.demographics[1] * 0.1) + (1- this.demographics);
            this.GDP /= 0.95;
            ChangeComplianceModifier(0.1);
            this.publicOpinion /= 0.9;
        }   

        ///<summary>
        /// Closes recreational areas
        /// </summary>
        public void CloseRecreationalAreas()
        {
            this._interactivity *= this.compliance * 0.7;
            this.GDP *= 0.95;
            ChangeComplianceModifier(-0.1);
            this.publicOpinion *= 0.9;
            this.CloseRecreationalAreasCompliance = this.compliance;
            this.CloseRecAreaBool = true;
        }

        ///<summary>
        /// Cancels close recreational areas
        /// </summary>
        public void CancelCloseRecreationalAreas()
        {
            this._interactivity /= this.CloseRecreationalAreasCompliance * 0.7;
            this.GDP /= 0.95;
            ChangeComplianceModifier(0.1);
            this.publicOpinion /= 0.9;
            this.CloseRecAreaBool = false;

        }

        ///<summary>
        /// Shields at risk groups - interactivty lowered depending on proportional of 65+
        /// </summary>
        public void ShieldingProgram()
        {
            double vulnerable = this.demographics[6] + this.demographics[7] + this.demographics[8];
            this._interactivity *= (this.compliance * vulnerable * 0.5) + (1-vulnerable);
            ChangeComplianceModifier(-0.1);
            this.ShieldingProgramCompliance = this.compliance;
        }

        ///<summary>
        /// Undoes shielding program
        /// </summary>
        public void CancelShieldingProgram()
        {
            double vulnerable = this.demographics[6] + this.demographics[7] + this.demographics[8];
            this._interactivity /= (this.ShieldingProgramCompliance * vulnerable * 0.5) + (1-vulnerable);
            ChangeComplianceModifier(0.1);
        }

        ///<summary>
        /// Implements public opinion and gdp effects of movement restrictions
        /// </summary>
        public void MovementRestrictions()
        {
            this.publicOpinion *= 0.9;
            this.GDP *= 0.95;
        }

        ///<summary>
        /// Cancels effects of movement restrictions
        ///</summary>
        public void CancelMovementRestrictions()
        {
            this.publicOpinion /= 0.9;
            this.GDP /= 0.95;
        }

        ///<summary>
        /// Implements public opinion, gdp and compliance modifers effects of close borders
        ///</summary>
        public void CloseBorders()
        {
            this.publicOpinion *= 0.9;
            this.GDP *= 0.95;
            ChangeComplianceModifier(-0.1);
        }

        ///<summary>
        /// Cancels effects of close borders
        /// </summary>
        public void CancelCloseBorders()
        {
            this.publicOpinion /= 0.9;
            this.GDP /= 0.95;
            ChangeComplianceModifier(0.1);
        }

        ///<summary>
        /// Furlough scheme - lowers interactivity if enough to prevent people from moving raound for work
        /// </summary>
        public void FurloughScheme(int n)
        {
            if (n > 1000)
            {
                this._interactivity *= 0.9;
            }
            if (this.StayAtHomeBool & this.CloseRecAreaBool)
            {
                this.GDP *= 0.95;
            } else
            {
                this.GDP *= 0.9;
            }
        }

        ///<summary>
        /// Cancels furlough scheme
        /// </summary>
        public void CancelFurloughScheme(int n)
        {
            if (n > 10)
            {
                this._interactivity /= 0.9;
            }
        }

        ///<summary>
        /// Information press releases reduce interactivity and improves public opinion - one time action with permenant, diminishing value
        /// </summary>
        public void InformationPressRelease()
        {
            this._interactivity *= Math.Min(1, 0.6 + (this.numPressReleases * 0.05));
            this.publicOpinion = Math.Min(1, this.publicOpinion + (this.numPressReleases * 0.05));
        }

        ///<summary>
        /// Mask mandate reduces interactivity based on compliance, maskProvided, mask effectiveness (0-1)
        /// </summary>
        public void MaskMandate(bool maskProvided, double maskEffectiveness)
        {
            if (maskProvided)
            {
                this._interactivity *= this.compliance * (1 - maskEffectiveness);
                this.publicOpinion *= 0.8;
            } else 
            {
                this._interactivity *= this.compliance * 0.6;
                this.publicOpinion *= 0.5;
            }
            this.MaskMandateCompliance = this.compliance;
        }

        ///<summary>
        /// Cancels mask mandate
        /// </summary>
        public void CancelMaskMandate(bool maskProvided, double maskEffectiveness)
        {
            if (maskProvided)
            {
                this._interactivity /= this.MaskMandateCompliance * (1 - maskEffectiveness);
                this.publicOpinion /= 0.8;
            } else
            {
                this._interactivity /= this.MaskMandateCompliance * 0.6;
                this.publicOpinion /= 0.5;
            }
        }

        ///<summary>
        /// Health drive - reduces local lethality
        /// </summary>
        public void HealthDrive()
        {
            this.localLethality *= 0.95;
        }

        ///<summary>
        /// Cancels health drive
        /// </summary>
        public void CancelHealthDrive()
        {
            this.localLethality /= 0.95;
        }

        ///<summary>
        /// Social distancing - reduction in interactivity at the cost of public opinion and compliance
        /// </summary>
        public void SocialDistancing(double distance)
        {
            //TODO: add distance measure to virus - effectiveness based on distance measure in virus spreadability
            this._interactivity *= this.compliance * 0.5;
            this.publicOpinion *= 0.9;
            ChangeComplianceModifier(-0.1);
            this.SocialDistancingCompliance = this.compliance
        }

        ///<summary>
        /// Cancels social distancing program
        /// </summary>
        public void CancelSocialDistancing(double distance)
        {
            this._interactivity /= this.SocialDistancingCompliance * 0.5;
            this.publicOpinion /= 0.9;
            ChangeComplianceModifier(0.1);
        }

        ///<summary>
        /// Invests in health services for a reduction in lethality + increase in public opinion
        /// </summary>
        public void InvestInHealthServices(int investment)
        {
            this.localLethality -= (investment / 1000000) * 0.1;
            this.publicOpinion /= (investment / 1000000) * 0.9;
        }

        ///<summary>
        /// Cancles investment in health services
        /// </summary>
        public void CancelInvestInHealthServices(int investment)
        {
            this.localLethality += (investment / 1000000) * 0.1;
            this.publicOpinion *= (investment / 1000000) * 0.9;
        }

        ///<summary>
        /// Implements curfew - reduction in interactivity, at the cost of GDP if recreational areas are already closed and public opinion
        /// </summary>
        public void Curfew()
        {
            this._interactivity *= this.compliance * 0.8;
            this.publicOpinion *= 0.9;
            if (!CloseRecAreaBool)
            {
                this.GDP *= 0.95;
            }
            this.CurfewCompliance = this.compliance;
        }

        ///<summary>
        /// Cancels curfew
        /// </summary>
        public void CancelCurfew()
        {
            this._interactivity /= this.CurfewCompliance * 0.8;
            this.publicOpinion /= 0.9;
        }

        ///<summary>
        ///Administers vaccine to offer a permanent reduction in interactivity based on age group provided
        ///</summary>
        public void AdministerVaccine(int ageRange)
        {
            //should maybe change populations but with current implementation thats a little awkward and I couldn't get it to work well
            this._interactivity = Math.Max(0, this._interactivity - (this._interactivity * this.demographics[ageRange]));
            //TODO: add serious probability to vaccine for effect
            //TODO: add vaccine compliancy 
        }
    }
}
