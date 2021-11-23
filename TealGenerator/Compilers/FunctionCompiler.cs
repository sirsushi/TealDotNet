using System;
using System.Collections.Generic;
using TealCompiler.AbstractSyntaxTree;
using TealCompiler.TealGenerator.Assembly;

namespace TealCompiler.TealGenerator.Compilers
{
	public static class FunctionCompiler
	{
		public static void Compile(this Function p_function, CompiledProgramState p_state)
		{
			if (!p_state.IsFlagSet(CompilerFlags.MainFunction))
				p_state.Labelize(p_function.Name);
			
			p_function.Block.Compile(p_state);
		}
	}
}