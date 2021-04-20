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

		private IDictionary<State, IEnumerable<object>> examples;

		public PartialImageSpec(IDictionary<State, IEnumerable<object>> examples): base(examples.Keys) {
			this.examples = examples;
		}
        protected override bool CorrectOnProvided(State state, object output)
        {
			// Console.Out.WriteLine((examples[state] as List<int[][]>).Count);
			var space = (examples[state] as List<int[][]>)[0];
			var candidate = output as int[][];
			// Console.Out.WriteLine("space: ");
			// print(space);
			// Console.Out.WriteLine("Candidate: ");
			// print(candidate);
			for (int i = 0; i < space.Length; i++) {
				for (int j = 0; j < space[0].Length; j++) {
					if (space[i][j] == 10) {
						if (candidate[i][j] == 0) return false;
					} else if (space[i][j] < 0) {
						if (candidate[i][j] == -space[i][j]) return false;
					} else {
						if (space[i][j] != candidate[i][j]) return false;
					}
				}
			}
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
			var result = new Dictionary<State, IEnumerable<object>>();
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
                Console.Out.WriteLine();
            }
        }
    }
}