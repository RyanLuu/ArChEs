using System.Collections.Generic;
using System.Linq;

namespace Arches
{
    public abstract class Invariant
    {

        public Invariant()
        {
            reseed();
        }

        public abstract void reseed();
        public abstract Image generate(Image input);
        public List<Image> generateN(Image input, int n)
        {
            List<Image> ret = new List<Image>();
            for (int i = 0; i < n; i++)
            {
                ret.Add(generate(input));
                reseed();
            }
            return ret;
        }
    }

    public class ColormapInvariant : Invariant
    {

        private int[] colormap;

        public override void reseed()
        {
            colormap = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 }.OrderBy(item => Program.rnd.Next()).ToArray<int>();
        }

        public override Image generate(Image input)
        {
            Image ret = input.Clone() as Image;
            for (int i = 0; i < ret.data.Length; i++)
            {
                if (ret.data[i] != 0)
                {
                    ret.data[i] = colormap.ElementAt(ret.data[i] - 1);
                }
            }
            return ret;
        }
    }
    public class RotationInvariant : Invariant
    {
        int rots;
        public override void reseed()
        {
            rots = Program.rnd.Next(3) + 1;
        }

        public override Image generate(Image input)
        {
            Image ret = input;
            for (int i = 0; i < rots; i++)
            {
                ret = Semantics.Orthogonal(ret, 2);
            }
            return ret;
        }
    }

    public class ReflectionInvariant : Invariant
    {
        bool x;

        public override void reseed()
        {
            x = Program.rnd.Next() % 2 == 0;
        }

        public override Image generate(Image input)
        {
            return x ? Semantics.Orthogonal(input, 1) : Semantics.Orthogonal(input, 0);
        }
    }

    public class TranslationInvariant : Invariant
    {
        bool x, y;
        public override void reseed()
        {
            int r = Program.rnd.Next(3);
            x = r == 0 || r == 2;
            y = r == 1 || r == 2;
        }
        public override Image generate(Image input)
        {
            Image ret = new Image(input.x, input.y, input.w + (x ? 1 : 0), input.h + (y ? 1 : 0));
            for (int i = 0; i < input.data.Length; i++)
            {
                ret.data[i + (x ? (i / input.w + 1) : 0) + (y ? ret.w : 0)] = input.data[i];
            }
            return ret;
        }
    }
}
