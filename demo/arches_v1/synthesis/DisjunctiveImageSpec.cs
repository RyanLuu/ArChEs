using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.Specifications;
using Microsoft.ProgramSynthesis.Specifications.Serialization;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Arches
{
    public static class Disjunctions
    {
        public const int ANY = 0b1111111111;
        public const int NONE = 0b0000000000;
        public const int NONZERO = 0b1111111110;
        public const int ZERO = 0b0000000001;
    }

    public class Disjunction
    {
        
        public int d;

        public Disjunction()
        {
            this.d = 0; // empty set
        }

        public Disjunction(int d)
        {
            this.d = d;
        }

        public Disjunction(List<int> colors)
        {
            this.d = 0;
            colors.ForEach(color => d |= 1 << color);
        }

        public bool Allows(int color)
        {
            return ((d >> color) & 1) == 1;
        }

        public ISet<int> ToSet()
        {
            ISet<int> ret = new HashSet<int>();
            for (int i = 0; i < 10; i++)
            {
                if (Allows(i))
                {
                    ret.Add(i);
                }
            }
            return ret;
        }

        public Disjunction Complement()
        {
            return new Disjunction(d ^ 0b1111111111);
        }

        public Disjunction UnionWith(Disjunction other)
        {
            return new Disjunction(this.d |= other.d);
        }

        public static Disjunction Intersect(Disjunction a, Disjunction b)
        {
            return new Disjunction(a.d & b.d);
        }

        public Boolean IsEmpty()
        {
            return this.d == 0;
        }

        public override bool Equals(object obj)
        {
            if (obj is int)
            {
                return this.d == (int) obj;
            }
            else if (obj is Disjunction)
            {
                return this.d == ((Disjunction) obj).d;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.d.GetHashCode();
        }

        public override string ToString()
        {
            return Convert.ToString(this.d, 2).PadLeft(10, '0');
        }
    }
    public class DisjunctiveImage
    {
        public int x, y;
        public int w, h;
        public Disjunction[] ddata;

        public DisjunctiveImage(int x, int y, int w, int h)
        {
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;
            this.ddata = new Disjunction[w * h];
            for (int i = 0; i < this.ddata.Length; i++)
            {
                this.ddata[i] = new Disjunction();  // all pixels are empty set
            }
        }

        public DisjunctiveImage(Image image)
        {
            this.x = image.x;
            this.y = image.y;
            this.w = image.w;
            this.h = image.h;
            this.ddata = new Disjunction[image.data.Length];
            for (int i = 0; i < this.ddata.Length; i++)
            {
                this.ddata[i] = new Disjunction(new List<int> { image.data[i] });
            }
        }

        public bool InBounds(int ax, int ay)
        {
            return ax >= this.x && ax < this.x + this.w && ay >= this.y && ay < this.y + this.h;
        }

        public bool isEmptySet()
        {
            for (int i = 0; i < this.ddata.Length; i++)
            {
                if (this.ddata[i].IsEmpty())
                {
                    return true;
                }
            }
            return false;
        }

        public Disjunction getDisjunction(int ax, int ay)
        {
            if (ax < this.x || ay < this.y || ax >= this.x + this.w || ay >= this.y + this.h)
            {
                return new Disjunction(Disjunctions.ANY);
            }
            return this.ddata[(ay - this.y) * this.w + (ax - this.x)];
        }

        public void setDisjunction(int ax, int ay, Disjunction d)
        {
            if (ax < this.x || ay < this.y || ax >= this.x + this.w || ay >= this.y + this.h)
            {
                throw new ArgumentOutOfRangeException();
            }
            this.ddata[(ay - this.y) * this.w + (ax - this.x)] = d;
        }

        public override string ToString()
        {
            string ret = "";
            for (int i = 0; i < this.ddata.Length; i++)
            {
                ret += this.ddata[i] + " ";
                if (i % this.w == this.w - 1)
                {
                    ret += "\n";
                }
            }
            return ret;
        }

        public override bool Equals(object obj)
        {
            DisjunctiveImage other = obj as DisjunctiveImage;
            if (other == null)
            {
                return false;
            }
            if (this.x != other.x || this.y != other.y || this.w != other.w || this.h != other.h)
            {
                return false;
            }
            for (int i = 0; i < this.ddata.Length; i++)
            {
                if (this.ddata[i] != other.ddata[i])
                {
                    return false;
                }
            }
            return true;
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
            
            for (int ay = space.y; ay < space.y + space.h; ay++)
            {
                for (int ax = space.x; ax < space.x + space.w; ax++)
                {
                    if (!space.getDisjunction(ax, ay).Allows(candidate.getPixel(ax, ay)))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        protected override bool EqualsOnInput(State state, Spec other)
        {
            DisjunctiveImageSpec otherDIS = (DisjunctiveImageSpec)other;
            DisjunctiveImage d0 = (DisjunctiveImage)this.dis[state];
            DisjunctiveImage d1 = (DisjunctiveImage)otherDIS.dis[state];
            return d0.Equals(d1);
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