using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TealCompiler.TealGenerator.Assembly;
using TealDotNet.Semantic;

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
		private Stack<StackType> StackTracker { get; } = new();
		private Dictionary<string, int> VariablesPosition { get; } = new();

		private Stack<int> StackOffset { get; } = new();

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
			foreach (StackType l_type in p_opcode.Pops.Reverse())
			{
				if (StackTracker.Pop() == l_type || l_type == StackType.Any)
					throw new CompilationException("Wrong stacked type");
			}
			
			Output.Add(new OpcodeInstruction(p_opcode, p_params));

			foreach (StackType l_type in p_opcode.Pushes)
			{
				StackTracker.Push(l_type);
			}
		}

		public void Labelize(string p_label)
		{
			Output.Add(new LabelInstruction(p_label));
		}

		public void Comment(string p_comment)
		{
			Output.Add(new CommentInstruction(p_comment));
		}

		public void RegisterVariablePosition(string p_variableName)
		{
			VariablesPosition[p_variableName] = StackTracker.Count;
		}

		public int GetVariablePosition(string p_variableName)
		{
			return StackTracker.Count - VariablesPosition[p_variableName];
		}
	}
}