using System;
using System.Collections.Generic;
using System.Linq;
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
            
        [WitnessFunction(nameof(Semantics.FilterColor), 1)]
        public DisjunctiveExamplesSpec WitnessFilterColor(GrammarRule rule, ExampleSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, object> example in spec.Examples)
            {
                State inputState = example.Key;
                var input = inputState[rule.Body[0]] as Image;
                var output = example.Value as Image;
                var occurrences = new List<int>();

                var filterColor = 0;
                for (int i = 0; i < output.data.Length; i++)
                {
                    if (output.data[i] != 0)
                    {
                        filterColor = output.data[i];
                        break;
                    }
                }

                var isFilterValid = true;
                for (int i = 0; i < output.data.Length; i++) {
                    if (input.data[i] != filterColor && output.data[i] != 0) {
                        isFilterValid = false;
                        break;
                    }
                }

                if (isFilterValid) 
                {
                    if (filterColor == 0)
                    {
                        for (int i = 1; i < 10; i++)
                            occurrences.Add(i);  // all filters of a empty image are empty
                    }
                    else
                    {
                        occurrences.Add(filterColor);  // any other filter of nonempty image will be different
                    }
                }

                if (occurrences.Count == 0) return null;
                result[inputState] = occurrences.Cast<object>();
            }
            return new DisjunctiveExamplesSpec(result);
        }

        [WitnessFunction(nameof(Semantics.Recolor), 1)]
        public DisjunctiveExamplesSpec WitnessRecolor(GrammarRule rule, ExampleSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (KeyValuePair<State, object> example in spec.Examples)
            {
                State inputState = example.Key;
                var input = inputState[rule.Body[0]] as Image;
                var output = example.Value as Image;
                var occurrences = new List<int>();

                var newColor = 0;
                for (int i = 0; i < output.data.Length; i++)
                {
                    if (output.data[i] != 0)
                    {
                        newColor = output.data[i];
                        break;
                    }
                }

                if (newColor == 0)
                    for (int i = 1; i < 10; i++)
                        occurrences.Add(i);  // all filters of a empty image are empty
                else
                    occurrences.Add(newColor);  // any other filter of nonempty image will be different

                if (occurrences.Count == 0) return null;
                result[inputState] = occurrences.Cast<object>();
            }
            return new DisjunctiveExamplesSpec(result);
        }
    }
}