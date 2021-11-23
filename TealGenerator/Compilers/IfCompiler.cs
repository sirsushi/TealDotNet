using System;
using TealCompiler.AbstractSyntaxTree;
using TealCompiler.TealGenerator.Assembly;

namespace TealCompiler.TealGenerator.Compilers
{
	public static class IfCompiler
	{
		public static void Compile(this IfInstruction p_instruction, CompiledProgramState p_state)
		{
			p_instruction.Condition.Compile(p_state);

			Guid l_id = Guid.NewGuid();
			if (p_instruction.ElseBlock != null)
			{
				// if NOT condition => jump else
				p_state.Write(Opcodes.bz, l_id.ToString() + "_else");
				
				p_instruction.IfBlock.Compile(p_state);

				// jump endif
				p_state.Write(Opcodes.b, l_id.ToString() + "_endif");
				p_state.Labelize(l_id.ToString() + "_else");

				p_instruction.ElseBlock.Compile(p_state);
			}
			else
			{
				// if NOT condition => jump endif
				p_state.Write(Opcodes.bz, l_id.ToString() + "_endif");

				p_instruction.IfBlock.Compile(p_state);
			}

			p_state.Labelize(l_id.ToString() + "_endif");
		}
	}
}