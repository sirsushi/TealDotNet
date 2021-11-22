using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TealCompiler.AbstractSyntaxTree;
using TealDotNet.Lexer;

namespace TealDotNet
{
	public static class Compiler
	{
		public static void Main()
		{
			string l_programSource = File.ReadAllText("algoloto.az");
			string l_processedProgramSource = Preprocessor.Preprocessor.ProcessText(l_programSource);
			List<AzurLexer.LexerToken> l_tokens = AzurLexer.ParseText(l_processedProgramSource).ToList();
			Program l_program = Syntax.Analyzer.Analyze(l_tokens);
			Semantic.Analyzer.Analyze(l_program, Semantic.Analyzer.Flags.ApprovalProgram | Semantic.Analyzer.Flags.ClearStateProgram);
			//File.WriteAllText("tealProgram.teal", Generator.Compile(l_program));
			Console.WriteLine($"{l_program.Functions.Count}");
		}
	}
}