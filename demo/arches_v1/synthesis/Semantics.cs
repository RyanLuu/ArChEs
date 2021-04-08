namespace Arches
{
    public static class Semantics
    {
        public static Image FilterColor(Image image, int color)
        {
            Image output = new Image(image.x, image.y, image.w, image.h);

            for (int i = 0; i < output.data.Length; i++)
            {
                output.data[i] = (image.data[i] == color) ? color : 0;
            }

            return output;
        }

        public static Image Recolor(Image image, int color)
        {
            Image output = new Image(image.x, image.y, image.w, image.h);

            for (int i = 0; i < output.data.Length; i++)
            {
                output.data[i] = (image.data[i] != 0) ? color : 0;
            }

            return output;
        }

        public static Image Origin(Image image)
        {
            Image output = image.Clone() as Image;
            output.x = 0;
            output.y = 0;
            return output;
        }
    }
}