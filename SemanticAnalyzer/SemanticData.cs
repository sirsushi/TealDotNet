using System.Collections.Generic;
using System.Linq;
using TealCompiler.AbstractSyntaxTree;

namespace TealDotNet.SemanticAnalyzer
{
	public partial class SemanticAnalyzer
	{
		private class Scope
		{
			public bool Inherit { get; set; } = true;
			public Dictionary<string, AzurType> Variables { get; } = new();
			public Dictionary<string, AzurType> Constants { get; } = new();
			public Dictionary<string, Function> Functions { get; } = new();
		}
		
		private class SemanticData
		{
			public Program Program { get; set; }
			private IList<Scope> Scopes { get; } = new List<Scope>();

			public void EnterScope(bool p_inherit = true)
			{
				Scopes.Add(new()
				{
					Inherit = p_inherit
				});
			}

			public void ExitScope()
			{
				Scopes.RemoveAt(Scopes.Count - 1);
			}

			private Scope CurrentScope => Scopes.Last();

			public void RegisterVariable(string p_name)
			{
				CurrentScope.Variables.Add(p_name, new AzurType());
			}

			public void RegisterVariable(string p_name, AzurType p_type)
			{
				CurrentScope.Variables.Add(p_name, p_type);
			}

			public void RegisterConstant(string p_name)
			{
				CurrentScope.Constants.Add(p_name, new AzurType());
			}

			public void RegisterConstant(string p_name, AzurType p_type)
			{
				CurrentScope.Constants.Add(p_name, p_type);
			}

			public bool ReadableVariableExist(string p_name)
			{
				foreach (Scope l_scope in Scopes.Reverse())
				{
					if (l_scope.Variables.ContainsKey(p_name) || l_scope.Constants.ContainsKey(p_name)) return true;
					if (!l_scope.Inherit) break;
				}

				return false;
			}
			
			public bool WritableVariableExist(string p_name)
			{
				foreach (Scope l_scope in Scopes.Reverse())
				{
					if (l_scope.Variables.ContainsKey(p_name)) return true;
					if (!l_scope.Inherit) break;
				}

				return false;
			}
			
			public void RegisterFunction(Function p_function)
			{
				CurrentScope.Functions.Add(p_function.Name, p_function);
			}
			
			public bool FunctionExist(string p_name)
			{
				foreach (Scope l_scope in Scopes.Reverse())
				{
					if (l_scope.Functions.ContainsKey(p_name)) return true;
					if (!l_scope.Inherit) break;
				}

				return false;
			}
		}
	}
}