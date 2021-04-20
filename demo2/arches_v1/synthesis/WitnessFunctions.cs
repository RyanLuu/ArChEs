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

        [WitnessFunction(nameof(Semantics.Recolor), 0, DependsOnParameters = new[] { 1 })]
        public PartialImageSpec WitnessRecolor2(GrammarRule rule, DisjunctiveExamplesSpec spec, ExampleSpec colorSpec) {
            var result = new Dictionary<State, IEnumerable<object>>();
            foreach (var example in spec.DisjunctiveExamples) {
                State inputState = example.Key;
                var occurrences = new List<int[][]>();
                foreach (int[][] output in example.Value) {
                    int color =  (int)colorSpec.Examples[inputState];
                    int[][] preimage = new int[output.Length][];
                    for (int i = 0; i < preimage.Length; i++) {
                        preimage[i] = new int[output[i].Length];
                    }
                    for (int i = 0; i < output.Length; i++) {
                        for (int j = 0; j < output[0].Length; j++) {
                            if (output[i][j] == 0) {
                                preimage[i][j] = 0;
                            } else if (output[i][j] == color) {
                                preimage[i][j] = 10;
                            } else {
                                Console.Out.WriteLine("ending early");
                                return null;
                            }
                        }
                    }
                    occurrences.Add(preimage);
                }
                result[inputState] = occurrences.Cast<object>();
                
            }
            return new PartialImageSpec(result);
        } 

        [WitnessFunction(nameof(Semantics.Recolor), 1)]
        public DisjunctiveExamplesSpec WitnessRecolor(GrammarRule rule, ExampleSpec spec) {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, object> example in spec.Examples) {
                State inputState = example.Key;
                var output = example.Value as int[][];
                
                var occurrences = new List<int>();
                int candidate = -1;
                for (int i = 0; i < output.Length; i++) {
                    for (int j = 0; j < output[0].Length; j++) {
                        if (output[i][j] != 0) {
                            if (candidate == -1) {
                                candidate = output[i][j]; 
                            } else if (output[i][j] != candidate) {
                                Console.Out.WriteLine("end early, color");
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

        [WitnessFunction(nameof(Semantics.Filter), 0, DependsOnParameters = new[] { 1 })]
        public PartialImageSpec WitnesFilter1(GrammarRule rule, DisjunctiveExamplesSpec spec, ExampleSpec colorSpec) {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (var example in spec.DisjunctiveExamples) {
                State inputState = example.Key;
                var occurrences = new List<int[][]>();
                foreach (int[][] output in example.Value) {
                    int color =  (int)colorSpec.Examples[inputState];
                    int[][] preimage = new int[output.Length][];
                    for (int i = 0; i < preimage.Length; i++) {
                        preimage[i] = new int[output[i].Length];
                    }
                    for (int i = 0; i < output.Length; i++) {
                        for (int j = 0; j < output[0].Length; j++) {
                            if (output[i][j] == 0) {
                                preimage[i][j] = -color;
                            } else if (output[i][j] == color) {
                                preimage[i][j] = color;
                            } else {
                                Console.Out.WriteLine("ending early");
                                return null;
                            }
                        }
                    }
                    occurrences.Add(preimage);
                }
                result[inputState] = occurrences.Cast<object>();
                
            }
            return new PartialImageSpec(result);
        } 

        [WitnessFunction(nameof(Semantics.Filter), 1)]
        public DisjunctiveExamplesSpec WitnessFilter2(GrammarRule rule, ExampleSpec spec) {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, object> example in spec.Examples) {
                State inputState = example.Key;
                var output = example.Value as int[][];
                var occurrences = new List<int>();
                int candidate = -1;
                for (int i = 0; i < output.Length; i++) {
                    for (int j = 0; j < output[0].Length; j++) {
                        if (output[i][j] != 0) {
                            if (candidate == -1) {
                                candidate = output[i][j]; 
                            } else if (output[i][j] != candidate) {
                                Console.Out.WriteLine("end early, color");
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
                Console.Out.WriteLine();
            }
        }
    }
}