using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.Specifications;
using Microsoft.ProgramSynthesis.Specifications.Serialization;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Arches
{
    public class DisjunctiveImage
    {
        public int x, y;
        public int w, h;
        public int[] ddata;

        public DisjunctiveImage(int x, int y, int w, int h)
        {
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;
            this.ddata = new int[w * h];
            for (int i = 0; i < this.ddata.Length; i++)
            {
                this.ddata[i] = 0;  // all pixels are empty set
            }
        }

        public DisjunctiveImage(Image image)
        {
            this.x = image.x;
            this.y = image.y;
            this.w = image.w;
            this.h = image.h;
            this.ddata = new int[image.data.Length];
            for (int i = 0; i < this.ddata.Length; i++)
            {
                this.ddata[i] = 1 << image.data[i];
            }
        }
        public bool IsSingleton()
        {
            for (int i = 0; i < this.ddata.Length; i++)
            {
                if (DisjunctToSet(this.ddata[i]).Count != 1)
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsEmptySet()
        {
            for (int i = 0; i < this.ddata.Length; i++)
            {
                if (DisjunctToSet(this.ddata[i]).Count == 0)
                {
                    return true;
                }
            }
            return false;
        }


        // HELPER FUNCTIONS
        public static int DisjunctAny     = 0b1111111111;
        public static int DisjunctNone    = 0b0000000000;
        public static int DisjunctNonzero = 0b1111111110;

        public static int DisjunctComplement(int disjunct)
        {
            return disjunct ^ 0b1111111111;
        }

        public static int ColorToDisjunct(int color)
        {
            return 1 << color;
        }

        public static bool DisjunctAllows(int disjunct, int color)
        {
            return ((disjunct >> color) & 1) == 1;
        }

        public static ISet<int> DisjunctToSet(int disjunct)
        {
            ISet<int> ret = new HashSet<int>();
            for (int i = 0; i < 10; i++)
            {
                if (DisjunctAllows(disjunct, i))
                {
                    ret.Add(i);
                }
            }
            return ret;
        }
    }

    public class DisjunctiveImageSpec : Spec
    {

        public IDictionary<State, object> dis;
        public DisjunctiveImageSpec(IDictionary<State, object> dis) : base(dis.Keys)
        {
            this.dis = dis;
        }

        protected override bool CorrectOnProvided(State state, object output)
        {
            DisjunctiveImage space = this.dis[state] as DisjunctiveImage;
            Image candidate = output as Image;
            
            if (candidate.h != space.h || candidate.w != space.w)
            {
                return false;
            }

            for (int i = 0; i < candidate.data.Length; i++)
            {
                if (!DisjunctiveImage.DisjunctAllows(space.ddata[i], candidate.data[i]))
                {
                    return false;
                }
            }
            return true;
        }

        protected override bool EqualsOnInput(State state, Spec other)
        {
            throw new NotImplementedException();
        }

        protected override int GetHashCodeOnInput(State state)
        {
            return this.dis[state].GetHashCode();
        }

        protected override XElement InputToXML(State input, Dictionary<object, int> identityCache)
        {
            throw new NotImplementedException();
        }

        protected override XElement SerializeImpl(Dictionary<object, int> identityCache, SpecSerializationContext context)
        {
            throw new NotImplementedException();
        }

        protected override Spec TransformInputs(Func<State, State> transformer)
        {
            var result = new Dictionary<State, object>();
            foreach (var input in this.dis.Keys)
            {
                result[transformer(input)] = this.dis[input];
            }
            return new DisjunctiveImageSpec(result);
        }
    }
}