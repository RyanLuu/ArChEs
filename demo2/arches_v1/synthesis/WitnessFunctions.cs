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
        [WitnessFunction(nameof(Semantics.Recolor), 0, DependsOnParameters = new[] { 1 })]
        public PartialImageSpec WitnessRecolor_SingleParam(GrammarRule rule, PartialImageSpec spec, ExampleSpec colorSpec)
        {
            var result = new Dictionary<State, object>();
            foreach (var example in spec.PartialImageExamples)
            {
                State inputState = example.Key;
                var output = example.Value as int[][];
                int color = (int)colorSpec.Examples[inputState];
                // create blank preimage
                int[][] preimage = new int[output.Length][];
                for (int i = 0; i < preimage.Length; i++) { preimage[i] = new int[output[i].Length]; }
                // loop through all pixels of output image
                for (int i = 0; i < output.Length; i++)
                {
                    for (int j = 0; j < output[0].Length; j++)
                    {
                        // if output is 0 then the preimage must also be 0 because recolor does not affect pixels with color 0
                        if (output[i][j] == 0) { preimage[i][j] = 0; }
                        // If it's the number on output, it could be anything on input
                        else if (output[i][j] == color) { preimage[i][j] = 10; }
                        // Negative case, the output is an over-specified negative number
                        else if (output[i][j] < 0)
                        {
                            // If our output value is -color, that won't work since that would mean
                            // we applied a recolor(image, color) and got an output that wasn't color
                            if (output[i][j] == -color) { return null; }
                            // Otherwise, that's alright! We can just set the preimage to be whatever we want (10)
                            else { preimage[i][j] = 10; }
                        }
                        else { throw new NotSupportedException(); }
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
        //public DisjunctiveExamplesSpec WitnessRecolor_ColorParam_DepSingle(GrammarRule rule, ExampleSpec spec, PartialImageSpec singleSpec)
        public DisjunctiveExamplesSpec WitnessRecolor_ColorParam(GrammarRule rule, PartialImageSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();
            foreach (KeyValuePair<State, object> example in spec.PartialImageExamples)
            {
                State inputState = example.Key;
                var output = example.Value as int[][];
                int candidate = -11;
                for (int i = 0; i < output.Length; i++)
                {
                    for (int j = 0; j < output[i].Length; j++)
                    {
                        if (output[i][j] != 0)
                        {
                            // First nonzero found!
                            if (candidate == -11) { candidate = output[i][j]; }
                            // Second unique nonzero --> invalid entry 
                            else if (candidate != output[i][j])
                            {
                                Console.WriteLine("Ending Early, found multiple nonzero values on output");
                                return null;
                            }
                        }
                    }
                }
                // No candidates found, so return null
                if (candidate == -11){return null;}
                var occurrences = new List<int>();
                // Negative number, meaning it can be anything but -candidate
                if (candidate < 0)
                {
                    Console.WriteLine("I got here");
                    for (int i = 1; i < 10; i++)
                    {
                        if (i == -candidate) { continue; }
                        occurrences.Add(i);
                    }
                }
                // Normal number so add it
                else { occurrences.Add(candidate); }
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
        [WitnessFunction(nameof(Semantics.Filter), 0, DependsOnParameters = new[] { 1 })]
        public PartialImageSpec WitnessFilter_SingleParam(GrammarRule rule, PartialImageSpec spec, ExampleSpec colorSpec)
        {
            var result = new Dictionary<State, object>();
            foreach (var example in spec.PartialImageExamples)
            {
                State inputState = example.Key;
                var output = example.Value as int[][];
                int color = (int)colorSpec.Examples[inputState];
                // create blank preimage
                int[][] preimage = new int[output.Length][];
                for (int i = 0; i < preimage.Length; i++) { preimage[i] = new int[output[i].Length]; }

                // loop through all pixels of output image
                for (int i = 0; i < output.Length; i++)
                {
                    for (int j = 0; j < output[0].Length; j++)
                    {
                        // if output is 0 then the preimage must be any color other than color, so -color 
                        if (output[i][j] == 0) { preimage[i][j] = -color; }
                        else if (output[i][j] == color) { preimage[i][j] = color; }
                        // We don't care what the output is, we just know it's nonzero and positive, like our color!
                        else if (output[i][j] == 10) { preimage[i][j] = color; }
                        // We found a positive value, but it wasn't our expected value after applying filter, 
                        // so return null! (unless it's 10!)
                        else if (output[i][j] > 0 && output[i][j] < 10) { return null; }
                        // Negative case, the output is an over-specified negative number
                        else if (output[i][j] < 0)
                        {
                            Console.WriteLine("You better believe I got here!");
                            // If our output value is -color, that's only a problem
                            // when our input is color. So set preimage to -color 
                            if (output[i][j] == -color) { preimage[i][j] = -color; }
                            // Pretty sure this scenario is impossible?
                            else { throw new Exception("Inconceivable!"); }
                        }
                        else { throw new NotSupportedException(); }
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
        public DisjunctiveExamplesSpec WitnessFilter_ColorParam(GrammarRule rule, PartialImageSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();
            foreach (KeyValuePair<State, object> example in spec.PartialImageExamples)
            {
                State inputState = example.Key;
                var output = example.Value as int[][];
                // Loop through all pixels of output image and confirm there's only 1 nonzero color value
                int candidate = -11;
                for (int i = 0; i < output.Length; i++)
                {
                    for (int j = 0; j < output[i].Length; j++)
                    {
                        if (output[i][j] != 0)
                        {
                            // First time we encounter a nonzero value, so set candidate to be that
                            if (candidate == -11) { candidate = output[i][j]; }
                            // Second unique nonzero value found; means we didn't run Filter
                            else if (candidate != output[i][j])
                            {
                                Console.WriteLine("Ending Early, found multiple nonzero values on output");
                                return null;
                            }
                        }
                    }
                }
                // Didn't get a single nonzero entry, so return null
                if (candidate == -11)
                {
                    return null;
                }
                var occurrences = new List<int>();
                // Negative number, meaning it can be anything but -candidate in [1-9]
                if (candidate < 0)
                {
                    for (int i = 1; i < 10; i++)
                    {
                        if (i == -candidate) { continue; }
                        occurrences.Add(i);
                    }
                }
                // This could be literally *any* nonzero, positive value in [1-9]!
                else if (candidate == 10)
                {
                    for (int i = 1; i < 10; i++)
                    {
                        occurrences.Add(i);
                    }
                }
                // Otherwise, it's a normal number in [1-9] and we can add it 
                else
                {
                    occurrences.Add(candidate);
                }
                result[inputState] = occurrences.Cast<object>();
            }
            return new DisjunctiveExamplesSpec(result);
        }

        public static int[][] clone(int[][] m)
        {
            int[][] copy = new int[m.Length][];
            for (int i = 0; i < m.Length; i++)
            {
                copy[i] = new int[m[0].Length];
            }
            for (int i = 0; i < m.Length; i++)
            {
                for (int j = 0; j < m.Length; j++)
                {
                    copy[i][j] = m[i][j];
                }
            }
            return copy;
        }
        public static bool compare(int[][] im1, int[][] im2)
        {
            for (int i = 0; i < im1.Length; i++)
            {
                for (int j = 0; j < im1[0].Length; j++)
                {
                    if (im1[i][j] != im2[i][j]) return false;
                }
            }
            return true;
        }

        public static void print(int[][] m)
        {
            for (int i = 0; i < m.Length; i++)
            {
                for (int j = 0; j < m[0].Length; j++)
                {
                    Console.Out.Write(m[i][j] + " ");
                }
                Console.WriteLine();
            }
        }
    }
}