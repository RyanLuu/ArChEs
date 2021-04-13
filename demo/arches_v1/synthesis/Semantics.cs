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

        public static Image Orthogonal(Image inputImage, int option)
        {
            int[,] inputArr = inputImage.toArray();
            int[,] outputArr = null;

            int max_index_d0 = -1;
            int max_index_d1 = -1;
            switch (option)
            {
                case 0: // Y_AXIS
                    max_index_d0 = inputArr.GetUpperBound(0);
                    max_index_d1 = inputArr.GetUpperBound(1);
                    // Step 1: Flip the rows along the Y_AXIS (horizontal)
                    outputArr = new int[max_index_d0 + 1, max_index_d1 + 1];
                    for (int i = 0; i <= max_index_d0; i++)
                    {
                        for (int j = 0; j <= max_index_d1; j++)
                        {
                            outputArr[i,j] = inputArr[i, max_index_d1 - j];
                        }
                    }
                    break;
                case 1: // X_AXIS
                    max_index_d0 = inputArr.GetUpperBound(0);
                    max_index_d1 = inputArr.GetUpperBound(1);
                    // Step 1: Flip the rows along the X_AXIS (vertical)
                    outputArr = new int[max_index_d0 + 1, max_index_d1 + 1];
                    for (int i = 0; i <= max_index_d0; i++)
                    {
                        for (int j = 0; j <= max_index_d1; j++)
                        {
                            outputArr[i,j] = inputArr[max_index_d0 - i, j];
                        }
                    }
                    break;
                case 2: // ROT_90
                    // Step 0: Avoid re-computing upper bounds throughout (note dimensions are flipped)
                    max_index_d0 = inputArr.GetUpperBound(1);
                    max_index_d1 = inputArr.GetUpperBound(0);
                    // Step 1: Find the transpose of the matrix
                    int[,] tmpArr = new int[max_index_d0 + 1, max_index_d1 + 1];
                    for (int i = 0; i <= max_index_d0; i++)
                    {
                        for (int j = 0; j <= max_index_d1; j++)
                        {
                            tmpArr[i,j] = inputArr[j,i];
                        }
                    }
                    // Step 2: Reverse rows in the transpose
                    outputArr = new int[max_index_d0 + 1, max_index_d1 + 1];
                    for (int i = 0; i <= max_index_d0; i++) {
                        for (int j = 0; j <= max_index_d1; j++) {
                            outputArr[i,j] = tmpArr[i,max_index_d1 - j];
                        }
                    }
                    // Step 3: Profit (Convert this back into an Image)
                    break;
                default:
                    break;
            }
            // return null if option wasn't supported, or our image
            if (outputArr == null) {
                return null;
            }
            return new Image(outputArr);
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