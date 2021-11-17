using System;
using System.Collections.Generic;
using System.Linq;
using Sprache;
using TealCompiler.AbstractSyntaxTree;

namespace TealCompiler
{
	public partial class Grammar
	{
		private Parser<Expression> Expression =>
			Parse.Ref(() => BinaryOperation)
				.Or(UnaryOperation)
				.Or(Call.Or<Expression>(Variable))
				.Or(Const);
		private Parser<Expression> SubExpression =>
			Parse.Ref(() => BinaryOperation).Contained(Parse.Char('(').Token(), Parse.Char(')').Token())
				.Or(UnaryOperation)
				.Or(Call.Or<Expression>(Variable))
				.Or(Const);

		private class OperatorList
		{
			public bool RightToLeft { get; set; }
			public string[] List { get; set; }

			public Parser<string> OperatorParser()
			{
				var l_parser = Parse.String(List[0]);
				for (int i = 1; i < List.Length; i++)
				{
					l_parser = l_parser.Or(Parse.String(List[i]));
				}

				return l_parser.Token().Text();
			}

			private delegate Parser<Expression> ChainDelegate(Parser<string> op,
				Parser<Expression> operand,
				Func<string, Expression, Expression, Expression>
					chaining);

			public Parser<Expression> Chain(Parser<Expression> p_operand)
			{
				ChainDelegate l_chainer = Parse.ChainOperator;
				if (RightToLeft)
					l_chainer = Parse.ChainRightOperator;

				return l_chainer(OperatorParser(), p_operand,
					(p_op, p_operand1, p_operand2) =>
					new BinaryOperationInstruction()
					{
						Operator = p_op,
						LeftValue = p_operand1,
						RightValue = p_operand2
					});
			}
		}


		private OperatorList[] m_binaryOperatorPrecedence = 
		{
			new(){RightToLeft = true, List=new[]
				{"="}},
			new(){RightToLeft = true, List=new[]
				{"?", ":"}},
			new(){RightToLeft = false, List=new[]
				{"||"}},
			new(){RightToLeft = false, List=new[]
				{"&&"}},
			new(){RightToLeft = false, List=new[]
				{"|"}},
			new(){RightToLeft = false, List=new[]
				{"^"}},
			new(){RightToLeft = false, List=new[]
				{"&"}},
			new(){RightToLeft = false, List=new[]
				{"==", "!="}},
			new(){RightToLeft = false, List=new[]
				{"<", ">", "<=", ">="}},
			new(){RightToLeft = false, List=new[]
				{"<<", ">>"}},
			new(){RightToLeft = false, List=new[]
				{"+", "-"}},
			new(){RightToLeft = false, List=new[]
				{"*", "/", "%"}},
			new(){RightToLeft = true, List=new[]
				{"**"}},
			new(){RightToLeft = true, List=new[]
				{"..", "..."}}
		};

		private Parser<Expression> BinaryOperation
		{
			get
			{
				Parser<Expression> l_parser = SubExpression;
				foreach (OperatorList l_operatorList in m_binaryOperatorPrecedence.Reverse())
				{
					l_parser = l_operatorList.Chain(l_parser);
				}

				return l_parser;
			}
		}

		private Parser<string> PrefixUnaryOperator =>
			new OperatorList() {List = new[] {"!", "~"}}.OperatorParser();

		private Parser<UnaryOperationInstruction> UnaryOperation =>
			from prefixOp in PrefixUnaryOperator
			from expression in
				Parse.Ref(() => BinaryOperation).Contained(Parse.Char('(').Token(), Parse.Char(')').Token())
					.XOr(Call.Or<Expression>(Variable))
					.XOr(Const)
			select new UnaryOperationInstruction()
			{
				Operator = prefixOp,
				Value = expression
			};
	}
}