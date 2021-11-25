using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TealCompiler.AbstractSyntaxTree;
using TealCompiler.TealGenerator;
using TealCompiler.TealGenerator.Assembly;
using TealCompiler.TealGenerator.Compilers;
using TealDotNet.Lexer;

namespace TealDotNet
{
	public static class Compiler
	{
		public static void Main()
		{
			string l_programSource = File.ReadAllText("tealProgram.az");
			List<AzurLexer.LexerToken> l_tokens = AzurLexer.ParseText(l_programSource).ToList();
			Program l_program = Syntax.Analyzer.Analyze(l_tokens);
			Semantic.Analyzer.Analyze(l_program, Semantic.Analyzer.Flags.ApprovalProgram);
			CompiledProgramState l_state = new CompiledProgramState(CompilerFlags.ApprovalProgram);
			l_program.Compile(l_state);
			//File.WriteAllText("tealProgram.teal", Generator.Compile(l_program));
			foreach (TealInstruction l_instruction in l_state.Output)
			{
				Console.WriteLine(l_instruction);
			}
		}
	}
}