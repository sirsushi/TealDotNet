using System.IO;
using TealCompiler;
using TealCompiler.Tokens;

namespace TealDotNet
{
	public static class Compiler
	{
		public static void Main()
		{
			Grammar.ParseProgram(File.ReadAllText("tealProgram.az"));
		}
	}
}