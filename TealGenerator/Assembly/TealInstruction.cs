using System.Collections.Generic;

namespace TealCompiler.TealGenerator.Assembly
{
	public interface ITealGenerator
	{
		IEnumerable<TealInstruction> ToTEAL();
	}
	public class TealInstruction
	{
		
	}

	public class OpcodeInstruction : TealInstruction
	{
		public OpcodeInstruction(Opcode p_opcode, params object[] p_params)
		{
			Opcode = p_opcode;
			Params = p_params;
		}

		public Opcode Opcode { get; set; }
		public object[] Params { get; set; }

		public override string ToString()
		{
			return $"{Opcode.Name} {string.Join(' ', Params)}";
		}
	}

	public class LabelInstruction : TealInstruction
	{
		public LabelInstruction(string p_name)
		{
			Name = p_name;
		}

		public string Name { get; set; }

		public override string ToString()
		{
			return $"{Name}:";
		}
	}

	public class CommentInstruction : TealInstruction
	{
		public CommentInstruction(string p_comment)
		{
			Comment = p_comment;
		}

		public string Comment { get; set; }

		public override string ToString()
		{
			return $"//{Comment}";
		}
	}
}