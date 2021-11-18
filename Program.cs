using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TealCompiler;
using TealCompiler.AbstractSyntaxTree;
using TealCompiler.TealGenerator;
using TealDotNet.Lexer;

namespace TealDotNet
{
	public static class Compiler
	{
		public static void Main()
		{
			string l_programSource = File.ReadAllText("tealProgram.az");
			string l_processedProgramSource = Preprocessor.Preprocessor.ProcessText(l_programSource);
			List<AzurLexer.LexerToken> l_tokens = AzurLexer.ParseText(l_processedProgramSource).ToList();
			Program l_program = SyntaxAnalyzer.SyntaxAnalyzer.Analyze(l_tokens);
			//File.WriteAllText("tealProgram.teal", Generator.Compile(l_program));
			Console.WriteLine($"{l_program.Functions.Count}");
		}
	}
}