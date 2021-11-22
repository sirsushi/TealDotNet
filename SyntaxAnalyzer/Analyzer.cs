using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using TealCompiler.AbstractSyntaxTree;
using TealDotNet.Lexer;

namespace TealDotNet.Syntax
{
	public class Analyzer
	{
		public class SyntaxException : Exception
		{
			public SyntaxException(string p_expected, AzurLexer.LexerToken p_token)
			:base($"{p_expected} expected, but found {p_token}. ({p_token.Position})")
			{
			}

			public SyntaxException(AzurLexer.LexerToken p_token)
				: base($"Unexpected {p_token}. ({p_token.Position})")
			{
			}
		}
		private class TokenEnumerator
		{
			private List<AzurLexer.LexerToken> m_tokens;
			private int m_index;

			public TokenEnumerator(List<AzurLexer.LexerToken> p_tokens)
			{
				m_tokens = p_tokens;
				m_index = -1;
			}

			public void Move(int p_index)
			{
				m_index = p_index;
			}

			public int Pos()
			{
				return m_index;
			}

			public AzurLexer.LexerToken Current => m_index < m_tokens.Count ? m_tokens[m_index] : null;

			public AzurLexer.LexerToken Next()
			{
				m_index++;
				return Current;
			}

			public string NextIdentifier()
			{
				if (Next() is AzurLexer.IdentifierToken l_token)
					return l_token.Data;

				throw new SyntaxException("Identifier", Current);
			}

			public bool CheckSymbol(string p_symbols)
			{
				if (Next() is AzurLexer.SymbolToken l_token && l_token.Data == p_symbols)
					return true;
				throw new SyntaxException($"'{p_symbols}'", Current);
			}

			public string CheckSymbols(params string[] p_symbols)
			{
				if (Next() is AzurLexer.SymbolToken l_token)
				{
					for (int i = 0; i < p_symbols.Length; i++)
					{
						if (l_token.Data == p_symbols[i]) return p_symbols[i];
					}
				}

				throw new SyntaxException(string.Join(", ", $"'{p_symbols}'"), Current);
			}

			public bool LookAhead(string p_token, bool p_goAheadWhenFound = false)
			{
				if (m_tokens[m_index + 1].Data == p_token)
				{
					if (p_goAheadWhenFound)
						m_index++;
					return true;
				}

				return false;
			}

			public bool LookAheadMany(string[] p_tokens, bool p_goAheadWhenFound = false)
			{
				for (int i = 0; i < p_tokens.Length; i++)
				{
					if (m_tokens[m_index + 1].Data == p_tokens[i])
					{
						if (p_goAheadWhenFound)
							m_index++;
						return true;
					}
				}

				return false;
			}

			public bool LookMany(string[] p_tokens)
			{
				for (int i = 0; i < p_tokens.Length; i++)
				{
					if (Current.Data == p_tokens[i])
					{
						return true;
					}
				}

				return false;
			}

			public IEnumerable<AzurLexer.LexerToken> EnumerateUntilCorresponding(string p_close)
			{
				string l_open = Current.Data;
				int l_parenthesesCount = 1;
				yield return Current;

				while (l_parenthesesCount > 0)
				{
					var l_token = Next();
					if (l_token.Data == l_open)
						l_parenthesesCount++;
					if (l_token.Data == p_close)
						l_parenthesesCount--;

					yield return l_token;
				}
			}

			public AzurLexer.NumberToken NextNumber()
			{
				if (Next() is AzurLexer.NumberToken l_token)
					return l_token;

				throw new SyntaxException("Number", Current);
			}
		}

		public static Program Analyze(List<AzurLexer.LexerToken> p_tokens)
		{
			TokenEnumerator l_enumerator = new(p_tokens.Where(t => t is not AzurLexer.CommentToken).ToList());
			
			Program l_program = new Program();

			l_program.FirstToken = p_tokens.First();
			l_program.LastToken = p_tokens.Last();

			while (l_enumerator.Next() != null)
			{
				if (l_enumerator.Current is not AzurLexer.IdentifierToken {Data: Keywords.Def})
					throw new SyntaxException("Function definition", l_enumerator.Current);
				l_program.Functions.Add(AnalyzeFunction(l_enumerator));
			}

			return l_program;
		}

		private static Function AnalyzeFunction(TokenEnumerator p_enumerator)
		{
			Function l_function = new();

			l_function.FirstToken = p_enumerator.Current;

			l_function.Name = p_enumerator.NextIdentifier();

			l_function.MainToken = p_enumerator.Current;
			
			p_enumerator.CheckSymbol("(");

			if (!p_enumerator.LookAhead(")", true))
			{
				do
				{
					l_function.Parameters.Add(p_enumerator.NextIdentifier());
				} while (p_enumerator.CheckSymbols(",", ")") != ")");
			}

			l_function.Block = AnalyzeCodeBlock(p_enumerator, false);

			l_function.LastToken = p_enumerator.Current;

			return l_function;
		}

		private static CodeBlock AnalyzeCodeBlock(TokenEnumerator p_enumerator, bool p_oneLineSimplifiable)
		{
			CodeBlock l_block = new();

			if (p_oneLineSimplifiable && !p_enumerator.LookAhead("{"))
			{
				l_block.Instructions.Add(AnalyzeInstruction(p_enumerator));

				l_block.FirstToken = l_block.Instructions[0].FirstToken;
				l_block.LastToken = l_block.Instructions[0].LastToken;
			}
			else
			{
				p_enumerator.CheckSymbol("{");

				l_block.FirstToken = p_enumerator.Current;

				while (!p_enumerator.LookAhead("}", true))
				{
					l_block.Instructions.Add(AnalyzeInstruction(p_enumerator));
				}

				l_block.LastToken = p_enumerator.Current;
			}

			return l_block;
		}

		private static Instruction AnalyzeInstruction(TokenEnumerator p_enumerator)
		{
			Instruction l_instruction;
			switch (p_enumerator.Next())
			{
				case AzurLexer.IdentifierToken {Data: Keywords.If}:
					l_instruction = AnalyzeIfInstruction(p_enumerator);
					break;
				case AzurLexer.IdentifierToken {Data: Keywords.For}:
					l_instruction = AnalyzeForInstruction(p_enumerator);
					break;
				case AzurLexer.IdentifierToken {Data: Keywords.While}:
					l_instruction = AnalyzeWhileInstruction(p_enumerator);
					break;
				case AzurLexer.IdentifierToken {Data: Keywords.Do}:
					l_instruction = AnalyzeDoWhileInstruction(p_enumerator);
					break;
				case AzurLexer.IdentifierToken {Data: Keywords.Switch}:
					l_instruction = AnalyzeSwitchInstruction(p_enumerator);
					break;
				case AzurLexer.IdentifierToken {Data: Keywords.Return}:
				case AzurLexer.IdentifierToken {Data: Keywords.Throw}:
				case AzurLexer.IdentifierToken {Data: Keywords.Exit}:
					l_instruction = AnalyzeReturnInstruction(p_enumerator);
					p_enumerator.CheckSymbol(";");
					break;
				default:
					p_enumerator.Move(p_enumerator.Pos() - 1);
					l_instruction = AnalyzeExpression(p_enumerator);
					p_enumerator.CheckSymbol(";");
					break;
			}

			return l_instruction;
		}

		private static Instruction AnalyzeIfInstruction(TokenEnumerator p_enumerator)
		{
			IfInstruction l_instruction = new();

			l_instruction.FirstToken = p_enumerator.Current;

			l_instruction.MainToken = p_enumerator.Current;

			p_enumerator.CheckSymbol("(");

			l_instruction.Condition = AnalyzeExpression(p_enumerator);

			p_enumerator.CheckSymbol(")");

			l_instruction.IfBlock = AnalyzeCodeBlock(p_enumerator, true);

			if (p_enumerator.LookAhead(Keywords.Else, true))
			{
				l_instruction.ElseBlock = AnalyzeCodeBlock(p_enumerator, true);
			}

			l_instruction.LastToken = p_enumerator.Current;

			return l_instruction;
		}

		private static Instruction AnalyzeForInstruction(TokenEnumerator p_enumerator)
		{
			throw new NotImplementedException();
		}

		private static Instruction AnalyzeWhileInstruction(TokenEnumerator p_enumerator)
		{
			throw new NotImplementedException();
		}

		private static Instruction AnalyzeDoWhileInstruction(TokenEnumerator p_enumerator)
		{
			throw new NotImplementedException();
		}

		private static Instruction AnalyzeSwitchInstruction(TokenEnumerator p_enumerator)
		{
			SwitchInstruction l_instruction = new();

			l_instruction.FirstToken = p_enumerator.Current;

			l_instruction.MainToken = p_enumerator.Current;

			p_enumerator.CheckSymbol("(");

			l_instruction.TestedValue = AnalyzeExpression(p_enumerator);

			p_enumerator.CheckSymbol(")");

			p_enumerator.CheckSymbol("{");
			
			while (!p_enumerator.LookAhead("}", true))
			{
				switch (p_enumerator.Next())
				{
					case AzurLexer.IdentifierToken {Data: Keywords.Case}:
						l_instruction.Cases.Add(AnalyzeCase(p_enumerator));
						break;
					case AzurLexer.IdentifierToken {Data: Keywords.Default}:
						l_instruction.DefaultCase = AnalyzeCodeBlock(p_enumerator, false);
						p_enumerator.CheckSymbol("}");
						return l_instruction;
				}
			}

			l_instruction.LastToken = p_enumerator.Current;

			return l_instruction;
		}

		private static SwitchCase AnalyzeCase(TokenEnumerator p_enumerator)
		{
			SwitchCase l_case = new SwitchCase();

			l_case.FirstToken = p_enumerator.Current;

			l_case.MainToken = p_enumerator.Current;

			do
			{
				l_case.Values.Add(AnalyzeExpression(p_enumerator));
			} while (!p_enumerator.LookAhead("{") && p_enumerator.CheckSymbol(","));

			l_case.Block = AnalyzeCodeBlock(p_enumerator, false);

			l_case.LastToken = p_enumerator.Current;

			return l_case;
		}

		private static Instruction AnalyzeReturnInstruction(TokenEnumerator p_enumerator)
		{
			return new ReturnInstruction()
			{
				FirstToken = p_enumerator.Current,
				MainToken = p_enumerator.Current,
				Operator = p_enumerator.Current.Data,
				Value = AnalyzeExpression(p_enumerator),
				LastToken = p_enumerator.Current
			};
		}

		private class OperatorList
		{
			public bool RightToLeft { get; set; }
			public string[] List { get; set; }
		}


		private static OperatorList[] s_binaryOperatorPrecedence = 
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

		private static Expression AnalyzeSubExpression(int p_precedence, TokenEnumerator p_enumerator)
		{
			List<Expression> l_subExpressions = new();
			List<AzurLexer.LexerToken> l_operators = new();
			List<AzurLexer.LexerToken> l_nextSubExpressionTokens = new();

			if (p_precedence >= s_binaryOperatorPrecedence.Length) // Leaf
			{
				if (p_enumerator.LookAhead("(", true))
				{
					Expression l_parenthesedExpression = AnalyzeExpression(p_enumerator);
					p_enumerator.CheckSymbol(")");
					return l_parenthesedExpression;
				}

				if (p_enumerator.LookAheadMany(new[] {"!", "~"}, true))
				{
					AzurLexer.LexerToken l_operator = p_enumerator.Current;
					return new UnaryOperationInstruction()
					{
						Operator = l_operator.Data,
						FirstToken = l_operator,
						MainToken = l_operator,
						Value = AnalyzeSubExpression(p_precedence, p_enumerator),
						LastToken = p_enumerator.Current
					};
				}

				p_enumerator.Next();

				Expression l_result = null;

				if (p_enumerator.Current is AzurLexer.NumberToken l_number)
				{
					l_result = new Uint64ConstExpression()
					{
						Value = Convert.ToUInt64(l_number.Data, l_number.Base),
						FirstToken = l_number,
						MainToken = l_number,
						LastToken = l_number
					};
				}
				else if (p_enumerator.Current is AzurLexer.StringToken l_string)
				{
					l_result = new BytesConstExpression()
					{
						Value = Encoding.UTF8.GetBytes(l_string.Data),
						FirstToken = l_string,
						MainToken = l_string,
						LastToken = l_string
					};
				}
				else if (p_enumerator.Current is AzurLexer.SymbolToken {Data: "["})
				{
					BytesConstExpression l_const = new BytesConstExpression();
					List<byte> l_bytes = new List<byte>();

					l_const.FirstToken = p_enumerator.Current;

					while (p_enumerator.Current.Data != "]")
					{
						var l_byte = p_enumerator.NextNumber();
						l_bytes.Add(Convert.ToByte(l_byte.Data, l_byte.Base));
						p_enumerator.CheckSymbols(",", "]");
					}

					l_const.Value = l_bytes.ToArray();

					l_const.LastToken = p_enumerator.Current;

					l_result = l_const;
				}
				else if (p_enumerator.Current is AzurLexer.IdentifierToken l_identifier)
				{
					
					Variable l_variable = new Reference()
					{
						Name = l_identifier.Data,
						FirstToken = l_identifier,
						MainToken = l_identifier,
						LastToken = l_identifier
					};

					while (p_enumerator.Next() != null)
					{
						if (p_enumerator.Current.Data == "(") // Call
						{
							CallInstruction l_callInstruction = new CallInstruction();
							l_callInstruction.FunctionName = l_variable;
							l_callInstruction.FirstToken = l_variable.FirstToken;
							if (!p_enumerator.LookAhead(")", true))
							{
								do
								{
									l_callInstruction.Parameters.Add(AnalyzeExpression(p_enumerator));
								} while (p_enumerator.CheckSymbols(",", ")") != ")");
							}

							l_callInstruction.LastToken = p_enumerator.Current;

							return l_callInstruction;
						}
						else if (p_enumerator.Current.Data == "[")
						{
							var l_arrayAccess = new ArrayAccessInstruction()
							{
								Array = l_variable,
								FirstToken = l_variable.FirstToken,
								Index = AnalyzeExpression(p_enumerator)
							};
							p_enumerator.CheckSymbol("]");

							l_arrayAccess.LastToken = p_enumerator.Current;
							l_variable = l_arrayAccess;
						}
						else if (p_enumerator.Current.Data == ".")
						{
							var l_memberAccess = new MemberAccessInstruction()
							{
								FirstToken = l_variable.FirstToken,
								MainToken = p_enumerator.Current,
								Member = new Reference()
								{
									Name = p_enumerator.NextIdentifier()
								},
								LastToken = p_enumerator.Current,
								Owner = l_variable
							};

							l_variable = l_memberAccess;
						}
						else
						{
							throw new SyntaxException(p_enumerator.Current);
						}
					}

					return l_variable;
				}
				
				if (p_enumerator.Next() != null || l_result == null)
					throw new SyntaxException(p_enumerator.Current);

				return l_result;
			}

			while (p_enumerator.Next() != null)
			{
				if (p_enumerator.LookMany(s_binaryOperatorPrecedence[p_precedence].List)) // Operator
				{
					if (l_nextSubExpressionTokens.Count == 0)
						throw new SyntaxException(p_enumerator.Current);
					l_subExpressions.Add(AnalyzeSubExpression(p_precedence + 1,
						new TokenEnumerator(l_nextSubExpressionTokens)));
					l_operators.Add(p_enumerator.Current);
					l_nextSubExpressionTokens = new();
				}
				else // Operand
				{
					if (p_enumerator.Current.Data == "(")
					{
						l_nextSubExpressionTokens.AddRange(p_enumerator.EnumerateUntilCorresponding(")"));
					}
					else
					{
						l_nextSubExpressionTokens.Add(p_enumerator.Current);
					}
				}
			}

			if (l_nextSubExpressionTokens.Count == 0)
				throw new SyntaxException(l_operators.Last());
			l_subExpressions.Add(AnalyzeSubExpression(p_precedence + 1,
				new TokenEnumerator(l_nextSubExpressionTokens)));

			Expression l_expression;
			if (s_binaryOperatorPrecedence[p_precedence].RightToLeft)
			{
				l_expression = l_subExpressions[^1];
				for (int i = l_subExpressions.Count - 2; i >= 0; i--)
				{
					l_expression = new BinaryOperationInstruction()
					{
						Operator = l_operators[i].Data,
						LeftValue = l_subExpressions[i],
						RightValue = l_expression,
						MainToken = l_operators[i],
						FirstToken = l_subExpressions[i].FirstToken,
						LastToken = l_expression.LastToken
					};
				}
			}
			else
			{
				l_expression = l_subExpressions[0];
				for (int i = 1; i < l_subExpressions.Count; i++)
				{
					l_expression = new BinaryOperationInstruction()
					{
						Operator = l_operators[i - 1].Data,
						LeftValue = l_expression,
						RightValue = l_subExpressions[i],
						MainToken = l_operators[i - 1],
						FirstToken = l_expression.FirstToken,
						LastToken = l_subExpressions[i].LastToken
					};
				}
			}

			return l_expression;
		}

		private static Expression AnalyzeExpression(TokenEnumerator p_enumerator)
		{
			List<AzurLexer.LexerToken> l_tokens = new();

			while (!p_enumerator.LookAheadMany(new [] {",", ";", ")", "]", "{", "}"}))
			{
				p_enumerator.Next();
				if (p_enumerator.Current.Data == "(")
					l_tokens.AddRange(p_enumerator.EnumerateUntilCorresponding(")"));
				else if (p_enumerator.Current.Data == "[")
					l_tokens.AddRange(p_enumerator.EnumerateUntilCorresponding("]"));
				else
				{
					l_tokens.Add(p_enumerator.Current);
				}
			}

			if (l_tokens.Count == 0)
				throw new SyntaxException(p_enumerator.Current);
			try
			{
				return AnalyzeSubExpression(0, new TokenEnumerator(l_tokens));
			}
			catch (SyntaxException e)
			{
				throw e;
			}
			catch
			{
				throw new SyntaxException(p_enumerator.Next());
			}
		}
	}
}