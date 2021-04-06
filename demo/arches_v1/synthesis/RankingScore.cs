using System;
using System.Text.RegularExpressions;
using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.Features;

namespace Arches
{
    public class RankingScore : Feature<double>
    {
        public RankingScore(Grammar grammar) : base(grammar, "Score")
        {
        }

        [FeatureCalculator(nameof(Semantics.Substring))]
        public static double Substring(double v, double start, double end)
        {
            return start * end;
        }

        [FeatureCalculator(nameof(Semantics.Max))]
        public static double Max(double v, double i) {
            Console.WriteLine("scoring max");
            return i;
        }

        [FeatureCalculator("i", Method = CalculationMethod.FromLiteral)]
        public static double I(int i)
        {
            Console.WriteLine("scoring i");
            return 1.0 / Math.Abs(i);
        }

        [FeatureCalculator("j", Method = CalculationMethod.FromLiteral)]
        public static double J(int j)
        {
            Console.WriteLine("scoring j");
            if (j < 1) return Double.NegativeInfinity;
            return j;
        }

        [FeatureCalculator(nameof(Semantics.AbsPos))]
        public static double AbsPos(double v, double k)
        {
            return k;
        }

        [FeatureCalculator("k", Method = CalculationMethod.FromLiteral)]
        public static double K(int k)
        {
            return 1.0 / Math.Abs(k);
        }

        [FeatureCalculator(nameof(Semantics.RelPos))]
        public static double RelPos(double x, double rr)
        {
            return rr;
        }

        [FeatureCalculator("rr", Method = CalculationMethod.FromLiteral)]
        public static double RR(Tuple<Regex, Regex> tuple)
        {
            return 1;
        }
    }
}