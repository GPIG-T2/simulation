using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WHO.Tracking;
using Xunit;

namespace WHO.Test
{
    public class TestTrigger
    {

        [Fact]
        public void TestTriggerDepthCheck()
        {
            var firstDepth = (1, 3);
            var secondDepth = (-1, -1);
            var thirdDepth = (-1, 2);
            var fourthDepth = (2, -1);
            Assert.False(ITrigger.IsValidDepth(0, firstDepth));
            Assert.True(ITrigger.IsValidDepth(1, firstDepth));
            Assert.True(ITrigger.IsValidDepth(2, firstDepth));
            Assert.False(ITrigger.IsValidDepth(3, firstDepth));

            Assert.True(ITrigger.IsValidDepth(0, secondDepth));
            Assert.True(ITrigger.IsValidDepth(10, secondDepth));

            Assert.True(ITrigger.IsValidDepth(0, thirdDepth));
            Assert.True(ITrigger.IsValidDepth(1, thirdDepth));
            Assert.False(ITrigger.IsValidDepth(2, thirdDepth));

            Assert.False(ITrigger.IsValidDepth(1, fourthDepth));
            Assert.True(ITrigger.IsValidDepth(2, fourthDepth));
            Assert.True(ITrigger.IsValidDepth(10, fourthDepth));

        }

        [Fact]
        public void TestBasicConstructorNoDepth()
        {
            TrackingValue parameter = TrackingValue.AsymptomaticInfectedInfectious;
            TrackingFunction comparisonFunction = TrackingFunction.GREATER_THAN;
            float threshold = 1.2f;
            Action<List<string>> resultingAction = (_) => { };
            int timespan = 1;
            BasicTrigger trigger = new(parameter, comparisonFunction, threshold, resultingAction, timespan);
            Assert.Equal(parameter, trigger.Parameter);
            Assert.Equal(comparisonFunction, trigger.ComparisonFunction);
            Assert.Equal(threshold, trigger.Threshold);
            Assert.Equal(resultingAction, trigger.ResultingAction);
            Assert.Equal(timespan, trigger.Timespan);
            Assert.Equal((-1, -1), trigger.DepthRange);
        }

        [Fact]
        public void TestBasicConstructorWithDepth()
        {
            TrackingValue parameter = TrackingValue.AsymptomaticInfectedInfectious;
            TrackingFunction comparisonFunction = TrackingFunction.GREATER_THAN;
            float threshold = 1.2f;
            Action<List<string>> resultingAction = (_) => { };
            int timespan = 1;
            (int, int) depthRange = (1, 4);
            BasicTrigger trigger = new(parameter, comparisonFunction, threshold, resultingAction, timespan, depthRange);
            Assert.Equal(parameter, trigger.Parameter);
            Assert.Equal(comparisonFunction, trigger.ComparisonFunction);
            Assert.Equal(threshold, trigger.Threshold);
            Assert.Equal(resultingAction, trigger.ResultingAction);
            Assert.Equal(timespan, trigger.Timespan);
            Assert.Equal(depthRange, trigger.DepthRange);
        }

        [Fact]
        public void TestBasicApplySuccess1Day()
        {
            LocationStatus status = new("A1");
            LocationTracker tracker = new("A1", status);
            InfectionTotals totals1 = new(status.Location, 1, 1, 1, 1, 1, 1, 1);
            InfectionTotals totals2 = new(status.Location, 2, 0, 1, 1, 1, 1, 1);
            tracker.Track(totals1);
            tracker.Track(totals2);
            TrackingValue parameter = TrackingValue.Uninfected;
            TrackingFunction comparisonFunction = TrackingFunction.GREATER_THAN;
            float threshold = 1.9f;
            int timespan = 0;
            bool functionCalled = false;
            BasicTrigger trigger = new(parameter, comparisonFunction, threshold, (_) => functionCalled = true, timespan);
            trigger.Apply(tracker);
            Assert.True(functionCalled);
        }

        [Fact]
        public void TestBasicApplyFail1Day()
        {
            LocationStatus status = new("A1");
            LocationTracker tracker = new("A1", status);
            InfectionTotals totals1 = new(status.Location, 1, 1, 1, 1, 1, 1, 1);
            InfectionTotals totals2 = new(status.Location, 1, 1, 1, 1, 1, 1, 1);
            tracker.Track(totals1);
            tracker.Track(totals2);
            TrackingValue parameter = TrackingValue.Uninfected;
            TrackingFunction comparisonFunction = TrackingFunction.GREATER_THAN;
            float threshold = 1f;
            int timespan = 0;
            bool functionCalled = false;
            BasicTrigger trigger = new(parameter, comparisonFunction, threshold, (_) => functionCalled = true, timespan);
            trigger.Apply(tracker);
            Assert.False(functionCalled);
        }

        [Fact]
        public void TestBasicApplySuccess2Day()
        {
            LocationStatus status = new("A1");
            LocationTracker tracker = new("A1", status);
            InfectionTotals totals1 = new(status.Location, 2, 0, 1, 1, 1, 1, 1);
            InfectionTotals totals2 = new(status.Location, 2, 0, 1, 1, 1, 1, 1);
            InfectionTotals totals3 = new(status.Location, 1, 1, 1, 1, 1, 1, 1);
            InfectionTotals totals4 = new(status.Location, 1, 1, 1, 1, 1, 1, 1);
            tracker.Track(totals1);
            tracker.Track(totals2);
            tracker.Track(totals3);
            tracker.Track(totals4);
            TrackingValue parameter = TrackingValue.Uninfected;
            TrackingFunction comparisonFunction = TrackingFunction.LESS_THAN;
            float threshold = 0.6f;
            int timespan = 1;
            bool functionCalled = false;
            BasicTrigger trigger = new(parameter, comparisonFunction, threshold, (_) => functionCalled = true, timespan);
            trigger.Apply(tracker);
            Assert.True(functionCalled);
        }

        [Fact]
        public void TestBasicApplyFail2Day()
        {
            LocationStatus status = new("A1");
            LocationTracker tracker = new("A1", status);
            InfectionTotals totals1 = new(status.Location, 1, 1, 1, 1, 1, 1, 1);
            InfectionTotals totals2 = new(status.Location, 1, 1, 1, 1, 1, 1, 1);
            InfectionTotals totals3 = new(status.Location, 1, 1, 1, 1, 1, 1, 1);
            InfectionTotals totals4 = new(status.Location, 1, 1, 1, 1, 1, 1, 1);
            tracker.Track(totals1);
            tracker.Track(totals2);
            tracker.Track(totals3);
            tracker.Track(totals4);
            TrackingValue parameter = TrackingValue.Uninfected;
            TrackingFunction comparisonFunction = TrackingFunction.LESS_THAN;
            float threshold = 0.6f;
            int timespan = 1;
            bool functionCalled = false;
            BasicTrigger trigger = new(parameter, comparisonFunction, threshold, (_) => functionCalled = true, timespan);
            trigger.Apply(tracker);
            Assert.False(functionCalled);
        }


        [Fact]
        public void TestBasicApplyNotEnoughData()
        {
            LocationStatus status = new("A1");
            LocationTracker tracker = new("A1", status);
            InfectionTotals totals1 = new(status.Location, 2, 0, 1, 1, 1, 1, 1);
            InfectionTotals totals2 = new(status.Location, 2, 0, 1, 1, 1, 1, 1);
            InfectionTotals totals3 = new(status.Location, 1, 1, 1, 1, 1, 1, 1);
            tracker.Track(totals1);
            tracker.Track(totals2);
            tracker.Track(totals3);
            TrackingValue parameter = TrackingValue.Uninfected;
            TrackingFunction comparisonFunction = TrackingFunction.LESS_THAN;
            float threshold = 0.6f;
            int timespan = 1;
            bool functionCalled = false;
            BasicTrigger trigger = new(parameter, comparisonFunction, threshold, (_) => functionCalled = true, timespan);
            trigger.Apply(tracker);
            Assert.False(functionCalled);
        }

        [Fact]
        public void TestCustomConstructorNoDepth()
        {
            TrackingValue parameter = TrackingValue.AsymptomaticInfectedInfectious;
            int timespan = 1;
            Action<List<string>> resultingAction = (_) => { };
            Func<CustomTrackingFunctionParameters, bool> comparisonFunction = (_) => true;
            CustomTrigger trigger = new(parameter, comparisonFunction, resultingAction, timespan);
            Assert.Equal(parameter, trigger.Parameter);
            Assert.Equal(comparisonFunction, trigger.ComparisonFunction);
            Assert.Equal(resultingAction, trigger.ResultingAction);
            Assert.Equal(timespan, trigger.Timespan);
            Assert.Equal((-1, -1), trigger.DepthRange);
        }

        [Fact]
        public void TestCustomConstructorWithDepth()
        {
            TrackingValue parameter = TrackingValue.AsymptomaticInfectedInfectious;
            int timespan = 1;
            Action<List<string>> resultingAction = (_) => { };
            Func<CustomTrackingFunctionParameters, bool> comparisonFunction = (_) => true;
            (int, int) depthRange = (1, 4);
            CustomTrigger trigger = new(parameter, comparisonFunction, resultingAction, timespan, depthRange);
            Assert.Equal(parameter, trigger.Parameter);
            Assert.Equal(comparisonFunction, trigger.ComparisonFunction);
            Assert.Equal(resultingAction, trigger.ResultingAction);
            Assert.Equal(timespan, trigger.Timespan);
            Assert.Equal(depthRange, trigger.DepthRange);
        }

        [Fact]
        public void TestCustomApplySuccess1Day()
        {
            LocationStatus status = new("A1");
            LocationTracker tracker = new("A1", status);
            InfectionTotals totals1 = new(status.Location, 2, 1, 1, 1, 1, 1, 1);
            InfectionTotals totals2 = new(status.Location, 4, 0, 0, 1, 1, 1, 1);
            tracker.Track(totals1);
            tracker.Track(totals2);
            TrackingValue parameter = TrackingValue.Uninfected;
            Func<CustomTrackingFunctionParameters, bool> comparisonFunction = (p) => p.Change > 1.5f && p.CurrentParameterCount > 3;
            int timespan = 0;
            bool functionCalled = false;
            CustomTrigger trigger = new(parameter, comparisonFunction, (_) => functionCalled = true, timespan);
            trigger.Apply(tracker);
            Assert.True(functionCalled);
        }

        [Fact]
        public void TestCustomApplyFail1Day()
        {
            LocationStatus status = new("A1");
            LocationTracker tracker = new("A1", status);
            InfectionTotals totals1 = new(status.Location, 2, 1, 1, 1, 1, 1, 1);
            InfectionTotals totals2 = new(status.Location, 4, 0, 0, 1, 1, 1, 1);
            tracker.Track(totals1);
            tracker.Track(totals2);
            TrackingValue parameter = TrackingValue.Uninfected;
            Func<CustomTrackingFunctionParameters, bool> comparisonFunction = (p) => p.Change > 1.5f && p.CurrentParameterCount < 3;
            int timespan = 0;
            bool functionCalled = false;
            CustomTrigger trigger = new(parameter, comparisonFunction, (_) => functionCalled = true, timespan);
            trigger.Apply(tracker);
            Assert.False(functionCalled);
        }

        [Fact]
        public void TestCustomApplySuccess2Day()
        {
            LocationStatus status = new("A1");
            LocationTracker tracker = new("A1", status);
            InfectionTotals totals1 = new(status.Location, 2, 1, 1, 1, 1, 1, 1);
            InfectionTotals totals2 = new(status.Location, 2, 1, 1, 1, 1, 1, 1);
            InfectionTotals totals3 = new(status.Location, 4, 0, 0, 1, 1, 1, 1);
            InfectionTotals totals4 = new(status.Location, 4, 0, 0, 1, 1, 1, 1);
            tracker.Track(totals1);
            tracker.Track(totals2);
            tracker.Track(totals3);
            tracker.Track(totals4);
            TrackingValue parameter = TrackingValue.Uninfected;
            Func<CustomTrackingFunctionParameters, bool> comparisonFunction = (p) => p.Change > 1.5f && p.CurrentParameterCount > 3;
            int timespan = 1;
            bool functionCalled = false;
            CustomTrigger trigger = new(parameter, comparisonFunction, (_) => functionCalled = true, timespan);
            trigger.Apply(tracker);
            Assert.True(functionCalled);
        }

        [Fact]
        public void TestCustomApplyFail2Day()
        {
            LocationStatus status = new("A1");
            LocationTracker tracker = new("A1", status);
            InfectionTotals totals1 = new(status.Location, 2, 1, 1, 1, 1, 1, 1);
            InfectionTotals totals2 = new(status.Location, 2, 1, 1, 1, 1, 1, 1);
            InfectionTotals totals3 = new(status.Location, 2, 1, 1, 1, 1, 1, 1);
            InfectionTotals totals4 = new(status.Location, 2, 1, 1, 1, 1, 1, 1);
            tracker.Track(totals1);
            tracker.Track(totals2);
            tracker.Track(totals3);
            tracker.Track(totals4);
            TrackingValue parameter = TrackingValue.Uninfected;

            Func<CustomTrackingFunctionParameters, bool> comparisonFunction = (p) => p.Change > 1.5f && p.CurrentParameterCount < 3;
            int timespan = 1;
            bool functionCalled = false;
            CustomTrigger trigger = new(parameter, comparisonFunction, (_) => functionCalled = true, timespan);
            trigger.Apply(tracker);
            Assert.False(functionCalled);
        }

        [Fact]
        public void TestCustomApplyNotEnoughData()
        {
            LocationStatus status = new("A1");
            LocationTracker tracker = new("A1", status);
            InfectionTotals totals1 = new(status.Location, 2, 1, 1, 1, 1, 1, 1);
            InfectionTotals totals2 = new(status.Location, 2, 1, 1, 1, 1, 1, 1);
            InfectionTotals totals3 = new(status.Location, 4, 0, 0, 1, 1, 1, 1);
            tracker.Track(totals1);
            tracker.Track(totals2);
            tracker.Track(totals3);
            TrackingValue parameter = TrackingValue.Uninfected;
            Func<CustomTrackingFunctionParameters, bool> comparisonFunction = (p) => p.Change > 1.5f && p.CurrentParameterCount > 3;
            int timespan = 1;
            bool functionCalled = false;
            CustomTrigger trigger = new(parameter, comparisonFunction, (_) => functionCalled = true, timespan);
            trigger.Apply(tracker);
            Assert.False(functionCalled);
        }

    }
}
