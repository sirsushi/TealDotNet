﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using TealCompiler.TealGenerator.Assembly;

namespace TealCompiler
{
	namespace Tokens
	{
		public class Program
		{
			public Function[] Functions { get; set; }

			public IEnumerable<Opcode> ToTEAL()
			{
				yield break;
			}
		}

		public class Function
		{
			public string Name { get; set; }
			public string[] Parameters { get; set; }
			public CodeBlock Block { get; set; }

			public override string ToString()
			{
				return $"{Name}({string.Join(", ", Parameters)})";
			}
		}

		public class CodeBlock
		{
			public Instruction[] Instructions { get; set; }

			public override string ToString()
			{
				return $"{{{Instructions.Length}}}";
			}
		}

		public class Instruction { }

		public class IfInstruction : Instruction
		{
			public Expression Condition { get; set; }
			public CodeBlock IfBlock { get; set; }
			public CodeBlock ElseBlock { get; set; }

			public override string ToString()
			{
				return $"if {Condition} {IfBlock}" + (ElseBlock != null ? $"else {ElseBlock}" : "");
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
		}

		public class SwitchInstruction : Instruction
		{
			public Expression TestedValue { get; set; }
			public SwitchCase[] Cases { get; set; }
			public CodeBlock DefaultCase { get; set; }

			public override string ToString()
			{
				return $"switch {TestedValue} {{ {(Cases.Length > 0 ? $"{Cases.Length} case(s)" : "")} {(DefaultCase != null ? "+ default" : "")} }}";
			}
		}

		public class SwitchCase
		{
			public Expression[] Values { get; set; }
			public CodeBlock Block { get; set; }

			public override string ToString()
			{
				return $"case {Values.Select(v => v.ToString()).Aggregate((v1, v2) => $"{v1}, {v2}")} {Block}";
			}
		}

		public class Expression : Instruction { }

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
			
			public override string ToString()
			{
				return $"\"{Encoding.UTF8.GetString(Value)}\"";
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
		}

		public class LineExpression : Expression
		{
			
		}

		public class CallInstruction : LineExpression
		{
			public string FunctionName { get; set; }
			public Expression[] Parameters { get; set; }

			public override string ToString()
			{
				return $"{FunctionName}({string.Join(", ", Parameters.Select(param => param.ToString()))})";
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
		}
		
		public class MemberAccessInstruction : Variable
		{
			public Variable Owner { get; set; }
			public Variable Member { get; set; }

			public override string ToString()
			{
				return $"{Owner}.{Member}";
			}
		}
	}
}