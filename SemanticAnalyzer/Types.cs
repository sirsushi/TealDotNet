using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;

namespace TealDotNet.Semantic
{
	public class AzurField
	{
		public AzurField(AzurType p_type)
		{
			Type = p_type;
		}

		public bool ConstReference { get; set; }
		public bool ConstMembers { get; set; }
		public AzurType Type { get; set; }

		public static AzurField Constant(AzurType p_type)
		{
			return new(p_type)
			{
				ConstMembers = true,
				ConstReference = true
			};
		}
	}

	public class AzurType
	{
		public AzurType ToArray(AzurType p_index)
		{
			return new AzurStruct(Name+"[]")
			{
				Fields =
				{
					{"len", AzurField.Constant(Types.Uint64)}
				},
				ArrayIndexType = p_index,
				ArrayValue = new AzurField(this)
			};
		}

		public virtual AzurField Get(string p_name)
		{
			return null;
		}

		public string Name { get; set; }

		public AzurType(string p_name)
		{
			Name = p_name;
		}
	}
	
	public class AzurStruct : AzurType
	{
		public AzurType Parent { get; set; }
		public Dictionary<string, AzurField> Fields { get; } = new();
		public AzurType ArrayIndexType { get; set; }
		public AzurField ArrayValue { get; set; }

		public override AzurField Get(string p_name)
		{
			if (Fields.TryGetValue(p_name, out AzurField l_field)) return l_field;
			if (p_name == "[]") return ArrayValue;
			if (Parent != null) return Parent.Get(p_name);
			return base.Get(p_name);
		}

		public AzurStruct(string p_name) : base(p_name)
		{
		}
	}

	public class AzurEnum : AzurType
	{
		public List<string> Values { get; } = new();

		public AzurEnum(string p_name) : base(p_name)
		{
		}
	}
	
	public static class Types
	{
		public static AzurType Any { get; } = new(nameof(Any));
		public static AzurType Uint64 { get; } = new(nameof(Uint64));
		public static AzurType Bytes { get; } = new(nameof(Bytes));
		public static AzurType Uint64Array { get; } = Uint64.ToArray(Uint64);
		public static AzurType BytesArray { get; } = Bytes.ToArray(Uint64);

		public static AzurEnum Hash { get; } = new AzurEnum(nameof(Hash))
		{
			Values =
			{
				"SHA256",
				"Keccak256",
				"SHA512_256"
			}
		};
		
		public static AzurEnum OnComplete { get; } = new AzurEnum(nameof(OnComplete))
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

		public static AzurType StateSchema { get; } = new AzurStruct(nameof(StateSchema))
		{
			Fields =
			{
				{"NumUint", new AzurField(Uint64)},
				{"NumByteSlice", new AzurField(Uint64)},
			}
		};

		public static AzurType State { get; set; } = new AzurStruct(nameof(State))
		{
			Fields =
			{
				{"OptedIn", new AzurField(Uint64)}
			},
			ArrayIndexType = Bytes,
			ArrayValue = new AzurField(Any)
		};
		
		public static AzurType AssetParam { get; } = new AzurStruct(nameof(AssetParam))
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

		public static AzurType ApplicationParam { get; } = new AzurStruct(nameof(ApplicationParam))
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

		public static AzurType AssetHolding { get; } = new AzurStruct(nameof(AssetHolding))
		{
			Fields =
			{
				{"AssetBalance", new AzurField(Uint64)},
				{"AssetFrozen", new AzurField(Uint64)}
			}
		};

		public static AzurType Asset { get; } = new AzurStruct(nameof(Asset))
		{
			Fields =
			{
				{"AssetHolding", new AzurField(AssetHolding)},
				{"AssetParam", new AzurField(AssetParam)}
			}
		};
		
		public static AzurType Application { get; } = new AzurStruct(nameof(Application))
		{
			Fields =
			{
				{"Params", new AzurField(ApplicationParam)},
				{"Creator", new AzurField(Bytes)},
				{"Address", new AzurField(Bytes)},
				{"State", new AzurField(State)},
			}
		};
		
		public static AzurType Account { get; } = new AzurStruct(nameof(Account))
		{
			Fields =
			{
				{"Balance", new AzurField(Uint64)},
				{"MinBalance", new AzurField(Uint64)},
				{"State", new AzurField(State.ToArray(Application))},
				{"Assets", new AzurField(AssetHolding.ToArray(Asset))}
			}
		};

		public static AzurType Global { get; } = new AzurStruct(nameof(Global))
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
		
		public static AzurType Transaction { get; } = new AzurStruct(nameof(Transaction))
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

		public static AzurType PaymentTransaction { get; } = new AzurStruct(nameof(PaymentTransaction))
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

		public static AzurType ParticipationTransaction { get; } = new AzurStruct(nameof(ParticipationTransaction))
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

		public static AzurType AssetTransferTransaction { get; } = new AzurStruct(nameof(AssetTransferTransaction))
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

		public static AzurType ApplicationCallTransaction { get; } = new AzurStruct(nameof(ApplicationCallTransaction))
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

		public static AzurType AssetConfigurationTransaction { get; } = new AzurStruct(nameof(AssetConfigurationTransaction))
		{
			Parent = Transaction,
			Fields =
			{
				// Asset creation/modification
				{"AssetID", new AzurField(Uint64)},
				{"Params", new AzurField(AssetParam)},
			}
		};

		public static AzurType AssetFreezeTransaction { get; } = new AzurStruct(nameof(AssetFreezeTransaction))
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

		public static AzurType InnerTransactionResult { get; } = new AzurStruct(nameof(InnerTransactionResult))
		{
			Fields =
			{
				// Inner transaction
				{"Logs", new AzurField(BytesArray)},
			}
		};
	}
}