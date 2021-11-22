using System;

namespace TealDotNet.Semantic
{
	public partial class Analyzer
	{
		private static void RegisterSmartContractConstants()
		{
			Data.RegisterEnum(Types.OnComplete);
			Data.RegisterConstant("args", Types.BytesArray);
		}

		private static void RegisterSmartContractFunctions()
		{
			
		}
	}
}