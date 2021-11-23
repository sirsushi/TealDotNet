using TealCompiler.AbstractSyntaxTree;
using TealCompiler.TealGenerator.Assembly;
using TealDotNet.Syntax;

namespace TealCompiler.TealGenerator.Compilers
{
	public static class ReturnCompiler
	{
		public static void Compile(this ReturnInstruction l_return, CompiledProgramState p_state)
		{
			switch (l_return.Operator)
			{
				case Keywords.Return:
				{
					if (p_state.IsFlagSet(CompilerFlags.MainFunction))
					{
						if (l_return.Value != null)
							l_return.Value.Compile(p_state);
						else
							p_state.Write(Opcodes.pushint, 1);
						p_state.Write(Opcodes.@return);
					}
					else
					{
						if (l_return.Value != null)
							l_return.Value.Compile(p_state);
						p_state.Write(Opcodes.retsub);
					}

					break;
				}
				case Keywords.Throw:
				{
					p_state.Write(Opcodes.log, (l_return.Value as BytesConstExpression).GetUTF8String());
					p_state.Write(Opcodes.err);
					break;
				}
				case Keywords.Exit:
				{
					l_return.Value.Compile(p_state);
					p_state.Write(Opcodes.@return);
					break;
				}
			}
		}
	}
}