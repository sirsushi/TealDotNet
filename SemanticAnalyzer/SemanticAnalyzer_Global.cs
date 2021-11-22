﻿using System;
using TealCompiler.AbstractSyntaxTree;

namespace TealDotNet.Semantic
{
	public partial class Analyzer
	{
		private static void RegisterGlobalConstants()
		{
			Data.RegisterConstant("Txn", Types.ApplicationCallTransaction);
			Data.RegisterConstant("TxnGroup", Types.Transaction.ToArray(Types.Uint64));
			Data.RegisterConstant("Global", Types.Global);

			Data.RegisterConstant("true", Types.Uint64);
			Data.RegisterConstant("false", Types.Uint64);
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