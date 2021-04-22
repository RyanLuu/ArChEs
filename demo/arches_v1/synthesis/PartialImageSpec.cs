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

		public IDictionary<State, object> PartialImageExamples;

		public PartialImageSpec(IDictionary<State, object> PartialImageExamples): base(PartialImageExamples.Keys) {
			this.PartialImageExamples = PartialImageExamples;
		}

		// compares an actual image to our partial image representation to check for a match
        protected override bool CorrectOnProvided(State state, object output)
        {
			// extract the partial image
			Image space = this.PartialImageExamples[state] as Image;
			// the candidate image to see if it matches the partial image
			Image candidate = output as Image;
            Console.WriteLine("candidate:");
            Console.WriteLine(candidate.ToString());
            Console.WriteLine("space:");
            Console.WriteLine(space.ToString());

            // Check that Image dimensions are equivalent
            if (candidate.h != space.h || candidate.w != space.w) {
                Console.WriteLine("Ending Early in CorrectOnProvided due to Dimension mismatch"); 
                return false;
            }

            for (int i = 0; i < candidate.data.Length; i++)
            {
				// if the partial image is 10, then the candidate can be anything except 0
                if (space.data[i] == 10) {
                    if (candidate.data[i] == 0) {
                        Console.WriteLine("Ending Early in CorrectOnProvided\nspace value:{0}\ncandidate value:{1}\n",space.data[i], candidate.data[i]); 
                        return false;
                    }
                }
				// if the partial image is -x, then then candidate can be anything except x
                // TODO: Anything except x... But what if x is 0?
                else if (space.data[i] < 0) {
                    if (candidate.data[i] == -space.data[i]) {
                            Console.WriteLine("Ending Early in CorrectOnProvided\nspace value:{0}\ncandidate value:{1}\n",space.data[i], candidate.data[i]); 
                            return false;
                    }
                }
                // No more special values, the candidate and partial must match
                else if (space.data[i] <= 9 && space.data[i] >= 0) {
                    if (space.data[i] != candidate.data[i]) {
                            Console.WriteLine("Ending Early in CorrectOnProvided\nspace value:{0}\ncandidate value:{1}\n",space.data[i], candidate.data[i]); 
                        return false;
                    }
                }
                else {
                    throw new NotSupportedException(); // We don't support that value in our images yet
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
			return this.PartialImageExamples[state].GetHashCode();
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
			foreach (var input in this.PartialImageExamples.Keys) {
				result[transformer(input)] = this.PartialImageExamples[input];
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