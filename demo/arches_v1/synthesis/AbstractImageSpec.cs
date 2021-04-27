using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.Specifications;
using Microsoft.ProgramSynthesis.Specifications.Serialization;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Arches
{
    public static class AbstractConstants 
    {
        public const int ANY = 0b1111111111;
        public const int NONE = 0b0000000000;
        public const int NONZERO = 0b1111111110;
        public const int ZERO = 0b0000000001;
    }

    public class AbstractValue 
    {
        
        public int d;

        public  AbstractValue()
        {
            this.d = 0; // empty set
        }

        public  AbstractValue(AbstractValue ab_val) // We expect this to be a color!
        {
            this.d = ab_val.d;
        }

        public  AbstractValue(int internal_data) // We expect this to NOT be a color!
        {
            this.d = internal_data;
        }

        public  AbstractValue(List<int> colors)
        {
            this.d = 0;
            colors.ForEach(color => d |= 1 << color);
        }

        public bool Allows(int color)
        {
            return ((d >> color) & 1) == 1;
        }

        public bool ContainsAllColors(AbstractValue ab_val)
        {
            bool result = true;
            for (int i = 0; i < 10; i++)
            {
                if (ab_val.Allows(i) && !this.Allows(i))
                {
                    result = false;
                    break;
                }
            }
            return result;
        }


         public int HammingWeight()
        {
            // count number of 1 bits in the d int
            int count = 0;
            for (int i = 0; i < 10; i++) {if (this.Allows(i)) {count++;}}
            return count;
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

        public  AbstractValue Complement()
        {
            return new  AbstractValue(d ^ 0b1111111111);
        }

        public  AbstractValue UnionWith( AbstractValue other)
        {
            return new  AbstractValue(this.d |= other.d);
        }

        public static  AbstractValue Intersect( AbstractValue a,  AbstractValue b)
        {
            return new  AbstractValue(a.d & b.d);
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
            else if (obj is  AbstractValue)
            {
                return this.d == (( AbstractValue) obj).d;
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
    public class AbstractImage
    {
        public int x, y;
        public int w, h;
        public  AbstractValue[] abstract_data;

        public AbstractImage(int x, int y, int w, int h)
        {
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;
            this.abstract_data = new  AbstractValue[w * h];
            for (int i = 0; i < this.abstract_data.Length; i++)
            {
                this.abstract_data[i] = new  AbstractValue();  // all pixels are empty set
            }
        }

        public AbstractImage(Image image)
        {
            this.x = image.x;
            this.y = image.y;
            this.w = image.w;
            this.h = image.h;
            this.abstract_data = new  AbstractValue[image.data.Length];
            for (int i = 0; i < this.abstract_data.Length; i++)
            {
                this.abstract_data[i] = new  AbstractValue(new List<int> { image.data[i] });
                Program.DEBUG("");
                Program.DEBUG("" + image.data[i]);
                Program.DEBUG("" + this.abstract_data[i]);
                Program.DEBUG("");
            }
        }
        public AbstractImage(AbstractImage abstract_image)
        {
            this.x = abstract_image.x;
            this.y = abstract_image.y;
            this.w = abstract_image.w;
            this.h = abstract_image.h;
            this.abstract_data = (AbstractValue[]) abstract_image.abstract_data.Clone();
        }

        public bool InBounds(int ax, int ay)
        {
            return ax >= this.x && ax < this.x + this.w && ay >= this.y && ay < this.y + this.h;
        }

        public bool isEmptySet()
        {
            for (int i = 0; i < this.abstract_data.Length; i++)
            {
                if (this.abstract_data[i].IsEmpty())
                {
                    return true;
                }
            }
            return false;
        }

        public  AbstractValue getAbstractValueAtPixel(int ax, int ay)
        {
            if (ax < this.x || ay < this.y || ax >= this.x + this.w || ay >= this.y + this.h)
            {
                return new  AbstractValue( AbstractConstants.ANY);
            }
            return this.abstract_data[(ay - this.y) * this.w + (ax - this.x)];
        }

        public void setAbstractValueAtPixel(int ax, int ay,  AbstractValue d)
        {
            if (ax < this.x || ay < this.y || ax >= this.x + this.w || ay >= this.y + this.h)
            {
                throw new ArgumentOutOfRangeException();
            }
            this.abstract_data[(ay - this.y) * this.w + (ax - this.x)] = d;
        }

        public override string ToString()
        {
            string ret = "";
            for (int i = 0; i < this.abstract_data.Length; i++)
            {
                ret += this.abstract_data[i] + " ";
                if (i % this.w == this.w - 1)
                {
                    ret += "\n";
                }
            }
            return ret;
        }

        public override bool Equals(object obj)
        {
            AbstractImage other = obj as AbstractImage;
            if (other == null)
            {
                return false;
            }
            if (this.x != other.x || this.y != other.y || this.w != other.w || this.h != other.h)
            {
                return false;
            }
            for (int i = 0; i < this.abstract_data.Length; i++)
            {
                if (this.abstract_data[i] != other.abstract_data[i])
                {
                    return false;
                }
            }
            return true;
        }
    }

    public class AbstractImageSpec : Spec
    {

        public IDictionary<State, object> AbstractImageExamples;
        public AbstractImageSpec(IDictionary<State, object> AbstractImageExamples) : base(AbstractImageExamples.Keys)
        {
            this.AbstractImageExamples = AbstractImageExamples;
        }

        protected override bool CorrectOnProvided(State state, object output)
        {
            AbstractImage space = this.AbstractImageExamples[state] as AbstractImage;
            Image candidate = output as Image;
            Program.DEBUG("space");
            Program.DEBUG(space.ToString());
            Program.DEBUG("candidate");
            Program.DEBUG(candidate.ToString());

            for (int ay = space.y; ay < space.y + space.h; ay++)
            {
                for (int ax = space.x; ax < space.x + space.w; ax++)
                {
                    if (!space.getAbstractValueAtPixel(ax, ay).Allows(candidate.getPixel(ax, ay)))
                    {
                        Program.DEBUG(String.Format("space[{0},{1}] = {2}",ax,ay,space.getAbstractValueAtPixel(ax,ay)));
                        Program.DEBUG(String.Format("candidate[{0},{1}] = {2}",ax,ay,candidate.getPixel(ax,ay)));
                        Program.DEBUG(String.Format("{0},{1}",ax,ay));

                        Program.DEBUG("false");
                        return false;
                    }
                }
            }
                        Program.DEBUG("true");
            return true;
        }

        protected override bool EqualsOnInput(State state, Spec other)
        {
            AbstractImageSpec otherDIS = (AbstractImageSpec)other;
            AbstractImage d0 = (AbstractImage)this.AbstractImageExamples[state];
            AbstractImage d1 = (AbstractImage)otherDIS.AbstractImageExamples[state];
            return d0.Equals(d1);
        }

        protected override int GetHashCodeOnInput(State state)
        {
            return this.AbstractImageExamples[state].GetHashCode();
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
            foreach (var input in this.AbstractImageExamples.Keys)
            {
                result[transformer(input)] = this.AbstractImageExamples[input];
            }
            return new AbstractImageSpec(result);
        }
    }
}