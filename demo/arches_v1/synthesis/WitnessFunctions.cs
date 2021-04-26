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

        [WitnessFunction(nameof(Semantics.Compose), 0, DependsOnParameters = new[] { 1 })]
        public DisjunctiveImageSpec WitnessCompose_A(GrammarRule rule, DisjunctiveImageSpec spec, DisjunctiveImageSpec bSpec)
        {
            var result = new Dictionary<State, object>();
            foreach (var example in spec.dis)
            {
                State inputState = example.Key;
                var output = example.Value as DisjunctiveImage;
                if (output.isEmptySet())
                {
                    return null;
                }
                DisjunctiveImage b = (DisjunctiveImage) bSpec.dis[inputState];
                
                DisjunctiveImage preimage = new DisjunctiveImage(output.x, output.y, output.w, output.h);
                for (int ay = preimage.y; ay < preimage.y + preimage.h; ay++)
                {
                    for (int ax = preimage.x; ax < preimage.x + preimage.w; ax++)
                    {
                        Disjunction od = output.getDisjunction(ax, ay);
                        if (b.InBounds(ax, ay))
                        {
                            Disjunction bd = b.getDisjunction(ax, ay);
                            if (bd.Allows(0))
                            {
                                if (od.Equals(Disjunctions.ZERO))
                                {
                                    // output is zero ==> a and b must be zero
                                    if (!bd.Allows(0))
                                    {
                                        return null;
                                    }
                                    preimage.setDisjunction(ax, ay, new Disjunction(Disjunctions.ZERO));
                                }
                                else // output is zero or nonzero
                                {
                                    if (Disjunction.Intersect(bd, od).IsEmpty())
                                    {
                                        // output != b ==> b = 0, a must satisfy output
                                        if (!bd.Allows(0))
                                        {
                                            return null;
                                        }
                                        preimage.setDisjunction(ax, ay, new Disjunction(od.d)); // clone out of fear and respect
                                    }
                                    else
                                    {
                                        // b can satisfy output, a can be anything
                                        preimage.setDisjunction(ax, ay, new Disjunction(Disjunctions.ANY));
                                    }
                                }
                            }
                            else
                            {
                                if (Disjunction.Intersect(bd, od).IsEmpty())
                                {
                                    return null; // b and output are disjunct
                                }
                                else
                                {
                                    // b overwrites a; a can be anything
                                    preimage.setDisjunction(ax, ay, new Disjunction(Disjunctions.ANY));
                                }
                            }
                        }
                        else
                        {
                            preimage.setDisjunction(ax, ay, new Disjunction(od.d)); // clone out of fear and respect
                        }
                        
                    }
                }
                result[inputState] = preimage;
            }
            return new DisjunctiveImageSpec(result);
        }

        [WitnessFunction(nameof(Semantics.Compose), 1)]
        public DisjunctiveImageSpec WitnessCompose_B(GrammarRule rule, DisjunctiveImageSpec spec)
        {
            var result = new Dictionary<State, object>();
            foreach (var example in spec.dis)
            {
                State inputState = example.Key;
                var output = example.Value as DisjunctiveImage;
                if (output.isEmptySet())
                {
                    return null;
                }

                DisjunctiveImage preimage = new DisjunctiveImage(output.x, output.y, output.w, output.h);
                for (int ay = preimage.y; ay < preimage.y + preimage.h; ay++)
                {
                    for (int ax = preimage.x; ax < preimage.x + preimage.w; ax++)
                    {
                        Disjunction od = output.getDisjunction(ax, ay);
                        preimage.setDisjunction(ax, ay, new Disjunction(od.d).UnionWith(new Disjunction(Disjunctions.ZERO)));
                    }
                }
                result[inputState] = preimage;
            }
            return new DisjunctiveImageSpec(result);
        }

        // Witness for single in Recolor
        // Given output image return all possible preimages
        // Because there would be trillions of preimages use the DisjunctiveImageSpec for a compact representation
        // 10 -> Match any color except 0
        // -x -> Match any color except x
        // The DependsOnParameter allows us to use the color value in this function
        // colorSpec allows us to know what the color that the other Recolor witness function selected for this image
        [WitnessFunction(nameof(Semantics.Recolor), 0, DependsOnParameters = new[] { 1 })]
        public DisjunctiveImageSpec WitnessRecolor_SingleParam(GrammarRule rule, DisjunctiveImageSpec spec, ExampleSpec colorSpec)
        {
            var result = new Dictionary<State, object>();
            foreach (var example in spec.dis)
            {
                State inputState = example.Key;
                var output = example.Value as DisjunctiveImage;
                int color = (int)colorSpec.Examples[inputState];
                // create blank preimage
                DisjunctiveImage preimage = new DisjunctiveImage(output.x, output.y, output.w, output.h);
                // loop through all pixels of output image
                for (int i = 0; i < output.ddata.Length; i++)
                {
                    ISet<int> colorSet = output.ddata[i].ToSet();

                    if (colorSet.Contains(0))
                    {
                        preimage.ddata[i].UnionWith(new Disjunction(new List<int> { 0 }));
                    }
                    if (colorSet.Contains(color))
                    {
                        preimage.ddata[i].UnionWith(new Disjunction(Disjunctions.NONZERO));
                    }
                    if (preimage.ddata[i].IsEmpty()) // empty set (output not 0 or color)
                    {
                        return null;
                    }
                }
                result[inputState] = preimage;
            }
            return new DisjunctiveImageSpec(result);
        }

        // Witness for color attribute of Recolor
        // Given the output image return all possible color values
        // Because the other witness function depends on this one we cannot know the preimage
        // However the preimage is not needed to figure out what the color value could be
        [WitnessFunction(nameof(Semantics.Recolor), 1)]
        //public DisjunctiveExamplesSpec WitnessRecolor_ColorParam_DepSingle(GrammarRule rule, ExampleSpec spec, DisjunctiveImageSpec singleSpec)
        public DisjunctiveExamplesSpec WitnessRecolor_ColorParam(GrammarRule rule, DisjunctiveImageSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();
            foreach (KeyValuePair<State, object> example in spec.dis)
            {
                State inputState = example.Key;
                var output = example.Value as DisjunctiveImage;
                ISet<int> candidateSet = new HashSet<int>(new [] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });

                for (int i = 0; i < output.ddata.Length; i++)
                {
                    ISet<int> colorSet = output.ddata[i].ToSet();
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
        // Because there would be trillions of preimages use the DisjunctiveImageSpec for a compact representation
        // 10 -> Match any color except 0
        // -x -> Match any color except x
        // The DependsOnParameter allows us to use the color value in this function
        // colorSpec allows us to know what the color that the other Filter witness function selected for this image 
        [WitnessFunction(nameof(Semantics.FilterColor), 0, DependsOnParameters = new[] { 1 })]
        public DisjunctiveImageSpec WitnessFilter_SingleParam(GrammarRule rule, DisjunctiveImageSpec spec, ExampleSpec colorSpec)
        {
            var result = new Dictionary<State, object>();
            foreach (var example in spec.dis)
            {
                State inputState = example.Key;
                var output = example.Value as DisjunctiveImage;
                int color = (int)colorSpec.Examples[inputState];
                // create blank preimage
                DisjunctiveImage preimage = new DisjunctiveImage(output.x, output.y, output.w, output.h);
                // loop through all pixels of output image
                for (int i = 0; i < output.ddata.Length; i++)
                {
                    ISet<int> colorSet = output.ddata[i].ToSet();

                    if (colorSet.Contains(0))
                    {
                        preimage.ddata[i].UnionWith(new Disjunction(new List<int> { color }).Complement());
                    }
                    if (colorSet.Contains(color))
                    {
                        preimage.ddata[i].UnionWith(new Disjunction(new List<int> { color }));
                    }
                    if (preimage.ddata[i].IsEmpty()) // empty set (output not 0 or color)
                    {
                        return null;
                    }
                }
                result[inputState] = preimage;
            }
            return new DisjunctiveImageSpec(result);
        }

        // Witness for color attribute of Filter
        // Given the output image return all possible color values
        // Because the other witness function depends on this one we cannot know the preimage
        // However the preimage is not needed to figure out what the color value could be 
        [WitnessFunction(nameof(Semantics.FilterColor), 1)]
        public DisjunctiveExamplesSpec WitnessFilter_ColorParam(GrammarRule rule, DisjunctiveImageSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();
            foreach (KeyValuePair<State, object> example in spec.dis)
            {
                State inputState = example.Key;
                var output = example.Value as DisjunctiveImage;
                ISet<int> candidateSet = new HashSet<int>(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });

                for (int i = 0; i < output.ddata.Length; i++)
                {
                    ISet<int> colorSet = output.ddata[i].ToSet();
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
        public DisjunctiveExamplesSpec WitnessOrthogonal_OrthOptionParam(GrammarRule rule, DisjunctiveImageSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();
            int Y_AXIS = 0;
            int X_AXIS = 1;
            int ROT_90 = 2;
            foreach (KeyValuePair<State, object> example in spec.dis)
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
        public DisjunctiveImageSpec WitnessOrthogonal_SingleParam(GrammarRule rule, DisjunctiveImageSpec spec, ExampleSpec orthOptionSpec)
        {
            var result = new Dictionary<State, object>();
            int Y_AXIS = 0;
            int X_AXIS = 1;
            int ROT_90 = 2;
            foreach (KeyValuePair<State, object> example in spec.dis)
            {
                State inputState = example.Key;
                var output = example.Value as DisjunctiveImage;
                int orthOption = (int)orthOptionSpec.Examples[inputState];
                DisjunctiveImage preimage = null;
                // TODO: We want to handle changes in x and y
                if (orthOption == Y_AXIS || orthOption == X_AXIS)
                {
                    preimage = new DisjunctiveImage(output.x, output.y, output.w, output.h);
                }
                else if (orthOption == ROT_90) {
                    preimage = new DisjunctiveImage(output.x, output.y, output.h, output.w);
                }
                else {throw new NotSupportedException("We don't support that option for Orthogonal yet");}

                if (orthOption == Y_AXIS)
                {
                    for (int i = 0; i < output.h; i++)
                    {
                        for (int j = 0; j < output.w; j++)
                        {
                            preimage.ddata[i * preimage.w + j] = output.ddata[i * output.w + (output.w - j - 1)];
                        }
                    }
                }
                else if (orthOption == X_AXIS)
                {
                    for (int i = 0; i < output.h; i++)
                    {
                        for (int j = 0; j < output.w; j++)
                        {
                            preimage.ddata[i * preimage.w + j] = output.ddata[(output.h - i - 1) * output.w + j];
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
                                preimage.ddata[i * preimage.w + j] = output.ddata[j * output.w + (output.w - 1 - i)];
                            }
                        }
                }
                else {throw new NotSupportedException("We don't support that option for Orthogonal yet");}
                result[inputState] = preimage;
            }
            return new DisjunctiveImageSpec(result);
        }

        [WitnessFunction(nameof(Semantics.Identity), 0)]
        public DisjunctiveExamplesSpec WitnessIdentity(GrammarRule rule, DisjunctiveImageSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (var example in spec.dis)
            {
                State inputState = example.Key;
                // extract output image
                var output = example.Value as DisjunctiveImage;
                if (output == null) { return null; }
                var occurrences = new List<DisjunctiveImage>();
                occurrences.Add(output);
                result[inputState] = occurrences.Cast<object>();
            }
            return new DisjunctiveExamplesSpec(result);
        }
    }
}