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
		public override string ToString()
		{
			string s = "";
			for (int i = 0; i <= Math.Max(input.GetUpperBound(0), output.GetUpperBound(0)); i++) {
				for (int j = 0; j <= input.GetUpperBound(1); j++)
                {
					s += (input.GetUpperBound(0) >= i) ? input[i, j] : ' ';
				}
				s += " > ";
				for (int j = 0; j <= output.GetUpperBound(1); j++)
				{
					s += (output.GetUpperBound(0) >= i) ? output[i, j] : ' ';
				}
				s += '\n';
			}
			return s;
		}
	}
}