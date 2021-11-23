using System.Collections.Generic;
using System.Linq;
using Sprache;

namespace TealDotNet.Preprocessor
{
	public class Preprocessor
	{
		public class DefineProcessor : IPositionAware<DefineProcessor>
		{
			public string Name { get; set; }
			public string Value { get; set; }
			public Position Position { get; set; }
			public int Length { get; set; }
			public string Text { get; set; }

			public DefineProcessor SetPos(Position startPos, int length)
			{
				Position = startPos;
				Length = length;
				return this;
			}
		}

		private static Parser<DefineProcessor> Define =>
			from garbage in Parse.AnyChar.Except(Parse.Char('#')).Many()
			from define in (
				from action in Parse.String("#define").Token()
				from name in Parse.Identifier(Parse.Letter, Parse.LetterOrDigit.Or(Parse.Char('_'))).Token()
				from value in Parse.AnyChar.Until(Parse.LineTerminator).Text()
				select new DefineProcessor()
				{
					Name = name,
					Value = value
				}).Positioned()
			select define;
		
		
		public static List<DefineProcessor> ProcessText(string p_text)
		{
			List<DefineProcessor> l_defines = Define.Many().Parse(p_text).ToList();

			return l_defines;
		}
	}
}