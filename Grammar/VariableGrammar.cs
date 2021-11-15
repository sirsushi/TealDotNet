using Sprache;
using TealCompiler.Tokens;

namespace TealCompiler
{
	public partial class Grammar
	{
		private Parser<Variable> Variable =>
			from variable in Identifier
				.Select(reference => new Reference()
				{
					Name = reference
				})
				.Then<Variable, Variable>((var) =>
					from index in Expression
						.Contained(Parse.Char('[').Token(), Parse.Char(']').Token())
						.Optional()
					select index.IsDefined
						? new ArrayAccessInstruction()
						{
							Array = var,
							Index = index.Get()
						}
						: var)
				.Then((var) =>
					from member in Parse.Char('.').Token()
						.Then(_ => Variable).Optional()
					select member.IsDefined
						? new MemberAccessInstruction()
						{
							Owner = var,
							Member = member.Get()
						}
						: var)
			select variable;
	}
}