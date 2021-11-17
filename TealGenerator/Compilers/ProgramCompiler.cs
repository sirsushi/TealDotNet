using System;
using System.Collections.Generic;
using System.Linq;
using TealCompiler.TealGenerator.Assembly;
using TealCompiler.Tokens;

namespace TealCompiler.TealGenerator.Compilers
{
	public class ProgramCompiler : Compiler<ProgramCompiler.Flags>
	{
		public enum Flags
		{
			ApprovalProgram,
			ClearStateProgram,
			Contract
		}
		public Program Program { get; set; }


		public override IEnumerable<TealInstruction> Compile(Flags p_flags)
		{
			Function l_mainFunction = Program.Functions.FirstOrDefault(f => f.Name == p_flags.ToString());
			if (l_mainFunction == null) throw new InvalidOperationException($"Can't compile {p_flags}");

			l_mainFunction.Block.Find<CallInstruction>();
		}
	}
}