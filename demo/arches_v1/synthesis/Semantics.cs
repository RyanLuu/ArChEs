﻿using System;
using System.Text.RegularExpressions;

namespace ProseTutorial
{
    public static class Semantics
    {
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