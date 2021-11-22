using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic.CompilerServices;
using TealCompiler.AbstractSyntaxTree;

namespace TealDotNet.SemanticAnalyzer
{
	public partial class SemanticAnalyzer
	{
		[Flags]
		public enum Flags
		{
			ApprovalProgram = 1,
			ClearStateProgram = 2,
			Signature = 4
		}
		public class SemanticException : Exception
		{
			
		}

		private static SemanticData Data { get; set; }
		
		public static void Analyze(Program p_program, Flags p_flags)
		{
			Data = new SemanticData()
			{
				Program = p_program
			};
			
			AnalyzeProgram(p_program, p_flags);
		}

		private static void AnalyzeProgram(Program p_program, Flags p_flags)
		{
			Data.EnterScope();

			if (p_flags.HasFlag(Flags.Signature) && p_flags.HasFlag(Flags.ApprovalProgram | Flags.ClearStateProgram))
				throw new InvalidOperationException();

			if (p_flags.HasFlag(Flags.Signature))
			{
				RegisterSignatureConstants();
				RegisterSignatureFunctions();
			}
			else
			{
				RegisterSmartContractConstants();
				RegisterSmartContractFunctions();
			}

			RegisterGlobalConstants();
			RegisterGlobalFunctions();

			foreach (Function l_function in p_program.Functions)
			{
				Data.RegisterFunction(l_function);
			}
			
			if (p_flags.HasFlag(Flags.ApprovalProgram))
			{
				if (!Data.FunctionExist("ApprovalProgram"))
					throw new SemanticException();
			}

			if (p_flags.HasFlag(Flags.ClearStateProgram))
			{
				if (!Data.FunctionExist("ClearStateProgram"))
					throw new SemanticException();
			}

			if (p_flags.HasFlag(Flags.Signature))
			{
				if (!Data.FunctionExist("Signature"))
					throw new SemanticException();
			}

			foreach (Function l_function in p_program.Functions)
			{
				AnalyzeFunction(l_function);
			}
			
			Data.ExitScope();
		}

		private static void AnalyzeFunction(Function p_function)
		{
			Data.EnterScope();

			foreach (string l_parameter in p_function.Parameters)
			{
				Data.RegisterVariable(l_parameter);
			}
			
			AnalyzeBlock(p_function.Block);
			
			Data.ExitScope();
		}

		private static void AnalyzeBlock(CodeBlock p_block)
		{
			Data.EnterScope();

			foreach (Instruction l_blockInstructioninstr in p_block.Instructions)
			{
				switch (l_blockInstructioninstr)
				{
					case IfInstruction l_instruction:
						if (!l_instruction.Condition.EvaluateToUint64())
							throw new SemanticException();
						AnalyzeBlock(l_instruction.IfBlock);
						if (l_instruction.ElseBlock != null)
							AnalyzeBlock(l_instruction.ElseBlock);
						break;
					case SwitchInstruction l_instruction:
						var l_testType = l_instruction.TestedValue.EvaluateTo();
						foreach (SwitchCase l_case in l_instruction.Cases)
						{
							if (l_case.Values.Any(v => v.EvaluateTo() != l_testType))
								throw new SemanticException();
							AnalyzeBlock(l_case.Block);
						}
						if (l_instruction.DefaultCase != null)
							AnalyzeBlock(l_instruction.DefaultCase);
						break;
					case ForInstruction l_instruction:
						break;
					case WhileInstruction l_instruction:
						break;
					case DoWhileInstruction l_instruction:
						break;
					case ReturnInstruction l_instruction:
						AnalyzeExpression(l_instruction.Value);
						break;
					case Expression l_instruction:
						AnalyzeExpression(l_instruction);
						break;
				}
			}
			
			Data.ExitScope();
		}

		private static void AnalyzeExpression(Expression p_expression)
		{
			switch (p_expression)
			{
				case BinaryOperationInstruction {Operator: "="} l_expression:
					if (l_expression.LeftValue is not Variable or CallInstruction)
						throw new SemanticException();
					AnalyzeVariableAssigned(l_expression.LeftValue as Variable);
					AnalyzeExpression(l_expression.RightValue);
					break;
				
			}
		}

		private static void AnalyzeVariableAssigned(Variable p_variable)
		{
			
		}
	}
}