using System;
using System.Collections.Generic;
using System.Data;
using TealCompiler.AbstractSyntaxTree;
using TealDotNet.Lexer;

namespace TealDotNet.SyntaxAnalyzer
{
	public class SyntaxAnalyzer
	{
		public class SyntaxException : Exception
		{
			public SyntaxException(string p_expected, AzurLexer.LexerToken p_token)
			:base($"{p_expected} expected, but found {p_token}. ({p_token.Position})")
			{
			}
		}
		private class TokenEnumerator
		{
			private List<AzurLexer.LexerToken> m_tokens;
			private int m_index;
			private Stack<int> m_savedIndexes;

			public TokenEnumerator(List<AzurLexer.LexerToken> p_tokens)
			{
				m_savedIndexes = new Stack<int>();
				m_tokens = p_tokens;
				m_index = -1;
			}

			public void PushIndex()
			{
				m_savedIndexes.Push(m_index);
			}

			public void PopIndex()
			{
				m_index = m_savedIndexes.Pop();
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

			public string CheckSymbol(params string[] p_symbols)
			{
				if (Next() is AzurLexer.SymbolToken l_token)
				{
					for (int i = 0; i < p_symbols.Length; i++)
					{
						if (l_token.Data == p_symbols[i]) return p_symbols[i];
					}
				}

				throw new SyntaxException(string.Join(", ", p_symbols), Current);
			}

			public bool LookAheadSymbol(string p_symbol, bool p_goAheadWhenTrue = false)
			{
				if (m_tokens[m_index + 1] is AzurLexer.SymbolToken l_token && l_token.Data == p_symbol)
				{
					if (p_goAheadWhenTrue)
						m_index++;
					return true;
				}

				return false;
			}
		}

		public static Program Analyze(List<AzurLexer.LexerToken> p_tokens)
		{
			TokenEnumerator l_enumerator = new TokenEnumerator(p_tokens);
			
			Program l_program = new Program();

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
			
			l_function.Name = p_enumerator.NextIdentifier();
			
			p_enumerator.CheckSymbol("(");

			if (!p_enumerator.LookAheadSymbol(")", true))
			{
				do
				{
					l_function.Parameters.Add(p_enumerator.NextIdentifier());
				} while (p_enumerator.CheckSymbol(",", ")") != ")");
			}

			l_function.Block = AnalyzeCodeBlock(p_enumerator, false);

			return l_function;
		}

		private static CodeBlock AnalyzeCodeBlock(TokenEnumerator p_enumerator, bool p_oneLineSimplifiable)
		{
			CodeBlock l_block = new CodeBlock();

			if (p_oneLineSimplifiable && !p_enumerator.LookAheadSymbol("{"))
			{
				l_block.Instructions.Add(AnalyzeInstruction(p_enumerator));
			}
			else
			{
				p_enumerator.CheckSymbol("{");

				while (!p_enumerator.LookAheadSymbol("}", true))
				{
					l_block.Instructions.Add(AnalyzeInstruction(p_enumerator));
				}
			}

			return l_block;
		}

		private static Instruction AnalyzeInstruction(TokenEnumerator p_enumerator)
		{
			Instruction l_instruction;
			switch (p_enumerator.Next())
			{
				case AzurLexer.IdentifierToken {Data: Keywords.If}:
					break;
				case AzurLexer.IdentifierToken {Data: Keywords.For}:
					break;
				case AzurLexer.IdentifierToken {Data: Keywords.While}:
					break;
				case AzurLexer.IdentifierToken {Data: Keywords.Do}:
					break;
				case AzurLexer.IdentifierToken {Data: Keywords.Switch}:
					break;
				case AzurLexer.IdentifierToken {Data: Keywords.Return}:
					break;
				case AzurLexer.IdentifierToken {Data: Keywords.Throw}:
					break;
				case AzurLexer.IdentifierToken {Data: Keywords.Exit}:
					break;
				default:
					l_instruction = AnalyzeExpression(p_enumerator);
					break;
			}

			return l_instruction;
		}
	}
}