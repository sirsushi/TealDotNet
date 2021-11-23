using System.Collections.Generic;
using TealCompiler.TealGenerator.Assembly;

namespace TealCompiler.TealGenerator
{
	public class CompiledProgramState
	{
		public CompiledProgramState(CompilerFlags p_initialFlags)
		{
			Flags = p_initialFlags;
		}

		private CompilerFlags Flags { get; set; }
		private List<TealInstruction> Output { get; } = new();
		private Stack<string> StackTracker { get; } = new();

		public void SetFlag(CompilerFlags p_flags)
		{
			Flags |= p_flags;
		}

		public void ResetFlag(CompilerFlags p_flags)
		{
			Flags &= ~p_flags;
		}

		public bool IsFlagSet(CompilerFlags p_flags)
		{
			return Flags.HasFlag(p_flags);
		}

		public void Write(Opcode p_opcode, params object[] p_params)
		{
			Output.Add(new OpcodeInstruction(p_opcode, p_params));
		}

		public void Labelize(string p_label)
		{
			Output.Add(new LabelInstruction(p_label));
		}

		public void Comment(string p_comment)
		{
			Output.Add(new CommentInstruction(p_comment));
		}

		public void Stack()
		{
			
		}
	}
}