using System.Collections.Generic;
using Models;
using Models.Parameters;
using Xunit;
using WHO.Extensions;

namespace WHO.Test
{
    public class TestLocationStatus
    {

        [Fact]
        public void TestConstructor()
        {
            List<string> location = new() { "A1", "B3" };
            string expectedKey = "A1,B3";
            LocationStatus status = new(location);
            Assert.Equal(expectedKey, status.LocationKey);
            Assert.Equal(location, status.Location);
            Assert.Equal(0, status.ActionCount);
        }

        [Fact]
        public void TestAddAction()
        {
            List<string> location = new() { "A1" };
            int actionId = 99;

            LocationStatus status = new(location);
            WhoAction action = new(actionId, new MaskMandate(status.Location, 1));

            status.AddAction(action);

            Assert.Equal(1, status.ActionCount);
            Assert.Equal(action, status.GetAction(actionId));
        }

        [Fact]
        public void TestRetrieveActionTypes()
        {
            List<string> location = new() { "A1", "B3" };
            int actionIdMask = 98;
            int actionIdMask2 = 99;
            int actionIdShield = 100;

            LocationStatus status = new(location);

            WhoAction maskAction = new(actionIdMask, new MaskMandate(status.Location, 1));
            WhoAction maskAction2 = new(actionIdMask2, new MaskMandate(status.Location, 2));
            WhoAction shieldAction = new(actionIdShield, new ShieldingProgram(status.Location));

            status.AddAction(maskAction);
            status.AddAction(maskAction2);
            status.AddAction(shieldAction);

            Assert.Equal(3, status.ActionCount);

            List<int> maskActions = status.GetActionsOfType(MaskMandate.ActionName);

            Assert.Equal(2, maskActions.Count);
            Assert.Equal(actionIdMask, maskActions[0]);
            Assert.Equal(actionIdMask2, maskActions[1]);

            List<int> shieldActions = status.GetActionsOfType(ShieldingProgram.ActionName);
            Assert.Single(shieldActions);
            Assert.Equal(actionIdShield, shieldActions[0]);
        }

        [Fact]
        public void TestRemoveAction()
        {
            List<string> location = new() { "A1" };
            int actionId = 99;
            LocationStatus status = new(location);
            WhoAction action = new(actionId, new MaskMandate(status.Location, 1));
            status.AddAction(action);
            Assert.Equal(1, status.ActionCount);
            status.RemoveAction(actionId);
            Assert.Equal(0, status.ActionCount);
        }

        [Fact]
        public void TestGetActionSuccess()
        {
            List<string> location = new() { "A1" };
            int actionId = 99;
            LocationStatus status = new(location);
            WhoAction action = new(actionId, new MaskMandate(status.Location, 1));
            status.AddAction(action);
            WhoAction? actionOut = status.GetAction(actionId);
            Assert.NotNull(actionOut);
            Assert.Equal(action, actionOut);
        }

        [Fact]
        public void TestGetActionFail()
        {
            List<string> location = new() { "A1" };
            int actionId = 99;
            LocationStatus status = new(location);
            WhoAction? actionOut = status.GetAction(actionId);
            Assert.Null(actionOut);
        }

    }
}
