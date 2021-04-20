using System;
using System.Text.RegularExpressions;

namespace Arches
{
    public static class Semantics
    {

        public static int[][] Recolor(int[][] input, int color) {
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
            PartialImageSpec.print(recoloredImage);
            return recoloredImage;
        }

        public static int[][] Filter(int[][] input, int color) {
            int[][] filteredImage = new int[input.Length][];
            for (int i = 0; i < input.Length; i++) {
                filteredImage[i] = new int[input[0].Length];
            }
            for (int i = 0; i < input.Length; i++) {
                for (int j = 0; j < input[0].Length; j++) {
                    if (input[i][j] == color) {
                        filteredImage[i][j] = color;
                    } else {
                        filteredImage[i][j]= 0;
                    }
                }
            }
            return filteredImage;
        }
    }
}