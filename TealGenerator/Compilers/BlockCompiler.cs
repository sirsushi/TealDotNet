using System;
using System.Collections.Generic;
using System.Linq;
using TealCompiler.AbstractSyntaxTree;
using TealCompiler.TealGenerator.Assembly;

namespace TealCompiler.TealGenerator.Compilers
{
	public static class BlockCompiler
	{
		public static void Compile(this CodeBlock p_block, CompiledProgramState p_state)
		{
			List<string> l_variables = p_block.Instructions
				.OfType<BinaryOperationInstruction>()
				.Where(op => op.Operator == "=" && op.LeftValue is Reference)
				.Select(op => op.LeftValue as Reference)
				.Select(reference => reference.Name)
				.Distinct()
				.Where(reference => !p_state.IsVariableRegistered(reference))
				.ToList();
			foreach (string l_assignationTarget in l_variables)
			{
				p_state.RegisterVariablePosition(l_assignationTarget);
			}
			foreach (Instruction l_instruction in p_block.Instructions)
			{
				switch (l_instruction)
				{
					case DoWhileInstruction l_doWhileInstruction:
						break;
					case Expression l_expression:
						l_expression.Compile(p_state);
						break;
					case ForInstruction l_forInstruction:
						break;
					case IfInstruction l_ifInstruction:
						l_ifInstruction.Compile(p_state);
						break;
					case ReturnInstruction l_returnInstruction:
						l_returnInstruction.Compile(p_state);
						break;
					case SwitchInstruction l_switchInstruction:
						l_switchInstruction.Compile(p_state);
						break;
					case WhileInstruction l_whileInstruction:
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(l_instruction));
				}
			}

			foreach (string l_assignationTarget in l_variables)
			{
				p_state.UnregisterVariablePosition(l_assignationTarget);
			}
		}
	}
}