using System.Collections.Generic;
using System.Linq;
using TealCompiler.AbstractSyntaxTree;

namespace TealDotNet.Semantic
{
	public partial class Analyzer
	{
		private class Scope
		{
			public bool Inherit { get; set; } = true;
			public Dictionary<string, AzurField> Variables { get; } = new();
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
				CurrentScope.Variables.Add(p_name, new AzurField(Types.Any));
			}

			public void RegisterVariable(string p_name, AzurType p_type)
			{
				CurrentScope.Variables.Add(p_name, new AzurField(p_type));
			}

			public void RegisterConstant(string p_name)
			{
				CurrentScope.Variables.Add(p_name, AzurField.Constant(Types.Any));
			}

			public void RegisterConstant(string p_name, AzurType p_type)
			{
				CurrentScope.Variables.Add(p_name, AzurField.Constant(p_type));
			}

			public void RegisterEnum(AzurEnum p_enum)
			{
				foreach (string l_value in p_enum.Values)
				{
					CurrentScope.Variables.Add(l_value, AzurField.Constant(p_enum));
				}
			}

			public AzurField GetVariable(string p_name)
			{
				foreach (Scope l_scope in Scopes.Reverse())
				{
					if (l_scope.Variables.ContainsKey(p_name)) return l_scope.Variables[p_name];
					if (!l_scope.Inherit) break;
				}

				return null;
			}
			
			public void RegisterFunction(Function p_function)
			{
				CurrentScope.Functions.Add(p_function.Name, p_function);
			}
			
			public Function GetFunction(string p_name)
			{
				foreach (Scope l_scope in Scopes.Reverse())
				{
					if (l_scope.Functions.ContainsKey(p_name)) return l_scope.Functions[p_name];
					if (!l_scope.Inherit) break;
				}

				return null;
			}
		}
	}
}