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

                var isFilterValid = true;
                for (int i = 0; i < output.data.Length; i++)
                {
                    if (input.data[i] != filterColor && output.data[i] != 0)
                    {
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

        [WitnessFunction(nameof(Semantics.Orthogonal), 1)]
        public DisjunctiveExamplesSpec WitnessOrthogonal(GrammarRule rule, ExampleSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();
            int Y_AXIS = 0;
            int X_AXIS = 1;
            int ROT_90 = 2;
            foreach (KeyValuePair<State, object> example in spec.Examples)
            {
                State inputState = example.Key;
                var input = inputState[rule.Body[0]] as Image;
                var output = example.Value as Image;
                var occurrences = new List<int>();

                // If output dims correspond to the input dims
                if (input.h == output.h && input.w == output.w)
                {
                    // Check if it could have been a y_axis flip
                    bool y_axis_check = true;
                    bool x_axis_check = true;
                    for (int i = 0; i < output.h; i++)
                    {
                        for (int j = 0; j < output.w; j++)
                        {
                            y_axis_check &=
                                (output.data[i * output.w + (output.w - j - 1)] == input.data[input.w + j]);
                            x_axis_check &=
                                (output.data[(output.h - i - 1) * output.w + j] == input.data[i * input.w + j]);
                        }
                        if (!y_axis_check && !x_axis_check) { break; }
                    }
                    if (y_axis_check) { occurrences.Add(Y_AXIS); }
                    if (x_axis_check) { occurrences.Add(X_AXIS); }
                    // If the dimensions are equivalent (AKA, it's square), could have been a ROT_90...
                    if (output.h == output.w && input.h == input.w)
                    {
                        bool rot_90_check = true;
                        for (int i = 0; i < output.h; i++)
                        {
                            for (int j = 0; j < output.w; j++)
                            {
                                // Quick condition to check for square matrix
                                // output[output.w - j - 1,i] == input[i,j]
                                rot_90_check &=
                                    (output.data[(output.w - j - 1) * output.w + i] == input.data[i * input.w + j]);
                            }
                            if (!rot_90_check) { break; }
                        }
                        if (rot_90_check) { occurrences.Add(ROT_90); }
                    }
                }
                // else if, output dims don't correspond to the input dims, so probably ROT_90
                else if (output.w == input.h && output.h == input.w)
                {
                    // TODO: Determine if there's a more efficient way to do this than literally 
                    // running the function!
                    if (Semantics.Orthogonal(input, ROT_90).Equals(output)) {
                        occurrences.Add(ROT_90);
                    }
                }
                // else... This means that there's some complete mismatch in input/output dimensions
                // that can't have been the result of an orthogonal operation!
                if (occurrences.Count == 0) { return null; }
                result[inputState] = occurrences.Cast<object>();
            }
            return new DisjunctiveExamplesSpec(result);
        }
    }
}