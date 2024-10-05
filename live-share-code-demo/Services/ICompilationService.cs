using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;

namespace live_share_code_demo.services
{
    public interface ICompilationService
    {
        string CompileAndRun(string userCode, string funName, string testInputs, string expectedResult);
    }

    public class CompilationService : ICompilationService
    {
        public string CompileAndRun(string userCode, string funName, string testInputs, string expectedResult)
        {
            // var result = solution.{funName}(4,[2,4,6,8],'c'); all these 4,[2,4,6,8],'c' are one string
            // Wrap the user code with a predefined structure

            //var result = solution.{funName}({testInputs});

            var wrappedCode = $@"
                        using System;
                        using System.Collections.Generic;
                
                        public class Program
                        {{
                            public static void Main(string[] args)
                            {{
                                Solution solution = new Solution();
                              var result = solution.{funName}({testInputs});
                
                                Console.WriteLine(result);
                            }}
                         }}{userCode}";

            string inputCodeResult = CompileAndExecute(wrappedCode);
            if (inputCodeResult == expectedResult)
                return "Test Passed";
            return inputCodeResult;
        }



        private string CompileAndExecute(string code)
        {
            // Create a syntax tree from the user's code
            var syntaxTree = CSharpSyntaxTree.ParseText(code);

            // Collect necessary references
            var references = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
                .Select(a => MetadataReference.CreateFromFile(a.Location))
                .ToList();

            // Compile the code into a memory stream
            var compilation = CSharpCompilation.Create(
                "DynamicCompilation",
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.ConsoleApplication)
            );

            using var ms = new MemoryStream();
            var result = compilation.Emit(ms);

            // Handle compilation errors
            if (!result.Success)
            {
                var failures = result.Diagnostics.Where(diagnostic =>
                    diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);

                string errorMessage = "Compilation failed:\n";
                foreach (var diagnostic in failures)
                {
                    errorMessage += $"\n\n{diagnostic.Id}: {diagnostic.GetMessage()}\n";
                }
                return errorMessage;
            }

            // Load the compiled assembly
            ms.Seek(0, SeekOrigin.Begin);
            var assembly = Assembly.Load(ms.ToArray());

            // Capture the output from Console.WriteLine
            var outputWriter = new StringWriter();
            Console.SetOut(outputWriter);

            //// Simulate input using StringReader to feed test inputs into Console.ReadLine()
            //var inputReader = new StringReader(string.Join(Environment.NewLine, testInputs));
            //Console.SetIn(inputReader);

            // Find the entry point (Main method) in the user's code and invoke it
            var entryPoint = assembly.EntryPoint;
            if (entryPoint != null)
            {
                entryPoint.Invoke(null, new object[] { new string[] { } });
            }

            // Capture and return the program's output
            return outputWriter.ToString().Trim();
        }

        // just for testing


        //private string CompileAndExecute(string code)
        //{
        //    // Create a syntax tree from the user's code
        //    var syntaxTree = CSharpSyntaxTree.ParseText(code);

        //    // Collect necessary references
        //    var references = AppDomain.CurrentDomain
        //        .GetAssemblies()
        //        .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
        //        .Select(a => MetadataReference.CreateFromFile(a.Location))
        //        .ToList();

        //    // Compile the code into a memory stream
        //    var compilation = CSharpCompilation.Create(
        //        "DynamicCompilation",
        //        new[] { syntaxTree },
        //        references,
        //        new CSharpCompilationOptions(OutputKind.ConsoleApplication)
        //    );

        //    using var ms = new MemoryStream();
        //    var result = compilation.Emit(ms);

        //    // Handle compilation errors
        //    if (!result.Success)
        //    {
        //        var failures = result.Diagnostics.Where(diagnostic =>
        //            diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);

        //        string errorMessage = "Compilation failed:\n";
        //        foreach (var diagnostic in failures)
        //        {
        //            errorMessage += $"{diagnostic.Id}: {diagnostic.GetMessage()}\n";
        //        }
        //        return errorMessage;
        //    }

        //    // Load the compiled assembly
        //    ms.Seek(0, SeekOrigin.Begin);
        //    var assembly = Assembly.Load(ms.ToArray());

        //    // Capture the output from Console.WriteLine
        //    var outputWriter = new StringWriter();
        //    Console.SetOut(outputWriter);

        //    // Create an instance of the Solution class
        //    var solutionType = assembly.GetType("Solution");
        //    var solutionInstance = Activator.CreateInstance(solutionType);

        //    // Prepare parameters for the user-defined method
        //    var parameters = testInputs.Split(',').Select(input => Convert.ChangeType(input.Trim(), typeof(int))).ToArray();

        //    // Find the user-defined method and invoke it
        //    var userMethod = solutionType.GetMethod(funName);
        //    if (userMethod != null)
        //    {
        //        var result = userMethod.Invoke(solutionInstance, parameters);
        //        Console.WriteLine(result); // This will output the result to be captured
        //    }

        //    // Capture and return the program's output
        //    return outputWriter.ToString();
        //}

        private void tmp()
        {
            Solution s = new Solution();
            s.add(1, 2, [2, 3, 5], "ali", 'c', [[2, 3, 5], [2, 3, 45]], 3.4, 999);
        }
    }
    // just for testing

    public class Solution
    {
        public double add(int a, int b, List<int> tmp, string s, char c, List<List<int>> ints, double d, long l)
        {
            return a + b;
        }
    }
}
