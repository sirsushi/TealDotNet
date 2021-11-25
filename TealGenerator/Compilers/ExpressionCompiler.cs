using System;
using System.Collections.Generic;
using TealCompiler.AbstractSyntaxTree;
using TealCompiler.TealGenerator.Assembly;
using TealDotNet.Semantic;

namespace TealCompiler.TealGenerator.Compilers
{
	public static class ExpressionCompiler
	{
		public static void Compile(this Expression p_instruction, CompiledProgramState p_state)
		{
			switch (p_instruction)
			{
				case BinaryOperationInstruction l_binaryOperationInstruction:
					l_binaryOperationInstruction.Compile(p_state);
					break;
				case UnaryOperationInstruction l_unaryOperationInstruction:
					l_unaryOperationInstruction.Value.Compile(p_state);
					switch (l_unaryOperationInstruction.Operator)
					{
						case "!":
						{
							p_state.Write(Opcodes.not);
							break;
						}
						case "~":
						{
							if (l_unaryOperationInstruction.Value.EvaluateExpressionType() == Types.Uint64)
								p_state.Write(Opcodes.complement);
							else if (l_unaryOperationInstruction.Value.EvaluateExpressionType() == Types.Bytes)
								p_state.Write(Opcodes.bcomplement);
							break;
						}
					}
					break;
				case CallInstruction l_callInstruction:
					foreach (Expression l_parameter in l_callInstruction.Parameters)
					{
						l_parameter.Compile(p_state);
					}
					p_state.Write(Opcodes.callsub, l_callInstruction.FunctionRef.Name);
					break;
				case BytesConstExpression l_bytesConstExpression:
					p_state.Write(Opcodes.pushbytes, l_bytesConstExpression.GetBase64String());
					break;
				case Uint64ConstExpression l_uint64ConstExpression:
					p_state.Write(Opcodes.pushint, l_uint64ConstExpression.Value);
					break;
				case Variable l_variable:
					ReadVariable(l_variable, p_state);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(p_instruction));
			}
		}

		private static void ReadVariable(Variable p_variable, CompiledProgramState p_state)
		{
			if (p_variable is Reference l_reference)
			{
				switch (l_reference.Name)
				{
					case "true":
					{
						p_state.Write(Opcodes.pushint, 1);
						break;
					}
					case "false":
					{
						p_state.Write(Opcodes.pushint, 0);
						break;
					}
					default:
					{
						p_state.Write(Opcodes.dig, p_state.GetVariablePosition(l_reference.Name));
						break;
					}
				}
			}
		}
	}
}