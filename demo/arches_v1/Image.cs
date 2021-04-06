using System;

namespace Arches
{
	public class Image
    {
        public int x, y;
        public int w, h;
        public int[] data;

        public Image(int x, int y, int w, int h)
        {
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;
            this.data = new int[w * h];
        }

        public Image(int[,] array)
        {
            this.x = 0;
            this.y = 0;
            this.w = array.GetLength(1);
            this.h = array.GetLength(0);
            this.data = new int[array.Length];
            int k = 0;
            for (int i = 0; i <= array.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= array.GetUpperBound(1); j++)
                {
                    this.data[k++] = array[i, j];
                }
            }
        }

        public bool isEmpty()
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] != 0)
                {
                    return false;
                }
            }
            return true;
        }

        public override string ToString()
        {
            string s = "";
            for (int i = 0; i < this.data.Length; i++)
            {
                s += this.data[i];
                if (i % w == w - 1)
                {
                    s += '\n';
                }
            }
            return s;
        }
    }
}