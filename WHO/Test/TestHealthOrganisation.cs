using Interface.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.Common;
using Models;
using Moq;
using Xunit;
using WHO.Tracking;

namespace WHO.Test
{
    [Collection("RequiresHealthOrganisation")]
    public class TestHealthOrganisation
    {

        public TestHealthOrganisation()
        {
            if (HealthOrganisation.Instance != null)
            {
                HealthOrganisation.SetInstanceForTestingOnly(null);
            }
        }

        [Fact]
        public void TestConstructor()
        {
            HealthOrganisation org = new(client: null);
            Assert.Equal(org, HealthOrganisation.Instance);
            Assert.Throws<InvalidOperationException>(() => new HealthOrganisation(client: null));
        }

        [Fact]
        public void TestFirstTurn()
        {
            Mock<IClient> clientMock = new();
            Mock<HealthOrganisation> healthOrgMock = new(clientMock.Object);
            clientMock.Setup(c => c.GetStatus()).Returns(Task.FromResult(new SimulationStatus(true, 0, 4)));
            clientMock.Setup(c => c.EndTurn()).Callback(() => healthOrgMock.Object.Stop()).Returns(Task.FromResult(new SimulationStatus(false, 0, 4)));
            clientMock.Setup(c => c.GetSettings()).Returns(Task.FromResult(new SimulationSettings(new SimulationSettings.SimulationTurnLength("second", 5), new() { new("A1") }, new SimulationSettings.SimulationEffectivenesses(new EffectivenessQuality(0.3f, 0.5f), new(), new EffectivenessQuality(0.5f, 0.3f)), new List<Edge>())));
        }

        [Fact]
        public void TestGetWhoActionWhenCreatingRestrictions()
        {
            HealthOrganisation org = new(client: null);
            LocationTracker locationTracker = new("A1", null);
            
            org.LocationTrackers.Add("A1", locationTracker);
            List<string> loc = new List<string>();
            loc.Add("A1");

            locationTracker.Track(new(loc, 100, 10, 10, 10, 10, 10, 20));

            List<Object> actions = org.getWhoActions(loc, ActionCostCalculator.ActionMode.Create, 100000000);
            Assert.Equal(15, actions.Count());
        }

        [Fact]
        public void TestGetWhoActionWhenDeletingRestrictions()
        {
            HealthOrganisation org = new(client: null);
            LocationTracker locationTracker = new("A1", null);

            org.LocationTrackers.Add("A1", locationTracker);
            List<string> loc = new List<string>();
            loc.Add("A1");

            locationTracker.Track(new(loc, 100, 10, 10, 10, 10, 10, 20));

            List<Object> actions = org.getWhoActions(loc, ActionCostCalculator.ActionMode.Delete, 100000000);
            Assert.Equal(15, actions.Count());
        }

        [Fact]
        public void TestCalculateBestActionWhenInfectionRateIsGreaterThanThresholdAndThereIsAnUnlimitedAmountOfMoney()
        {
            HealthOrganisation org = new(client: null);
            LocationTracker locationTrackerA1 = new("A1", null);
            LocationTracker locationTrackerAll = new("_all", null);

            org.LocationTrackers.Add("A1", locationTrackerA1);
            org.LocationTrackers.Add("_all", locationTrackerAll);

            List<string> loc = new List<string>();
            loc.Add("A1");

            locationTrackerA1.Track(new(loc, 100, 10, 10, 10, 10, 10, 20));
            locationTrackerAll.Track(new(loc, 1000, 100, 100, 100, 100, 100, 200));

            org.calculateBestAction(100000000, loc, 0);

            Assert.Equal(15, org._tasksToExecute.Count());
        }

        [Fact]
        public void TestCalculateBestActionWhenInfectionRateIsLessThanThresholdAndThereIsAnUnlimitedAmountOfMoney()
        {
            HealthOrganisation org = new(client: null);
            LocationTracker locationTrackerA1 = new("A1", null);
            LocationTracker locationTrackerAll = new("_all", null);

            org.LocationTrackers.Add("A1", locationTrackerA1);
            org.LocationTrackers.Add("_all", locationTrackerAll);

            List<string> loc = new List<string>();
            loc.Add("A1");

            locationTrackerA1.Track(new(loc, 100, 10, 10, 10, 10, 10, 20));
            locationTrackerAll.Track(new(loc, 1000, 100, 100, 100, 100, 100, 200));

            org.calculateBestAction(100000000, loc, 1);

            Assert.Equal(15, org._tasksToExecute.Count());
        }

        [Fact]
        public void TestCalculateBestActionWhenInfectionRateIsGreaterThanThresholdAndThereIsALimitedAmountOfMoney()
        {
            HealthOrganisation org = new(client: null);
            LocationTracker locationTrackerA1 = new("A1", null);
            LocationTracker locationTrackerAll = new("_all", null);

            org.LocationTrackers.Add("A1", locationTrackerA1);
            org.LocationTrackers.Add("_all", locationTrackerAll);

            List<string> loc = new List<string>();
            loc.Add("A1");

            locationTrackerA1.Track(new(loc, 100, 10, 10, 10, 10, 10, 20));
            locationTrackerAll.Track(new(loc, 1000, 100, 100, 100, 100, 100, 200));

            org.calculateBestAction(100, loc, 0);

            Assert.Equal(4, org._tasksToExecute.Count());
        }

        [Fact]
        public void TestCalculateBestActionWhenInfectionRateIsLessThanThresholdAndThereIsALimitedAmountOfMoney()
        {
            HealthOrganisation org = new(client: null);
            LocationTracker locationTrackerA1 = new("A1", null);
            LocationTracker locationTrackerAll = new("_all", null);

            org.LocationTrackers.Add("A1", locationTrackerA1);
            org.LocationTrackers.Add("_all", locationTrackerAll);

            List<string> loc = new List<string>();
            loc.Add("A1");

            locationTrackerA1.Track(new(loc, 100, 10, 10, 10, 10, 10, 20));
            locationTrackerAll.Track(new(loc, 1000, 100, 100, 100, 100, 100, 200));

            org.calculateBestAction(100, loc, 1);

            Assert.Equal(8, org._tasksToExecute.Count());
        }

    }
}
