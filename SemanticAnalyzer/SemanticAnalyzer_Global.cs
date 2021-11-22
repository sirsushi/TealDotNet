using System;
using TealCompiler.AbstractSyntaxTree;

namespace TealDotNet.SemanticAnalyzer
{
	public partial class SemanticAnalyzer
	{
		private static void RegisterGlobalConstants()
		{
			Data.RegisterConstant("Txn", Types.ApplicationCallTransaction);
			Data.RegisterConstant("TxnGroup", Types.Transaction.ToArray(Types.Uint64));
			Data.RegisterConstant("Global", Types.Global);
		}

		private static void RegisterGlobalFunctions()
		{
			Data.RegisterFunction(new Function()
			{
				Name = "Hash",
				Parameters = new() {"text", "hashFunction"}
			});
		}
	}
}