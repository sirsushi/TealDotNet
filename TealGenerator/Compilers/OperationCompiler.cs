using System;
using System.Collections.Generic;
using TealCompiler.AbstractSyntaxTree;
using TealCompiler.TealGenerator.Assembly;

namespace TealCompiler.TealGenerator.Compilers
{
	public static class OperationCompiler
	{
		public static void Compile(this BinaryOperationInstruction p_operation, CompiledProgramState p_state)
		{
			Dictionary<string, Action<BinaryOperationInstruction, CompiledProgramState>> l_compilers = new()
			{
				{"=", CompileAssignation},
				{"+", CompileAdd}
			};
			l_compilers[p_operation.Operator](p_operation, p_state);
		}

		private static void CompileAssignation(BinaryOperationInstruction p_operation, CompiledProgramState p_state)
		{
			p_operation.RightValue.Compile(p_state);

			if (p_operation.LeftValue is Reference l_reference)
			{
				int l_position = p_state.GetVariablePosition(l_reference.Name);
				p_state.Write(Opcodes.uncover, l_position);
				p_state.Write(Opcodes.pop);
				p_state.Write(Opcodes.cover, l_position - 1);
			}
		}

		private static void CompileAdd(BinaryOperationInstruction p_operation, CompiledProgramState p_state)
		{
			p_operation.LeftValue.Compile(p_state);
			p_operation.RightValue.Compile(p_state);
			p_state.Write(Opcodes.add);
		}
	}
}