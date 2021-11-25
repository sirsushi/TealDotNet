using System;
using System.Collections.Generic;
using TealCompiler.AbstractSyntaxTree;

namespace TealCompiler.TealGenerator.Compilers
{
	public static class OperationCompiler
	{
		public static void Compile(this BinaryOperationInstruction p_operation, CompiledProgramState p_state)
		{
			Dictionary<string, Action<BinaryOperationInstruction, CompiledProgramState>> l_compilers = new()
			{
				{"=", CompileAssignation}
			};
		}

		private static void CompileAssignation(BinaryOperationInstruction p_operation, CompiledProgramState p_state)
		{
			p_operation.RightValue.Compile(p_state);

			if (p_operation.LeftValue is Reference l_reference)
			{
				p_state.RegisterVariablePosition(l_reference.Name);
			}
		}
	}
}