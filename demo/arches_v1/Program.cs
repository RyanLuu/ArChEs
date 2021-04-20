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
            InputGrammarText = File.ReadAllText("synthesis/grammar/arches.grammar"),
            References = CompilerReference.FromAssemblyFiles(typeof(Program).GetTypeInfo().Assembly)
        }).Value;

        private static SynthesisEngine _prose;

        private static readonly Dictionary<State, object> Examples = new Dictionary<State, object>();
        private static string _taskName;
        private static ProgramNode _topProgram;

        private static void Main(string[] args)
        {
            _prose = ConfigureSynthesis();
            var menu = @"Select one of the options: 
1 - provide training task
2 - run top synthesized program on test
3 - exit";
            var option = 0;
            while (option != 3)
            {
                Console.Out.WriteLine(menu);
                try
                {
                    option = short.Parse(Console.ReadLine());
                }
                catch (Exception)
                {
                    Console.Out.WriteLine("Invalid option. Try again.");
                    continue;
                }

                RunOption(option);
                
            }
        }

        private static void RunOption(int option)
        {
            switch (option)
            {
                case 1:
                    LearnFromNewExample();
                    break;
                case 2:
                    RunOnNewInput();
                    break;
                default:
                    Console.Out.WriteLine("Invalid option. Try again.");
                    break;
            }
        }

        private static void LearnFromNewExample()
        {
            Task task;
            while (true)
            {
                try
                {
                    Console.Out.Write("Enter a task name: ");
                    _taskName = Console.ReadLine().Trim();
                    task = TaskLoader.LoadTask("tasks/" + _taskName + ".json");
                    break;
                }
                catch (FileNotFoundException)
                {
                    Console.Out.WriteLine("Could not find task "+ _taskName + ".json. Try again.");
                }
                catch (DirectoryNotFoundException)
                {
                    Console.Out.WriteLine("Could not find task " + _taskName + ".json. Try again.");
                }
                catch (Exception)
                {
                    Console.Out.WriteLine("Unable to parse " + _taskName + ".json. Try again.");
                }

            }
            
            Example[] train = task.train;
            Console.Out.WriteLine("Learning a program for examples:\n");
            Examples.Clear();
            foreach (Example example in train)
            {
                example.Print();
                Console.Out.WriteLine();
                State inputState = State.CreateForExecution(Grammar.InputSymbol, new Image(example.input));
                Examples.Add(inputState, new Image(example.output));
            }

            var spec = new ExampleSpec(Examples);

            var scoreFeature = new RankingScore(Grammar);
            int K = 4;
            ProgramSet topPrograms = _prose.LearnGrammarTopK(spec, scoreFeature, K, null);
            if (topPrograms.IsEmpty)
            {
                Console.Out.WriteLine("No program was found for this specification.");
            }
            else
            {
                _topProgram = topPrograms.RealizedPrograms.First();
                Console.Out.WriteLine("Top " + K + " learned programs:");
                var counter = 1;
                foreach (ProgramNode program in topPrograms.RealizedPrograms)
                {
                    if (counter > 4) break;
                    Console.Out.WriteLine("==========================");
                    Console.Out.WriteLine("Program {0}: ", counter);
                    Console.Out.WriteLine(program.PrintAST(ASTSerializationFormat.HumanReadable));
                    counter++;
                }
                Console.Out.WriteLine("==========================");
            }
        }

        private static void RunOnNewInput()
        {
            if (_topProgram == null)
            {
                Console.Out.WriteLine("No program was synthesized. Try to provide new examples first.");
                return;
            }
            Console.Out.WriteLine("Top program: {0}", _topProgram);
            Console.Out.WriteLine("Task: {0}", _taskName);
            Task task;
            task = TaskLoader.LoadTask("tasks/" + _taskName + ".json");

            Example[] test = task.test;
            foreach (Example example in test)
            {
                Console.WriteLine("EXPECTED:");
                example.Print();
                Image newInput = new Image(example.input);
                State newInputState = State.CreateForExecution(Grammar.InputSymbol, newInput);
                Example result = new Example();
                result.input = newInput.toArray();
                result.output = (_topProgram.Invoke(newInputState) as Image).toArray();

                Console.Out.WriteLine("RESULT:");
                result.Print();
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
