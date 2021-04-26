using System;
using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.AST;
using Microsoft.ProgramSynthesis.Features;

namespace Arches
{
    public class RankingScore : Feature<double>
    {
        public RankingScore(Grammar grammar) : base(grammar, "Score")
        {
        }
        protected override double GetFeatureValueForVariable(VariableNode variable) => 0;

        [FeatureCalculator(nameof(Semantics.FilterColor))]
        public static double FilterColor(double image, double color)
        {
            return image + color;
        }

        [FeatureCalculator(nameof(Semantics.Recolor))]
        public static double Recolor(double image, double color)
        {
            return image + color;
        }

        [FeatureCalculator(nameof(Semantics.Orthogonal))]
        public static double Orthogonal(double image, double orth_option)
        {
            return image + orth_option;
        }

        [FeatureCalculator(nameof(Semantics.Compose))]
        public static double Compose(double top, double bottom)
        {
            return top + bottom;
        }

        [FeatureCalculator(nameof(Semantics.Origin))]
        public static double Origin(double image)
        {
            return image;  // no penalty
        }

        [FeatureCalculator(nameof(Semantics.Identity))]
        public static double Identity(double image)
        {
            return -1 + image;  // don't want layers of identity function
        }

        [FeatureCalculator("color", Method = CalculationMethod.FromLiteral)]
        public static double Color(int color)
        {
            if (color < 0 || color > 9)
            {
                return Double.NegativeInfinity;
            }
            return -1.0;
        }


        [FeatureCalculator("orth_option", Method = CalculationMethod.FromLiteral)]
        public static double OrthoOption(int orth_option)
        {
            if (orth_option < 0 || orth_option > 2) // currently, we only support 3 options
            {
                return Double.NegativeInfinity;
            }
            return -1.0;
        }
    }
}