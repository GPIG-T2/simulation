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
using Models.Parameters;

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
        public async void TestFirstTurn()
        {
            Mock<IClient> clientMock = new();
            Mock<HealthOrganisation> healthOrgMock = new(clientMock.Object);
            clientMock.SetupSequence(c => c.GetStatus()).Returns(Task.FromResult(new SimulationStatus(false, 0, 4))).Returns(Task.FromResult(new SimulationStatus(true, 0, 4)));
            clientMock.Setup(c => c.EndTurn()).Callback(() => healthOrgMock.Object.Stop()).Returns(Task.FromResult(new SimulationStatus(false, 0, 4)));
            LocationDefinition location = new("A1");
            location.SubLocations = new() { new("B2") };
            clientMock.Setup(c => c.GetSettings()).Returns(Task.FromResult(new SimulationSettings(new SimulationSettings.SimulationTurnLength("second", 5), new() { location }, new SimulationSettings.SimulationEffectivenesses(new EffectivenessQuality(0.3f, 0.5f), new(), new EffectivenessQuality(0.5f, 0.3f)), new List<Edge>())));
            clientMock.Setup(c => c.GetInfoTotals(It.Is<SearchRequest>((req) => req.Locations[0].Count == 1))).Returns(Task.FromResult(new List<InfectionTotals>() { new InfectionTotals(new() { "A1" }, 1, 2, 3, 4, 5, 6, 7) }));
            clientMock.Setup(c => c.GetInfoTotals(It.Is<SearchRequest>((req) => req.Locations[0].Count == 2))).Returns(Task.FromResult(new List<InfectionTotals>() { new InfectionTotals(new() { "A1", "B2" }, 2, 3, 4, 5, 6, 7, 8) }));
            await healthOrgMock.Object.Run();
            // 3 Because including the _all category
            Assert.Equal(3, healthOrgMock.Object.LocationTrackers.Count);
            Assert.Equal(2, healthOrgMock.Object.LocationStatuses.Count);

            Assert.True(healthOrgMock.Object.TriggerCount > 0);
            await healthOrgMock.Object.DisposeAsync();
        }

        [Fact]
        public async void TestAlreadyRunning()
        {
            Mock<IClient> clientMock = new();
            Mock<HealthOrganisation> healthOrgMock = new(clientMock.Object);
            healthOrgMock.Object.Running = true;
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await healthOrgMock.Object.Run());
        }

        [Fact]
        public async void TestSecondTurn()
        {
            int turn = 1;
            Mock<IClient> clientMock = new();
            Mock<HealthOrganisation> healthOrgMock = new(clientMock.Object);
            clientMock.Setup(c => c.GetStatus()).Returns(Task.FromResult(new SimulationStatus(true, 0, 4)));
            clientMock.Setup(c => c.EndTurn()).Callback(() => { if (++turn > 2) { healthOrgMock.Object.Running = false; } }).Returns(Task.FromResult(new SimulationStatus(false, 0, 4)));
            LocationDefinition location = new("A1");
            location.SubLocations = new() { new("B2") };
            clientMock.Setup(c => c.GetSettings()).Returns(Task.FromResult(new SimulationSettings(new SimulationSettings.SimulationTurnLength("second", 5), new() { location }, new SimulationSettings.SimulationEffectivenesses(new EffectivenessQuality(0.3f, 0.5f), new(), new EffectivenessQuality(0.5f, 0.3f)), new List<Edge>())));
            clientMock.Setup(c => c.GetInfoTotals(It.Is<SearchRequest>((req) => turn == 1 && req.Locations[0].Count == 1))).Returns(Task.FromResult(new List<InfectionTotals>() { new InfectionTotals(new() { "A1" }, 1, 2, 3, 4, 5, 6, 7) }));
            clientMock.Setup(c => c.GetInfoTotals(It.Is<SearchRequest>((req) => turn == 1 && req.Locations[0].Count == 2))).Returns(Task.FromResult(new List<InfectionTotals>() { new InfectionTotals(new() { "A1", "B2" }, 2, 3, 4, 5, 6, 7, 8) }));
            clientMock.Setup(c => c.GetInfoTotals(It.Is<SearchRequest>((req) => turn == 2 && req.Locations[0].Count == 1))).Returns(Task.FromResult(new List<InfectionTotals>() { new InfectionTotals(new() { "A1" }, 2, 3, 4, 5, 6, 7, 8) }));
            clientMock.Setup(c => c.GetInfoTotals(It.Is<SearchRequest>((req) => turn == 2 && req.Locations[0].Count == 2))).Returns(Task.FromResult(new List<InfectionTotals>() { new InfectionTotals(new() { "A1", "B2" }, 3, 4, 5, 6, 7, 8, 9) }));
            healthOrgMock.Object.AddLocalTrigger(new CustomTrigger(TrackingValue.Uninfected, (p) => p.CurrentParameterCount == 3, (l) => healthOrgMock.Object.TasksToExecute.Add(new WhoAction(0, new CloseSchools(l))), 0));
            clientMock.Setup(c => c.ApplyActions(It.Is<List<WhoAction>>(a => a[0].Id == 0 && Enumerable.SequenceEqual(a[0].Parameters.Location, new List<string>() { "A1", "B2" }) && a[0].Mode == "create"))).Returns(Task.FromResult<List<ActionResult>>(new() { new ActionResult(0, 200, "") })).Verifiable();
            await healthOrgMock.Object.Run();
            Assert.Equal(1, healthOrgMock.Object.LocationStatuses["A1B2"].ActionCount);
            Assert.Single(healthOrgMock.Object.LocationStatuses["A1B2"].GetActionsOfType(CloseSchools.ActionName));
            clientMock.VerifyAll();
        }

        [Fact]
        public async void TestGlobalTriggers()
        {
            int turn = 1;
            Mock<IClient> clientMock = new();
            Mock<HealthOrganisation> healthOrgMock = new(clientMock.Object);
            clientMock.Setup(c => c.GetStatus()).Returns(Task.FromResult(new SimulationStatus(true, 0, 4)));
            clientMock.Setup(c => c.EndTurn()).Callback(() => { if (++turn > 2) { healthOrgMock.Object.Running = false; } }).Returns(Task.FromResult(new SimulationStatus(false, 0, 4)));
            clientMock.Setup(c => c.GetSettings()).Returns(Task.FromResult(new SimulationSettings(new SimulationSettings.SimulationTurnLength("second", 5), new() { new("A1"), new("B2") }, new SimulationSettings.SimulationEffectivenesses(new EffectivenessQuality(0.3f, 0.5f), new(), new EffectivenessQuality(0.5f, 0.3f)), new List<Edge>())));
            clientMock.Setup(c => c.GetInfoTotals(It.Is<SearchRequest>((req) => turn == 1 && req.Locations[0][0] == "A1"))).Returns(Task.FromResult(new List<InfectionTotals>() { new InfectionTotals(new() { "A1" }, 1, 2, 3, 4, 5, 6, 7) }));
            clientMock.Setup(c => c.GetInfoTotals(It.Is<SearchRequest>((req) => turn == 1 && req.Locations[0][0] == "B2"))).Returns(Task.FromResult(new List<InfectionTotals>() { new InfectionTotals(new() { "B2" }, 2, 3, 4, 5, 6, 7, 8) }));
            clientMock.Setup(c => c.GetInfoTotals(It.Is<SearchRequest>((req) => turn == 2 && req.Locations[0][0] == "A1"))).Returns(Task.FromResult(new List<InfectionTotals>() { new InfectionTotals(new() { "A1" }, 2, 3, 4, 5, 6, 7, 8) }));
            clientMock.Setup(c => c.GetInfoTotals(It.Is<SearchRequest>((req) => turn == 2 && req.Locations[0][0] == "B2"))).Returns(Task.FromResult(new List<InfectionTotals>() { new InfectionTotals(new() { "B2" }, 3, 4, 5, 6, 7, 8, 9) }));
            healthOrgMock.Object.AddGlobalTrigger(new CustomTrigger(TrackingValue.Uninfected, (p) => p.CurrentParameterCount == 5, (l) => healthOrgMock.Object.TasksToExecute.Add(new WhoAction(0, new Loan(1000))), 0));
            clientMock.Setup(c => c.ApplyActions(It.Is<List<WhoAction>>(a => a[0].Id == 0 && a[0].Action == Loan.ActionName && a[0].Mode == "create"))).Returns(Task.FromResult<List<ActionResult>>(new() { new ActionResult(0, 200, "") })).Verifiable();
            await healthOrgMock.Object.Run();
            clientMock.VerifyAll();
        }

        [Fact]
        public async void TestSecondTurnFailedAction()
        {
            int turn = 1;
            Mock<IClient> clientMock = new();
            Mock<HealthOrganisation> healthOrgMock = new(clientMock.Object);
            clientMock.Setup(c => c.GetStatus()).Returns(Task.FromResult(new SimulationStatus(true, 0, 4)));
            clientMock.Setup(c => c.EndTurn()).Callback(() => { if (++turn > 2) { healthOrgMock.Object.Running = false; } }).Returns(Task.FromResult(new SimulationStatus(false, 0, 4)));
            LocationDefinition location = new("A1");
            location.SubLocations = new() { new("B2") };
            clientMock.Setup(c => c.GetSettings()).Returns(Task.FromResult(new SimulationSettings(new SimulationSettings.SimulationTurnLength("second", 5), new() { location }, new SimulationSettings.SimulationEffectivenesses(new EffectivenessQuality(0.3f, 0.5f), new(), new EffectivenessQuality(0.5f, 0.3f)), new List<Edge>())));
            clientMock.Setup(c => c.GetInfoTotals(It.Is<SearchRequest>((req) => turn == 1 && req.Locations[0].Count == 1))).Returns(Task.FromResult(new List<InfectionTotals>() { new InfectionTotals(new() { "A1" }, 1, 2, 3, 4, 5, 6, 7) }));
            clientMock.Setup(c => c.GetInfoTotals(It.Is<SearchRequest>((req) => turn == 1 && req.Locations[0].Count == 2))).Returns(Task.FromResult(new List<InfectionTotals>() { new InfectionTotals(new() { "A1", "B2" }, 2, 3, 4, 5, 6, 7, 8) }));
            clientMock.Setup(c => c.GetInfoTotals(It.Is<SearchRequest>((req) => turn == 2 && req.Locations[0].Count == 1))).Returns(Task.FromResult(new List<InfectionTotals>() { new InfectionTotals(new() { "A1" }, 2, 3, 4, 5, 6, 7, 8) }));
            clientMock.Setup(c => c.GetInfoTotals(It.Is<SearchRequest>((req) => turn == 2 && req.Locations[0].Count == 2))).Returns(Task.FromResult(new List<InfectionTotals>() { new InfectionTotals(new() { "A1", "B2" }, 3, 4, 5, 6, 7, 8, 9) }));
            healthOrgMock.Object.AddLocalTrigger(new CustomTrigger(TrackingValue.Uninfected, (p) => p.CurrentParameterCount == 3, (l) => healthOrgMock.Object.TasksToExecute.Add(new WhoAction(0, new CloseSchools(l))), 0));
            clientMock.Setup(c => c.ApplyActions(It.Is<List<WhoAction>>(a => a[0].Id == 0 && Enumerable.SequenceEqual(a[0].Parameters.Location, new List<string>() { "A1", "B2" }) && a[0].Mode == "create"))).Returns(Task.FromResult<List<ActionResult>>(new() { new ActionResult(0, 502, "") })).Verifiable();
            await healthOrgMock.Object.Run();
            Assert.Equal(0, healthOrgMock.Object.LocationStatuses["A1B2"].ActionCount);
            clientMock.VerifyAll();
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

            Assert.Equal(15, org.TasksToExecute.Count());
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

            Assert.Equal(15, org.TasksToExecute.Count());
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

            Assert.Equal(4, org.TasksToExecute.Count());
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

            Assert.Equal(8, org.TasksToExecute.Count());
        }

    }
}
