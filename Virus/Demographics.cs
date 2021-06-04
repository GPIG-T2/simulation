using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virus
{
    /// <summary>
    /// Splits data into different demographics - can be used as percentage of population (adds to 1) or simply as a container with named variables
    /// </summary>
    public class Demographics
    {
        /// <summary>
        /// Data for the under 5 demographic
        /// </summary>
        public double UnderFive { get; set; }

        /// <summary>
        /// Data for the 5-17 demographic
        /// </summary>
        public double FiveToSeventeen { get; set; }

        /// <summary>
        /// Data for the 18-29 demographic
        /// </summary>
        public double EighteenToTwentyNine { get; set; }

        /// <summary>
        /// Data for 30-39
        /// </summary>
        public double ThirtyToThirtyNine { get; set; }

        /// <summary>
        /// Data for 40-49
        /// </summary>
        public double FourtyToFourtyNine { get; set; }

        /// <summary>
        /// Data for 50-64
        /// </summary>
        public double FiftyToSixtyFour { get; set; }

        /// <summary>
        /// Data for 65-74
        /// </summary>
        public double SixtyFiveToSeventyFour { get; set; }

        /// <summary>
        /// Data for 75-84
        /// </summary>
        public double SeventyFiveToEightyFour { get; set; }

        /// <summary>
        /// Data for over 85
        /// </summary>
        public double OverEightyFive { get; set; }

        public Demographics(
            double underFive,
            double fiveToSeventeen,
            double eighteenToTwentyNine,
            double thirtyToThirtyNine,
            double fourtyToFourtyNine,
            double fiftyToSixtyFour,
            double sixtyFiveToSeventyFour,
            double seventyFiveToEightyFour,
            double overEightyFive
            )
        {
            this.UnderFive = underFive;
            this.FiveToSeventeen = fiveToSeventeen;
            this.EighteenToTwentyNine = eighteenToTwentyNine;
            this.ThirtyToThirtyNine = thirtyToThirtyNine;
            this.FourtyToFourtyNine = fourtyToFourtyNine;
            this.FiftyToSixtyFour = fiftyToSixtyFour;
            this.SixtyFiveToSeventyFour = sixtyFiveToSeventyFour;
            this.SeventyFiveToEightyFour = seventyFiveToEightyFour;
            this.OverEightyFive = overEightyFive;
        }

        /// <summary>
        /// Get the string representation of the object
        /// </summary>
        /// <returns>The string representation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class Demographics {\n");
            sb.Append("  UnderFive: ").Append(this.UnderFive).Append("\n");
            sb.Append("  FiveToSeventeen: ").Append(this.FiveToSeventeen).Append("\n");
            sb.Append("  EighteenToTwentyNine: ").Append(this.EighteenToTwentyNine).Append("\n");
            sb.Append("  ThirtyToThirtyNine: ").Append(this.ThirtyToThirtyNine).Append("\n");
            sb.Append("  FourtyToFourtyNine: ").Append(this.FourtyToFourtyNine).Append("\n");
            sb.Append("  FiftyToSixtyFour: ").Append(this.FiftyToSixtyFour).Append("\n");
            sb.Append("  SixtyFiveToSeventyFour: ").Append(this.SixtyFiveToSeventyFour).Append("\n");
            sb.Append("  SeventyFiveToEightyFour: ").Append(this.SeventyFiveToEightyFour).Append("\n");
            sb.Append("  OverEightyFive: ").Append(this.OverEightyFive).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

    }
}
