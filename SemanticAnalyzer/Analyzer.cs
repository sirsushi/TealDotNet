using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic.CompilerServices;
using TealCompiler.AbstractSyntaxTree;

namespace TealDotNet.Semantic
{
	public partial class Analyzer
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
			public SemanticException(Node p_syntaxTreeNode, string p_message)
				: base($"{p_message}: {p_syntaxTreeNode.GetSourceExtract()} at {p_syntaxTreeNode.FirstToken.Position}")
			{
			}
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
				if (Data.GetFunction("ApprovalProgram") == null)
					throw new SemanticException(p_program, "Missing main function");
			}

			if (p_flags.HasFlag(Flags.ClearStateProgram))
			{
				if (Data.GetFunction("ClearStateProgram") == null)
					throw new SemanticException(p_program, "Missing main function");
			}

			if (p_flags.HasFlag(Flags.Signature))
			{
				if (Data.GetFunction("Signature") == null)
					throw new SemanticException(p_program, "Missing main function");
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

		private static AzurType EvaluateExpressionType(Expression p_expression)
		{
			if (p_expression is Variable l_variable) return GetField(l_variable, Data.GetVariable, out bool _).Type;
			return Types.Any;
		}

		private static bool CanBeUint64(Expression p_expression)
		{
			AzurType l_type = EvaluateExpressionType(p_expression);
			return l_type == Types.Uint64 || l_type == Types.Any;
		}

		private static bool IsBaseType(AzurType p_type)
		{
			return p_type == Types.Any || p_type == Types.Uint64 || p_type == Types.Bytes;
		}

		private static bool IsCompatible(AzurType p_type1, AzurType p_type2)
		{
			if (p_type1 == Types.Any)
			{
				return IsBaseType(p_type2);
			}
			else if (p_type2 == Types.Any)
			{
				return IsBaseType(p_type1);
			}

			return p_type1 == p_type2;
		}

		private static void AnalyzeBlock(CodeBlock p_block)
		{
			Data.EnterScope();

			foreach (Instruction l_blockInstructioninstr in p_block.Instructions)
			{
				switch (l_blockInstructioninstr)
				{
					case IfInstruction l_instruction:
						if (!CanBeUint64(l_instruction.Condition))
							throw new SemanticException(l_instruction, "Condition must be Uint64, {}");
						AnalyzeBlock(l_instruction.IfBlock);
						if (l_instruction.ElseBlock != null)
							AnalyzeBlock(l_instruction.ElseBlock);
						break;
					case SwitchInstruction l_instruction:
						AzurType l_testType = EvaluateExpressionType(l_instruction.TestedValue);
						foreach (SwitchCase l_case in l_instruction.Cases)
						{
							foreach (Expression l_caseValue in l_case.Values)
							{
								if (!IsCompatible(EvaluateExpressionType(l_caseValue), l_testType))
									throw new SemanticException(l_caseValue, $"Must be of the same type of tested value {l_testType.Name}");
							}
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
				case BinaryOperationInstruction {Operator: "="} l_assignation:
				{
					if (l_assignation.LeftValue is not Variable)
						throw new SemanticException(l_assignation, "Left part is not assignable");
					AzurField l_field = GetField(l_assignation.LeftValue as Variable, Data.GetVariable,
						out bool l_writable);
					if (l_field == null)
					{
						if (l_assignation.LeftValue is Reference l_reference)
							Data.RegisterVariable(l_reference.Name);
						else
							throw new SemanticException(l_assignation, "Left part is not assignable");
					}
					else
					{
						if (l_field.ConstReference || !l_writable)
							throw new SemanticException(l_assignation, "Left part is not assignable");
					}

					AnalyzeExpression(l_assignation.RightValue);
					break;
				}
				case BinaryOperationInstruction l_operation:
				{
					AnalyzeExpression(l_operation.LeftValue);
					AnalyzeExpression(l_operation.RightValue);
					break;
				}
				case UnaryOperationInstruction l_operation:
				{
					AnalyzeExpression(l_operation.Value);
					break;
				}
				case Variable l_variable:
				{
					AzurField l_field = GetField(l_variable, Data.GetVariable, out bool _);
					if (l_field == null) throw new SemanticException(l_variable, "Variable is not declared before first use");
					break;
				}
			}
		}

		private static AzurField GetField(Variable p_variable, Func<string, AzurField> p_fieldGetter, out bool p_writable)
		{
			AzurField l_field = null;
			p_writable = true;
			
			switch (p_variable)
			{
				case CallInstruction l_call:
				{
					var l_functionReference = l_call.FunctionName as Reference;
					if (Data.GetFunction(l_functionReference.Name) == null)
						throw new SemanticException(l_call, "Function not defined");
					l_field = AzurField.Constant(Types.Any);
					p_writable = false;
					break;
				}
				case Reference l_reference:
				{
					l_field = p_fieldGetter(l_reference.Name);
					if (l_field != null)
						p_writable = !l_field.ConstMembers;
					break;
				}
				case MemberAccessInstruction l_memberAccess:
				{
					l_field = GetField(l_memberAccess.Owner, p_fieldGetter, out p_writable);
					if (l_field == null)
						throw new SemanticException(l_memberAccess.Owner, "Variable is not declared before first use");
					l_field = GetField(l_memberAccess.Member, l_field.Type.Get, out bool l_memberWritable);
					if (l_field == null) throw new SemanticException(l_memberAccess,
						$"{l_memberAccess.Member.Name} is not part of {EvaluateExpressionType(l_memberAccess.Owner).Name}");
					p_writable &= l_memberWritable;
					break;
				}
				case ArrayAccessInstruction l_arrayAccess:
				{
					l_field = GetField(l_arrayAccess.Array, p_fieldGetter, out p_writable);
					if (l_field == null)
						throw new SemanticException(l_arrayAccess, "Variable is not declared before first use");
					l_field = l_field.Type.Get("[]");
					if (l_field == null) throw new SemanticException(l_arrayAccess,
						$"{EvaluateExpressionType(l_arrayAccess.Array).Name} doesn't have a [] accessor");
					p_writable &= !l_field.ConstMembers;
					AnalyzeExpression(l_arrayAccess.Index);
					
					break;
				}
				default:
					throw new SemanticException(p_variable, "Unmanaged token");
			}

			return l_field;
		}
	}
}