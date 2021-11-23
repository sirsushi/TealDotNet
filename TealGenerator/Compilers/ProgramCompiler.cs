using System;
using System.Collections.Generic;
using System.Linq;
using TealCompiler.AbstractSyntaxTree;
using TealCompiler.TealGenerator.Assembly;

namespace TealCompiler.TealGenerator.Compilers
{
	public static class ProgramCompiler
	{
		public static void Compile(this Program p_program, CompiledProgramState p_state)
		{
			Function l_mainFunction = null;
			if (p_state.IsFlagSet(CompilerFlags.ApprovalProgram))
				l_mainFunction = p_program.Functions.First(f => f.Name == "ApprovalProgram");
			else if (p_state.IsFlagSet(CompilerFlags.ClearStateProgram))
				l_mainFunction = p_program.Functions.First(f => f.Name == "ClearStateProgram");
			else if (p_state.IsFlagSet(CompilerFlags.Signature))
				l_mainFunction = p_program.Functions.First(f => f.Name == "Signature");

			if (l_mainFunction == null) return;
			
			List<Function> l_usedFunctions = new();
			Queue<Function> l_functionsToTest = new();
			l_functionsToTest.Enqueue(l_mainFunction);
			while (l_functionsToTest.Count > 0)
			{
				Function l_nextFunctionToTest = l_functionsToTest.Dequeue();
				l_usedFunctions.Add(l_nextFunctionToTest);
				List<CallInstruction> l_calls = l_nextFunctionToTest.Find<CallInstruction>().ToList();
				foreach (CallInstruction l_call in l_calls)
				{
					Function l_function = p_program.Functions.First(f => f.Name == l_call.FunctionRef.Name);
					if (!l_usedFunctions.Contains(l_function))
					{
						l_functionsToTest.Enqueue(l_function);
					}
				}
			}

			foreach (Function l_function in l_usedFunctions)
			{
				if (l_function == l_mainFunction)
					p_state.SetFlag(CompilerFlags.MainFunction);
				else
					p_state.ResetFlag(CompilerFlags.MainFunction);
				l_function.Compile(p_state);
			}
		}
	}
}