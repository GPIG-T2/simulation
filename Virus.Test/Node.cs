using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Virus.Test
{
    public class TestNode
    {
        private readonly Node _node;
        private readonly Virus _virus;

        public TestNode()
        {
            this._node = new Node(0, 100000, new Demographics(10, 10, 10, 10, 10, 10, 10, 10, 10), "node", new Models.Coordinate(1, 2), new Demographics(0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1), 0, 0);
            this._virus = new Virus(new Demographics(1, 1, 1, 1, 1, 1, 1, 1, 1), new Demographics(0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1), new Demographics(0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1), new Demographics(0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1), new Demographics(0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(10)]
        [InlineData(100)]
        public void TestInfect(long toInfect)
        {
            Assert.False(this._node.Totals.IsInfected);

            this._node.Infect(toInfect);

            Assert.Equal(this._node.TotalPopulation - toInfect, this._node.Totals.Uninfected);
            Assert.Equal(toInfect, this._node.Totals.AsymptomaticInfectedNotInfectious);
            Assert.Equal(0, this._node.Totals.AsymptomaticInfectedInfectious);
            Assert.Equal(0, this._node.Totals.Symptomatic);
            Assert.Equal(0, this._node.Totals.SeriousInfection);
            Assert.Equal(0, this._node.Totals.Dead);
            Assert.Equal(0, this._node.Totals.RecoveredImmune);
        }

        [Fact]
        public void TestUpdate()
        {
            this._node.Infect(10);

            Assert.Equal(10, this._node.Totals.AsymptomaticInfectedNotInfectious);

            this._node.Update(this._virus);

            Assert.Equal(10, this._node.Totals.AsymptomaticInfectedNotInfectious);

            this._node.Update(this._virus);

            Assert.Equal(10, this._node.Totals.AsymptomaticInfectedNotInfectious);

            this._node.Update(this._virus);

            Assert.Equal(0, this._node.Totals.AsymptomaticInfectedNotInfectious);
            Assert.Equal(10, this._node.Totals.AsymptomaticInfectedInfectious);
            Assert.Equal(0, this._node.Totals.Symptomatic);
            Assert.Equal(0, this._node.Totals.SeriousInfection);

            this._node.Update(this._virus);
            this._node.Update(this._virus);

            Assert.Equal(10, this._node.Totals.AsymptomaticInfectedInfectious);
            Assert.Equal(0, this._node.Totals.Symptomatic);
            Assert.Equal(0, this._node.Totals.SeriousInfection);

            this._node.Update(this._virus);
            this._node.Update(this._virus);
            this._node.Update(this._virus);

            Assert.True(this._node.Totals.AsymptomaticInfectedInfectious > 0);
            Assert.InRange(this._node.Totals.Symptomatic, 1, 10);
        }
    }
}
