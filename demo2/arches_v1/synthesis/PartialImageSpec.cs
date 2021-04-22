using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using Microsoft.ProgramSynthesis.Specifications.Serialization;
using Microsoft.ProgramSynthesis.Specifications;
using Microsoft.ProgramSynthesis;

namespace Arches {
    public class PartialImageSpec : Spec
    {

		public IDictionary<State, object> examples;

		public PartialImageSpec(IDictionary<State, object> examples): base(examples.Keys) {
			this.examples = examples;
		}

		// compares an actual image to our partial image representation to check for a match
        protected override bool CorrectOnProvided(State state, object output)
        {
			// extract the partial image
			var space = examples[state] as int[][];
			// the candidate image to see if it matches the partial image
			var candidate = output as int[][];
			// TODO handle this whenn it's an int 
			Console.WriteLine(output.GetType());
			Console.WriteLine("space");
			print(space);
			Console.WriteLine("candidate");
			print(candidate);
			for (int i = 0; i < space.Length; i++) {
				for (int j = 0; j < space[0].Length; j++) {
					// if the partial image is 10, then the candidate can be anything except 0
					if (space[i][j] == 10) {
						if (candidate[i][j] == 0) return false;
					// if the partial image is -x, then then candidate can be anything except x
					} else if (space[i][j] < 0) {
						if (candidate[i][j] == -space[i][j]) return false;
					} else {
						// No more special color values, the candidate and the partial must match
						if (space[i][j] != candidate[i][j]) return false;
					}
				}
			}
			Console.WriteLine("matched");
			return true;
        }

        protected override bool EqualsOnInput(State state, Spec other)
        {
            throw new NotImplementedException();
        }

        protected override int GetHashCodeOnInput(State state)
        {
			return examples[state].GetHashCode();
            // throw new NotImplementedException();
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
			foreach (var input in examples.Keys) {
				result[transformer(input)] = examples[input];
			}
			return new PartialImageSpec(result);
        }

		public static void print(int[][] m) {
            for (int i = 0; i < m.Length; i++) {
                for (int j = 0; j < m[0].Length; j++) {
                    Console.Out.Write(m[i][j] + " ");
                }
                Console.WriteLine();
            }
        }
    }
}