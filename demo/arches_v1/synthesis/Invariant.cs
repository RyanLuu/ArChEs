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

        private IOrderedEnumerable<int> colormap;

        public override void reseed()
        {
            colormap = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 }.OrderBy(item => Program.rnd.Next());
        }

        public override Image generate(Image input)
        {
            Image ret = input.Clone() as Image;
            for (int i = 0; i < ret.data.Length; i++)
            {
                if (ret.data[i] != 0)
                {
                    ret.data[i] = colormap.ElementAt(i - 1);
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
            Image ret = input.Clone() as Image;
            for (int i = 0; i < rots; i++)
            {
                ret = Semantics.Orthogonal(ret, 2);
            }
            return ret;
        }
    }

    public class TranslationInvariant : Invariant
    {
        int off;
        public override void reseed()
        {
            off = Program.rnd.Next(8);
        }
        public override Image generate(Image input)
        {
            // TODO: consider changing data
            Image ret = input.Clone() as Image;
            int[,] nbs = { {-1,-1},{-1,0},{-1,1},{0,-1},{0,1},{1,-1},{1,0},{1,1} };
            ret.x += nbs[off, 0];
            ret.y += nbs[off, 1];
            return ret;
        }
    }
}