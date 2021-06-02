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

        public override string ToString()
        {
            return $"Coordinate({this.X}, {this.Y})";
        }
    }
}
