using System.Collections.Generic;
using Interface.Client;
using Models.Parameters;
using Moq;
using WHO.Extensions;
using WHO.Tracking;
using Xunit;

namespace WHO.Test
{
    [Collection("RequiresHealthOrganisation")]
    public class TestActionCostCalculator
    {

        private HealthOrganisation _healthOrganisation;
        private static Dictionary<string, LocationTracker> LocationTrackerInfo = new();
        private static List<string> TestLocation = new() { "A1" };
        private static Mock<IClient> MockClient = new Mock<IClient>();
        private static int PeopleAtTestLocation = 10;

        public TestActionCostCalculator()
        {
            if (HealthOrganisation.Instance != null)
            {
                HealthOrganisation.SetInstanceForTestingOnly(null);
            }
            this._healthOrganisation = new(MockClient.Object);
            this._healthOrganisation.SetLocationTrackerFromTest(LocationTrackerInfo);
            SetPopulation(TestLocation.ToKey(), PeopleAtTestLocation);
        }

        private static double GetPressReleaseCost()
        {
            return PeopleAtTestLocation * ActionCostCalculator.PressReleaseCost;
        }

        private static void SetPopulation(string location, int count)
        {
            LocationTracker tracker = new(location, null);
            tracker.Track(new(new() { location }, count, 0, 0, 0, 0, 0, 0));
            LocationTrackerInfo[location] = tracker;
        }

        [Fact]
        public void TestInformationPressRelease()
        {
            InformationPressRelease action = new(0, TestLocation);
            Assert.Equal(PeopleAtTestLocation * 0.01d, ActionCostCalculator.CalculateCost(action, ActionCostCalculator.ActionMode.Create));
            Assert.Equal(-1, ActionCostCalculator.CalculateCost(action, ActionCostCalculator.ActionMode.Delete));
        }

        [Fact]
        public void TestTestAndIsolate()
        {
            int testQuanity = 15;
            TestAndIsolation badQuality = new(0, -1, testQuanity, TestLocation, true);
            TestAndIsolation goodQuality = new(1, -1, testQuanity, TestLocation, true);
            TestAndIsolation invalid = new(2, -1, testQuanity, TestLocation, true);
            Assert.Equal(ActionCostCalculator.BadTestCost * testQuanity, ActionCostCalculator.CalculateCost(badQuality, ActionCostCalculator.ActionMode.Create));
            Assert.Equal(ActionCostCalculator.GoodTestCost * testQuanity, ActionCostCalculator.CalculateCost(goodQuality, ActionCostCalculator.ActionMode.Create));
            Assert.Equal(-1, ActionCostCalculator.CalculateCost(invalid, ActionCostCalculator.ActionMode.Create));
            Assert.Equal(0, ActionCostCalculator.CalculateCost(badQuality, ActionCostCalculator.ActionMode.Delete));
            Assert.Equal(0, ActionCostCalculator.CalculateCost(goodQuality, ActionCostCalculator.ActionMode.Delete));
        }

        [Fact]
        public void TestStayAtHome()
        {
            StayAtHome action = new(TestLocation);
            Assert.Equal(GetPressReleaseCost(), ActionCostCalculator.CalculateCost(action, ActionCostCalculator.ActionMode.Create));
            Assert.Equal(GetPressReleaseCost(), ActionCostCalculator.CalculateCost(action, ActionCostCalculator.ActionMode.Delete));
        }

        [Fact]
        public void TestCloseSchools()
        {
            CloseSchools action = new(TestLocation);
            Assert.Equal(GetPressReleaseCost(), ActionCostCalculator.CalculateCost(action, ActionCostCalculator.ActionMode.Create));
            Assert.Equal(GetPressReleaseCost(), ActionCostCalculator.CalculateCost(action, ActionCostCalculator.ActionMode.Delete));
        }

        [Fact]
        public void TestCloseRecreationalLocations()
        {
            CloseRecreationalLocations action = new(TestLocation);
            Assert.Equal(GetPressReleaseCost(), ActionCostCalculator.CalculateCost(action, ActionCostCalculator.ActionMode.Create));
            Assert.Equal(GetPressReleaseCost(), ActionCostCalculator.CalculateCost(action, ActionCostCalculator.ActionMode.Delete));
        }

        [Fact]
        public void TestShieldingProgram()
        {
            ShieldingProgram action = new(TestLocation);
            Assert.Equal(GetPressReleaseCost(), ActionCostCalculator.CalculateCost(action, ActionCostCalculator.ActionMode.Create));
            Assert.Equal(GetPressReleaseCost(), ActionCostCalculator.CalculateCost(action, ActionCostCalculator.ActionMode.Delete));
        }

        [Fact]
        public void TestMovementRestrictions()
        {
            MovementRestrictions action = new(TestLocation, 10);
            Assert.Equal(GetPressReleaseCost(), ActionCostCalculator.CalculateCost(action, ActionCostCalculator.ActionMode.Create));
            Assert.Equal(GetPressReleaseCost(), ActionCostCalculator.CalculateCost(action, ActionCostCalculator.ActionMode.Delete));
        }

        [Fact]
        public void TestCloseBorders()
        {
            CloseBorders action = new(TestLocation);
            Assert.Equal(GetPressReleaseCost(), ActionCostCalculator.CalculateCost(action, ActionCostCalculator.ActionMode.Create));
            Assert.Equal(GetPressReleaseCost(), ActionCostCalculator.CalculateCost(action, ActionCostCalculator.ActionMode.Delete));
        }

        [Fact]
        public void TestInvestInVaccine()
        {
            const int amountInvested = 1000;
            InvestInVaccine action = new(amountInvested);
            Assert.Equal(amountInvested, ActionCostCalculator.CalculateCost(action, ActionCostCalculator.ActionMode.Create));
            Assert.Equal(-1, ActionCostCalculator.CalculateCost(action, ActionCostCalculator.ActionMode.Delete));
        }

        [Fact]
        public void TestFurlough()
        {
            const int amountInvested = 1000;
            Furlough action = new(amountInvested, TestLocation);
            Assert.Equal(GetPressReleaseCost(), ActionCostCalculator.CalculateCost(action, ActionCostCalculator.ActionMode.Create));
            Assert.Equal(GetPressReleaseCost(), ActionCostCalculator.CalculateCost(action, ActionCostCalculator.ActionMode.Delete));
        }

        [Fact]
        public void TestLoan()
        {
            Loan action = new(1000);
            Assert.Equal(0, ActionCostCalculator.CalculateCost(action, ActionCostCalculator.ActionMode.Create));
            Assert.Equal(-1, ActionCostCalculator.CalculateCost(action, ActionCostCalculator.ActionMode.Delete));
        }

        [Fact]
        public void TestMaskMandate()
        {
            double prc = GetPressReleaseCost();
            MaskMandate mask0 = new(TestLocation, 0);
            MaskMandate mask1 = new(TestLocation, 1);
            MaskMandate mask2 = new(TestLocation, 2);
            MaskMandate maskInvalid = new(TestLocation, 3);
            MaskMandate maskDelete = new(TestLocation, 0);
            Assert.Equal(0, ActionCostCalculator.CalculateCost(mask0, ActionCostCalculator.ActionMode.Create));
            Assert.Equal(prc + ActionCostCalculator.LowLevelMaskCost * PeopleAtTestLocation, ActionCostCalculator.CalculateCost(mask1, ActionCostCalculator.ActionMode.Create));
            Assert.Equal(prc + ActionCostCalculator.HighLevelMaskCost * PeopleAtTestLocation, ActionCostCalculator.CalculateCost(mask2, ActionCostCalculator.ActionMode.Create));
            Assert.Equal(-1, ActionCostCalculator.CalculateCost(maskInvalid, ActionCostCalculator.ActionMode.Create));
            Assert.Equal(prc, ActionCostCalculator.CalculateCost(maskDelete, ActionCostCalculator.ActionMode.Delete));
        }

        [Fact]
        public void TestHealthDrive()
        {
            HealthDrive action = new(TestLocation);
            Assert.Equal(GetPressReleaseCost(), ActionCostCalculator.CalculateCost(action, ActionCostCalculator.ActionMode.Create));
            Assert.Equal(GetPressReleaseCost(), ActionCostCalculator.CalculateCost(action, ActionCostCalculator.ActionMode.Delete));
        }

        [Fact]
        public void TestInvestInHealthServices()
        {
            const int amountInvested = 1000;
            InvestInHealthServices action = new(amountInvested);
            Assert.Equal(amountInvested, ActionCostCalculator.CalculateCost(action, ActionCostCalculator.ActionMode.Create));
            Assert.Equal(0, ActionCostCalculator.CalculateCost(action, ActionCostCalculator.ActionMode.Delete));
        }

        [Fact]
        public void TestSocialDistanceMandate()
        {
            SocialDistancingMandate action = new(TestLocation, 1);
            Assert.Equal(GetPressReleaseCost(), ActionCostCalculator.CalculateCost(action, ActionCostCalculator.ActionMode.Create));
            Assert.Equal(GetPressReleaseCost(), ActionCostCalculator.CalculateCost(action, ActionCostCalculator.ActionMode.Delete));
        }

        [Fact]
        public void TestCurfew()
        {
            Curfew action = new(TestLocation);
            Assert.Equal(GetPressReleaseCost(), ActionCostCalculator.CalculateCost(action, ActionCostCalculator.ActionMode.Create));
            Assert.Equal(GetPressReleaseCost(), ActionCostCalculator.CalculateCost(action, ActionCostCalculator.ActionMode.Delete));
        }

    }
}
