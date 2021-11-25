using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic.CompilerServices;
using TealCompiler.AbstractSyntaxTree;

namespace TealDotNet.Semantic
{
	public static partial class Analyzer
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

			foreach (Expression l_expression in p_program.Constants)
			{
				if (l_expression is not BinaryOperationInstruction {Operator: "="} l_operation ||
				    l_operation.LeftValue is not Reference l_constantReference)
					throw new SemanticException(l_expression, "Can only do constant assignation outside of a function");
				Data.RegisterConstant(l_constantReference.Name, EvaluateExpressionType(l_operation.RightValue));
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
	}
}