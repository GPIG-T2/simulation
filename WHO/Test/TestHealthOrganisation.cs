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
        public void TestCalculateBestAction()
        {
            Mock<IClient> clientMock = new();
            Mock<HealthOrganisation> healthOrgMock = new(clientMock.Object);
            
        }

        [Fact]
        public void TestGetWhoActions()
        {
            Mock<IClient> clientMock = new();
            Mock<HealthOrganisation> healthOrgMock = new(clientMock.Object);
        }

    }
}
