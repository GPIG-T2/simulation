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
        #region Constants

        #region Test and Isolate

        public const double GoodTestEfficacy = 0.7;
        public const double BadTestEfficacy = 0.5;

        public const double TestAndIsolateGdp = 0.95;

        public const double CancelTestAndIsolateGdp = 1 / 0.97;

        #endregion

        #region Stay at Home

        public const double StayAtHomeEfficacy = 0.05;
        public const double StayAtHomeCompliance = -0.1;
        public const double StayAtHomeGdp = 0.95;

        public const double CancelStayAtHomeCompliance = 0.07;
        public const double CancelStayAtHomeGdp = 1 / 0.97;

        #endregion

        #region Close Schools

        public const double CloseSchoolsEfficacy = 0.3;
        public const double CloseSchoolsCompliance = -0.1;
        public const double CloseSchoolsPublicOpinion = 0.9;
        public const double CloseSchoolsGdp = 0.95;

        public const double CancelCloseSchoolsCompliance = 0.07;
        public const double CancelCloseSchoolsPublicOpinion = 0.93;
        public const double CancelCloseSchoolsGdp = 1 / 0.97;

        #endregion

        #region Close Recreational Areas

        public const double CloseRecreationalAreasEfficacy = 0.6;
        public const double CloseRecreationalAreasCompliance = -0.1;
        public const double CloseRecreationalAreasPublicOpinion = 0.9;
        public const double CloseRecreationalAreasGdp = 0.95;

        public const double CancelCloseRecreationalAreasCompliance = 0.07;
        public const double CancelCloseRecreationalAreasPublicOpinion = 0.93;
        public const double CancelCloseRecreationalAreasGdp = 1 / 0.97;

        #endregion

        #region Shielding Program

        public const double ShieldingProgramEfficacy = 0.2;
        public const double ShieldingProgramCompliance = -0.1;

        public const double CancelShieldingProgramCompliance = 0.07;

        #endregion

        #region Movement Restrictions

        public const double MovementRestrictionEfficacy = 0.4;
        public const double MovementRestrictionPublicOpinion = 0.9;
        public const double MovementRestrictionGdp = 0.95;

        public const double CancelMovementRestrictionPublicOpinion = 0.93;
        public const double CancelMovementRestrictionGdp = 1 / 0.97;

        #endregion

        #region Close Borders

        public const double CloseBorderPublicOpinion = 0.9;
        public const double CloseBorderCompliance = -0.1;
        public const double CloseBorderGdp = 0.95;

        public const double CancelCloseBorderPublicOpinion = 0.93;
        public const double CancelCloseBorderCompliance = 0.07;
        public const double CancelCloseBorderGdp = 1 / 0.97;

        #endregion

        #region Furlough

        public const double FurloughMaxEffectiveMoney = 3000;
        public const double FurloughBestEfficacy = 0.4;
        public const double FurloughBetterGDP = 0.95;
        public const double FurloughWorseGDP = 0.90;

        #endregion

        #region Information Press Release

        public const double InformationPressReleaseBestEfficacy = 0.6;
        public const double InformationPressReleaseDiminishmentRate = 0.05;
        public const double InformationPressReleaseBestPublicOpinionBoost = 0.5;

        #endregion

        #region Mask Mandate

        public const double MaskMandateProvidedPublicOpinion = 0.7;
        public const double MaskMandateUnprovidedPublicOpinion = 0.4;

        public const double CancelMaskMandateProvidedPublicOpinion = 0.75;
        public const double CancelMaskMandateUnprovidedPublicOpinion = 0.5;

        #endregion

        #region Health Drive

        public const double HealthDriveLethalityModifier = 0.9;

        public const double CancelHealthDriveLethalityModifier = 0.95;

        #endregion

        #region Social Distancing

        public const double SocialDistancingEfficacy = 0.7;
        public const double SocialDistancingPublicOpinion = 0.9;
        public const double SocialDistancingComplianceChange = -0.1;

        public const double CancelSocialDistancingPublicOpinion = 0.95;
        public const double CancelSocialDistancingComplianceChange = 0.07;

        #endregion

        #region Invest In Health Services

        public const double InvestInHealthServicesMaximumEffectiveInvestment = 10_000_000;
        public const double InvestInHealthServicesLethality = 0.9;
        public const double InvestInHealthServicesPublicOpinion = 0.7;

        #endregion

        #region Curfew

        public const double CurfewEfficacy = 0.3;
        public const double CurfewPublicOpinion = 0.5;
        public const double CurfewGDP = 0.9;
        public const double CancelCurfewPublicOpinion = 0.9;

        #endregion

        public const int InfectiousWait = 2;
        public const int SympomaticWait = 4;
        public const int SeriousWait = 6;
        public const int DeadWait = 8;

        #endregion

        public int Index { get; }
        public List<string> Location { get; }
        public long TotalPopulation { get; set; }
        public Models.InfectionTotals Totals { get; }
        public Models.Coordinate Position { get; }
        public string Name { get; set; }

        private readonly Random _random = new();
        private readonly long _startPopulation;
        private readonly CycleQueue<long> _asympHistory = new(14); // infections last 14 days
        private readonly CycleQueue<long> _sympHistory = new(14);
        private readonly CycleQueue<long> _seriousHistory = new(14);
        private Demographics _interactivity;
        private double _interactivityModifier = 1;

        private bool _testing = false;
        private bool _sympTesting = false;
        private long _goodTests = 0;
        private long _badTests = 0;
        public long TestsAdministered { get; set; } = 0;
        public long PositiveTests { get; set; } = 0;
        private readonly CycleQueue<long> _isolationHistory = new(14);
        private readonly CycleQueue<long> _falseIsolationHistory = new(14);
        private long _testingCapacity;
        private long _isolated = 0;
        private long _falseIsolated = 0;

        // variables for WHO effects and effectiveness
        public long Gdp { get; set; }
        public Demographics NodeDemographics { get; set; } // list of proportions following {<5, 5-17, 18-29, 30-9, 40-9, 50-64, 65-74, 75-84, 85+}
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
        private double _movementRestrictionsCompliance = 0.99;
        private double _maskMandateCompliance = 0.8;
        private double _healthDriveCompliance = 0.8;
        private double _socialDistancingCompliance = 0.8;
        private double _curfewCompliance = 0.8;

        // used to see if certain schemes are active
        private bool _stayAtHomeBool = false;
        private bool _closeRecArea = false;

        public Node(int index, long population, Demographics interactivity, string name, Models.Coordinate position, Demographics demographics, long gdp, long testingCapacity)
        {
            this.Index = index;
            this.Location = new List<string> { $"N{index}" };
            this._startPopulation = population;
            this.TotalPopulation = population;
            this.Totals = new Models.InfectionTotals(this.Location, population, 0, 0, 0, 0, 0, 0);

            this._interactivity = interactivity;
            this.Name = name;
            this.Position = position;
            this.NodeDemographics = demographics;
            this.Gdp = gdp;
            this._testingCapacity = testingCapacity;
        }

        /// <summary>
        /// Takes current populations and updates to form new set of populations
        /// based off given virus, interactivity within the node.
        /// </summary>
        /// <param name="virus"></param>
        public void Update(Virus virus)
        {
            //first applies tests - with test and isolate a portion need to be removed from the infectious population
            long turnIsolated = 0;
            long falsePositives = 0;
            if (this._testing)
            {
                //testing only symptomatic people vs whole population - ignores people who are symptomatic but not infected (untracked)
                if (this._sympTesting)
                {
                    //will use all goodtests first before resorting to bad tests
                    if (this._goodTests > this._testingCapacity)
                    {
                        turnIsolated += (long)((this.Totals.Symptomatic / (double)this.TotalPopulation) * GoodTestEfficacy * this._testingCapacity);
                        this.PositiveTests += turnIsolated;
                        this.TestsAdministered += this._testingCapacity;
                        this._goodTests -= this._testingCapacity;
                    }
                    else if ((this._badTests + this._goodTests) > this._testingCapacity)
                    {
                        turnIsolated += (long)((this.Totals.Symptomatic / (double)this.TotalPopulation) * GoodTestEfficacy * this._goodTests);
                        turnIsolated += (long)((this.Totals.Symptomatic / (double)this.TotalPopulation) * BadTestEfficacy * (this._testingCapacity - this._goodTests));
                        this.PositiveTests += turnIsolated;
                        this.TestsAdministered += this._testingCapacity;
                        this._badTests -= this._testingCapacity - this._goodTests;
                        this._goodTests = 0;
                    }
                    else
                    {
                        turnIsolated += (long)((this.Totals.Symptomatic / (double)this.TotalPopulation) * GoodTestEfficacy * this._goodTests);
                        turnIsolated += (long)((this.Totals.Symptomatic / (double)this.TotalPopulation) * BadTestEfficacy * this._badTests);
                        this.PositiveTests += turnIsolated;
                        this.TestsAdministered += this._goodTests + this._badTests;
                        this._badTests = 0;
                        this._goodTests = 0;
                    }
                }
                else
                {
                    if (this._goodTests > this._testingCapacity)
                    {
                        turnIsolated += (long)(((this.TotalPopulation - this.Totals.Uninfected - this.Totals.RecoveredImmune) / (double)this.TotalPopulation) * GoodTestEfficacy * this._testingCapacity);
                        falsePositives += (long)(((this.Totals.Uninfected + this.Totals.RecoveredImmune) / (double)this.TotalPopulation) * (1 - GoodTestEfficacy) * this._testingCapacity);
                        this.TestsAdministered += this._testingCapacity;
                        this.PositiveTests += turnIsolated + falsePositives;
                        this._goodTests -= this._testingCapacity;
                    }
                    else if ((this._badTests + this._goodTests) > this._testingCapacity)
                    {
                        turnIsolated += (long)(((this.TotalPopulation - this.Totals.Uninfected - this.Totals.RecoveredImmune) / (double)this.TotalPopulation) * GoodTestEfficacy * this._goodTests);
                        falsePositives += (long)(((this.Totals.Uninfected + this.Totals.RecoveredImmune) / (double)this.TotalPopulation) * (1 - GoodTestEfficacy) * this._goodTests);
                        turnIsolated += (long)(((this.TotalPopulation - this.Totals.Uninfected - this.Totals.RecoveredImmune) / (double)this.TotalPopulation) * BadTestEfficacy * (this._testingCapacity - this._goodTests));
                        falsePositives += (long)(((this.Totals.Uninfected + this.Totals.RecoveredImmune) / (double)this.TotalPopulation) * (1 - BadTestEfficacy) * (this._testingCapacity - this._goodTests));
                        this.TestsAdministered += this._testingCapacity;
                        this.PositiveTests += turnIsolated + falsePositives;
                        this._badTests -= this._testingCapacity - this._goodTests;
                        this._goodTests = 0;
                    }
                    else
                    {
                        turnIsolated += (long)(((this.TotalPopulation - this.Totals.Uninfected - this.Totals.RecoveredImmune) / (double)this.TotalPopulation) * GoodTestEfficacy * this._goodTests);
                        falsePositives += (long)(((this.Totals.Uninfected + this.Totals.RecoveredImmune) / (double)this.TotalPopulation) * (1 - GoodTestEfficacy) * this._goodTests);
                        turnIsolated += (long)(((this.TotalPopulation - this.Totals.Uninfected - this.Totals.RecoveredImmune) / (double)this.TotalPopulation) * BadTestEfficacy * this._badTests);
                        falsePositives += (long)(((this.Totals.Uninfected + this.Totals.RecoveredImmune) / (double)this.TotalPopulation) * (1 - BadTestEfficacy) * this._badTests);
                        this.TestsAdministered += this._goodTests + this._badTests;
                        this.PositiveTests += turnIsolated + falsePositives;
                        this._badTests = 0;
                        this._goodTests = 0;
                    }
                }
            }

            //adds people isolated this turn to total isolation numbers + isolation history
            turnIsolated = (long)(this._compliance * turnIsolated);
            falsePositives = (long)(this._compliance * falsePositives);
            this._isolated -= this._isolationHistory.Front;
            this._falseIsolated -= this._falseIsolationHistory.Front;
            this._isolated += turnIsolated;
            this._falseIsolated += falsePositives;
            this._isolationHistory.Push(turnIsolated);
            this._falseIsolationHistory.Push(falsePositives);

            // finds the total number of infectious - could be separated for different interactivities
            long totalInfectious = this.Totals.AsymptomaticInfectedInfectious
                + this.Totals.Symptomatic
                + this.Totals.SeriousInfection
                - this._isolated;

            // find the aggregate interactivity by multiplying demographic interactivities by node demographics
            double aggregateInteractivity = this.AggregateDemographics(this._interactivity);

            // infectiousness (infectious interactions) decided by the number of infectious people, the portion of the population which can be infected, and the interactivity of the node
            // TODO: Unsure if uninfected/totalPopulation is appropriate for this utiliziation - may need specific statstical method
            double infectiousness = totalInfectious
                * Extensions.Sigmoid(2.6, 10, ((double)this.Totals.Uninfected - this._falseIsolated) / (double)this.TotalPopulation)
                * aggregateInteractivity * this._interactivityModifier;
            infectiousness *= this.AggregateDemographics(virus.Infectivity); //multiplies interactions * infectivity to get total number of people infected
            // the infectiousness is the number of people infected + the chance of 1 more
            long infected = (long)Math.Floor(infectiousness);

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
            int aUninf2InfOffset = -InfectiousWait;
            int aInf2SympOffset = -SympomaticWait;

            // following 2 days, all asympUninf go to asympInf
            this.Totals.AsymptomaticInfectedNotInfectious -= this._asympHistory[aUninf2InfOffset];
            this.Totals.AsymptomaticInfectedInfectious += this._asympHistory[aUninf2InfOffset];

            // follwing 4 days (2 days after asympUninf -> asympInf) a portion of the asympInf move to symp depdendent on symptomaticity
            long aInf2Symp = (long)Math.Floor((double)this._asympHistory[aInf2SympOffset]
                * this.AggregateDemographics(virus.Symptomaticity)); //rounds to 0 under 1
            this.Totals.AsymptomaticInfectedInfectious -= aInf2Symp;
            this.Totals.Symptomatic += aInf2Symp;
            // moves histories
            this._sympHistory[aInf2SympOffset] += aInf2Symp;
            this._asympHistory[aInf2SympOffset] -= aInf2Symp;

            // symp (2 day min) -> serious (2 day min) 
            int symp2SeriousOffset = -SeriousWait;
            int serious2DeadOffset = -DeadWait;

            // a portion (25% - should be variable) of symptomatic go to serious after 2 days
            // TODO - replace 0.5 with proper virus variable
            long symp2Serious = (long)Math.Floor((double)this._sympHistory[symp2SeriousOffset]
                * this.AggregateDemographics(virus.SeriousRate));
            this.Totals.Symptomatic -= symp2Serious;
            this.Totals.SeriousInfection += symp2Serious;
            this._sympHistory[symp2SeriousOffset] -= symp2Serious;
            this._seriousHistory[symp2SeriousOffset] += symp2Serious;

            // a portion of serious cases result in death - depdenent on virus fatality rate
            long serious2Dead = (long)Math.Floor((double)this._seriousHistory[serious2DeadOffset]
                * this.AggregateDemographics(virus.Fatality) * this._localLethality);
            this.Totals.SeriousInfection -= serious2Dead;
            this.Totals.Dead += serious2Dead;
            this.TotalPopulation -= serious2Dead; //removes dead people from the current population
            this._seriousHistory[serious2DeadOffset] -= serious2Dead; //removed from history for recovery tracking

            // at the end of the 14 day period - everyone infected 14 days ago still alive recovers
            long asymp2Recovered = this._asympHistory[1];
            long symp2Recovered = this._sympHistory[1];
            long serious2Recovered = this._seriousHistory[1];
            this.Totals.AsymptomaticInfectedInfectious -= asymp2Recovered;
            this.Totals.Symptomatic -= symp2Recovered;
            this.Totals.SeriousInfection -= serious2Recovered;
            this.Totals.RecoveredImmune += asymp2Recovered + symp2Recovered + serious2Recovered;

            // moves a proportion of recovered to uninfected based on virus reinfectivity
            long reinfections = (long)Math.Floor(this.Totals.RecoveredImmune * this.AggregateDemographics(virus.Reinfectivity));
            this.Totals.RecoveredImmune -= reinfections;
            this.Totals.Uninfected += reinfections;

            //changes complaince by the compliance modifier
            this._compliance = this._baseCompliance * this._complianceModifiers;

            // Increment head
            this._asympHistory.Push(0);
            this._sympHistory.Push(0);
            this._seriousHistory.Push(0);
        }

        /// <summary>
        /// Infects a given number of people by taking them from uninfected to asympUninf
        /// </summary>
        /// <param name="pop"></param>
        public void Infect(long pop)
        {
            if (pop > this.Totals.Uninfected)
            {
                pop = this.Totals.Uninfected;
            }
            this.Totals.Uninfected -= pop;
            this._asympHistory.Front += pop;
            this.Totals.AsymptomaticInfectedNotInfectious += pop;
        }

        /// <summary>
        /// Vaccinates a given number of people, taking them from uninfected to recovered immune
        /// </summary>
        /// <param name="pop"></param>
        private void VaccinatePeople(long pop)
        {
            if (pop > this.Totals.Uninfected)
            {
                pop = this.Totals.Uninfected;
            }
            this.Totals.Uninfected -= pop;
            this.Totals.RecoveredImmune += pop;
        }

        /// <summary>
        /// Gets an average value by applying across all demographics
        /// </summary>
        /// <param name="demographics"></param>
        /// <returns>An aggregate average value taking into account demographic makeup</returns>
        private double AggregateDemographics(Demographics demographics)
        {
            return this.NodeDemographics.UnderFive * demographics.UnderFive
                + this.NodeDemographics.FiveToSeventeen * demographics.FiveToSeventeen
                + this.NodeDemographics.EighteenToTwentyNine * demographics.EighteenToTwentyNine
                + this.NodeDemographics.ThirtyToThirtyNine * demographics.ThirtyToThirtyNine
                + this.NodeDemographics.FourtyToFourtyNine * demographics.FourtyToFourtyNine
                + this.NodeDemographics.FiftyToSixtyFour * demographics.FiftyToSixtyFour
                + this.NodeDemographics.SixtyFiveToSeventyFour * demographics.SixtyFiveToSeventyFour
                + this.NodeDemographics.SeventyFiveToEightyFour * demographics.SeventyFiveToEightyFour
                + this.NodeDemographics.OverEightyFive * demographics.OverEightyFive;
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
        public void TestAndIsolate(bool goodTest, int quarantinePeriod, long testQuantity, bool sympTest)
        {
            // TODO: implementation of testing/quarantine - unsure on how to implement quarantine period with current model
            this.ModifyGdp(TestAndIsolateGdp);
            this._testing = true;
            this._sympTesting = sympTest;
            if (goodTest)
            {
                this._goodTests += testQuantity;
            }
            else
            {
                this._badTests += testQuantity;
            }

            this._testAndIsolateCompliance = this._compliance;
        }

        /// <summary>
        /// Undoes test and isolate - inputs need to match the original test and isolate
        /// </summary>
        public void CancelTestAndIsolate(bool goodTest, int quarantinePeriod, long testQuantity, bool sympTest)
        {
            this.ModifyGdp(CancelTestAndIsolateGdp);
            this._testing = false;
        }

        /// <summary>
        /// Stay at home order - lowers interactivity within a node by a preset amount, contains a cost to compliance
        /// </summary>
        public void StayAtHomeOrder()
        {
            this._interactivityModifier *= StayAtHomeEfficacy / this._compliance;
            this.ChangeComplianceModifier(StayAtHomeCompliance);
            this.ModifyGdp(StayAtHomeGdp); // arbitrary GDP cost chosen
            this._stayAtHomeCompliance = this._compliance;
            this._stayAtHomeBool = true;
        }

        /// <summary>
        /// Undoes stay at home order
        /// </summary>
        public void CancelStayAtHomeOrder()
        {
            this._interactivityModifier /= StayAtHomeEfficacy / this._stayAtHomeCompliance;
            this.ChangeComplianceModifier(CancelStayAtHomeCompliance);
            this.ModifyGdp(CancelStayAtHomeGdp);
            this._stayAtHomeBool = false;
        }

        /// <summary>
        /// Closes schools - lowers interactivity based on how much of the population is school age (5-17) - full compliance
        /// </summary>
        public void CloseSchools()
        {
            this._interactivity.FiveToSeventeen *= CloseSchoolsEfficacy;
            this.ModifyGdp(CloseSchoolsGdp);
            this.ChangeComplianceModifier(CloseSchoolsCompliance);
            this.PublicOpinion *= CloseSchoolsPublicOpinion;
        }

        /// <summary>
        /// Cancels close schools
        /// </summary>
        public void CancelCloseSchools()
        {
            this._interactivity.FiveToSeventeen /= CloseSchoolsEfficacy;
            this.ModifyGdp(CancelCloseSchoolsGdp);
            this.ChangeComplianceModifier(CancelCloseSchoolsCompliance);
            this.PublicOpinion /= CancelCloseSchoolsPublicOpinion;
        }

        /// <summary>
        /// Closes recreational areas
        /// </summary>
        public void CloseRecreationalAreas()
        {
            this._interactivityModifier *= CloseRecreationalAreasEfficacy / this._compliance;
            this.ModifyGdp(CloseRecreationalAreasGdp);
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
            this._interactivityModifier /= CloseRecreationalAreasEfficacy / this._closeRecreationalAreasCompliance;
            this.ModifyGdp(CancelCloseRecreationalAreasGdp);
            this.ChangeComplianceModifier(CancelCloseRecreationalAreasCompliance);
            this.PublicOpinion /= CancelCloseRecreationalAreasPublicOpinion;
            this._closeRecArea = false;

        }

        /// <summary>
        /// Shields at risk groups - lowers the demographic value of 65+ so they have less effect overall
        /// </summary>
        public void ShieldingProgram()
        {
            this.NodeDemographics.SixtyFiveToSeventyFour *= ShieldingProgramEfficacy / this._compliance;
            this.NodeDemographics.SeventyFiveToEightyFour *= ShieldingProgramEfficacy / this._compliance;
            this.NodeDemographics.OverEightyFive *= ShieldingProgramEfficacy / this._compliance;
            this.ChangeComplianceModifier(ShieldingProgramCompliance);
            this._shieldingProgramCompliance = this._compliance;
        }

        /// <summary>
        /// Undoes shielding program
        /// </summary>
        public void CancelShieldingProgram()
        {
            this._interactivity.SixtyFiveToSeventyFour /= ShieldingProgramEfficacy / this._shieldingProgramCompliance;
            this._interactivity.SeventyFiveToEightyFour /= ShieldingProgramEfficacy / this._shieldingProgramCompliance;
            this._interactivity.OverEightyFive /= ShieldingProgramEfficacy / this._shieldingProgramCompliance;
            this.ChangeComplianceModifier(CancelShieldingProgramCompliance);
        }

        /// <summary>
        /// Implements public opinion and gdp effects of movement restrictions
        /// </summary>
        public void MovementRestrictions()
        {
            this._interactivityModifier *= MovementRestrictionEfficacy / this._movementRestrictionsCompliance;
            this.PublicOpinion *= MovementRestrictionPublicOpinion;
            this.ModifyGdp(MovementRestrictionGdp);
        }

        /// <summary>
        /// Cancels effects of movement restrictions
        /// </summary>
        public void CancelMovementRestrictions()
        {
            this._interactivityModifier /= MovementRestrictionEfficacy / this._movementRestrictionsCompliance;
            this.PublicOpinion /= CancelMovementRestrictionGdp;
            this.ModifyGdp(CancelMovementRestrictionPublicOpinion);
        }

        /// <summary>
        /// Implements public opinion, gdp and compliance modifers effects of close borders
        /// </summary>
        public void CloseBorders()
        {
            this.PublicOpinion *= CloseBorderPublicOpinion;
            this.ModifyGdp(CloseBorderGdp);
            this.ChangeComplianceModifier(CloseBorderCompliance);
        }

        /// <summary>
        /// Cancels effects of close borders
        /// </summary>
        public void CancelCloseBorders()
        {
            this.PublicOpinion /= CancelCloseBorderPublicOpinion;
            this.ModifyGdp(CancelCloseBorderGdp);
            this.ChangeComplianceModifier(CancelCloseBorderCompliance);
        }

        /// <summary>
        /// Furlough scheme - lowers interactivity in working age (assumes working age ends at average age of retirement - 65) depending on how much is given
        /// </summary>
        public void FurloughScheme(int n)
        {
            double efficacy = 1 - (Math.Min(n / FurloughMaxEffectiveMoney, 1) * (1 - FurloughBestEfficacy));
            this._interactivity.EighteenToTwentyNine *= efficacy;
            this._interactivity.ThirtyToThirtyNine *= efficacy;
            this._interactivity.FourtyToFourtyNine *= efficacy;
            this._interactivity.FiftyToSixtyFour *= efficacy;

            this.ModifyGdp(this._stayAtHomeBool & this._closeRecArea ? FurloughBetterGDP : FurloughWorseGDP);
        }

        /// <summary>
        /// Cancels furlough scheme
        /// </summary>
        public void CancelFurloughScheme(int n)
        {
            double efficacy = 1 - (Math.Min(n / FurloughMaxEffectiveMoney, 1.0) * (1 - FurloughBestEfficacy));
            this._interactivity.EighteenToTwentyNine /= efficacy;
            this._interactivity.ThirtyToThirtyNine /= efficacy;
            this._interactivity.FourtyToFourtyNine /= efficacy;
            this._interactivity.FiftyToSixtyFour /= efficacy;
        }

        /// <summary>
        /// Information press releases reduce interactivity and improves public opinion - one time action with permenant, diminishing value
        /// </summary>
        public void InformationPressRelease()
        {
            this._interactivityModifier *= Math.Min(1, InformationPressReleaseBestEfficacy + (this.NumPressReleases * InformationPressReleaseDiminishmentRate));
            this.PublicOpinion = Math.Min(1, this.PublicOpinion + InformationPressReleaseBestPublicOpinionBoost - Math.Max((this.NumPressReleases * InformationPressReleaseDiminishmentRate), 0.5));
        }

        /// <summary>
        /// Mask mandate reduces interactivity based on compliance, maskProvided, mask effectiveness (0-1)
        /// </summary>
        public void MaskMandate(bool maskProvided, double maskEffectiveness)
        {
            this._interactivityModifier *= (1 - maskEffectiveness) / this._compliance;
            if (maskProvided)
            {
                this.PublicOpinion *= MaskMandateProvidedPublicOpinion;
            }
            else
            {
                this.PublicOpinion *= MaskMandateUnprovidedPublicOpinion;
            }
            this._maskMandateCompliance = this._compliance;
        }

        /// <summary>
        /// Cancels mask mandate
        /// </summary>
        public void CancelMaskMandate(bool maskProvided, double maskEffectiveness)
        {
            this._interactivityModifier /= (1 - maskEffectiveness) / this._maskMandateCompliance;
            if (maskProvided)
            {
                this.PublicOpinion /= CancelMaskMandateProvidedPublicOpinion;
            }
            else
            {
                this.PublicOpinion /= CancelMaskMandateUnprovidedPublicOpinion;
            }
        }

        /// <summary>
        /// Health drive - reduces local lethality
        /// </summary>
        public void HealthDrive()
        {
            this._localLethality *= HealthDriveLethalityModifier;
        }

        /// <summary>
        /// Cancels health drive
        /// </summary>
        public void CancelHealthDrive()
        {
            this._localLethality /= CancelHealthDriveLethalityModifier;
        }

        /// <summary>
        /// Social distancing - reduction in interactivity at the cost of public opinion and compliance
        /// </summary>
        public void SocialDistancing(double distance)
        {
            //TODO: add distance measure to virus - effectiveness based on distance measure in virus spreadability
            this._interactivityModifier *= SocialDistancingEfficacy / this._compliance;
            this.PublicOpinion *= SocialDistancingPublicOpinion;
            this.ChangeComplianceModifier(SocialDistancingComplianceChange);
            this._socialDistancingCompliance = this._compliance;
        }

        /// <summary>
        /// Cancels social distancing program
        /// </summary>
        public void CancelSocialDistancing(double distance)
        {
            this._interactivityModifier /= SocialDistancingEfficacy / this._socialDistancingCompliance;
            this.PublicOpinion /= CancelSocialDistancingPublicOpinion;
            this.ChangeComplianceModifier(CancelSocialDistancingComplianceChange);
        }

        /// <summary>
        /// Invests in health services for a reduction in lethality + increase in public opinion
        /// </summary>
        public void InvestInHealthServices(int investment)
        {
            this._localLethality -= (investment / InvestInHealthServicesMaximumEffectiveInvestment) * InvestInHealthServicesLethality;
            this.PublicOpinion /= (investment / InvestInHealthServicesMaximumEffectiveInvestment) * InvestInHealthServicesPublicOpinion;
        }

        /// <summary>
        /// Cancles investment in health services
        /// </summary>
        public void CancelInvestInHealthServices(int investment)
        {
            this._localLethality += (investment / InvestInHealthServicesMaximumEffectiveInvestment) * InvestInHealthServicesLethality;
            this.PublicOpinion *= (investment / InvestInHealthServicesMaximumEffectiveInvestment) * InvestInHealthServicesPublicOpinion;
        }

        /// <summary>
        /// Implements curfew - reduction in interactivity, at the cost of GDP if recreational areas are already closed and public opinion
        /// </summary>
        public void Curfew()
        {
            this._interactivityModifier *= CurfewEfficacy / this._compliance;
            this.PublicOpinion *= CurfewPublicOpinion;
            if (!this._closeRecArea)
            {
                this.ModifyGdp(CurfewGDP);
            }
            this._curfewCompliance = this._compliance;
        }

        /// <summary>
        /// Cancels curfew
        /// </summary>
        public void CancelCurfew()
        {
            this._interactivityModifier /= CurfewEfficacy / this._curfewCompliance;
            this.PublicOpinion /= CancelCurfewPublicOpinion;
        }

        /// <summary>
        /// Vaccinates people of a specific age range, taking them directly from uninfected to recovered immmune
        /// Sets the demographic value to 0 to adjust aggregate virus rates
        /// </summary>
        public void AdministerVaccine(int ageRange)
        {
        }

        private void ModifyGdp(double ratio) => this.Gdp = (long)Math.Round(this.Gdp * ratio);

        public static explicit operator Models.LocationDefinition(Node node)
            => new(node.Location[0], node.Position, node.Name);
    }
}
