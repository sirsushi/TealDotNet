using System.Collections.Generic;
using System.Linq;
using TealCompiler.AbstractSyntaxTree;

namespace TealCompiler.TealGenerator.Compilers
{
	public static class SwitchCompiler
	{
		public static void Compile(this SwitchInstruction l_switch, CompiledProgramState p_state)
		{
			if (!l_switch.Cases.Any() && l_switch.DefaultCase == null)
			{
				return;
			}

			CodeBlock l_block = l_switch.DefaultCase;

			// Convert switch to if forest
			for (int i = l_switch.Cases.Count - 1; i >= 0; i--)
			{
				Expression l_condition = new BinaryOperationInstruction()
				{
					Operator = "==",
					LeftValue = l_switch.TestedValue,
					RightValue = l_switch.Cases[i].Values[0]
				};
				for (int j = 1; j < l_switch.Cases[i].Values.Count; j++)
				{
					l_condition = new BinaryOperationInstruction()
					{
						Operator = "||",
						LeftValue = new BinaryOperationInstruction()
						{
							Operator = "==",
							LeftValue = l_switch.TestedValue,
							RightValue = l_switch.Cases[i].Values[j]
						},
						RightValue = l_condition
					};
				}

				l_block = new CodeBlock()
				{
					Instructions = new List<Instruction>
					{
						new IfInstruction()
						{
							Condition = l_condition,
							IfBlock = l_switch.Cases[i].Block,
							ElseBlock = l_block
						}
					}
				};
			}

			l_block.Compile(p_state);
		}
	}
}