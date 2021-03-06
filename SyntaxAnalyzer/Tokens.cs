using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TealCompiler.TealGenerator.Assembly;
using TealDotNet.Lexer;

namespace TealCompiler
{
	namespace AbstractSyntaxTree
	{
		public class Node
		{
			public AzurLexer.LexerToken MainToken { get; set; }
			public AzurLexer.LexerToken FirstToken { get; set; }
			public AzurLexer.LexerToken LastToken { get; set; }

			public string GetSourceExtract()
			{
				return FirstToken.Origin.Substring(FirstToken.Position.Pos,
					LastToken.Position.Pos - FirstToken.Position.Pos + LastToken.Length);
			}

			public IEnumerable<T> Find<T>() where T : Node
			{
				if (this is T) yield return this as T;

				foreach (T l_child in FindChildren().SelectMany(c => c.Find<T>()))
				{
					yield return l_child;
				}
			}

			public virtual IEnumerable<Node> FindChildren()
			{
				yield break;
			}
		}
		public class Program : Node
		{
			public List<Expression> Constants { get; } = new();
			public List<Function> Functions { get; } = new();

			public override IEnumerable<Node> FindChildren()
			{
				return Constants.Concat<Node>(Functions);
			}
		}

		public class Function : Node
		{
			public string Name { get; set; }
			public List<string> Parameters { get; set; } = new();
			public CodeBlock Block { get; set; }

			public override string ToString()
			{
				return $"{Name}({string.Join(", ", Parameters)})";
			}

			public override IEnumerable<Node> FindChildren()
			{
				yield return Block;
			}
		}

		public class CodeBlock : Node
		{
			public List<Instruction> Instructions { get; set; } = new();

			public override string ToString()
			{
				return $"{{{Instructions.Count}}}";
			}

			public override IEnumerable<Node> FindChildren()
			{
				return Instructions;
			}
		}

		public class Instruction : Node
		{ }

		public class IfInstruction : Instruction
		{
			public Expression Condition { get; set; }
			public CodeBlock IfBlock { get; set; }
			public CodeBlock ElseBlock { get; set; }

			public override string ToString()
			{
				return $"if {Condition} {IfBlock}" + (ElseBlock != null ? $"else {ElseBlock}" : "");
			}

			public override IEnumerable<Node> FindChildren()
			{
				yield return Condition;
				yield return IfBlock;
				if (ElseBlock != null)
					yield return ElseBlock;
			}
		}

		public class WhileInstruction : Instruction
		{
			public Expression Condition { get; set; }
			public CodeBlock Block { get; set; }

			public override string ToString()
			{
				return $"while {Condition} {Block}";
			}

			public override IEnumerable<Node> FindChildren()
			{
				yield return Condition;
				yield return Block;
			}
		}
		
		public class DoWhileInstruction : Instruction
		{
			public Expression Condition { get; set; }
			public CodeBlock Block { get; set; }

			public override string ToString()
			{
				return $"do {Block} while {Condition}";
			}

			public override IEnumerable<Node> FindChildren()
			{
				yield return Condition;
				yield return Block;
			}
		}
		
		public class ForInstruction : Instruction
		{
			public Reference Variable { get; set; }
			public Expression Range { get; set; }
			public CodeBlock Block { get; set; }

			public override string ToString()
			{
				return $"for {Variable} in {Range} {Block}";
			}

			public override IEnumerable<Node> FindChildren()
			{
				yield return Variable;
				yield return Range;
				yield return Block;
			}
		}

		public class SwitchInstruction : Instruction
		{
			public Expression TestedValue { get; set; }
			public List<SwitchCase> Cases { get; set; } = new();
			public CodeBlock DefaultCase { get; set; }

			public override string ToString()
			{
				return $"switch {TestedValue} {{ {(Cases.Count > 0 ? $"{Cases.Count} case(s)" : "")} {(DefaultCase != null ? "+ default" : "")} }}";
			}

			public override IEnumerable<Node> FindChildren()
			{
				yield return TestedValue;
				foreach (SwitchCase l_case in Cases)
				{
					yield return l_case;
				}

				if (DefaultCase != null)
					yield return DefaultCase;
			}
		}

		public class SwitchCase : Node
		{
			public List<Expression> Values { get; set; } = new();
			public CodeBlock Block { get; set; }

			public override string ToString()
			{
				return $"case {Values.Select(v => v.ToString()).Aggregate((v1, v2) => $"{v1}, {v2}")} {Block}";
			}

			public override IEnumerable<Node> FindChildren()
			{
				foreach (Expression l_value in Values)
				{
					yield return l_value;
				}
				yield return Block;
			}
		}

		public class Expression : Instruction
		{
		}

		public class ConstExpression : Expression { }

		public class Uint64ConstExpression : ConstExpression
		{
			public ulong Value { get; set; }

			public override string ToString()
			{
				return Value.ToString();
			}
		}
		
		public class BytesConstExpression : ConstExpression
		{
			public byte[] Value { get; set; }

			public string GetUTF8String()
			{
				return Encoding.UTF8.GetString(Value);
			}

			public string GetBase64String()
			{
				byte[] l_base64 = new byte[Value.Length * 4 / 3];
				Span<byte> l_span = new Span<byte>(l_base64);
				Base64.EncodeToUtf8(new ReadOnlySpan<byte>(Value), l_span, out int l_consumed, out int l_written);
				return Encoding.UTF8.GetString(l_base64);
			}
			
			public override string ToString()
			{
				return $"\"{GetUTF8String()}\"";
			}
		}

		public class UnaryOperationInstruction : Expression
		{
			public string Operator { get; set; }
			public Expression Value { get; set; }
			public bool Suffix { get; set; }

			public override string ToString()
			{
				return Suffix ? $"{Value}{Operator}" : $"{Operator}{Value}";
			}

			public override IEnumerable<Node> FindChildren()
			{
				yield return Value;
			}
		}
		
		public class BinaryOperationInstruction : Expression
		{
			public string Operator { get; set; }
			public Expression LeftValue { get; set; }
			public Expression RightValue { get; set; }

			public override string ToString()
			{
				return $"({LeftValue} {Operator} {RightValue})";
			}

			public override IEnumerable<Node> FindChildren()
			{
				yield return LeftValue;
				yield return RightValue;
			}
		}

		public class CallInstruction : Variable
		{
			public Reference FunctionRef { get; set; }
			public List<Expression> Parameters { get; set; } = new();

			public override string ToString()
			{
				return $"{FunctionRef}({string.Join(", ", Parameters.Select(param => param.ToString()))})";
			}

			public override IEnumerable<Node> FindChildren()
			{
				yield return FunctionRef;
				foreach (Expression l_parameter in Parameters)
				{
					yield return l_parameter;
				}
			}
		}

		public class ReturnInstruction : Instruction
		{
			public string Operator { get; set; }
			public Expression Value { get; set; }

			public override string ToString()
			{
				return $"{Operator} {Value}";
			}

			public override IEnumerable<Node> FindChildren()
			{
				yield return Value;
			}
		}

		public class Variable : Expression
		{
			
		}

		public class Reference : Variable
		{
			public string Name { get; set; }

			public override string ToString()
			{
				return Name;
			}
		}

		public class ArrayAccessInstruction : Variable
		{
			public Variable Array { get; set; }
			public Expression Index { get; set; }

			public override string ToString()
			{
				return $"{Array}[{Index}]";
			}

			public override IEnumerable<Node> FindChildren()
			{
				yield return Array;
				yield return Index;
			}
		}
		
		public class MemberAccessInstruction : Variable
		{
			public Variable Owner { get; set; }
			public Reference Member { get; set; }

			public override string ToString()
			{
				return $"{Owner}.{Member}";
			}

			public override IEnumerable<Node> FindChildren()
			{
				yield return Owner;
				yield return Member;
			}
		}
	}
}