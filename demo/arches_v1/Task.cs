using System;
using System.IO;
using Newtonsoft.Json;

namespace Arches
{
	public class TaskLoader
    {
		public static Task LoadTask(string filepath)
		{
			using (StreamReader r = new StreamReader(filepath))
			{
				string json = r.ReadToEnd();
				return JsonConvert.DeserializeObject<Task>(json);
			}
		}
	}
	public class Task
	{
		public Example[] train;
		public Example[] test;
    }

	public class Example
    {
		public int[,] input;
		public int[,] output;

		public void Print()
		{
			for (int i = 0; i <= Math.Max(input.GetUpperBound(0), output.GetUpperBound(0)); i++) {
				for (int j = 0; j <= input.GetUpperBound(1); j++)
                {
					if (input.GetUpperBound(0) >= i) {
						Console.BackgroundColor = Image.colorMap(input[i, j]);
						Console.Write(input[i, j]);
					} else {
						Console.ResetColor();
						Console.Write(' ');
					}
				}
				Console.ResetColor();
				Console.Write(" --> ");
				for (int j = 0; j <= output.GetUpperBound(1); j++)
				{
					if (input.GetUpperBound(0) >= i)
					{
						Console.BackgroundColor = Image.colorMap(output[i, j]);
						Console.Write(output[i, j]);
					}
					else
					{
						Console.ResetColor();
						Console.Write(' ');
					}
				}
				Console.ResetColor();
				Console.WriteLine();
			}
		}
	}
}