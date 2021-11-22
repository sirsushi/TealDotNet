using System.Collections.Generic;
using TealCompiler.TealGenerator.Assembly;

namespace TealCompiler.TealGenerator
{
	public abstract class Compiler<TFlags>
		where TFlags : System.Enum
	{
		public IEnumerable<TealInstruction> Compile(int p_flags)
		{
			return Compile((TFlags)(object)p_flags);
		}
		public abstract IEnumerable<TealInstruction> Compile(TFlags p_flags);
	}
}