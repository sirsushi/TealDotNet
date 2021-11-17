using System.Linq;
using Sprache;
using TealCompiler.AbstractSyntaxTree;

namespace TealCompiler
{
	public partial class Grammar
	{
		private Parser<string> RawKeyword(string text) =>
			Parse.IgnoreCase(text)
				.Then(n =>
					Parse.Not(Parse.LetterOrDigit.Or(Parse.Char('_'))))
				.Return(text);

		Parser<string> Keyword(string text) =>
			from testKeyword in RawKeyword(text).Preview()
			where testKeyword.IsDefined
			from keyword in RawKeyword(text) 
			select keyword;

		private Parser<string> RawIdentifier =>
			from identifier in Parse.Identifier(Parse.Letter.Or(Parse.Char('_')),
				Parse.LetterOrDigit.Or(Parse.Char('_')))
			where !TealKeywords.Keywords.Contains(identifier)
			select identifier;

		private Parser<string> Identifier =>
			RawIdentifier.Token().Named("Identifier");

		private Parser<Program> Program =>
			from defines in Parse.Char('#').Then(_ => Parse.AnyChar.Until(Parse.LineTerminator)).Many()
			from functions in Function.Token().XMany()
			select new Program()
			{
				Functions = functions.ToList()
			};

		private static readonly Parser<string> StringLiteral =
			from openQuote in Parse.Char('"')
			from fragments in Parse.Char('\\').Then(_ => Parse.AnyChar.Select(c => $"\\{c}"))
				.Or(Parse.CharExcept("\\\"").Many().Text()).Many()
			from closeQuote in Parse.Char('"')
			select $"{string.Join(string.Empty, fragments)}";
		
		private Parser<Instruction> Instruction =>
			If
				.XOr<Instruction>(While)
				.XOr(For)
				.XOr(Switch)
				.XOr(LineInstruction);

		private Parser<Instruction> LineInstruction =>
			from operation in
				Return
					.Or<Instruction>(Expression)
					.Or(Call)
			from end in Parse.Char(';').Token()
			select operation;
	}
}