using System.Collections.Generic;
using System.Linq;

namespace TealCompiler
{
	public static class TealKeywords
	{
		private static List<string> s_keywords = new()
		{
			"def", "if", /*"elif",*/ "else", "for", "in", "while", "switch", "case", "default", "continue", "break", "true", "false", "return", "exit", "throw"
		};
		private static List<string> s_reservedIdentifiers = new()
		{
			"txn"
		};

		public static IEnumerable<string> Keywords => s_keywords;

		public static IEnumerable<string> ReservedWords =>
			s_keywords.Concat(s_reservedIdentifiers);
	}
}