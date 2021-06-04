using System;

namespace Models
{
    /// <summary>
    /// A euclidian coordinate.
    /// </summary>
    public struct Coordinate
    {
        /// <summary>
        /// The X component of the coordinate.
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// The Y component of the coordinate.
        /// </summary>
        public double Y { get; set; }

        public Coordinate(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        public override string ToString()
        {
            return $"Coordinate({this.X}, {this.Y})";
        }
    }
}
