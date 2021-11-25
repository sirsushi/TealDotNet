using System;
using System.Collections.Generic;
using TealCompiler.TealGenerator.Assembly;

namespace TealCompiler.TealGenerator
{
	[Flags]
	public enum CompilerFlags
	{
		ApprovalProgram = 1,
		ClearStateProgram = 2,
		Signature = 4,
		MainFunction = 8
	}

	public class CompilationException : Exception
	{
		public CompilationException(string? message) : base(message)
		{
		}
	}
}