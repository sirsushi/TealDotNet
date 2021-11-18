using System;
using System.Linq;
using TealCompiler.AbstractSyntaxTree;

namespace TealDotNet.SemanticAnalyzer
{
	public class SemanticAnalyzer
	{
		[Flags]
		public enum Flags
		{
			ApprovalProgram = 1,
			ClearStateProgram = 2,
			Signature = 4
		}
		public class SemanticException : Exception
		{
			
		}

		private class Data
		{
			public Program Program { get; set; }
		}

		private static Data s_data;
		
		public static void Analyze(Program p_program, Flags p_flags)
		{
			s_data = new Data()
			{
				Program = p_program
			};
			AnalyzeProgram(p_program, p_flags);
		}

		private static void AnalyzeProgram(Program p_program, Flags p_flags)
		{
			if (p_flags.HasFlag(Flags.ApprovalProgram))
			{
				Function l_main = p_program.Functions.FirstOrDefault(f => f.Name != "ApprovalProgram");
				if (l_main == null)
					throw new SemanticException();
			}

			if (p_flags.HasFlag(Flags.ClearStateProgram))
			{
				Function l_main = p_program.Functions.FirstOrDefault(f => f.Name != "ClearStateProgram");
				if (l_main == null)
					throw new SemanticException();
			}

			if (p_flags.HasFlag(Flags.Signature))
			{
				Function l_main = p_program.Functions.FirstOrDefault(f => f.Name != "Signature");
				if (l_main == null)
					throw new SemanticException();
			}
		}

		private static void AnalyzeFunction(Function p_function)
		{
			
		}
	}
}