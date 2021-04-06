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
            return color;
        }

        [FeatureCalculator(nameof(Semantics.Recolor))]
        public static double Recolor(double image, double color)
        {
            return color;
        }

        [FeatureCalculator("color", Method = CalculationMethod.FromLiteral)]
        public static double Color(int color)
        {
            if (color < 0 || color > 9)
            {
                return Double.NegativeInfinity;
            }
            return 0.0;
        }
    }
}