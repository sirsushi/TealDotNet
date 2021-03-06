using System;
using System.Collections.Generic;
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

			public IEnumerable<TealInstruction> ToApprovalProgram()
			{
				Function l_approvalProgram = Functions.FirstOrDefault(f => f.Name == "ApprovalProgram");
				if (l_approvalProgram == null) throw new InvalidOperationException("ApprovalProgram() function is not present in the file.");

				foreach (var l_instruction in l_approvalProgram.ToTEAL(true)) yield return l_instruction;
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

			public IEnumerable<TealInstruction> ToTEAL(bool p_main = false)
			{
				yield return new LabelInstruction(Name);

				foreach (var l_instruction in Block.ToTEAL()) yield return l_instruction;

				if (p_main)
					yield return new OpcodeInstruction(Opcodes.@return);
				else
					yield return new OpcodeInstruction(Opcodes.retsub);
			}
		}

		public class CodeBlock
		{
			public Instruction[] Instructions { get; set; }

			public override string ToString()
			{
				return $"{{{Instructions.Length}}}";
			}

			public IEnumerable<TealInstruction> ToTEAL()
			{
				foreach (Instruction l_instruction in Instructions)
				{
					foreach (var l_tealInstruction in l_instruction.ToTEAL()) yield return l_tealInstruction;
				}
			}

			public IEnumerable<TInstruction> Find<TInstruction>() where TInstruction : Instruction
			{
				foreach (Instruction l_instruction in Instructions)
				{
					if (l_instruction is TInstruction)
						yield return (TInstruction)l_instruction;
					foreach (TInstruction l_subInstruction in l_instruction.Find<TInstruction>())
					{
						yield return l_subInstruction;
					}
				}
			}
		}

		public class Instruction
		{
			public IEnumerable<TealInstruction> ToTEAL()
			{
				yield return new CommentInstruction(ToString());
				foreach (var l_instruction in ToTEALInternal()) yield return l_instruction;
			}

			protected virtual IEnumerable<TealInstruction> ToTEALInternal()
			{
				yield break;
			}

			public virtual IEnumerable<TInstruction> Find<TInstruction>()
			{
				yield break;
			}
		}

		public class IfInstruction : Instruction
		{
			public Expression Condition { get; set; }
			public CodeBlock IfBlock { get; set; }
			public CodeBlock ElseBlock { get; set; }

			public override string ToString()
			{
				return $"if {Condition} {IfBlock}" + (ElseBlock != null ? $"else {ElseBlock}" : "");
			}

			protected override IEnumerable<TealInstruction> ToTEALInternal()
			{
				foreach (var l_instruction in Condition.ToTEAL()) yield return l_instruction;

				Guid l_id = Guid.NewGuid();
				if (ElseBlock != null)
				{
					// if NOT condition => jump else
					yield return new OpcodeInstruction(Opcodes.bz, l_id.ToString() + "_else");

					foreach (var l_instruction in IfBlock.ToTEAL()) yield return l_instruction;

					// jump endif
					yield return new OpcodeInstruction(Opcodes.b, l_id.ToString() + "_endif");
					yield return new LabelInstruction(l_id.ToString() + "_else");

					foreach (var l_instruction in ElseBlock.ToTEAL()) yield return l_instruction;
					
					yield return new LabelInstruction(l_id.ToString() + "_endif");
				}
				else
				{
					// if NOT condition => jump endif
					yield return new OpcodeInstruction(Opcodes.bz, l_id.ToString() + "_endif");

					foreach (var l_instruction in IfBlock.ToTEAL()) yield return l_instruction;

					yield return new LabelInstruction(l_id.ToString() + "_endif");
				}
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

			protected override IEnumerable<TealInstruction> ToTEALInternal()
			{
				if (Cases.Length == 0 && DefaultCase == null)
				{
					yield break;
				}

				CodeBlock l_block = DefaultCase;

				for (int i = Cases.Length - 1; i >= 0; i--)
				{
					Expression l_condition = new BinaryOperationInstruction()
					{
						Operator = "==",
						LeftValue = TestedValue,
						RightValue = Cases[i].Values[0]
					};
					for (int j = 1; j < Cases[i].Values.Length; j++)
					{
						l_condition = new BinaryOperationInstruction()
						{
							Operator = "||",
							LeftValue = new BinaryOperationInstruction()
							{
								Operator = "==",
								LeftValue = TestedValue,
								RightValue = Cases[i].Values[j]
							},
							RightValue = l_condition
						};
					}
					l_block = new CodeBlock()
					{
						Instructions = new Instruction[]
						{
							new IfInstruction()
							{
								Condition = l_condition,
								IfBlock = Cases[i].Block,
								ElseBlock = l_block
							}
						}
					};
				}

				foreach (var l_instruction in l_block.ToTEAL()) yield return l_instruction;
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

		public class Expression : Instruction
		{
			public virtual StackType EvaluateTo()
			{
				return StackType.Any;
			}
			
			public bool EvaluateToUint64()
			{
				return EvaluateTo() == StackType.Uint64;
			}

			public bool EvaluateToBytes()
			{
				return EvaluateTo() == StackType.Bytes;
			}
		}

		public class ConstExpression : Expression { }

		public class Uint64ConstExpression : ConstExpression
		{
			public ulong Value { get; set; }

			public override string ToString()
			{
				return Value.ToString();
			}

			public override StackType EvaluateTo()
			{
				return StackType.Uint64;
			}

			protected override IEnumerable<TealInstruction> ToTEALInternal()
			{
				yield return new OpcodeInstruction(Opcodes.pushint, Value);
			}
		}
		
		public class BytesConstExpression : ConstExpression
		{
			public byte[] Value { get; set; }
			
			public override string ToString()
			{
				return $"\"{Encoding.UTF8.GetString(Value)}\"";
			}

			public override StackType EvaluateTo()
			{
				return StackType.Bytes;
			}

			protected override IEnumerable<TealInstruction> ToTEALInternal()
			{
				yield return new OpcodeInstruction(Opcodes.pushbytes, ToString());
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

			public override StackType EvaluateTo()
			{
				return Value.EvaluateTo();
			}

			protected override IEnumerable<TealInstruction> ToTEALInternal()
			{
				foreach (var l_instruction in Value.ToTEAL()) yield return l_instruction;

				switch (Operator)
				{
					case "!":
						if (Value.EvaluateToUint64())
							yield return new OpcodeInstruction(Opcodes.not);
						else
							throw new InvalidOperationException("Can't do 'not' operation on []byte.");
						break;
					case "~":
						if (Value.EvaluateToUint64())
							yield return new OpcodeInstruction(Opcodes.complement);
						else
							yield return new OpcodeInstruction(Opcodes.bcomplement);
						break;
				}
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

			public override StackType EvaluateTo()
			{
				return LeftValue.EvaluateToBytes() || RightValue.EvaluateToBytes() ? StackType.Bytes : StackType.Uint64;
			}

			protected override IEnumerable<TealInstruction> ToTEALInternal()
			{
				switch (Operator)
				{
					case "+":
						if (LeftValue.EvaluateToUint64() && RightValue.EvaluateToUint64())
						{
							foreach (var l_instruction in LeftValue.ToTEAL()) yield return l_instruction;
							foreach (var l_instruction in RightValue.ToTEAL()) yield return l_instruction;
							yield return new OpcodeInstruction(Opcodes.add);
						}
						else
						{
							foreach (var l_instruction in LeftValue.ToTEAL()) yield return l_instruction;
							if (LeftValue.EvaluateToUint64())
								yield return new OpcodeInstruction(Opcodes.itob);
							foreach (var l_instruction in RightValue.ToTEAL()) yield return l_instruction;
							if (RightValue.EvaluateToUint64())
								yield return new OpcodeInstruction(Opcodes.itob);
							yield return new OpcodeInstruction(Opcodes.badd);
						}
						break;
					case "-":
						if (LeftValue.EvaluateToUint64() && RightValue.EvaluateToUint64())
						{
							foreach (var l_instruction in LeftValue.ToTEAL()) yield return l_instruction;
							foreach (var l_instruction in RightValue.ToTEAL()) yield return l_instruction;
							yield return new OpcodeInstruction(Opcodes.sub);
						}
						else
						{
							foreach (var l_instruction in LeftValue.ToTEAL()) yield return l_instruction;
							if (LeftValue.EvaluateToUint64())
								yield return new OpcodeInstruction(Opcodes.itob);
							foreach (var l_instruction in RightValue.ToTEAL()) yield return l_instruction;
							if (RightValue.EvaluateToUint64())
								yield return new OpcodeInstruction(Opcodes.itob);
							yield return new OpcodeInstruction(Opcodes.bsub);
						}
						break;
					case "*":
						if (LeftValue.EvaluateToUint64() && RightValue.EvaluateToUint64())
						{
							foreach (var l_instruction in LeftValue.ToTEAL()) yield return l_instruction;
							foreach (var l_instruction in RightValue.ToTEAL()) yield return l_instruction;
							yield return new OpcodeInstruction(Opcodes.mul);
						}
						else
						{
							foreach (var l_instruction in LeftValue.ToTEAL()) yield return l_instruction;
							if (LeftValue.EvaluateToUint64())
								yield return new OpcodeInstruction(Opcodes.itob);
							foreach (var l_instruction in RightValue.ToTEAL()) yield return l_instruction;
							if (RightValue.EvaluateToUint64())
								yield return new OpcodeInstruction(Opcodes.itob);
							yield return new OpcodeInstruction(Opcodes.bmul);
						}
						break;
					case "/":
						if (LeftValue.EvaluateToUint64() && RightValue.EvaluateToUint64())
						{
							foreach (var l_instruction in LeftValue.ToTEAL()) yield return l_instruction;
							foreach (var l_instruction in RightValue.ToTEAL()) yield return l_instruction;
							yield return new OpcodeInstruction(Opcodes.div);
						}
						else
						{
							foreach (var l_instruction in LeftValue.ToTEAL()) yield return l_instruction;
							if (LeftValue.EvaluateToUint64())
								yield return new OpcodeInstruction(Opcodes.itob);
							foreach (var l_instruction in RightValue.ToTEAL()) yield return l_instruction;
							if (RightValue.EvaluateToUint64())
								yield return new OpcodeInstruction(Opcodes.itob);
							yield return new OpcodeInstruction(Opcodes.bdiv);
						}
						break;
					case "%":
						if (LeftValue.EvaluateToUint64() && RightValue.EvaluateToUint64())
						{
							foreach (var l_instruction in LeftValue.ToTEAL()) yield return l_instruction;
							foreach (var l_instruction in RightValue.ToTEAL()) yield return l_instruction;
							yield return new OpcodeInstruction(Opcodes.mod);
						}
						else
						{
							foreach (var l_instruction in LeftValue.ToTEAL()) yield return l_instruction;
							if (LeftValue.EvaluateToUint64())
								yield return new OpcodeInstruction(Opcodes.itob);
							foreach (var l_instruction in RightValue.ToTEAL()) yield return l_instruction;
							if (RightValue.EvaluateToUint64())
								yield return new OpcodeInstruction(Opcodes.itob);
							yield return new OpcodeInstruction(Opcodes.bmod);
						}
						break;
					case "**":
						if (LeftValue.EvaluateToUint64() && RightValue.EvaluateToUint64())
						{
							foreach (var l_instruction in LeftValue.ToTEAL()) yield return l_instruction;
							foreach (var l_instruction in RightValue.ToTEAL()) yield return l_instruction;
							yield return new OpcodeInstruction(Opcodes.exp);
						}
						else
							throw new InvalidOperationException("Type []byte is not compatible with operation '**'");
						break;
					case "=":
						if (LeftValue is not Variable l_variable) throw new InvalidOperationException("Trying to assign a value to something that is not a variable.");
						foreach (var l_instruction in RightValue.ToTEAL()) yield return l_instruction;
						foreach (var l_instruction in l_variable.ToAssignTEAL()) yield return l_instruction;
						break;
					case "==":
						foreach (var l_instruction in LeftValue.ToTEAL()) yield return l_instruction;
						foreach (var l_instruction in RightValue.ToTEAL()) yield return l_instruction;
						yield return new OpcodeInstruction(Opcodes.equal);
						break;
					case "!=":
						foreach (var l_instruction in LeftValue.ToTEAL()) yield return l_instruction;
						foreach (var l_instruction in RightValue.ToTEAL()) yield return l_instruction;
						yield return new OpcodeInstruction(Opcodes.notequal);
						break;
					case "&&":
						foreach (var l_instruction in LeftValue.ToTEAL()) yield return l_instruction;
						foreach (var l_instruction in RightValue.ToTEAL()) yield return l_instruction;
						yield return new OpcodeInstruction(Opcodes.conditionnal_and);
						break;
					case "||":
						foreach (var l_instruction in LeftValue.ToTEAL()) yield return l_instruction;
						foreach (var l_instruction in RightValue.ToTEAL()) yield return l_instruction;
						yield return new OpcodeInstruction(Opcodes.conditionnal_or);
						break;
					case "&":
						if (LeftValue.EvaluateToUint64() && RightValue.EvaluateToUint64())
						{
							foreach (var l_instruction in LeftValue.ToTEAL()) yield return l_instruction;
							foreach (var l_instruction in RightValue.ToTEAL()) yield return l_instruction;
							yield return new OpcodeInstruction(Opcodes.bitwise_and);
						}
						else
						{
							foreach (var l_instruction in LeftValue.ToTEAL()) yield return l_instruction;
							if (LeftValue.EvaluateToUint64())
								yield return new OpcodeInstruction(Opcodes.itob);
							foreach (var l_instruction in RightValue.ToTEAL()) yield return l_instruction;
							if (RightValue.EvaluateToUint64())
								yield return new OpcodeInstruction(Opcodes.itob);
							yield return new OpcodeInstruction(Opcodes.bbitwise_and);
						}
						break;
					case "|":
						if (LeftValue.EvaluateToUint64() && RightValue.EvaluateToUint64())
						{
							foreach (var l_instruction in LeftValue.ToTEAL()) yield return l_instruction;
							foreach (var l_instruction in RightValue.ToTEAL()) yield return l_instruction;
							yield return new OpcodeInstruction(Opcodes.bitwise_or);
						}
						else
						{
							foreach (var l_instruction in LeftValue.ToTEAL()) yield return l_instruction;
							if (LeftValue.EvaluateToUint64())
								yield return new OpcodeInstruction(Opcodes.itob);
							foreach (var l_instruction in RightValue.ToTEAL()) yield return l_instruction;
							if (RightValue.EvaluateToUint64())
								yield return new OpcodeInstruction(Opcodes.itob);
							yield return new OpcodeInstruction(Opcodes.bbitwise_or);
						}
						break;
					case "^":
						if (LeftValue.EvaluateToUint64() && RightValue.EvaluateToUint64())
						{
							foreach (var l_instruction in LeftValue.ToTEAL()) yield return l_instruction;
							foreach (var l_instruction in RightValue.ToTEAL()) yield return l_instruction;
							yield return new OpcodeInstruction(Opcodes.bitwise_xor);
						}
						else
						{
							foreach (var l_instruction in LeftValue.ToTEAL()) yield return l_instruction;
							if (LeftValue.EvaluateToUint64())
								yield return new OpcodeInstruction(Opcodes.itob);
							foreach (var l_instruction in RightValue.ToTEAL()) yield return l_instruction;
							if (RightValue.EvaluateToUint64())
								yield return new OpcodeInstruction(Opcodes.itob);
							yield return new OpcodeInstruction(Opcodes.bbitwise_xor);
						}
						break;
					case "<":
						if (LeftValue.EvaluateToUint64() && RightValue.EvaluateToUint64())
						{
							foreach (var l_instruction in LeftValue.ToTEAL()) yield return l_instruction;
							foreach (var l_instruction in RightValue.ToTEAL()) yield return l_instruction;
							yield return new OpcodeInstruction(Opcodes.lesser_than);
						}
						else
						{
							foreach (var l_instruction in LeftValue.ToTEAL()) yield return l_instruction;
							if (LeftValue.EvaluateToUint64())
								yield return new OpcodeInstruction(Opcodes.itob);
							foreach (var l_instruction in RightValue.ToTEAL()) yield return l_instruction;
							if (RightValue.EvaluateToUint64())
								yield return new OpcodeInstruction(Opcodes.itob);
							yield return new OpcodeInstruction(Opcodes.blesser_than);
						}
						break;
					case ">":
						if (LeftValue.EvaluateToUint64() && RightValue.EvaluateToUint64())
						{
							foreach (var l_instruction in LeftValue.ToTEAL()) yield return l_instruction;
							foreach (var l_instruction in RightValue.ToTEAL()) yield return l_instruction;
							yield return new OpcodeInstruction(Opcodes.greater_than);
						}
						else
						{
							foreach (var l_instruction in LeftValue.ToTEAL()) yield return l_instruction;
							if (LeftValue.EvaluateToUint64())
								yield return new OpcodeInstruction(Opcodes.itob);
							foreach (var l_instruction in RightValue.ToTEAL()) yield return l_instruction;
							if (RightValue.EvaluateToUint64())
								yield return new OpcodeInstruction(Opcodes.itob);
							yield return new OpcodeInstruction(Opcodes.bgreater_than);
						}
						break;
					case "<=":
						if (LeftValue.EvaluateToUint64() && RightValue.EvaluateToUint64())
						{
							foreach (var l_instruction in LeftValue.ToTEAL()) yield return l_instruction;
							foreach (var l_instruction in RightValue.ToTEAL()) yield return l_instruction;
							yield return new OpcodeInstruction(Opcodes.lesser_than_or_equal);
						}
						else
						{
							foreach (var l_instruction in LeftValue.ToTEAL()) yield return l_instruction;
							if (LeftValue.EvaluateToUint64())
								yield return new OpcodeInstruction(Opcodes.itob);
							foreach (var l_instruction in RightValue.ToTEAL()) yield return l_instruction;
							if (RightValue.EvaluateToUint64())
								yield return new OpcodeInstruction(Opcodes.itob);
							yield return new OpcodeInstruction(Opcodes.blesser_than_or_equal);
						}
						break;
					case ">=":
						if (LeftValue.EvaluateToUint64() && RightValue.EvaluateToUint64())
						{
							foreach (var l_instruction in LeftValue.ToTEAL()) yield return l_instruction;
							foreach (var l_instruction in RightValue.ToTEAL()) yield return l_instruction;
							yield return new OpcodeInstruction(Opcodes.greater_than_or_equal);
						}
						else
						{
							foreach (var l_instruction in LeftValue.ToTEAL()) yield return l_instruction;
							if (LeftValue.EvaluateToUint64())
								yield return new OpcodeInstruction(Opcodes.itob);
							foreach (var l_instruction in RightValue.ToTEAL()) yield return l_instruction;
							if (RightValue.EvaluateToUint64())
								yield return new OpcodeInstruction(Opcodes.itob);
							yield return new OpcodeInstruction(Opcodes.bgreater_than_or_equal);
						}
						break;
					case "<<":
						if (LeftValue.EvaluateToUint64() && RightValue.EvaluateToUint64())
						{
							foreach (var l_instruction in LeftValue.ToTEAL()) yield return l_instruction;
							foreach (var l_instruction in RightValue.ToTEAL()) yield return l_instruction;
							yield return new OpcodeInstruction(Opcodes.shl);
						}
						else
							throw new InvalidOperationException("Type []byte is not compatible with operation '<<'");

						break;
					case ">>":
						if (LeftValue.EvaluateToUint64() && RightValue.EvaluateToUint64())
						{
							foreach (var l_instruction in LeftValue.ToTEAL()) yield return l_instruction;
							foreach (var l_instruction in RightValue.ToTEAL()) yield return l_instruction;
							yield return new OpcodeInstruction(Opcodes.shr);
						}
						else
							throw new InvalidOperationException("Type []byte is not compatible with operation '>>'");
						break;
				}
			}
		}

		public class CallInstruction : Expression
		{
			public string FunctionName { get; set; }
			public Expression[] Parameters { get; set; }

			public override string ToString()
			{
				return $"{FunctionName}({string.Join(", ", Parameters.Select(param => param.ToString()))})";
			}

			public override StackType EvaluateTo()
			{
				return base.EvaluateTo();
			}

			protected override IEnumerable<TealInstruction> ToTEALInternal()
			{
				foreach (Expression l_parameter in Parameters)
				{
					foreach (var l_instruction in l_parameter.ToTEAL()) yield return l_instruction;
				}
				yield return new OpcodeInstruction(Opcodes.callsub, FunctionName);
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

			protected override IEnumerable<TealInstruction> ToTEALInternal()
			{
				if (Value != null)
				{
					foreach (var l_instruction in Value.ToTEAL())
						yield return l_instruction;
				}

				yield return new OpcodeInstruction(Opcodes.retsub);
			}
		}

		public class Variable : Expression
		{
			public virtual IEnumerable<TealInstruction> ToAssignTEAL()
			{
				throw new InvalidOperationException("Readonly variable");
			}
		}

		public class Reference : Variable
		{
			public string Name { get; set; }

			public override string ToString()
			{
				return Name;
			}

			protected override IEnumerable<TealInstruction> ToTEALInternal()
			{
				return base.ToTEALInternal();
			}

			public override IEnumerable<TealInstruction> ToAssignTEAL()
			{
				return base.ToAssignTEAL();
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