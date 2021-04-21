using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.Learning;
using Microsoft.ProgramSynthesis.Rules;
using Microsoft.ProgramSynthesis.Specifications;

namespace Arches
{
    public class WitnessFunctions : DomainLearningLogic
    {
        public WitnessFunctions(Grammar grammar) : base(grammar)
        {
        }

        // Witness for single in Recolor
        // Given output image return all possible preimages
        // Because there would be trillions of preimages use the PartialImageSpec for a compact representation
        // 10 -> Match any color except 0
        // -x -> Match any color except x
        // The DependsOnParameter allows us to use the color value in this function
        // colorSpec allows us to know what the color that the other Recolor witness function selected for this image
        [WitnessFunction(nameof(Semantics.Recolor), 0)]
        public PartialImageSpec WitnessRecolor2(GrammarRule rule, PartialImageSpec spec) {
            var result = new Dictionary<State, object>();
            foreach (var example in spec.examples) {
                State inputState = example.Key;

                var output = example.Value as int[][];
                int color = -1;
                for (int i = 0; i < output.Length; i++) {
                    for (int j = 0; j < output[i].Length; j++) {
                        if (output[i][j] != 0) {
                            if (color == -1) color = output[i][j];
                            if (color != output[i][j]) {
                                Console.WriteLine("ending early");
                                return null;
                            }
                        }
                    }
                }
                if (color == -1) {
                    Console.WriteLine("ending early");
                    return null; 
                }
                
                // create blank preimage
                int[][] preimage = new int[output.Length][];
                for (int i = 0; i < preimage.Length; i++) {
                    preimage[i] = new int[output[i].Length];
                }

                // loop through all pixels of output image
                for (int i = 0; i < output.Length; i++) {
                    for (int j = 0; j < output[0].Length; j++) {
                        // if output is 0 then the preimage must also be 0 because recolor does not affect pixels with color 0
                        if (output[i][j] == 0) {
                            preimage[i][j] = 0;
                        // if output is color then the preimage could be any non-zero color, this is denoted by 10
                        } else if (output[i][j] == color) {
                            preimage[i][j] = 10;
                        } else {
                            // If this occurs then recolor is not a valid operation for this output image
                            Console.WriteLine("ending early");
                            return null;
                        }
                    }
                }
                result[inputState] = preimage;
            }
            return new PartialImageSpec(result);
        } 

        // Witness for color attribute of Recolor
        // Given the output image return all possible color values
        // Because the other witness function depends on this one we cannot know the preimage
        // However the preimage is not needed to figure out what the color value could be
        [WitnessFunction(nameof(Semantics.Recolor), 1)]
        public DisjunctiveExamplesSpec WitnessRecolor(GrammarRule rule, ExampleSpec spec) {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, object> example in spec.Examples) {
                State inputState = example.Key;
                // extract output image
                var output = example.Value as int[][];
                
                var occurrences = new List<int>();
                // candidate color
                int candidate = -1;
                for (int i = 0; i < output.Length; i++) {
                    for (int j = 0; j < output[0].Length; j++) {
                        if (output[i][j] != 0) {
                            // if recolor candidate not found, now we have
                            if (candidate == -1) {
                                candidate = output[i][j]; 
                            // We found the candidate recolor but we found a pixel that is nonzero and isn't
                            // candidate color so we terminate as recolor isn't possible for this output
                            } else if (output[i][j] != candidate) {
                                Console.WriteLine("end early, color");
                                return null;
                            }
                        }
                    }
                }
                occurrences.Add(candidate);
                if (occurrences.Count == 0) return null;
                result[inputState] = occurrences.Cast<object>();
            }
            return new DisjunctiveExamplesSpec(result);
        }

        // Witness for single in Filter
        // Given output image return all possible preimages
        // Because there would be trillions of preimages use the PartialImageSpec for a compact representation
        // 10 -> Match any color except 0
        // -x -> Match any color except x
        // The DependsOnParameter allows us to use the color value in this function
        // colorSpec allows us to know what the color that the other Filter witness function selected for this image 
        [WitnessFunction(nameof(Semantics.Filter), 0)]
        public PartialImageSpec WitnesFilter1(GrammarRule rule, PartialImageSpec spec) {
            var result = new Dictionary<State, object>();

            foreach (var example in spec.examples) {
                State inputState = example.Key;
                var occurrences = new List<int[][]>();
                // There really only should be 1 image here, it's just an array out of convenience
                var output = example.Value as int[][];

                int color = -1;
                for (int i = 0; i < output.Length; i++) {
                    for (int j = 0; j < output[i].Length; j++) {
                        if (output[i][j] != 0) {
                            if (color == -1) color = output[i][j];
                            if (color != output[i][j]) {
                                Console.WriteLine("ending early");
                                return null;
                            }
                        }
                    }
                }
                if (color == -1) {
                    Console.WriteLine("ending early");
                    return null; 
                }

                // create blank preimage
                int[][] preimage = new int[output.Length][];
                for (int i = 0; i < preimage.Length; i++) {
                    preimage[i] = new int[output[i].Length];
                }

                // loop through all pixels of output image 
                for (int i = 0; i < output.Length; i++) {
                    for (int j = 0; j < output[0].Length; j++) {
                        // if output pixel is 0 then we know that the preimage could be any color
                        // except the filter color, hence when we do -color for the preimage
                        if (output[i][j] == 0) {
                            preimage[i][j] = -color;
                        // if the output pixel is the filter color then we know the only possibility
                        // is the filter color
                        } else if (output[i][j] == color) {
                            preimage[i][j] = color;
                        } else {
                            // We found a pixel in the output that wasn't 0 or the filter color
                            // So filter color is not possible.
                            Console.WriteLine("ending early");
                            return null;
                        }
                    }
                }
                result[inputState] = preimage;
            }
            return new PartialImageSpec(result);
        } 

        // Witness for color attribute of Filter
        // Given the output image return all possible color values
        // Because the other witness function depends on this one we cannot know the preimage
        // However the preimage is not needed to figure out what the color value could be 
        [WitnessFunction(nameof(Semantics.Filter), 1)]
        public DisjunctiveExamplesSpec WitnessFilter2(GrammarRule rule, ExampleSpec spec) {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, object> example in spec.Examples) {
                State inputState = example.Key;
                // extract output image
                var output = example.Value as int[][];
                var occurrences = new List<int>();
                // filter color candidate
                int candidate = -1;
                for (int i = 0; i < output.Length; i++) {
                    for (int j = 0; j < output[0].Length; j++) {
                        if (output[i][j] != 0) {
                            // if filter candidate has not found, we have now found it
                            if (candidate == -1) {
                                candidate = output[i][j]; 
                            // if fitler candidate was found but output pixel contained another
                            // nonzero pixel that didn't match the candidate color
                            // filter is not possible
                            } else if (output[i][j] != candidate) {
                                return null;
                            }
                        }
                    }
                }
                occurrences.Add(candidate);
                if (occurrences.Count == 0) return null;
                Console.WriteLine(occurrences);
                result[inputState] = occurrences.Cast<object>();
            }
            return new DisjunctiveExamplesSpec(result);
        }

        public static int[][] clone(int[][] m) {
            int[][] copy = new int[m.Length][];
            for (int i = 0; i < m.Length; i++) {
                copy[i] = new int[m[0].Length];
            }
            for (int i = 0; i < m.Length; i++) {
                for (int j = 0; j < m.Length; j++) {
                    copy[i][j] = m[i][j];
                }
            }
            return copy;
        }
        public static bool compare(int[][] im1, int[][] im2) {
            for (int i = 0; i < im1.Length; i++) {
                for (int j = 0; j < im1[0].Length; j++) {
                    if (im1[i][j] != im2[i][j]) return false;
                }
            }
            return true;
        }

        public static void print(int[][] m) {
            for (int i = 0; i < m.Length; i++) {
                for (int j = 0; j < m[0].Length; j++) {
                    Console.Out.Write(m[i][j] + " ");
                }
                Console.WriteLine();
            }
        }
    }
}