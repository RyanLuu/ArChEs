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
                var output = example.Value as Image;
                int color = (int)colorSpec.Examples[inputState];
                // create blank preimage
                Image preimage = new Image(output.x, output.y, output.w, output.h);
                // loop through all pixels of output image
                for (int i = 0; i < output.data.Length; i++)
                {
                    // if output is 0 then the preimage must also be 0 because recolor does not affect pixels with color 0
                    if (output.data[i] == 0) { preimage.data[i] = 0; }
                    // If it's the number on output, it could be anything on input
                    else if (output.data[i] == color) { preimage.data[i] = 10; }
                    // If it's 10, then that's bad (that means "any nonzero positive number in [1-9]!"
                    // We want the color clamp down the value from something partial like 10 --> color
                    else if (output.data[i] == 10) { 
                        Console.WriteLine("Ending early on WitnessRecolor_SingleParam, value 10 on output");
                        return null; 
                    }
                    // If it's a positive number that isn't 10, that's a bad sign 
                    // It means we couldn't have done recolor with the color we were provided!
                    else if (output.data[i] > 0 && output.data[i] < 10)
                    {
                        Console.WriteLine("Ending Early in WitnessRecolor_SingleParam, found multiple nonzero values on output");
                        return null;
                    }
                    // Negative case, the output is an over-specified negative number
                    else if (output.data[i] < 0)
                    {
                        // If our output value is -color, that won't work since that would mean
                        // we applied a recolor(image, color) and got an output that wasn't color
                        if (output.data[i] == -color)
                        {
                            Console.WriteLine("Ending Early in WitnessRecolor_SingleParam, found -color on output for Recolor(image, color)");
                            return null;
                        }
                        // Otherwise, that's alright! We can just set the preimage to be whatever we want (10)
                        else { preimage.data[i] = 10; }
                    }
                    else { throw new NotSupportedException(); }
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
                var output = example.Value as Image;
                int candidate = -11;
                for (int i = 0; i < output.data.Length; i++)
                {
                    if (output.data[i] != 0)
                    {
                        // First nonzero found!
                        if (candidate == -11) { candidate = output.data[i]; }
                        // Second unique nonzero --> invalid entry 
                        else if (candidate != output.data[i])
                        {
                            Console.WriteLine("Ending Early on WitnessRecolor_ColorParam, found multiple nonzero values on output");
                            return null;
                        }
                    }
                }
                // No candidates found, so return null
                if (candidate == -11)
                {
                    Console.WriteLine("Ending Early on WitnessRecolor_ColorParam, all zeroes on output");
                    return null;
                }
                var occurrences = new List<int>();
                // Negative number, meaning it can be anything but -candidate
                if (candidate < 0)
                {
                    for (int i = 1; i < 10; i++)
                    {
                        if (i == -candidate) { continue; }
                        occurrences.Add(i);
                    }
                }
                // Could be any number at all
                else if (candidate == 10)
                {
                    Console.WriteLine("Got here");
                    for (int i = 1; i < 10; i++) { occurrences.Add(i); }
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
        [WitnessFunction(nameof(Semantics.FilterColor), 0, DependsOnParameters = new[] { 1 })]
        public PartialImageSpec WitnessFilter_SingleParam(GrammarRule rule, PartialImageSpec spec, ExampleSpec colorSpec)
        {
            var result = new Dictionary<State, object>();
            foreach (var example in spec.PartialImageExamples)
            {
                State inputState = example.Key;
                var output = example.Value as Image;
                int color = (int)colorSpec.Examples[inputState];
                // create blank preimage
                Image preimage = new Image(output.x, output.y, output.w, output.h);
                // loop through all pixels of output image
                for (int i = 0; i < output.data.Length; i++)
                {
                    // if output is 0 then the preimage must be any color other than color, so -color 
                    if (output.data[i] == 0) { preimage.data[i] = -color; }
                    else if (output.data[i] == color) { preimage.data[i] = color; }
                    // If the value is 10, we didn't clamp down with filter, so return null
                    else if (output.data[i] == 10) { 
                        Console.WriteLine("Ending early on WitnessFilter_SingleParam, value 10 on output");
                        return null; 
                    }
                    // We found a positive value, but it wasn't our expected value after applying filter, 
                    // so return null! (unless it's 10!)
                    else if (output.data[i] > 0 && output.data[i] < 10)
                    {
                        Console.WriteLine("Ending early on WitnessFilter_SingleParam, multiple nonzero values on output");
                        return null;
                    }
                    // Negative case, the output is an over-specified negative number
                    else if (output.data[i] < 0)
                    {
                        // If our output value is -color, that's only a problem
                        // when our input is color. So set preimage to -color 
                        if (output.data[i] == -color) { preimage.data[i] = -color; }
                        // Pretty sure this scenario is impossible?
                        else { throw new Exception("Inconceivable!"); }
                    }
                    else { throw new NotSupportedException(); }
                }
                result[inputState] = preimage;
            }
            return new PartialImageSpec(result);
        }

        // Witness for color attribute of Filter
        // Given the output image return all possible color values
        // Because the other witness function depends on this one we cannot know the preimage
        // However the preimage is not needed to figure out what the color value could be 
        [WitnessFunction(nameof(Semantics.FilterColor), 1)]
        public DisjunctiveExamplesSpec WitnessFilter_ColorParam(GrammarRule rule, PartialImageSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();
            foreach (KeyValuePair<State, object> example in spec.PartialImageExamples)
            {
                State inputState = example.Key;
                var output = example.Value as Image;
                // Loop through all pixels of output image and confirm there's only 1 nonzero color value
                int candidate = -11;
                for (int i = 0; i < output.data.Length; i++)
                {
                    if (output.data[i] != 0)
                    {
                        // First time we encounter a nonzero value, so set candidate to be that
                        if (candidate == -11) { candidate = output.data[i]; }
                        // Second unique nonzero value found; means we didn't run Filter
                        else if (candidate != output.data[i])
                        {
                            Console.WriteLine("Ending Early on WitnessFilter_ColorParam, found multiple nonzero values on output");
                            return null;
                        }
                    }
                }
                // Didn't get a single nonzero entry, so return null
                if (candidate == -11)
                {
                    Console.WriteLine("Ending Early on WitnessFilter_ColorParam, all zeroes on output");
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

        // Given Partial Image Spec, return all possible transformations
        // Trivially, this is going to imply ALL of them, since for any 
        // output matrix, we can claim there existed some rotation or flip
        [WitnessFunction(nameof(Semantics.Orthogonal), 1)]
        public DisjunctiveExamplesSpec WitnessOrthogonal_OrthOptionParam(GrammarRule rule, PartialImageSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();
            int Y_AXIS = 0;
            int X_AXIS = 1;
            int ROT_90 = 2;
            foreach (KeyValuePair<State, object> example in spec.PartialImageExamples)
            {
                State inputState = example.Key;
                var occurrences = new List<int>();
                occurrences.Add(Y_AXIS);
                occurrences.Add(X_AXIS);
                occurrences.Add(ROT_90);
                result[inputState] = occurrences.Cast<object>();
            }
            return new DisjunctiveExamplesSpec(result);
        }


        /*
        Origin Function
        - you don't mess w/ x and y when you rotate
        - snapping back to 0 post-rotation
        */

        [WitnessFunction(nameof(Semantics.Orthogonal), 0, DependsOnParameters = new[] { 1 })]
        public PartialImageSpec WitnessOrthogonal_SingleParam(GrammarRule rule, PartialImageSpec spec, ExampleSpec orthOptionSpec)
        {
            var result = new Dictionary<State, object>();
            int Y_AXIS = 0;
            int X_AXIS = 1;
            int ROT_90 = 2;
            foreach (KeyValuePair<State, object> example in spec.PartialImageExamples)
            {
                State inputState = example.Key;
                var output = example.Value as Image;
                int orthOption = (int)orthOptionSpec.Examples[inputState];
                Image preimage = null;
                // TODO: We want to handle changes in x and y
                if (orthOption == Y_AXIS) { preimage = new Image(output.x, output.y, output.w, output.h); }
                else if (orthOption == X_AXIS) { preimage = new Image(output.x, output.y, output.w, output.h); }
                else if (orthOption == ROT_90) { preimage = new Image(output.x, output.y, output.h, output.w); }
                else {throw new NotSupportedException("We don't support that option for Orthogonal yet");}

                if (orthOption == Y_AXIS)
                {
                    for (int i = 0; i < output.h; i++)
                    {
                        for (int j = 0; j < output.w; j++)
                        {
                            preimage.data[preimage.w + j] = output.data[i * output.w + (output.w - j - 1)];
                        }
                    }
                }
                else if (orthOption == X_AXIS)
                {
                    for (int i = 0; i < output.h; i++)
                    {
                        for (int j = 0; j < output.w; j++)
                        {
                            preimage.data[i * preimage.w + j] = output.data[(output.h - i - 1) * output.w + j];
                        }
                    }
                }
                else if (orthOption == X_AXIS)
                {
                    // TODO: Verify this
                        for (int i = 0; i < preimage.h; i++) // n = preimage.h
                        {
                            for (int j = 0; j < preimage.w; j++) // m = preimage.w
                            {
                                // preimage[i,j] = output[j,n-1-i];
                                preimage.data[i * preimage.w + j] = output.data[j * output.w + (output.w - 1 - i)];
                            }
                        }
                }
                else {throw new NotSupportedException("We don't support that option for Orthogonal yet");}
                result[inputState] = preimage;
            }
            return new PartialImageSpec(result);
        }

        [WitnessFunction(nameof(Semantics.Identity), 0)]
        public DisjunctiveExamplesSpec WitnessIdentity(GrammarRule rule, PartialImageSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (var example in spec.PartialImageExamples)
            {
                Console.WriteLine("1");
                State inputState = example.Key;
                Console.WriteLine(example.Key);
                Console.WriteLine(example.Value as Image);
                Console.WriteLine(example.Value as Image == null);
                // extract output image
                var output = example.Value as Image;
                if (output == null) { return null; }
                var occurrences = new List<Image>();
                occurrences.Add(output);
                result[inputState] = occurrences.Cast<object>();
            }
            return new DisjunctiveExamplesSpec(result);
        }
    }
}