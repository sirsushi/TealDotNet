using System.Collections.Generic;
using System.Reflection.Metadata;

namespace TealDotNet.SemanticAnalyzer
{
	public class AzurField
	{
		public AzurField(AzurType p_type)
		{
			Type = p_type;
		}

		public bool ConstReference { get; set; }
		public bool Const { get; set; }
		public AzurType Type { get; set; }

		public static AzurField Constant(AzurType p_type)
		{
			return new(Types.Uint64)
			{
				Const = true,
				ConstReference = true
			};
		}
	}

	public class AzurType
	{
		public AzurType ToArray(AzurType p_index)
		{
			return new AzurStruct()
			{
				Fields =
				{
					{"len", AzurField.Constant(Types.Uint64)}
				},
				ArrayAccessor =
				{
					{
						p_index, new AzurField(this)
					}
				}
			};
		}
	}
	
	public class AzurStruct : AzurType
	{
		public AzurType Parent { get; set; }
		public Dictionary<string, AzurField> Fields { get; } = new();
		public Dictionary<AzurType, AzurField> ArrayAccessor { get; } = new();
	}

	public class AzurEnum : AzurType
	{
		public List<string> Values { get; } = new();
	}
	
	public static class Types
	{
		public static AzurType Any { get; } = new();
		public static AzurType Uint64 { get; } = new();
		public static AzurType Bytes { get; } = new();
		public static AzurType Uint64Array { get; } = Uint64.ToArray(Uint64);
		public static AzurType BytesArray { get; } = Bytes.ToArray(Uint64);

		public static AzurEnum Hash { get; } = new AzurEnum()
		{
			Values =
			{
				"SHA256",
				"Keccak256",
				"SHA512_256"
			}
		};
		
		public static AzurEnum OnComplete { get; } = new AzurEnum()
		{
			Values =
			{
				"NoOp",
				"OptIn",
				"CloseOut",
				"ClearState",
				"UpdateApplication",
				"DeleteApplication",
			}
		};

		public static AzurType StateSchema { get; } = new AzurStruct()
		{
			Fields =
			{
				{"NumUint", new AzurField(Uint64)},
				{"NumByteSlice", new AzurField(Uint64)},
			}
		};

		public static AzurType State { get; set; } = new AzurStruct()
		{
			Fields =
			{
				{"OptedIn", new AzurField(Uint64)}
			},
			ArrayAccessor =
			{
				{Bytes, new AzurField(Any)}
			}
		};
		
		public static AzurType AssetParam { get; } = new AzurStruct()
		{
			Fields =
			{
				{"Total", new AzurField(Uint64)},
				{"Decimals", new AzurField(Uint64)},
				{"DefaultFrozen", new AzurField(Uint64)},
				{"UnitName", new AzurField(Bytes)},
				{"Name", new AzurField(Bytes)},
				{"URL", new AzurField(Bytes)},
				{"MetadataHash", new AzurField(Bytes)},
				{"ManagerAddr", new AzurField(Bytes)},
				{"ReserveAddr", new AzurField(Bytes)},
				{"FreezeAddr", new AzurField(Bytes)},
				{"ClawbackAddr", new AzurField(Bytes)},
				{"Creator", new AzurField(Bytes)},
			}
		};

		public static AzurType ApplicationParam { get; } = new AzurStruct()
		{
			Fields =
			{
				{"ApprovalProgram", new AzurField(Bytes)},
				{"ClearStateProgram", new AzurField(Bytes)},
				{"GlobalStateSchema", new AzurField(StateSchema)},
				{"LocalStateSchema", new AzurField(StateSchema)},
				{"ExtraProgramPages", new AzurField(Uint64)},
			}
		};

		public static AzurType AssetHolding { get; } = new AzurStruct()
		{
			Fields =
			{
				{"AssetBalance", new AzurField(Uint64)},
				{"AssetFrozen", new AzurField(Uint64)}
			}
		};

		public static AzurType Asset { get; } = new AzurStruct()
		{
			Fields =
			{
				{"AssetHolding", new AzurField(AssetHolding)},
				{"AssetParam", new AzurField(AssetParam)}
			}
		};
		
		public static AzurType Application { get; } = new AzurStruct()
		{
			Fields =
			{
				{"Params", new AzurField(ApplicationParam)},
				{"Creator", new AzurField(Bytes)},
				{"Address", new AzurField(Bytes)},
				{"State", new AzurField(State)},
			}
		};
		
		public static AzurType Account { get; } = new AzurStruct()
		{
			Fields =
			{
				{"Balance", new AzurField(Uint64)},
				{"MinBalance", new AzurField(Uint64)},
				{"State", new AzurField(State.ToArray(Application))},
				{"Assets", new AzurField(AssetHolding.ToArray(Asset))}
			}
		};

		public static AzurType Global { get; } = new AzurStruct()
		{
			Fields =
			{
				{"MinTxnFee", new AzurField(Uint64)},
				{"MinBalance", new AzurField(Uint64)},
				{"MaxTxnLife", new AzurField(Uint64)},
				{"ZeroAddress", new AzurField(Bytes)},
				{"GroupSize", new AzurField(Uint64)},
				{"LogicSigVersion", new AzurField(Uint64)},
				{"Round", new AzurField(Uint64)},
				{"LatestTimestamp", new AzurField(Uint64)},
				{"CurrentApplication", new AzurField(Application)},
				{"GroupID", new AzurField(Bytes)},
			}
		};
		
		public static AzurType Transaction { get; } = new AzurStruct()
		{
			Fields =
			{
				// Transaction base
				{"Fee", new AzurField(Uint64)},
				{"FirstValid", new AzurField(Uint64)},
				{"FirstValidTime", new AzurField(Uint64)},
				{"LastValid", new AzurField(Uint64)},
				{"GroupIndex", new AzurField(Uint64)},
				{"TxID", new AzurField(Bytes)},
				{"RekeyTo", new AzurField(Bytes)},
				{"Note", new AzurField(Bytes)},
				{"Lease", new AzurField(Bytes)},
				{"Sender", new AzurField(Bytes)},
			}
		};

		public static AzurType PaymentTransaction { get; } = new AzurStruct()
		{
			Parent = Transaction,
			Fields =
			{
				// Payment
				{"Receiver", new AzurField(Bytes)},
				{"Amount", new AzurField(Uint64)},
				{"CloseRemainderTo", new AzurField(Bytes)},
			}
		};

		public static AzurType ParticipationTransaction { get; } = new AzurStruct()
		{
			Parent = Transaction,
			Fields =
			{
				// Participation key
				{"VotePK", new AzurField(Bytes)},
				{"SelectionPK", new AzurField(Bytes)},
				{"VoteFirst", new AzurField(Uint64)},
				{"VoteLast", new AzurField(Uint64)},
				{"VoteKeyDilution", new AzurField(Uint64)},
				{"Nonparticipation", new AzurField(Uint64)},
			}
		};

		public static AzurType AssetTransferTransaction { get; } = new AzurStruct()
		{
			Parent = Transaction,
			Fields =
			{
				// Asset Transfer
				{"AssetID", new AzurField(Uint64)},
				{"Amount", new AzurField(Uint64)},
				{"Receiver", new AzurField(Bytes)},
				{"CloseTo", new AzurField(Bytes)},
			}
		};

		public static AzurType ApplicationCallTransaction { get; } = new AzurStruct()
		{
			Parent = Transaction,
			Fields =
			{
				// Application Call
				{"ApplicationID", new AzurField(Uint64)},
				{"OnCompletion", new AzurField(OnComplete)},
				{"Args", new AzurField(BytesArray)},
				{"Accounts", new AzurField(Account)},
				{"Assets", new AzurField(Asset.ToArray(Uint64))},
				{"Applications", new AzurField(Application.ToArray(Uint64))},
				{"Scratch", new AzurField(Any.ToArray(Uint64))},
				{"Params", new AzurField(ApplicationParam)},
			}
		};

		public static AzurType AssetConfigurationTransaction { get; } = new AzurStruct()
		{
			Parent = Transaction,
			Fields =
			{
				// Asset creation/modification
				{"AssetID", new AzurField(Uint64)},
				{"Params", new AzurField(AssetParam)},
			}
		};

		public static AzurType AssetFreezeTransaction { get; } = new AzurStruct()
		{
			Parent = Transaction,
			Fields =
			{
				// Freezing asset
				{"AssetID", new AzurField(Uint64)},
				{"Account", new AzurField(Bytes)},
				{"Frozen", new AzurField(Uint64)},
			}
		};

		public static AzurType InnerTransactionResult { get; } = new AzurStruct()
		{
			Fields =
			{
				// Inner transaction
				{"Logs", new AzurField(BytesArray)},
			}
		};
	}
}