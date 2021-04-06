using System;
using System.Text.RegularExpressions;

namespace Arches
{
    public static class Semantics
    {

        public static int[][] Recolor(int[][] input, int color) {
            Console.WriteLine("recoloring");
            int[][] recoloredImage = new int[input.Length][];
            for (int i = 0; i < input.Length; i++) {
                recoloredImage[i] = new int[input[0].Length];
            }
            for (int i = 0; i < input.Length; i++) {
                for (int j = 0; j < input[0].Length; j++) {
                    if (input[i][j] != 0) {
                        recoloredImage[i][j] = color;
                    } else {
                        recoloredImage[i][j] = 0;
                    }
                }
            }
            return recoloredImage;
        }
    }
}