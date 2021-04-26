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
        public static Random rnd = new Random();

        public static bool DEBUG_STATUS = true;
        public static void DEBUG(string debug_message) {
            if (DEBUG_STATUS) {Console.WriteLine(debug_message);}
        }
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
                case 3:
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

            Console.Out.WriteLine(@"List the functions that this task is invariant under (i.e. g such that F(x)=y => F(g(x))=g(y)
c    Color mapping
r    Rotation
f    Reflection
t    Translation");

            List<Invariant> invariants = new List<Invariant>();
            while (true)
            {
                string invInput= Console.ReadLine().Trim();
                foreach (char c in invInput)
                {
                    switch (c)
                    {
                        case 'c':
                            invariants.Add(new ColormapInvariant());
                            break;
                        case 'r':
                            invariants.Add(new RotationInvariant());
                            break;
                        case 'f':
                            invariants.Add(new ReflectionInvariant());
                            break;
                        case 't':
                            invariants.Add(new TranslationInvariant());
                            break;
                        default:
                            Console.Out.WriteLine("Unknown invariant " + c);
                            continue;

                    }
                }
                break;
            }
            
            Console.Out.WriteLine(@"List the functions that this task is equivalent under (i.e. g such that F(x)=y => F(g(x))=y
c    Color mapping
r    Rotation
f    Reflection
t    Translation");
            List<Invariant> equivalences = new List<Invariant>();
            while (true)
            {
                string invInput = Console.ReadLine().Trim();
                foreach (char c in invInput)
                {
                    switch (c)
                    {
                        case 'c':
                            equivalences.Add(new ColormapInvariant());
                            break;
                        case 'r':
                            equivalences.Add(new RotationInvariant());
                            break;
                        case 'f':
                            equivalences.Add(new ReflectionInvariant());
                            break;
                        case 't':
                            equivalences.Add(new TranslationInvariant());
                            break;
                        default:
                            Console.Out.WriteLine("Unknown equivalence " + c);
                            continue;

                    }
                }
                break;
            }

            Example[] train = task.train;
            Console.Out.WriteLine("Learning a program for input examples:\n");
            Examples.Clear();
            foreach (Example example in train)
            {
                example.Print();
                Console.Out.WriteLine();
                State inputState = State.CreateForExecution(Grammar.InputSymbol, new Image(example.input));
                Examples.Add(inputState, new AbstractImage(new Image(example.output)));
            }

            if (equivalences.Count() > 0 || invariants.Count() > 0)
            {
                Console.Out.WriteLine("and generated examples:\n");
                foreach (Invariant inv in invariants)
                {
                    inv.reseed();
                    foreach (Example example in train)
                    {
                        Image newIn = inv.generate(new Image(example.input));
                        Image newOut = inv.generate(new Image(example.output));
                        Example newExample = Example.FromImages(newIn, newOut);
                        newExample.Print();
                        Console.Out.WriteLine();
                        State newInState = State.CreateForExecution(Grammar.InputSymbol, newIn);
                        Examples.Add(newInState, new AbstractImage(newOut));
                    }
                }
                foreach (Invariant eq in equivalences)
                {
                    eq.reseed();
                    foreach (Example example in train)
                    {
                        Image newIn = eq.generate(new Image(example.input));
                        Image newOut = new Image(example.output);
                        Example newExample = Example.FromImages(newIn, newOut);
                        newExample.Print();
                        Console.Out.WriteLine();
                        State newInState = State.CreateForExecution(Grammar.InputSymbol, newIn);
                        Examples.Add(newInState, new AbstractImage(newOut));
                    }
                }
            }

            //var spec = new ExampleSpec(Examples);
            var spec = new AbstractImageSpec(Examples);

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
                Example result = Example.FromImages(newInput, _topProgram.Invoke(newInputState) as Image);
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
