using System;
using System.Collections.Generic;

namespace Virus
{
    /// <summary>
    /// Representation of a single node within the system, i.e a geographic location.
    /// </summary>
    public class Node
    {
        // Effect constants - dictate how big effect certain actions should have - should probably be in config file
        public const double GoodTestEfficacy = 0.7;
        public const double BadTestEfficacy = 0.5;
        public const double TestAndIsolateGdp = 0.95;
        public const double CancelTestAndIsolateGdp = 0.97;

        public const double StayAtHomeEfficacy = 0.9;
        public const double StayAtHomeCompliance = -0.1;
        public const double StayAtHomeGdp = 0.95;
        public const double CancelStayAtHomeCompliance = 0.05;
        public const double CancelStayAtHomeGdp = 0.97;

        public const double CloseSchoolsEfficacy = 0.1;
        public const double CloseSchoolsGdp = 0.95;
        public const double CloseSchoolsCompliance = -0.1;
        public const double CloseSchoolsPublicOpinion = 0.9;
        public const double CancelCloseSchoolsGdp = 0.97;
        public const double CancelCloseSchoolsCompliance = 0.05;
        public const double CancelCloseSchoolsPublicOpinion = 0.93;

        public const double CloseRecreationalAreasEfficacy = 0.6;
        public const double CloseRecreationalAreasGdp = 0.95;
        public const double CloseRecreationalAreasCompliance = -0.1;
        public const double CloseRecreationalAreasPublicOpinion = 0.9;
        public const double CancelCloseRecreationalAreasGdp = 0.97;
        public const double CancelCloseRecreationalAreasCompliance = 0.05;
        public const double CancelCloseRecreationalAreasPublicOpinion = 0.93;

        public const double ShieldingProgramEfficacy = 0.5;
        public const double ShieldingProgramCompliance = -0.1;
        public const double CancelShieldingProgramCompliance = 0.05;

        public const double MovementRestrictionGdp = 0.95;
        public const double MovementRestrictionPublicOpinion = 0.9;
        public const double CancelMovementRestrictionGdp = 0.97;
        public const double CancelMovementRestrictionPublicOpinion = 0.93;

        public const double CloseBorderGdp = 0.95;
        public const double CloseBorderPublicOpinion = 0.9;
        public const double CloseBorderCompliance = -0.1;
        public const double CancelCloseBorderGdp = 0.97;
        public const double CancelCloseBorderPublicOpinion = 0.93;
        public const double CancelCloseBorderCompliance = 0.05;

        //TOO LAZY TO DO THE REST RIGHT NOW


        public int Index { get; }
        public List<string> Location { get; }
        public int TotalPopulation { get; set; }
        public Models.InfectionTotals Totals { get; }

        private readonly Random _random = new();
        private readonly int _startPopulation;
        private readonly int[] _asympHistory = new int[14]; // infections last 14 days
        private readonly int[] _sympHistory = new int[14];
        private readonly int[] _seriousHistory = new int[14];
        private double _interactivity;
        private int _historyHead = 0;

        // variables for WHO effects and effectiveness
        public double Gdp { get; set; } = 50;
        public double[] Demographics { get; set; } // list of proportions following {<5, 5-17, 18-29, 30-9, 40-9, 50-64, 65-74, 75-84, 85+}
            = new[] { 0.2, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1 };
        public double PublicOpinion { get; set; } = 1;
        public int NumPressReleases { get; set; } = 0;

        private readonly double _baseCompliance = 0.8;
        private double _compliance = 0.8; // between 0-1 as a modifier on WHO actions
        private double _complianceModifiers = 1; // used to track drops in compliance over time
        private double _localLethality = 1; // modifier on virus fatality for the local node

        // compliances used to remember compliances for specific policies - needed to undo
        private double _testAndIsolateCompliance = 0.8;
        private double _stayAtHomeCompliance = 0.8;
        private double _closeRecreationalAreasCompliance = 0.8;
        private double _shieldingProgramCompliance = 0.8;
        private double _maskMandateCompliance = 0.8;
        private double _healthDriveCompliance = 0.8;
        private double _socialDistancingCompliance = 0.8;
        private double _curfewCompliance = 0.8;

        // used to see if certain schemes are active
        private bool _stayAtHomeBool = false;
        private bool _closeRecArea = false;

        public Node(int index, int population, double interactivity)
        {
            this.Index = index;
            this.Location = new List<string> { $"N{index}" };
            this._startPopulation = population;
            this.TotalPopulation = population;
            this.Totals = new Models.InfectionTotals(this.Location, population, 0, 0, 0, 0, 0, 0);

            this._interactivity = interactivity;

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
            int serious2Dead = (int)Math.Floor((double)this._seriousHistory[serious2DeadHead] * virus.Fatality * _localLethality);
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
            this._compliance = this._baseCompliance * this._complianceModifiers;

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
        public void ChangeComplianceModifier(double change)
        {
            this._complianceModifiers += change;
            if (this._complianceModifiers > 1)
            {
                this._complianceModifiers = 1;
            }
            else if (this._complianceModifiers < 0)
            {
                this._complianceModifiers = 0;
            }

        }

        /// <summary>
        /// Does test and isolate within a node
        /// </summary>
        public void TestAndIsolate(bool goodTest, int quarantinePeriod, int testQuantity)
        {
            // TODO: implementation of testing/quarantine - unsure on how to implement quarantine period with current model
            this.Gdp *= TestAndIsolateGdp;

            // interactivity determined by the test quality and how many tests there are 
            if (goodTest)
            {
                this._interactivity *= this._compliance * GoodTestEfficacy * ((double)testQuantity / this.TotalPopulation);
            }
            else
            {
                this._interactivity *= this._compliance * BadTestEfficacy * ((double)testQuantity / this.TotalPopulation);
            }

            this._testAndIsolateCompliance = this._compliance;
        }

        /// <summary>
        /// Undoes test and isolate - inputs need to match the original test and isolate
        /// </summary>
        public void CancelTestAndIsolate(bool goodTest, int quarantinePeriod, int testQuantity)
        {
            this.Gdp /= CancelTestAndIsolateGdp;

            // interactivity determined by the test quality and how many tests there are 
            if (goodTest)
            {
                this._interactivity /= this._testAndIsolateCompliance * GoodTestEfficacy * ((double)testQuantity / this.TotalPopulation);
            }
            else
            {
                this._interactivity /= this._testAndIsolateCompliance * BadTestEfficacy * ((double)testQuantity / this.TotalPopulation);
            }
        }

        /// <summary>
        /// Stay at home order - lowers interactivity within a node by a preset amount, contains a cost to compliance
        /// </summary>
        public void StayAtHomeOrder()
        {
            this._interactivity *= this._compliance * StayAtHomeEfficacy; // preset to 0.9, make it changeable?
            this.ChangeComplianceModifier(StayAtHomeCompliance);
            this.Gdp *= StayAtHomeGdp; // arbitrary GDP cost chosen
            this._stayAtHomeCompliance = this._compliance;
            this._stayAtHomeBool = true;
        }

        /// <summary>
        /// Undoes stay at home order
        /// </summary>
        public void CancelStayAtHomeOrder()
        {
            this._interactivity /= this._stayAtHomeCompliance * StayAtHomeEfficacy;
            this.ChangeComplianceModifier(CancelStayAtHomeCompliance);
            this.Gdp /= CancelStayAtHomeGdp;
            this._stayAtHomeBool = false;
        }

        /// <summary>
        /// Closes schools - lowers interactivity based on how much of the population is school age (5-17) - full compliance
        /// </summary>
        public void CloseSchools()
        {
            this._interactivity *= (this.Demographics[1] * CloseSchoolsEfficacy) + (1 - this.Demographics[1]);
            this.Gdp *= CloseSchoolsGdp;
            this.ChangeComplianceModifier(CloseSchoolsCompliance);
            this.PublicOpinion *= CloseSchoolsPublicOpinion;
        }

        /// <summary>
        /// Cancels close schools
        /// </summary>
        public void CancelCloseSchools()
        {
            this._interactivity /= (this.Demographics[1] * CloseSchoolsEfficacy) + (1 - this.Demographics[1]);
            this.Gdp /= CancelCloseSchoolsGdp;
            this.ChangeComplianceModifier(CancelCloseSchoolsCompliance);
            this.PublicOpinion /= CancelCloseSchoolsPublicOpinion;
        }

        /// <summary>
        /// Closes recreational areas
        /// </summary>
        public void CloseRecreationalAreas()
        {
            this._interactivity *= this._compliance * CloseRecreationalAreasCompliance;
            this.Gdp *= CloseRecreationalAreasGdp;
            this.ChangeComplianceModifier(CloseRecreationalAreasCompliance);
            this.PublicOpinion *= CloseRecreationalAreasPublicOpinion;
            this._closeRecreationalAreasCompliance = this._compliance;
            this._closeRecArea = true;
        }

        /// <summary>
        /// Cancels close recreational areas
        /// </summary>
        public void CancelCloseRecreationalAreas()
        {
            this._interactivity /= this._closeRecreationalAreasCompliance * CloseRecreationalAreasEfficacy;
            this.Gdp /= CancelCloseRecreationalAreasGdp;
            this.ChangeComplianceModifier(CancelCloseRecreationalAreasCompliance);
            this.PublicOpinion /= CancelCloseRecreationalAreasPublicOpinion;
            this._closeRecArea = false;

        }

        /// <summary>
        /// Shields at risk groups - interactivty lowered depending on proportional of 65+
        /// </summary>
        public void ShieldingProgram()
        {
            double vulnerable = this.Demographics[6] + this.Demographics[7] + this.Demographics[8];
            this._interactivity *= (this._compliance * vulnerable * ShieldingProgramEfficacy) + (1 - vulnerable);
            this.ChangeComplianceModifier(ShieldingProgramCompliance);
            this._shieldingProgramCompliance = this._compliance;
        }

        /// <summary>
        /// Undoes shielding program
        /// </summary>
        public void CancelShieldingProgram()
        {
            double vulnerable = this.Demographics[6] + this.Demographics[7] + this.Demographics[8];
            this._interactivity /= (this._shieldingProgramCompliance * vulnerable * ShieldingProgramEfficacy) + (1 - vulnerable);
            this.ChangeComplianceModifier(CancelShieldingProgramCompliance);
        }

        /// <summary>
        /// Implements public opinion and gdp effects of movement restrictions
        /// </summary>
        public void MovementRestrictions()
        {
            this.PublicOpinion *= MovementRestrictionPublicOpinion;
            this.Gdp *= MovementRestrictionGdp;
        }

        /// <summary>
        /// Cancels effects of movement restrictions
        /// </summary>
        public void CancelMovementRestrictions()
        {
            this.PublicOpinion /= CancelMovementRestrictionGdp;
            this.Gdp /= CancelMovementRestrictionPublicOpinion;
        }

        /// <summary>
        /// Implements public opinion, gdp and compliance modifers effects of close borders
        /// </summary>
        public void CloseBorders()
        {
            this.PublicOpinion *= CloseBorderPublicOpinion;
            this.Gdp *= CloseBorderGdp;
            this.ChangeComplianceModifier(CloseBorderCompliance);
        }

        /// <summary>
        /// Cancels effects of close borders
        /// </summary>
        public void CancelCloseBorders()
        {
            this.PublicOpinion /= CancelCloseBorderPublicOpinion;
            this.Gdp /= CancelCloseBorderGdp;
            this.ChangeComplianceModifier(CancelCloseBorderCompliance);
        }

        /// <summary>
        /// Furlough scheme - lowers interactivity if enough to prevent people from moving raound for work
        /// </summary>
        public void FurloughScheme(int n)
        {
            if (n > 1000)
            {
                this._interactivity *= 0.9;
            }
            if (this._stayAtHomeBool & this._closeRecArea)
            {
                this.Gdp *= 0.95;
            }
            else
            {
                this.Gdp *= 0.9;
            }
        }

        /// <summary>
        /// Cancels furlough scheme
        /// </summary>
        public void CancelFurloughScheme(int n)
        {
            if (n > 10)
            {
                this._interactivity /= 0.9;
            }
        }

        /// <summary>
        /// Information press releases reduce interactivity and improves public opinion - one time action with permenant, diminishing value
        /// </summary>
        public void InformationPressRelease()
        {
            this._interactivity *= Math.Min(1, 0.6 + (this.NumPressReleases * 0.05));
            this.PublicOpinion = Math.Min(1, this.PublicOpinion + (this.NumPressReleases * 0.05));
        }

        /// <summary>
        /// Mask mandate reduces interactivity based on compliance, maskProvided, mask effectiveness (0-1)
        /// </summary>
        public void MaskMandate(bool maskProvided, double maskEffectiveness)
        {
            if (maskProvided)
            {
                this._interactivity *= this._compliance * (1 - maskEffectiveness);
                this.PublicOpinion *= 0.8;
            }
            else
            {
                this._interactivity *= this._compliance * 0.6;
                this.PublicOpinion *= 0.5;
            }
            this._maskMandateCompliance = this._compliance;
        }

        /// <summary>
        /// Cancels mask mandate
        /// </summary>
        public void CancelMaskMandate(bool maskProvided, double maskEffectiveness)
        {
            if (maskProvided)
            {
                this._interactivity /= this._maskMandateCompliance * (1 - maskEffectiveness);
                this.PublicOpinion /= 0.8;
            }
            else
            {
                this._interactivity /= this._maskMandateCompliance * 0.6;
                this.PublicOpinion /= 0.5;
            }
        }

        /// <summary>
        /// Health drive - reduces local lethality
        /// </summary>
        public void HealthDrive()
        {
            this._localLethality *= 0.95;
        }

        /// <summary>
        /// Cancels health drive
        /// </summary>
        public void CancelHealthDrive()
        {
            this._localLethality /= 0.95;
        }

        /// <summary>
        /// Social distancing - reduction in interactivity at the cost of public opinion and compliance
        /// </summary>
        public void SocialDistancing(double distance)
        {
            //TODO: add distance measure to virus - effectiveness based on distance measure in virus spreadability
            this._interactivity *= this._compliance * 0.5;
            this.PublicOpinion *= 0.9;
            this.ChangeComplianceModifier(-0.1);
            this._socialDistancingCompliance = this._compliance;
        }

        /// <summary>
        /// Cancels social distancing program
        /// </summary>
        public void CancelSocialDistancing(double distance)
        {
            this._interactivity /= this._socialDistancingCompliance * 0.5;
            this.PublicOpinion /= 0.9;
            this.ChangeComplianceModifier(0.1);
        }

        /// <summary>
        /// Invests in health services for a reduction in lethality + increase in public opinion
        /// </summary>
        public void InvestInHealthServices(int investment)
        {
            this._localLethality -= (investment / 1000000) * 0.1;
            this.PublicOpinion /= (investment / 1000000) * 0.9;
        }

        /// <summary>
        /// Cancles investment in health services
        /// </summary>
        public void CancelInvestInHealthServices(int investment)
        {
            this._localLethality += (investment / 1000000) * 0.1;
            this.PublicOpinion *= (investment / 1000000) * 0.9;
        }

        /// <summary>
        /// Implements curfew - reduction in interactivity, at the cost of GDP if recreational areas are already closed and public opinion
        /// </summary>
        public void Curfew()
        {
            this._interactivity *= this._compliance * 0.8;
            this.PublicOpinion *= 0.9;
            if (!this._closeRecArea)
            {
                this.Gdp *= 0.95;
            }
            this._curfewCompliance = this._compliance;
        }

        /// <summary>
        /// Cancels curfew
        /// </summary>
        public void CancelCurfew()
        {
            this._interactivity /= this._curfewCompliance * 0.8;
            this.PublicOpinion /= 0.9;
        }

        /// <summary>
        /// Administers vaccine to offer a permanent reduction in interactivity based on age group provided
        /// </summary>
        public void AdministerVaccine(int ageRange)
        {
            //should maybe change populations but with current implementation thats a little awkward and I couldn't get it to work well
            this._interactivity = Math.Max(0, this._interactivity - (this._interactivity * this.Demographics[ageRange]));
            //TODO: add serious probability to vaccine for effect
            //TODO: add vaccine compliancy 
        }
    }
}
