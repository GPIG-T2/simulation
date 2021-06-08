using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XUnit;

namespace Virus.Test
{
    class TestEdge
    {
        private Edge _edge;
        private Node _left;
        private Node _right;
        private Virus _virus;

        public TestEdge()
        {
            this._left = new Node(0, 100000, new Demographics(10, 10, 10, 10, 10, 10, 10, 10, 10), "left", new Models.Coordinate(1, 2), new Demographics(0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1), 0, 0);
            this._right = new Node(1, 100000, new Demographics(10, 10, 10, 10, 10, 10, 10, 10, 10), "right", new Models.Coordinate(1, 2), new Demographics(0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1), 0, 0);
            this._virus = new Virus(new Demographics(1, 1, 1, 1, 1, 1, 1, 1, 1), new Demographics(0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1), new Demographics(0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1), new Demographics(0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1), new Demographics(0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1));
            this._edge = new Edge("test", this._left, this._right, 50000, 10, 5);

        }

        [Fact]
        public void TestCloseEdge()
        {
            this._left.Infect(50000);
            this._right.Infect(50000);
            this._edge.CloseEdge();

            Assert.Equal((0, 0), this._edge.Update(this._virus));

        }

    }
}
