using System.Collections.Generic;
using System.Linq;
using Sprache;

namespace TealDotNet.Lexer
{
	public class AzurLexer
	{
		public class LexerToken : IPositionAware<LexerToken>
		{
			public string Data { get; set; }
			public Position Position { get; set; }
			public int Length { get; set; }
			public LexerToken SetPos(Position startPos, int length)
			{
				Position = startPos;
				Length = length;
				return this;
			}

			public override string ToString()
			{
				return Data;
			}
		}

		public class NumberToken : LexerToken
		{
			public int Base { get; set; }

			public override string ToString()
			{
				return $"{Data} [number]";
			}
		}

		public class StringToken : LexerToken
		{
			public override string ToString()
			{
				return $"\"{Data}\" [string]";
			}
		}

		public class IdentifierToken : LexerToken
		{
			public override string ToString()
			{
				return $"{Data} [identifier]";
			}
		}

		public class SymbolToken : LexerToken
		{
			public override string ToString()
			{
				return $"'{Data}'";
			}
		}

		private static string[] s_symbols = new[]
		{
			"...", "..", "**",
			"*", "/", "%", "+", "-",
			"<<", ">>", "<=", ">=",
			"<", ">", "==", "!=",
			"&&", "||", "^", "&",
			"|", "?", ":", "=", ".",
			"{", "}", "(", ")", "[", "]",
			"@", ",", ";", "!", "~"
		};

		private Parser<NumberToken> HexValue =>
			from code in Parse.IgnoreCase("0x")
			from value in Parse.Digit.Or(Parse.Chars("aAbBcCdDeEfF")).AtLeastOnce().Text()
			select new NumberToken()
			{
				Base = 16,
				Data = value,
				
			};

		private Parser<NumberToken> BinValue =>
			from code in Parse.IgnoreCase("0b")
			from value in Parse.Chars("01").AtLeastOnce().Text()
			select new NumberToken()
			{
				Base = 2,
				Data = value
			};

		private Parser<NumberToken> OctValue =>
			from value in Parse.Identifier(Parse.Char('0'), Parse.Chars("01234567"))
			select new NumberToken()
			{
				Base = 8,
				Data = value
			};

		private Parser<NumberToken> DecValue =>
			from value in Parse.Digit.AtLeastOnce().Text()
			select new NumberToken()
			{
				Base = 10,
				Data = value
			};

		private static readonly Parser<StringToken> StringLiteral =
			from openQuote in Parse.Char('"')
			from fragments in Parse.Char('\\').Then(_ => Parse.AnyChar.Select(c => $"\\{c}"))
				.Or(Parse.CharExcept("\\\"").Many().Text()).Many()
			from closeQuote in Parse.Char('"')
			select new StringToken()
			{
				Data = $"{string.Join(string.Empty, fragments)}"
			};

		private static Parser<SymbolToken> Symbols => s_symbols
			.Select(Parse.String)
			.Aggregate((p1, p2) => p1.Or(p2))
			.Text()
			.Select(t => new SymbolToken()
			{
				Data = t
			});

		private static Parser<IdentifierToken> Text => Parse.LetterOrDigit.Or(Parse.Char('_')).Many().Text()
			.Select(t => new IdentifierToken()
			{
				Data = t
			});

		private static Parser<IEnumerable<LexerToken>> Token => StringLiteral.Or<LexerToken>(Text).Or(Symbols).Positioned().Token().Many();

		public static IEnumerable<LexerToken> ParseText(string p_text)
		{
			return Token.Parse(p_text);
		}
	}
}