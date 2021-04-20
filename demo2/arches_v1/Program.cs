using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.AST;
using Microsoft.ProgramSynthesis.Compiler;
using Microsoft.ProgramSynthesis.Learning;
using Microsoft.ProgramSynthesis.Learning.Strategies;
using Microsoft.ProgramSynthesis.Specifications;
using Microsoft.ProgramSynthesis.VersionSpace;

namespace Arches 
{
    internal class Program
    {
        private static readonly Grammar Grammar = DSLCompiler.Compile(new CompilerOptions
        {
            InputGrammarText = File.ReadAllText("synthesis/grammar/transform.grammar"),
            References = CompilerReference.FromAssemblyFiles(typeof(Program).GetTypeInfo().Assembly)
        }).Value;

        private static SynthesisEngine _prose;

        private static readonly Dictionary<State, object> Examples = new Dictionary<State, object>();
        private static ProgramNode _topProgram;

        private static void Main(string[] args)
        {
            _prose = ConfigureSynthesis();

            int[][] inputTest = new int[4][];
                    inputTest[0] = new int[] {0, 2, 2, 0, 8, 3};
                    inputTest[1] = new int[] {0, 0, 0, 0, 0, 0};
                    inputTest[2] = new int[] {4, 1, 1, 1, 1, 1};
                    inputTest[3] = new int[] {9, 8, 4, 3, 0, 0};

            int[][] outputTest1 = new int[4][];
                    outputTest1[0] = new int[] {0, 5, 5, 0, 5, 5}; 
                    outputTest1[1] = new int[] {0, 0, 0, 0, 0, 0};
                    outputTest1[2] = new int[] {5, 5, 5, 5, 5, 5};
                    outputTest1[3] = new int[] {5, 5, 5, 5, 0, 0};

            int[][] outputTest2 = new int[4][];
                    outputTest2[0] = new int[] {0, 2, 2, 0, 0, 0}; 
                    outputTest2[1] = new int[] {0, 0, 0, 0, 0, 0};
                    outputTest2[2] = new int[] {0, 0, 0, 0, 0, 0};
                    outputTest2[3] = new int[] {0, 0, 0, 0, 0, 0};

            int[][] outputTest3 = new int[4][];
                    outputTest3[0] = new int[] {0, 3, 3, 0, 0, 0}; 
                    outputTest3[1] = new int[] {0, 0, 0, 0, 0, 0};
                    outputTest3[2] = new int[] {0, 0, 0, 0, 0, 0};
                    outputTest3[3] = new int[] {0, 0, 0, 0, 0, 0};
            
    
            State inputState = State.CreateForExecution(Grammar.InputSymbol, inputTest);
            Examples.Add(inputState, outputTest1); 
            

            var spec = new ExampleSpec(Examples);
            Console.Out.WriteLine("Learning a program for examples:");
            foreach (KeyValuePair<State, object> example in Examples)
                Console.WriteLine("\"{0}\" -> \"{1}\"", example.Key.Bindings.First().Value, example.Value);

            var scoreFeature = new RankingScore(Grammar);
            ProgramSet topPrograms = _prose.LearnGrammarTopK(spec, scoreFeature, 4, null);
            if (topPrograms.IsEmpty) {
                Console.WriteLine(spec);
                // throw new Exception("No program was found for this specification.");
            }

            _topProgram = topPrograms.RealizedPrograms.First();
            Console.Out.WriteLine("Top 4 learned programs:");
            var counter = 1;
            foreach (ProgramNode program in topPrograms.RealizedPrograms)
            {
                if (counter > 4) break;
                Console.Out.WriteLine("==========================");
                Console.Out.WriteLine("Program {0}: ", counter);
                Console.Out.WriteLine(program.PrintAST(ASTSerializationFormat.HumanReadable));
                counter++;
            }
        }

        public static SynthesisEngine ConfigureSynthesis()
        {
            var witnessFunctions = new WitnessFunctions(Grammar);
            var deductiveSynthesis = new DeductiveSynthesis(witnessFunctions);
            var synthesisExtrategies = new ISynthesisStrategy[] {deductiveSynthesis};
            var synthesisConfig = new SynthesisEngine.Config {Strategies = synthesisExtrategies};
            var prose = new SynthesisEngine(Grammar, synthesisConfig);
            return prose;
        }
    }
}