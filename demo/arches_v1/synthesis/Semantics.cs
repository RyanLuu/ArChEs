using System;
using System.Text.RegularExpressions;

namespace Arches
{
    public static class Semantics
    {
        public static int Max(int[] v, int start) {
            Console.WriteLine("testing max");
            int max = v[start];
            for (int i = start + 1; i < v.Length; i++) {
                if (v[i] > max) max = v[i];
            }
            return max;
        }
        public static int Add(int v) {
            return v + 1;
        }
        public static int Sub(int v) {
            return v - 1;
        }

        public static int First(int[] v) {
            return v[0];
        }
        public static string Substring(string v, int start, int end)
        {
            return v.Substring(start, end - start);
        }

        public static int? AbsPos(string v, int k)
        {
            return k > 0 ? k - 1 : v.Length + k + 1;
        }

        public static int? RelPos(string v, Tuple<Regex, Regex> rr)
        {
            Regex left = rr.Item1;
            Regex right = rr.Item2;
            MatchCollection rightMatches = right.Matches(v);
            MatchCollection leftMatches = left.Matches(v);
            foreach (Match leftMatch in leftMatches)
            {
                foreach (Match rightMatch in rightMatches)
                {
                    if (rightMatch.Index == leftMatch.Index + leftMatch.Length)
                    {
                        return leftMatch.Index + leftMatch.Length;
                    }
                }
            }
            return null;
        }
    }
}