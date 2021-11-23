using TealCompiler.AbstractSyntaxTree;

namespace TealDotNet.Semantic
{
	public partial class Analyzer
	{
		private static void AnalyzeBlock(CodeBlock p_block)
		{
			Data.EnterScope();

			foreach (Instruction l_blockInstructioninstr in p_block.Instructions)
			{
				switch (l_blockInstructioninstr)
				{
					case IfInstruction l_instruction:
						AnalyzeExpression(l_instruction.Condition);
						if (!CanBeUint64(l_instruction.Condition))
							throw new SemanticException(l_instruction, "Condition must be Uint64, {}");
						AnalyzeBlock(l_instruction.IfBlock);
						if (l_instruction.ElseBlock != null)
							AnalyzeBlock(l_instruction.ElseBlock);

						break;
					case SwitchInstruction l_instruction:
						AnalyzeExpression(l_instruction.TestedValue);
						AzurType l_testType = EvaluateExpressionType(l_instruction.TestedValue);
						foreach (SwitchCase l_case in l_instruction.Cases)
						{
							foreach (Expression l_caseValue in l_case.Values)
							{
								AnalyzeExpression(l_caseValue);
								if (!IsCompatible(EvaluateExpressionType(l_caseValue), l_testType))
									throw new SemanticException(l_caseValue,
										$"Must be of the same type of tested value {l_testType.Name}");
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
	}
}