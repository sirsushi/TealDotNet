using System;
using TealCompiler.AbstractSyntaxTree;

namespace TealDotNet.Semantic
{
	public partial class Analyzer
	{
		private static void AnalyzeExpression(Expression p_expression)
		{
			switch (p_expression)
			{
				case BinaryOperationInstruction {Operator: "="} l_assignation:
				{
					AnalyzeAssignation(l_assignation);
					break;
				}
				case BinaryOperationInstruction l_operation:
				{
					AnalyzeBinaryOperations(l_operation);
					break;
				}
				case UnaryOperationInstruction l_operation:
				{
					if (l_operation.Operator == "!" && !CanBeUint64(l_operation.Value))
						throw new SemanticException(l_operation,
							$"Operation '!' can only be done on Uint64");
					AnalyzeExpression(l_operation.Value);
					break;
				}
				case CallInstruction l_call:
				{
					Function l_function = Data.GetFunction(l_call.FunctionRef.Name);
					if (l_function == null)
						throw new SemanticException(l_call, "Unknown function");
					if (l_call.Parameters.Count != l_function.Parameters.Count)
						throw new SemanticException(l_call, "Wrong number of arguments");
					break;
				}
				case Variable l_variable:
				{
					AzurField l_field = GetField(l_variable, Data.GetVariable, out bool _);
					if (l_field == null)
						throw new SemanticException(l_variable, "Variable is not declared before first use");
					break;
				}
			}
		}

		private static void AnalyzeBinaryOperations(BinaryOperationInstruction l_operation)
		{
			AnalyzeExpression(l_operation.LeftValue);
			AnalyzeExpression(l_operation.RightValue);
			AzurType l_leftType = EvaluateExpressionType(l_operation.LeftValue);
			AzurType l_rightType = EvaluateExpressionType(l_operation.RightValue);
			if (l_operation.Operator == ":")
			{
				if (l_rightType != Types.Type)
					throw new SemanticException(l_operation, "Cast target must be a type");
			}

			if (l_operation.Operator is "==" or "!=" or ":")
			{
				if (l_operation.RightValue is Reference l_typeReference &&
				    l_rightType == Types.Type)
				{
					if (!l_leftType
						    .IsAssignableFrom(Types.Get(l_typeReference.Name)) &&
					    !Types.Get(l_typeReference.Name)
						    .IsAssignableFrom(l_leftType))
						throw new SemanticException(l_operation,
							$"Can't cast {l_operation.LeftValue} to {l_typeReference.Name}");
					return;
				}
			}

			if (!IsCompatible(l_leftType, l_rightType))
				throw new SemanticException(l_operation, "Can't operate on different type operands");
			if (!IsBaseType(l_leftType) || !IsBaseType(l_rightType))
				throw new SemanticException(l_operation, "Can't operate on non-basic type operands");
		}

		private static void AnalyzeAssignation(BinaryOperationInstruction l_assignation)
		{
			if (l_assignation.LeftValue is not Variable)
				throw new SemanticException(l_assignation, "Left part is not assignable");
			AzurField l_field = GetField(l_assignation.LeftValue as Variable, Data.GetVariable,
				out bool l_writable);
			if (l_field == null)
			{
				if (l_assignation.LeftValue is Reference l_reference)
					Data.RegisterVariable(l_reference.Name, EvaluateExpressionType(l_assignation.RightValue));
				else
					throw new SemanticException(l_assignation, "Left part is not assignable");
			}
			else
			{
				if (l_field.ConstReference || !l_writable)
					throw new SemanticException(l_assignation, "Left part is not assignable");
			}

			AnalyzeExpression(l_assignation.RightValue);
		}

		public static AzurType EvaluateExpressionType(this Expression p_expression)
		{
			if (p_expression is Variable l_variable) return GetField(l_variable, Data.GetVariable, out bool _).Type;
			if (p_expression is BinaryOperationInstruction
				{Operator: ":", RightValue: Reference l_castTypeReference})
				return Types.Get(l_castTypeReference.Name);
			if (p_expression is BinaryOperationInstruction
				{Operator: "==" or "!=" or "<" or ">" or "<=" or ">=" or "&&" or "||"})
				return Types.Uint64;
			if (p_expression is BinaryOperationInstruction l_operation)
				return EvaluateExpressionType(l_operation.LeftValue);
			if (p_expression is Uint64ConstExpression) return Types.Uint64;
			if (p_expression is BytesConstExpression) return Types.Bytes;
			return Types.Any;
		}

		private static AzurField GetField(Variable p_variable, Func<string, AzurField> p_fieldGetter, out bool p_writable)
		{
			AzurField l_field = null;
			p_writable = true;
			
			switch (p_variable)
			{
				case CallInstruction l_call:
				{
					throw new SemanticException(l_call, "Function call is not a field");
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