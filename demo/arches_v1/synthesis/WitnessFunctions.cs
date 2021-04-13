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

                if (input.x != output.x || input.y != output.y || input.w != output.w || input.h != output.h)
                {
                    return null;
                }

                var filterColor = 0;
                for (int i = 0; i < output.data.Length; i++)
                {
                    if (output.data[i] != 0)
                    {
                        filterColor = output.data[i];
                        break;
                    }
                }

                var filterValid = true;
                for (int i = 0; i < output.data.Length; i++) {
                    if (input.data[i] != filterColor && output.data[i] != 0) {
                        filterValid = false;
                        break;
                    }
                }

                if (filterValid) 
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

                if (input.x != output.x || input.y != output.y || input.w != output.w || input.h != output.h)
                {
                    return null;
                }
                int newColor = -1;
                for (int i = 0; i < input.data.Length; i++)
                {
                    if (input.data[i] != 0)
                    {
                        if (newColor == -1)
                        {
                            newColor = output.data[i];  // set newColor
                        }
                        else
                        {
                            if (output.data[i] != newColor)
                            {
                                return null;  // invalid recolor (map to multiple colors)
                            }
                        }
                    }
                    else
                    {
                        if (output.data[i] != 0)
                        {
                            return null;  // invalid recolor (zero -> nonzero)
                        }
                    }
                }
                if (newColor == -1)
                    for (int i = 1; i < 10; i++)
                        occurrences.Add(i);  // all recolors of a empty image are empty
                else
                    occurrences.Add(newColor);



                if (occurrences.Count == 0) return null;
                result[inputState] = occurrences.Cast<object>();
            }
            return new DisjunctiveExamplesSpec(result);
        }
    }
}