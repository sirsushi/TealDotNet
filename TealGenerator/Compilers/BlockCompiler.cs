using System;
using TealCompiler.AbstractSyntaxTree;

namespace TealCompiler.TealGenerator.Compilers
{
	public static class BlockCompiler
	{
		public static void Compile(this CodeBlock p_block, CompiledProgramState p_state)
		{
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
		}
	}
}