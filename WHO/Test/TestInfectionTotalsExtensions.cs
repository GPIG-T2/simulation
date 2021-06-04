using Models;
using WHO.Extensions;
using Xunit;

namespace WHO.Test
{
    public class TestInfectionTotalsExtensions
    {

        [Fact]
        public void TestAdd()
        {
            InfectionTotals totals0 = new(new() { "A1" }, 1, 3, 5, 6, 8, 10, 3);
            InfectionTotals totals1 = new(new() { "A1" }, 4, 7, 23, 1, 3, 7, 9);
            InfectionTotals expected = new(new() { "A1" }, 5, 10, 28, 7, 11, 17, 12);
            totals0.Add(totals1);
            Assert.True(expected.IsEqualTo(totals0));
        }

        [Fact]
        public void TestEqual()
        {
            InfectionTotals totals0 = new(new() { "A1" }, 1, 3, 5, 6, 8, 10, 3);
            InfectionTotals totals1 = new(new() { "A1" }, 4, 7, 23, 1, 3, 7, 9);
            InfectionTotals expected = totals0.Clone();
            Assert.True(totals0.IsEqualTo(expected));
            totals0.Add(totals1);
            Assert.False(totals0.IsEqualTo(expected));
        }

    }
}
