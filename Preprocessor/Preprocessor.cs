using System.Collections.Generic;
using System.Linq;
using Sprache;

namespace TealDotNet.Preprocessor
{
	public class Preprocessor
	{
		private class DefineProcessor : IPositionAware<DefineProcessor>
		{
			public string Name { get; set; }
			public string Value { get; set; }
			public Position Position { get; set; }
			public int Length { get; set; }
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
		
		
		public static string ProcessText(string p_text)
		{
			string l_processedText = p_text;
			List<DefineProcessor> l_defines = Define.Many().Parse(p_text).ToList();

			foreach (DefineProcessor l_define in l_defines.Reverse<DefineProcessor>())
			{
				l_processedText = l_processedText
					.Remove(l_define.Position.Pos, l_define.Length);
			}

			foreach (DefineProcessor l_define in l_defines)
			{
				l_processedText = l_processedText
					.Replace(l_define.Name, l_define.Value);
			}

			return l_processedText;
		}
	}
}