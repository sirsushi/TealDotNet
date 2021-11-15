using System.Collections.Generic;
using System.Linq;
using Sprache;
using TealCompiler.Tokens;

namespace TealCompiler
{
	public partial class Grammar
	{
		private Parser<Function> Function =>
			from defKeyword in Keyword("def").Token()
			from name in Identifier
			from parameters in ParametersDeclaration
			from block in CodeBlock
			select new Function()
			{
				Name = name,
				Parameters = parameters.ToArray(),
				Block = block
			};

		private Parser<IEnumerable<string>> ParametersDeclaration =>
			from parameters in Identifier
				.DelimitedBy(Parse.Char(',').Token())
				.Optional()
				.Contained(Parse.Char('(').Token(), Parse.Char(')').Token())
			select parameters.GetOrElse(Enumerable.Empty<string>());

		private Parser<CodeBlock> CodeBlock =>
			from instructions in Instruction.Many()
				.Contained(Parse.Char('{').Token(), Parse.Char('}').Token())
			select new CodeBlock()
			{
				Instructions = instructions.ToArray()
			};

		private Parser<CodeBlock> SimplifiedCodeBlock =>
			CodeBlock.Or(Instruction.Once().Select(instr => new CodeBlock()
			{
				Instructions = instr.ToArray()
			}));

		private Parser<CallInstruction> Call =>
			from identifier in Identifier
			from parameters in ParametersAssignation
			select new CallInstruction()
			{
				FunctionName = identifier,
				Parameters = parameters.ToArray()
			};

		private Parser<IEnumerable<Expression>> ParametersAssignation =>
			from parameters in Expression
				.DelimitedBy(Parse.Char(',').Token())
				.Optional()
				.Contained(Parse.Char('(').Token(), Parse.Char(')').Token())
			select parameters.GetOrElse(Enumerable.Empty<Expression>());

		private Parser<ReturnInstruction> Return =>
			from returnKeyword in Keyword("return")
				.Or(Keyword("exit"))
				.Or(Keyword("throw"))
				.Token()
			from value in Expression
			select new ReturnInstruction()
			{
				Operator = returnKeyword,
				Value = value
			};
	}
}