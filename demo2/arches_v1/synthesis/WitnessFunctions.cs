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

        [WitnessFunction(nameof(Semantics.Recolor), 1)]
        public DisjunctiveExamplesSpec WitnessRecolor(GrammarRule rule, ExampleSpec spec) {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, object> example in spec.Examples) {
                State inputState = example.Key;
                var input = inputState[rule.Body[0]] as int[][];
                var output = example.Value as int[][];
                
                var occurances = new List<int>();
                var newColor = -1;

                for (int i = 0; i < output.Length; i++) {
                    for (int j = 0; j < output[0].Length; i++) {
                        if (output[i][j] != 0) {
                            newColor = output[i][j];
                            break;
                        }
                    }
                    if (newColor != -1) break;
                }

                if (newColor == -1) {
                    for (int i = 1; i < 10; i++) {
                        occurances.Add(i);
                    }
                } else {
                    occurances.Add(newColor);
                }

                result[inputState] = occurances.Cast<object>();
            }
            return new DisjunctiveExamplesSpec(result);
        }
    }
}