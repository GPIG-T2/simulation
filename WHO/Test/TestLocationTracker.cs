using System;
using Models;
using WHO.Extensions;
using WHO.Tracking;
using Xunit;

namespace WHO.Test
{
    public class TestLocationTracker
    {

        [Fact]
        public void TestConstructor()
        {
            string location = "A1";
            LocationTracker tracker = new(location, null);
            Assert.Equal(location, tracker.LocationKey);
        }

        [Fact]
        public void TestTrack()
        {
            string location = "A1";
            LocationTracker tracker = new(location, null);
            InfectionTotals totals = new(new() { location }, 10, 25, 120, 53, 12, 4, 14);
            tracker.Track(totals);
            Assert.Equal(1, tracker.Count);
            InfectionTotals returnedTotals = tracker.Get(0);
            Assert.Equal(totals, returnedTotals);
        }

        [Fact]
        public void TestGetOutOfRange()
        {
            string location = "A1";
            LocationTracker tracker = new(location, null);
            InfectionTotals totals = new(new() { location }, 10, 25, 120, 53, 12, 4, 14);
            tracker.Track(totals);
            Assert.Throws<ArgumentOutOfRangeException>(() => tracker.Get(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => tracker.Get(1));
        }

        [Fact]
        public void TestGetChange()
        {
            string location = "A1";
            LocationTracker tracker = new(location, null);
            InfectionTotals totals1 = new(new() { location }, 10, 25, 120, 53, 12, 4, 14);
            InfectionTotals totals2 = new(new() { location }, 12, 20, 110, 73, 15, 3, 14);
            InfectionTotals difference = new(new() { location }, 2, -5, -10, 20, 3, -1, 0);
            tracker.Track(totals1);
            tracker.Track(totals2);
            InfectionTotals change = tracker.GetChange(0, 1);
            Assert.Equal(difference.Location, change.Location);
            Assert.True(difference.Equals(change));
        }

        [Fact]
        public void TestGetChangeOutOfRange()
        {
            string location = "A1";
            LocationTracker tracker = new(location, null);
            InfectionTotals totals1 = new(new() { location }, 10, 25, 120, 53, 12, 4, 14);
            InfectionTotals totals2 = new(new() { location }, 12, 20, 110, 73, 15, 3, 14);
            tracker.Track(totals1);
            tracker.Track(totals2);
            Assert.Throws<ArgumentOutOfRangeException>(() => tracker.GetChange(-1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => tracker.GetChange(0, 2));
        }

        [Fact]
        public void TestLatest()
        {
            string location = "A1";
            LocationTracker tracker = new(location, null);
            Assert.Null(tracker.Latest);
            InfectionTotals totals1 = new(new() { location }, 10, 25, 120, 53, 12, 4, 14);
            InfectionTotals totals2 = new(new() { location }, 12, 20, 110, 73, 15, 3, 14);
            tracker.Track(totals1);
            Assert.Equal(totals1, tracker.Latest);
            tracker.Track(totals2);
            Assert.Equal(totals2, tracker.Latest);
        }

        [Fact]
        public void TestGetSum()
        {
            string location = "A1";
            LocationTracker tracker = new(location, null);
            InfectionTotals totals1 = new(new() { location }, 10, 25, 120, 53, 12, 4, 14);
            InfectionTotals totals2 = new(new() { location }, 12, 20, 110, 73, 15, 3, 14);
            InfectionTotals totals3 = new(new() { location }, 14, 5, 120, 45, 12, 5, 10);
            InfectionTotals expectedSum = new(new() { location }, 36, 50, 350, 171, 39, 12, 38);
            tracker.Track(totals1);
            tracker.Track(totals2);
            tracker.Track(totals3);
            InfectionTotals sumOut = tracker.GetSum(0, 2);
            Assert.True(sumOut.Equals(expectedSum));
        }

        [Fact]
        public void TestGetSumOutOfRange()
        {
            string location = "A1";
            LocationTracker tracker = new(location, null);
            InfectionTotals totals1 = new(new() { location }, 10, 25, 120, 53, 12, 4, 14);
            InfectionTotals totals2 = new(new() { location }, 12, 20, 110, 73, 15, 3, 14);
            InfectionTotals totals3 = new(new() { location }, 14, 5, 120, 45, 12, 5, 10);
            tracker.Track(totals1);
            tracker.Track(totals2);
            tracker.Track(totals3);
            Assert.Throws<ArgumentOutOfRangeException>(() => tracker.GetSum(-1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => tracker.GetSum(2, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => tracker.GetSum(4, 5));
            Assert.Throws<ArgumentOutOfRangeException>(() => tracker.GetSum(1, 4));
        }
    }
}
