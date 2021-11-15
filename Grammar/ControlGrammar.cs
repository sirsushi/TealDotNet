using System.Collections.Generic;
using System.Linq;
using Sprache;
using TealCompiler.Tokens;

namespace TealCompiler
{
	public partial class Grammar
	{
		private Parser<IfInstruction> If =>
			from ifBlock in IfBlock
			/*from elifBlocks in ElifBlock.Many()*/
			from elseBlock in ElseBlock.Optional()
			select MergeBlocks(ifBlock, /*elifBlocks,*/ elseBlock);

		private IfInstruction MergeBlocks(IfInstruction ifBlock, /*IEnumerable<IfInstruction> elifBlocks,*/ IOption<CodeBlock> elseBlock)
		{
			IfInstruction l_current = ifBlock;
			/*foreach (IfInstruction l_elifBlock in elifBlocks)
			{
				l_current.ElseBlock = new CodeBlock()
				{
					Instructions = new[]
					{
						l_elifBlock
					}
				};
				l_current = l_elifBlock;
			}*/

			if (elseBlock.IsDefined)
				l_current.ElseBlock = elseBlock.Get();

			return ifBlock;
		}

		private Parser<IfInstruction> IfBlock =>
			from ifKeyword in Keyword("if").Token()
			from condition in Expression
			from code in SimplifiedCodeBlock
			select new IfInstruction()
			{
				Condition = condition,
				IfBlock = code
			};

		/*private Parser<IfInstruction> ElifBlock =>
			from ifKeyword in Keyword("elif").Token()
			from condition in Expression
			from code in SimplifiedCodeBlock
			select new IfInstruction()
			{
				Condition = condition,
				IfBlock = code
			};*/
		
		private Parser<CodeBlock> ElseBlock =>
			from ifKeyword in Keyword("else").Token()
			from code in SimplifiedCodeBlock
			select code;
		
		private Parser<WhileInstruction> While => Parse.Char('@').Select(_ => new WhileInstruction());
		private Parser<ForInstruction> For => Parse.Char('@').Select(_ => new ForInstruction());
		private Parser<SwitchInstruction> Switch =>
			from switchKeyword in Keyword("switch").Token()
			from testedValue in Expression
			from openBacket in Parse.Char('{').Token()
			from cases in SwitchCase.XMany()
			from defaultCase in DefaultCase.XOptional()
			from closeBracket in Parse.Char('}').Token()
			select new SwitchInstruction()
			{
				TestedValue = testedValue,
				Cases = cases.ToArray(),
				DefaultCase = defaultCase.GetOrDefault()
			};
		
		Parser<SwitchCase> SwitchCase =>
			from caseKeyword in Keyword("case").Token()
			from expression in Expression.DelimitedBy(Parse.Char(',').Token())
			from code in CodeBlock
			select new SwitchCase()
			{
				Values = expression.ToArray(),
				Block = code
			};
		Parser<CodeBlock> DefaultCase =>
			from caseKeyword in Keyword("default").Token()
			from code in CodeBlock
			select code;
	}
}