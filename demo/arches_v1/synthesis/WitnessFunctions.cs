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

        [WitnessFunction(nameof(Semantics.Compose), 1, DependsOnParameters = new[] { 0 })]
        public AbstractImageSpec WitnessCompose_BottomParam(GrammarRule rule, AbstractImageSpec spec, AbstractImageSpec topSpec)
        {

            Program.DEBUG("Entered WitnessCompose_BottomParam");
            var result = new Dictionary<State, object>();
            foreach (var example in spec.AbstractImageExamples)
            {
                State inputState = example.Key;
                var output = example.Value as AbstractImage;
                if (output.isEmptySet())
                {
                    Program.DEBUG("null from WitnessCompose_BottomParam --> output.isEmptySet()");
                    return null;
                }
                AbstractImage top_image = (AbstractImage)topSpec.AbstractImageExamples[inputState];
                // TODO: Handle different dimensions for bottom_preimage, currently we assume output is proper dimension
                AbstractImage bottom_preimage = new AbstractImage(output.x, output.y, output.w, output.h);
                for (int ay = bottom_preimage.y; ay < bottom_preimage.y + bottom_preimage.h; ay++)
                {
                    for (int ax = bottom_preimage.x; ax < bottom_preimage.x + bottom_preimage.w; ax++)
                    {
                        AbstractValue output_ab_val = output.getAbstractValueAtPixel(ax, ay);
                        if (top_image.InBounds(ax, ay)) // Checking that the x and y coords are in the bounds for top_image 
                        {
                            AbstractValue top_image_ab_val = output.getAbstractValueAtPixel(ax, ay);
                            // If either the top_image or the output images allowed no values (not even 0), then return null
                            // as we've entered an invalid state
                            if (top_image_ab_val.IsEmpty() || output_ab_val.IsEmpty())
                            {
                                Program.DEBUG("null from WitnessCompose_BottomParam --> top or output contained no valid values");
                                return null;
                            }
                            // Check if we ONLY allow 0 on output (HammingWeight is the # of 1 bits in our inner representation)
                            if (output_ab_val.Allows(0) && output_ab_val.HammingWeight() == 1)
                            {
                                // If the top_image doesn't allow 0, this is impossible. Return null
                                if (!top_image_ab_val.Allows(0))
                                {
                                    Program.DEBUG("null from WitnessCompose_BottomParam --> top image didn't allow 0");
                                    return null;
                                }
                                // if it allows 0 and no other values, that's good, just return 0 for bottom
                                else if (top_image_ab_val.HammingWeight() >= 1)
                                {
                                    bottom_preimage.setAbstractValueAtPixel(ax, ay,
                                            new AbstractValue(AbstractConstants.ZERO));
                                }
                                else { throw new Exception("WitnessCompose_BottomParam --> This shouldn't happen"); }
                            }
                            // output is ONLY nonzeros
                            else if (!output_ab_val.Allows(0) && output_ab_val.HammingWeight() >= 1)
                            {
                                // If the top_image doesn't allow 0, that's cool. 
                                // It means that our bottom_preimage can contain literally anything
                                // since its contents will be completely ignored
                                if (!top_image_ab_val.Allows(0))
                                {
                                    bottom_preimage.setAbstractValueAtPixel(ax, ay,
                                     new AbstractValue(AbstractConstants.ANY));
                                }
                                // otherwise, if top image does allow 0, our bottom preimage
                                // can take on the union of top_image's nonzero values and 
                                // the values in output
                                // Example:
                                // output: [1,2,3]
                                // top: [0,1]
                                // bottom: [1,2,3] 
                                else
                                {
                                    // First, sanity check! Ensure the nonzero elements of top
                                    // are contained within output, otherwise that's bad and we return null
                                    AbstractValue nonzero_top_image_ab_vals = AbstractValue.Intersect(
                                        new AbstractValue(AbstractConstants.NONZERO),
                                        top_image_ab_val
                                    );
                                    if (!output_ab_val.ContainsAllColors(nonzero_top_image_ab_vals))
                                    {
                                        Program.DEBUG("null from WitnessCompose_BottomParam --> output val wasn't superset of nonzero top vals");
                                        return null;
                                    }
                                    // Now, easy-peasy, just set bottom_preimage to output
                                    bottom_preimage.setAbstractValueAtPixel(ax, ay, new AbstractValue(output_ab_val.d));
                                }
                            }
                            // output contains nonzero AND allows 0
                            else if (output_ab_val.Allows(0) && output_ab_val.HammingWeight() > 1)
                            {
                                // If the top_image doesn't allow 0, that's bad!  
                                // means we can't get 0 on output, so return null
                                if (!top_image_ab_val.Allows(0))
                                {
                                    Program.DEBUG("null from WitnessCompose_BottomParam --> top image didn't allow 0, which is impossible given output accepts zero (and some other nonzeros)");
                                    return null;
                                }
                                // otherwise, if top image does allow 0, our bottom preimage
                                // can take on the union of top_image's nonzero values and 
                                // the nonzero values in output
                                // Example:
                                // output: [0,1,2,3]
                                // top: [0,1]
                                // bottom: [1,2,3] 
                                else
                                {
                                    // First, sanity check! Ensure the nonzero elements of top
                                    // are contained within nonzero els of output, otherwise that's bad and we return null
                                    AbstractValue nonzero_top_image_ab_vals = AbstractValue.Intersect(
                                        new AbstractValue(AbstractConstants.NONZERO),
                                        top_image_ab_val
                                    );
                                    AbstractValue nonzero_output_ab_vals = AbstractValue.Intersect(
                                        new AbstractValue(AbstractConstants.NONZERO),
                                        output_ab_val
                                    );
                                    if (!nonzero_output_ab_vals.ContainsAllColors(nonzero_top_image_ab_vals))
                                    {

                                        Program.DEBUG("null from WitnessCompose_BottomParam --> nonzero output val wasn't superset of nonzero top vals");
                                        return null;
                                    }
                                    // Now, easy-peasy, just set bottom_preimage to (nonzero elements) of output
                                    bottom_preimage.setAbstractValueAtPixel(ax, ay, new AbstractValue(nonzero_output_ab_vals.d));
                                }
                            }
                            else { throw new Exception("We should not have thrown this. Check WitnessCompose_BottomParam."); }
                        }
                        else
                        {
                            // TODO: Determine if we want to support this? 
                            bottom_preimage.setAbstractValueAtPixel(ax, ay, new AbstractValue(output_ab_val.d)); // clone out of fear and respect
                        }

                    }
                }
                result[inputState] = bottom_preimage;
            }
            return new AbstractImageSpec(result);
        }

        [WitnessFunction(nameof(Semantics.Compose), 0)]
        public AbstractImageSpec WitnessCompose_TopParam(GrammarRule rule, AbstractImageSpec spec)
        {
            Program.DEBUG("Entered WitnessCompose_TopParam");
            var result = new Dictionary<State, object>();
            foreach (var example in spec.AbstractImageExamples)
            {
                State inputState = example.Key;
                var output = example.Value as AbstractImage;
                // TODO: Determine if this is a problem
                if (output.isEmptySet())
                {
                    return null;
                }

                AbstractImage preimage = new AbstractImage(output.x, output.y, output.w, output.h);
                for (int ay = preimage.y; ay < preimage.y + preimage.h; ay++)
                {
                    for (int ax = preimage.x; ax < preimage.x + preimage.w; ax++)
                    {
                        AbstractValue od = output.getAbstractValueAtPixel(ax, ay);
                        preimage.setAbstractValueAtPixel(ax, ay, new AbstractValue(od.d).UnionWith(new AbstractValue(AbstractConstants.ZERO)));
                    }
                }
                result[inputState] = preimage;
            }
            return new AbstractImageSpec(result);
        }

        // Witness for single in Recolor
        // Given output image return all possible preimages
        // Because there would be trillions of preimages use the AbstractImageSpec for a compact representation
        // 10 -> Match any color except 0
        // -x -> Match any color except x
        // The DependsOnParameter allows us to use the color value in this function
        // colorSpec allows us to know what the color that the other Recolor witness function selected for this image
        [WitnessFunction(nameof(Semantics.Recolor), 0, DependsOnParameters = new[] { 1 })]
        public AbstractImageSpec WitnessRecolor_SingleParam(GrammarRule rule, AbstractImageSpec spec, ExampleSpec colorSpec)
        {
            var result = new Dictionary<State, object>();
            foreach (var example in spec.AbstractImageExamples)
            {
                State inputState = example.Key;
                var output = example.Value as AbstractImage;
                int color = (int)colorSpec.Examples[inputState];
                // create blank preimage
                AbstractImage preimage = new AbstractImage(output.x, output.y, output.w, output.h);
                // loop through all pixels of output image
                for (int i = 0; i < output.abstract_data.Length; i++)
                {
                    ISet<int> colorSet = output.abstract_data[i].ToSet();

                    if (colorSet.Contains(0))
                    {
                        preimage.abstract_data[i].UnionWith(new AbstractValue(new List<int> { 0 }));
                    }
                    if (colorSet.Contains(color))
                    {
                        preimage.abstract_data[i].UnionWith(new AbstractValue(AbstractConstants.NONZERO));
                    }
                    if (preimage.abstract_data[i].IsEmpty()) // empty set (output not 0 or color)
                    {
                        return null;
                    }
                }
                result[inputState] = preimage;
            }
            return new AbstractImageSpec(result);
        }

        // Witness for color attribute of Recolor
        // Given the output image return all possible color values
        // Because the other witness function depends on this one we cannot know the preimage
        // However the preimage is not needed to figure out what the color value could be
        [WitnessFunction(nameof(Semantics.Recolor), 1)]
        //public DisjunctiveExamplesSpec WitnessRecolor_ColorParam_DepSingle(GrammarRule rule, ExampleSpec spec, AbstractImageSpec singleSpec)
        public DisjunctiveExamplesSpec WitnessRecolor_ColorParam(GrammarRule rule, AbstractImageSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();
            foreach (KeyValuePair<State, object> example in spec.AbstractImageExamples)
            {
                State inputState = example.Key;
                var output = example.Value as AbstractImage;
                ISet<int> candidateSet = new HashSet<int>(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });

                for (int i = 0; i < output.abstract_data.Length; i++)
                {
                    ISet<int> colorSet = output.abstract_data[i].ToSet();
                    if (!colorSet.Contains(0))
                    {
                        candidateSet.IntersectWith(colorSet);
                    }
                }
                result[inputState] = candidateSet.ToList().Cast<object>();
            }
            return new DisjunctiveExamplesSpec(result);
        }

        // Witness for single in Filter
        // Given output image return all possible preimages
        // Because there would be trillions of preimages use the AbstractImageSpec for a compact representation
        // 10 -> Match any color except 0
        // -x -> Match any color except x
        // The DependsOnParameter allows us to use the color value in this function
        // colorSpec allows us to know what the color that the other Filter witness function selected for this image 
        [WitnessFunction(nameof(Semantics.FilterColor), 0, DependsOnParameters = new[] { 1 })]
        public AbstractImageSpec WitnessFilter_SingleParam(GrammarRule rule, AbstractImageSpec spec, ExampleSpec colorSpec)
        {
            var result = new Dictionary<State, object>();
            foreach (var example in spec.AbstractImageExamples)
            {
                State inputState = example.Key;
                var output = example.Value as AbstractImage;
                int color = (int)colorSpec.Examples[inputState];
                // create blank preimage
                AbstractImage preimage = new AbstractImage(output.x, output.y, output.w, output.h);
                // loop through all pixels of output image
                for (int i = 0; i < output.abstract_data.Length; i++)
                {
                    ISet<int> colorSet = output.abstract_data[i].ToSet();

                    if (colorSet.Contains(0))
                    {
                        preimage.abstract_data[i].UnionWith(new AbstractValue(new List<int> { color }).Complement());
                    }
                    if (colorSet.Contains(color))
                    {
                        preimage.abstract_data[i].UnionWith(new AbstractValue(new List<int> { color }));
                    }
                    if (preimage.abstract_data[i].IsEmpty()) // empty set (output not 0 or color)
                    {
                        return null;
                    }
                }
                result[inputState] = preimage;
            }
            return new AbstractImageSpec(result);
        }

        // Witness for color attribute of Filter
        // Given the output image return all possible color values
        // Because the other witness function depends on this one we cannot know the preimage
        // However the preimage is not needed to figure out what the color value could be 
        [WitnessFunction(nameof(Semantics.FilterColor), 1)]
        public DisjunctiveExamplesSpec WitnessFilter_ColorParam(GrammarRule rule, AbstractImageSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();
            foreach (KeyValuePair<State, object> example in spec.AbstractImageExamples)
            {
                State inputState = example.Key;
                var output = example.Value as AbstractImage;
                ISet<int> candidateSet = new HashSet<int>(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });

                for (int i = 0; i < output.abstract_data.Length; i++)
                {
                    ISet<int> colorSet = output.abstract_data[i].ToSet();
                    if (!colorSet.Contains(0))
                    {
                        candidateSet.IntersectWith(colorSet);
                    }
                }
                result[inputState] = candidateSet.ToList().Cast<object>();
            }
            return new DisjunctiveExamplesSpec(result);
        }

        // Given Disjunctive Image Spec, return all possible transformations
        // Trivially, this is going to imply ALL of them, since for any 
        // output matrix, we can claim there existed some rotation or flip
        [WitnessFunction(nameof(Semantics.Orthogonal), 1)]
        public DisjunctiveExamplesSpec WitnessOrthogonal_OrthOptionParam(GrammarRule rule, AbstractImageSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();
            int Y_AXIS = 0;
            int X_AXIS = 1;
            int ROT_90 = 2;
            foreach (KeyValuePair<State, object> example in spec.AbstractImageExamples)
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
        public AbstractImageSpec WitnessOrthogonal_SingleParam(GrammarRule rule, AbstractImageSpec spec, ExampleSpec orthOptionSpec)
        {
            var result = new Dictionary<State, object>();
            int Y_AXIS = 0;
            int X_AXIS = 1;
            int ROT_90 = 2;
            foreach (KeyValuePair<State, object> example in spec.AbstractImageExamples)
            {
                State inputState = example.Key;
                var output = example.Value as AbstractImage;
                int orthOption = (int)orthOptionSpec.Examples[inputState];
                AbstractImage preimage = null;
                // TODO: We want to handle changes in x and y
                if (orthOption == Y_AXIS || orthOption == X_AXIS)
                {
                    preimage = new AbstractImage(output.x, output.y, output.w, output.h);
                }
                else if (orthOption == ROT_90)
                {
                    preimage = new AbstractImage(output.x, output.y, output.h, output.w);
                }
                else { throw new NotSupportedException("We don't support that option for Orthogonal yet"); }

                if (orthOption == Y_AXIS)
                {
                    for (int i = 0; i < output.h; i++)
                    {
                        for (int j = 0; j < output.w; j++)
                        {
                            preimage.abstract_data[i * preimage.w + j] = output.abstract_data[i * output.w + (output.w - j - 1)];
                        }
                    }
                }
                else if (orthOption == X_AXIS)
                {
                    for (int i = 0; i < output.h; i++)
                    {
                        for (int j = 0; j < output.w; j++)
                        {
                            preimage.abstract_data[i * preimage.w + j] = output.abstract_data[(output.h - i - 1) * output.w + j];
                        }
                    }
                }
                else if (orthOption == ROT_90)
                {
                    // TODO: Verify this
                    for (int i = 0; i < preimage.h; i++) // n = preimage.h
                    {
                        for (int j = 0; j < preimage.w; j++) // m = preimage.w
                        {
                            // preimage[i,j] = output[j,n-1-i];
                            preimage.abstract_data[i * preimage.w + j] = output.abstract_data[j * output.w + (output.w - 1 - i)];
                        }
                    }
                }
                else { throw new NotSupportedException("We don't support that option for Orthogonal yet"); }
                result[inputState] = preimage;
            }
            return new AbstractImageSpec(result);
        }

        [WitnessFunction(nameof(Semantics.Identity), 0)]
        public AbstractImageSpec WitnessIdentity(GrammarRule rule, AbstractImageSpec spec)
        {
            var result = new Dictionary<State, object>();

            foreach (KeyValuePair<State, object> example in spec.AbstractImageExamples)
            {
                State inputState = example.Key;
                // extract output image
                var output = example.Value as AbstractImage;
                if (output == null) { return null; }
                result[inputState] = new AbstractImage(output);
            }
            return new AbstractImageSpec(result);
        }
    }
}