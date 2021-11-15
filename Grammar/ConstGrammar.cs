using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Sprache;
using TealCompiler.Tokens;

namespace TealCompiler
{
	public partial class Grammar
	{
		private Parser<ConstExpression> Const =>
			Uint64Const.Or<ConstExpression>(BytesConst);

		private Parser<string> HexValue =>
			from code in Parse.IgnoreCase("0x")
			from value in Parse.Digit.Or(Parse.Chars("aAbBcCdDeEfF")).AtLeastOnce().Text()
			select value;

		private Parser<string> BinValue =>
			from code in Parse.IgnoreCase("0b")
			from value in Parse.Chars("01").AtLeastOnce().Text()
			select value;

		private Parser<string> OctValue =>
			from value in Parse.Identifier(Parse.Char('0'), Parse.Chars("01234567"))
			select value;

		private Parser<string> DecValue =>
			from value in Parse.Digit.AtLeastOnce().Text()
			select value;
		
		private Parser<string> BoolValue =>
			from value in Keyword("true").Or(Keyword("false"))
			select value;


		private Parser<Uint64ConstExpression> Uint64Const =>
			HexValue.Select(value => new Uint64ConstExpression()
			{
				Value = Convert.ToUInt64(value, 16)
			})
				.Or(BinValue.Select(value => new Uint64ConstExpression()
				{
					Value = Convert.ToUInt64(value, 2)
				}))
				.Or(OctValue.Select(value => new Uint64ConstExpression()
				{
					Value = Convert.ToUInt64(value, 8)
				}))
				.Or(DecValue.Select(value => new Uint64ConstExpression()
				{
					Value = Convert.ToUInt64(value)
				}))
				.Or(BoolValue.Select(value => new Uint64ConstExpression()
				{
					Value = (value == "true" ? 1ul : 0ul)
				}));

		private Parser<BytesConstExpression> BytesConst =>
			Parse.Char('#').Then(_ => Uint64Const).Select(value => new BytesConstExpression()
			{
				Value = ConvertToByteArray(value.Value)
			})
				.Or(Uint64Const.Select(expr => (byte)expr.Value)
					.DelimitedBy(Parse.Char(',').Token())
					.Contained(Parse.Char('[').Token(), Parse.Char(']').Token())
					.Select(values => new BytesConstExpression()
					{
						Value = values.ToArray()
					}))
				.Or(StringLiteral.Select(value => new BytesConstExpression()
				{
					Value = Encoding.UTF8.GetBytes(value)
				}));

		private static byte[] ConvertToByteArray(ulong p_value)
		{
			if (p_value == 0) return new byte[0];
			byte[] l_result = new byte[8];

			int l_index = 0;

			while (p_value > 0)
			{
				l_result[l_index] = (byte)(p_value & 0xFF);
				p_value >>= 8;
				l_index++;
			}

			return l_result.Take(l_index).Reverse().ToArray();
		}
	}
}