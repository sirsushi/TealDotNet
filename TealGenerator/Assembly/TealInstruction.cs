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
	}

	public class LabelInstruction : TealInstruction
	{
		public LabelInstruction(string p_name)
		{
			Name = p_name;
		}

		public string Name { get; set; }
	}
}