using System.Linq;
using Sprache;
using TealCompiler.Tokens;

namespace TealCompiler
{
	public partial class Grammar
	{
		public static Program ParseProgram(string p_text)
		{
			Grammar l_grammar = new Grammar();
			return l_grammar.Program.Parse(p_text);
		}
	}
}