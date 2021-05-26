using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using Models;
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
        public void TestBasicConstructorNoDepth()
        {
            TrackingValue parameter = TrackingValue.AsymptomaticInfectedInfectious;
            TrackingFunction comparisonFunction = TrackingFunction.GREATER_THAN;
            float threshold = 1.2f;
            static void resultingAction(List<string> _) { }
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
            static void resultingAction(List<string> _) { }
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
        public void TestCustomConstructorNoDepth()
        {
            TrackingValue parameter = TrackingValue.AsymptomaticInfectedInfectious;
            int timespan = 1;
            static void resultingAction(List<string> _) { }
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
            static void resultingAction(List<string> _) { }
            static bool comparisonFunction(CustomTrackingFunctionParameters _) => true;
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
            static bool comparisonFunction(CustomTrackingFunctionParameters p) => p.Change > 1.9f && p.CurrentParameterCount > 3;
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
            static bool comparisonFunction(CustomTrackingFunctionParameters p) => p.Change > 1.5f && p.CurrentParameterCount < 3;
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
            static bool comparisonFunction(CustomTrackingFunctionParameters p) => p.Change > 1.5f && p.CurrentParameterCount > 3;
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
            static bool comparisonFunction(CustomTrackingFunctionParameters p) => p.Change > 1.5f && p.CurrentParameterCount < 3;
            int timespan = 1;
            bool functionCalled = false;
            CustomTrigger trigger = new(parameter, comparisonFunction, (_) => functionCalled = true, timespan);
            trigger.Apply(tracker);
            Assert.False(functionCalled);
        }

    }
}
