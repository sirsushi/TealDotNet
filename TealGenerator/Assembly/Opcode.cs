namespace TealCompiler.TealGenerator.Assembly
{
	public enum ParamType
	{
		Uint,
		Bytes,
		Label,
		UintArray,
		StackBytesArray
	}
	public enum StackType
	{
		Uint64,
		Bytes,
		Addr,
		Any
	}
	
	public enum Mode
	{
		Application,
		Signature,
		Any
	}

	public class Opcode
	{
		public int Code { get; set; } = -1;
		public string Name { get; set; } = "NoOp";
		public int[] Cost { get; set; } = new[] {1, 1, 1, 1, 1};
		public StackType[] Pops { get; set; } = new StackType[0];
		public StackType[] Pushes { get; set; } = new StackType[0];
		public ParamType[] Params { get; set; } = new ParamType[0];
		public Mode Mode { get; set; } = Mode.Any;

		public Opcode(int code, string name)
		{
			Code = code;
			Name = name;
		}
	}

	public static class Opcodes
	{
		public static Opcode err = new(0, nameof(err));
		public static Opcode sha256 = new(1, nameof(sha256))
		{
			Cost = new []{7, 35, 35, 35, 35},
			Pops = new [] {StackType.Bytes},
			Pushes = new [] {StackType.Bytes}
		};
		public static Opcode keccak256 = new(2, nameof(keccak256))
		{
			Cost = new []{26, 130, 130, 130, 130},
			Pops = new [] {StackType.Bytes},
			Pushes = new [] {StackType.Bytes}
		};
		public static Opcode sha512_256 = new(3, nameof(sha512_256))
		{
			Cost = new []{9, 45, 45, 45, 45},
			Pops = new [] {StackType.Bytes},
			Pushes = new [] {StackType.Bytes}
		};
		public static Opcode ed25519verify = new(4, nameof(ed25519verify))
		{
			Cost = new []{1900, 1900, 1900, 1900, 1900},
			Pops = new [] {StackType.Bytes, StackType.Bytes, StackType.Bytes},
			Pushes = new [] {StackType.Uint64}
		};
		
		public static Opcode ecdsa_verify = new(5, nameof(ecdsa_verify))
		{
			Cost = new []{-1, -1, -1, -1, 1700},
			Params = new [] {ParamType.Uint},
			Pops = new [] {StackType.Bytes, StackType.Bytes, StackType.Bytes, StackType.Bytes, StackType.Bytes},
			Pushes = new [] {StackType.Uint64}
		};
		
		public static Opcode ecdsa_pk_decompress = new(6, nameof(ecdsa_pk_decompress))
		{
			Cost = new []{-1, -1, -1, -1, 650},
			Params = new [] {ParamType.Uint},
			Pops = new [] {StackType.Bytes},
			Pushes = new [] {StackType.Bytes, StackType.Bytes}
		};

		public static Opcode ecdsa_pk_recover = new(7, nameof(ecdsa_pk_recover))
		{
			Cost = new[] {-1, -1, -1, -1, 2000},
			Params = new[] {ParamType.Uint},
			Pops = new[] {StackType.Bytes, StackType.Bytes, StackType.Bytes, StackType.Bytes},
			Pushes = new[] {StackType.Bytes, StackType.Bytes}
		};
		
		public static Opcode add = new(8, "+")
		{
			Pops = new[] {StackType.Uint64, StackType.Uint64},
			Pushes = new[] {StackType.Uint64}
		};
		
		public static Opcode sub = new(9, "-")
		{
			Pops = new[] {StackType.Uint64, StackType.Uint64},
			Pushes = new[] {StackType.Uint64}
		};
		
		public static Opcode div = new(0x0a, "/")
		{
			Pops = new[] {StackType.Uint64, StackType.Uint64},
			Pushes = new[] {StackType.Uint64}
		};
		
		public static Opcode mul = new(0x0b, "*")
		{
			Pops = new[] {StackType.Uint64, StackType.Uint64},
			Pushes = new[] {StackType.Uint64}
		};
		
		public static Opcode lesser_than = new(0x0c, "<")
		{
			Pops = new[] {StackType.Uint64, StackType.Uint64},
			Pushes = new[] {StackType.Uint64}
		};
		
		public static Opcode greater_than = new(0x0d, ">")
		{
			Pops = new[] {StackType.Uint64, StackType.Uint64},
			Pushes = new[] {StackType.Uint64}
		};
		
		public static Opcode lesser_than_or_equal = new(0x0e, "<=")
		{
			Pops = new[] {StackType.Uint64, StackType.Uint64},
			Pushes = new[] {StackType.Uint64}
		};
		
		public static Opcode greater_than_or_equal = new(0x0f, ">=")
		{
			Pops = new[] {StackType.Uint64, StackType.Uint64},
			Pushes = new[] {StackType.Uint64}
		};
		
		public static Opcode and = new(0x10, "&&")
		{
			Pops = new[] {StackType.Uint64, StackType.Uint64},
			Pushes = new[] {StackType.Uint64}
		};
		
		public static Opcode or = new(0x11, "||")
		{
			Pops = new[] {StackType.Uint64, StackType.Uint64},
			Pushes = new[] {StackType.Uint64}
		};
		
		public static Opcode equal = new(0x12, "==")
		{
			Pops = new[] {StackType.Any, StackType.Any},
			Pushes = new[] {StackType.Uint64}
		};
		
		public static Opcode notequal = new(0x13, "!=")
		{
			Pops = new[] {StackType.Any, StackType.Any},
			Pushes = new[] {StackType.Uint64}
		};
		
		public static Opcode not = new(0x14, "!")
		{
			Pops = new[] {StackType.Uint64},
			Pushes = new[] {StackType.Uint64}
		};
		
		public static Opcode len = new(0x15, nameof(len))
		{
			Pops = new[] {StackType.Bytes},
			Pushes = new[] {StackType.Uint64}
		};
		
		public static Opcode itob = new(0x16, nameof(itob))
		{
			Pops = new[] {StackType.Uint64},
			Pushes = new[] {StackType.Bytes}
		};
		
		public static Opcode btoi = new(0x17, nameof(btoi))
		{
			Pops = new[] {StackType.Bytes},
			Pushes = new[] {StackType.Uint64}
		};
		
		public static Opcode mod = new(0x18, "%")
		{
			Pops = new[] {StackType.Uint64, StackType.Uint64},
			Pushes = new[] {StackType.Uint64}
		};
		
		public static Opcode bitwise_or = new(0x19, "|")
		{
			Pops = new[] {StackType.Uint64, StackType.Uint64},
			Pushes = new[] {StackType.Uint64}
		};
		
		public static Opcode bitwise_and = new(0x1a, "&")
		{
			Pops = new[] {StackType.Uint64, StackType.Uint64},
			Pushes = new[] {StackType.Uint64}
		};
		
		public static Opcode bitwise_xor = new(0x1b, "^")
		{
			Pops = new[] {StackType.Uint64, StackType.Uint64},
			Pushes = new[] {StackType.Uint64}
		};
		
		public static Opcode complement = new(0x1c, "~")
		{
			Pops = new[] {StackType.Uint64},
			Pushes = new[] {StackType.Uint64}
		};

		public static Opcode mulw = new(0x1d, nameof(mulw))
		{
			Pops = new[] {StackType.Uint64, StackType.Uint64},
			Pushes = new[] {StackType.Uint64, StackType.Uint64}
		};

		public static Opcode addw = new(0x1e, nameof(addw))
		{
			Cost = new []{-1, 1, 1, 1, 1},
			Pops = new[] {StackType.Uint64, StackType.Uint64},
			Pushes = new[] {StackType.Uint64, StackType.Uint64}
		};

		public static Opcode divmodw = new(0x1f, nameof(divmodw))
		{
			Cost = new []{-1, -1, -1, 20, 20},
			Pops = new[] {StackType.Uint64, StackType.Uint64, StackType.Uint64, StackType.Uint64},
			Pushes = new[] {StackType.Uint64, StackType.Uint64, StackType.Uint64, StackType.Uint64}
		};
		
		public static Opcode intcblock = new(0x20, nameof(intcblock))
		{
			Params = new [] {ParamType.UintArray}
		};
		
		public static Opcode intc = new(0x21, nameof(intc))
		{
			Params = new [] {ParamType.Uint},
			Pushes = new[] {StackType.Uint64}
		};
		
		public static Opcode intc_0 = new(0x22, nameof(intc_0))
		{
			Pushes = new[] {StackType.Uint64}
		};
		
		public static Opcode intc_1 = new(0x23, nameof(intc_1))
		{
			Pushes = new[] {StackType.Uint64}
		};
		
		public static Opcode intc_2 = new(0x24, nameof(intc_2))
		{
			Pushes = new[] {StackType.Uint64}
		};
		
		public static Opcode intc_3 = new(0x25, nameof(intc_3))
		{
			Pushes = new[] {StackType.Uint64}
		};
		
		public static Opcode bytecblock = new(0x26, nameof(intcblock))
		{
			Params = new [] {ParamType.StackBytesArray}
		};
		
		public static Opcode bytec = new(0x27, nameof(bytec))
		{
			Params = new [] {ParamType.Uint},
			Pushes = new[] {StackType.Bytes}
		};
		
		public static Opcode bytec_0 = new(0x28, nameof(bytec_0))
		{
			Pushes = new[] {StackType.Bytes}
		};
		
		public static Opcode bytec_1 = new(0x29, nameof(bytec_1))
		{
			Pushes = new[] {StackType.Bytes}
		};
		
		public static Opcode bytec_2 = new(0x2a, nameof(bytec_2))
		{
			Pushes = new[] {StackType.Bytes}
		};
		
		public static Opcode bytec_3 = new(0x2b, nameof(bytec_3))
		{
			Pushes = new[] {StackType.Bytes}
		};
		
		public static Opcode arg = new(0x2c, nameof(arg))
		{
			Params = new [] {ParamType.Uint},
			Pushes = new[] {StackType.Bytes},
			Mode = Mode.Signature
		};
		
		public static Opcode arg_0 = new(0x2d, nameof(arg_0))
		{
			Pushes = new[] {StackType.Bytes},
			Mode = Mode.Signature
		};
		
		public static Opcode arg_1 = new(0x2e, nameof(arg_1))
		{
			Pushes = new[] {StackType.Bytes},
			Mode = Mode.Signature
		};
		
		public static Opcode arg_2 = new(0x2f, nameof(arg_2))
		{
			Pushes = new[] {StackType.Bytes},
			Mode = Mode.Signature
		};
		
		public static Opcode arg_3 = new(0x30, nameof(arg_3))
		{
			Pushes = new[] {StackType.Bytes},
			Mode = Mode.Signature
		};
		
		public static Opcode txn = new(0x31, nameof(txn))
		{
			Params = new [] {ParamType.Uint},
			Pushes = new[] {StackType.Any}
		};
		
		public static Opcode global = new(0x32, nameof(global))
		{
			Params = new [] {ParamType.Uint},
			Pushes = new[] {StackType.Any}
		};
		
		public static Opcode gtxn = new(0x33, nameof(gtxn))
		{
			Params = new [] {ParamType.Uint, ParamType.Uint},
			Pushes = new[] {StackType.Any}
		};
		
		public static Opcode load = new(0x34, nameof(load))
		{
			Params = new [] {ParamType.Uint},
			Pushes = new[] {StackType.Any}
		};
		
		public static Opcode store = new(0x35, nameof(store))
		{
			Params = new [] {ParamType.Uint},
			Pops = new[] {StackType.Any}
		};
		
		public static Opcode txna = new(0x36, nameof(txna))
		{
			Cost = new []{-1, 1, 1, 1, 1},
			Params = new [] {ParamType.Uint, ParamType.Uint},
			Pushes = new[] {StackType.Any}
		};
		
		public static Opcode gtxna = new(0x37, nameof(gtxna))
		{
			Cost = new []{-1, 1, 1, 1, 1},
			Params = new [] {ParamType.Uint, ParamType.Uint, ParamType.Uint},
			Pushes = new[] {StackType.Any}
		};
		
		public static Opcode gtxns = new(0x38, nameof(gtxns))
		{
			Cost = new []{-1, -1, 1, 1, 1},
			Params = new [] {ParamType.Uint},
			Pops = new [] {StackType.Uint64},
			Pushes = new[] {StackType.Any}
		};
		
		public static Opcode gtxnsa = new(0x39, nameof(gtxnsa))
		{
			Cost = new []{-1, -1, 1, 1, 1},
			Params = new [] {ParamType.Uint, ParamType.Uint},
			Pops = new [] {StackType.Uint64},
			Pushes = new[] {StackType.Any}
		};
		
		public static Opcode gload = new(0x3a, nameof(gload))
		{
			Cost = new []{-1, -1, -1, 1, 1},
			Params = new [] {ParamType.Uint, ParamType.Uint},
			Pushes = new[] {StackType.Any},
			Mode = Mode.Application
		};
		
		public static Opcode gloads = new(0x3b, nameof(gloads))
		{
			Cost = new []{-1, -1, -1, 1, 1},
			Params = new [] {ParamType.Uint},
			Pops = new [] {StackType.Uint64},
			Pushes = new[] {StackType.Any},
			Mode = Mode.Application
		};
		
		public static Opcode gaid = new(0x3c, nameof(gaid))
		{
			Cost = new []{-1, -1, -1, 1, 1},
			Params = new [] {ParamType.Uint},
			Pushes = new[] {StackType.Uint64},
			Mode = Mode.Application
		};
		
		public static Opcode gaids = new(0x3d, nameof(gaids))
		{
			Cost = new []{-1, -1, -1, 1, 1},
			Pops = new [] {StackType.Uint64},
			Pushes = new[] {StackType.Uint64},
			Mode = Mode.Application
		};
		
		public static Opcode loads = new(0x3e, nameof(loads))
		{
			Cost = new []{-1, -1, -1, -1, 1},
			Pops = new [] {StackType.Uint64},
			Pushes = new[] {StackType.Any}
		};
		
		public static Opcode stores = new(0x3f, nameof(stores))
		{
			Cost = new []{-1, -1, -1, -1, 1},
			Pops = new [] {StackType.Uint64, StackType.Any}
		};
		
		public static Opcode bnz = new(0x40, nameof(bnz))
		{
			Params = new [] {ParamType.Label},
			Pops = new [] {StackType.Uint64}
		};
		
		public static Opcode bz = new(0x41, nameof(bz))
		{
			Cost = new []{-1, 1, 1, 1, 1},
			Params = new [] {ParamType.Label},
			Pops = new [] {StackType.Uint64}
		};
		
		public static Opcode b = new(0x42, nameof(b))
		{
			Cost = new []{-1, 1, 1, 1, 1},
			Params = new [] {ParamType.Label}
		};
		
		public static Opcode @return = new(0x43, nameof(@return))
		{
			Cost = new []{-1, 1, 1, 1, 1},
			Pops = new [] {StackType.Uint64}
		};
		
		public static Opcode assert = new(0x44, nameof(assert))
		{
			Cost = new []{-1, -1, 1, 1, 1},
			Pops = new [] {StackType.Uint64}
		};
		
		public static Opcode pop = new(0x48, nameof(pop))
		{
			Pops = new [] {StackType.Any}
		};
		
		public static Opcode dup = new(0x49, nameof(dup))
		{
			Pushes = new [] {StackType.Any}
		};
		
		public static Opcode dup2 = new(0x4a, nameof(dup2))
		{
			Cost = new []{-1, 1, 1, 1, 1},
			Pushes = new [] {StackType.Any, StackType.Any}
		};
		
		public static Opcode dig = new(0x4b, nameof(dig))
		{
			Cost = new []{-1, -1, 1, 1, 1},
			Params = new [] {ParamType.Uint},
			Pushes = new [] {StackType.Any}
		};
		
		public static Opcode swap = new(0x4c, nameof(swap))
		{
			Cost = new []{-1, -1, 1, 1, 1},
			Pops = new [] {StackType.Any, StackType.Any},
			Pushes = new [] {StackType.Any, StackType.Any}
		};
		
		public static Opcode select = new(0x4d, nameof(select))
		{
			Cost = new []{-1, -1, 1, 1, 1},
			Pops = new [] {StackType.Any, StackType.Any, StackType.Uint64},
			Pushes = new [] {StackType.Any}
		};
		
		public static Opcode cover = new(0x4e, nameof(cover))
		{
			Cost = new []{-1, -1, -1, -1, 1},
			Params = new [] {ParamType.Uint},
			Pops = new [] {StackType.Any},
			Pushes = new [] {StackType.Any}
		};
		
		public static Opcode uncover = new(0x4f, nameof(uncover))
		{
			Cost = new []{-1, -1, -1, -1, 1},
			Params = new [] {ParamType.Uint},
			Pops = new [] {StackType.Any},
			Pushes = new [] {StackType.Any}
		};
		
		public static Opcode concat = new(0x50, nameof(concat))
		{
			Cost = new []{-1, 1, 1, 1, 1},
			Pops = new [] {StackType.Bytes, StackType.Bytes},
			Pushes = new [] {StackType.Bytes}
		};
		
		public static Opcode substring = new(0x51, nameof(substring))
		{
			Cost = new []{-1, 1, 1, 1, 1},
			Params = new [] {ParamType.Uint, ParamType.Uint},
			Pops = new [] {StackType.Bytes},
			Pushes = new [] {StackType.Bytes}
		};
		
		public static Opcode substring3 = new(0x52, nameof(substring3))
		{
			Cost = new []{-1, 1, 1, 1, 1},
			Pops = new [] {StackType.Bytes, StackType.Uint64, StackType.Uint64},
			Pushes = new [] {StackType.Bytes}
		};
		
		public static Opcode getbit = new(0x53, nameof(getbit))
		{
			Cost = new []{-1, -1, 1, 1, 1},
			Pops = new [] {StackType.Any, StackType.Uint64},
			Pushes = new [] {StackType.Uint64}
		};
		
		public static Opcode setbit = new(0x54, nameof(setbit))
		{
			Cost = new []{-1, -1, 1, 1, 1},
			Pops = new [] {StackType.Any, StackType.Uint64, StackType.Uint64},
			Pushes = new [] {StackType.Any}
		};
		
		public static Opcode getbyte = new(0x55, nameof(getbyte))
		{
			Cost = new []{-1, -1, -1, 1, 1},
			Pops = new [] {StackType.Bytes, StackType.Uint64},
			Pushes = new [] {StackType.Uint64}
		};
		
		public static Opcode setbyte = new(0x56, nameof(setbyte))
		{
			Cost = new []{-1, -1, 1, 1, 1},
			Pops = new [] {StackType.Bytes, StackType.Uint64, StackType.Uint64},
			Pushes = new [] {StackType.Bytes}
		};
		
		public static Opcode extract = new(0x57, nameof(extract))
		{
			Cost = new []{-1, -1, -1, -1, 1},
			Params = new [] {ParamType.Uint, ParamType.Uint},
			Pops = new [] {StackType.Bytes},
			Pushes = new [] {StackType.Bytes}
		};
		
		public static Opcode extract3 = new(0x58, nameof(extract3))
		{
			Cost = new []{-1, -1, -1, -1, 1},
			Pops = new [] {StackType.Bytes, StackType.Uint64, StackType.Uint64},
			Pushes = new [] {StackType.Bytes}
		};
		
		public static Opcode extract_uint16 = new(0x59, nameof(extract_uint16))
		{
			Cost = new []{-1, -1, -1, -1, 1},
			Pops = new [] {StackType.Bytes, StackType.Uint64},
			Pushes = new [] {StackType.Uint64}
		};
		
		public static Opcode extract_uint32 = new(0x5a, nameof(extract_uint32))
		{
			Cost = new []{-1, -1, -1, -1, 1},
			Pops = new [] {StackType.Bytes, StackType.Uint64},
			Pushes = new [] {StackType.Uint64}
		};
		
		public static Opcode extract_uint64 = new(0x5b, nameof(extract_uint64))
		{
			Cost = new []{-1, -1, -1, -1, 1},
			Pops = new [] {StackType.Bytes, StackType.Uint64},
			Pushes = new [] {StackType.Uint64}
		};
		
		public static Opcode balance = new(0x60, nameof(balance))
		{
			Cost = new []{-1, 1, 1, 1, 1},
			Pops = new [] {StackType.Addr},
			Pushes = new [] {StackType.Uint64},
			Mode = Mode.Application
		};
		
		public static Opcode app_opted_in = new(0x61, nameof(app_opted_in))
		{
			Cost = new []{-1, 1, 1, 1, 1},
			Pops = new [] {StackType.Addr, StackType.Uint64},
			Pushes = new [] {StackType.Uint64},
			Mode = Mode.Application
		};
		
		public static Opcode app_local_get = new(0x62, nameof(app_local_get))
		{
			Cost = new []{-1, 1, 1, 1, 1},
			Pops = new [] {StackType.Any, StackType.Bytes},
			Pushes = new [] {StackType.Any},
			Mode = Mode.Application
		};
		
		public static Opcode app_local_get_ex = new(0x63, nameof(app_local_get_ex))
		{
			Cost = new []{-1, 1, 1, 1, 1},
			Pops = new [] {StackType.Any, StackType.Uint64, StackType.Bytes},
			Pushes = new [] {StackType.Any, StackType.Uint64},
			Mode = Mode.Application
		};
		
		public static Opcode app_global_get = new(0x64, nameof(app_global_get))
		{
			Cost = new []{-1, 1, 1, 1, 1},
			Pops = new [] {StackType.Bytes},
			Pushes = new [] {StackType.Any},
			Mode = Mode.Application
		};
		
		public static Opcode app_global_get_ex = new(0x65, nameof(app_global_get_ex))
		{
			Cost = new []{-1, 1, 1, 1, 1},
			Pops = new [] {StackType.Uint64, StackType.Bytes},
			Pushes = new [] {StackType.Any, StackType.Uint64},
			Mode = Mode.Application
		};
		
		public static Opcode app_local_put = new(0x66, nameof(app_local_put))
		{
			Cost = new []{-1, 1, 1, 1, 1},
			Pops = new [] {StackType.Any, StackType.Bytes, StackType.Any},
			Mode = Mode.Application
		};
		
		public static Opcode app_global_put = new(0x67, nameof(app_global_put))
		{
			Cost = new []{-1, 1, 1, 1, 1},
			Pops = new [] {StackType.Bytes, StackType.Any},
			Mode = Mode.Application
		};
		
		public static Opcode app_local_del = new(0x68, nameof(app_local_del))
		{
			Cost = new []{-1, 1, 1, 1, 1},
			Pops = new [] {StackType.Any, StackType.Bytes},
			Mode = Mode.Application
		};
		
		public static Opcode app_global_del = new(0x69, nameof(app_global_del))
		{
			Cost = new []{-1, 1, 1, 1, 1},
			Pops = new [] {StackType.Bytes},
			Mode = Mode.Application
		};
		
		public static Opcode asset_holding_get = new(0x70, nameof(asset_holding_get))
		{
			Cost = new []{-1, 1, 1, 1, 1},
			Params = new [] {ParamType.Uint},
			Pops = new [] {StackType.Any, StackType.Uint64},
			Pushes = new [] {StackType.Any, StackType.Uint64},
			Mode = Mode.Application
		};
		
		public static Opcode asset_params_get = new(0x71, nameof(asset_params_get))
		{
			Cost = new []{-1, 1, 1, 1, 1},
			Params = new [] {ParamType.Uint},
			Pops = new [] {StackType.Uint64},
			Pushes = new [] {StackType.Any, StackType.Uint64},
			Mode = Mode.Application
		};
		
		public static Opcode app_params_get = new(0x72, nameof(app_params_get))
		{
			Cost = new []{-1, -1, -1, -1, 1},
			Params = new [] {ParamType.Uint},
			Pops = new [] {StackType.Uint64},
			Pushes = new [] {StackType.Any, StackType.Uint64},
			Mode = Mode.Application
		};
		
		public static Opcode min_balance = new(0x78, nameof(min_balance))
		{
			Cost = new []{-1, -1, 1, 1, 1},
			Pops = new [] {StackType.Any},
			Pushes = new [] {StackType.Uint64},
			Mode = Mode.Application
		};
		
		public static Opcode pushbytes = new(0x80, nameof(pushbytes))
		{
			Cost = new []{-1, -1, 1, 1, 1},
			Params = new [] {ParamType.Bytes},
			Pushes = new [] {StackType.Bytes}
		};
		
		public static Opcode pushint = new(0x81, nameof(pushint))
		{
			Cost = new []{-1, -1, 1, 1, 1},
			Params = new [] {ParamType.Uint},
			Pushes = new [] {StackType.Uint64}
		};
		
		public static Opcode callsub = new(0x88, nameof(callsub))
		{
			Cost = new []{-1, -1, -1, 1, 1},
			Params = new [] {ParamType.Label}
		};
		
		public static Opcode retsub = new(0x89, nameof(retsub))
		{
			Cost = new []{-1, -1, -1, 1, 1}
		};
		
		public static Opcode shl = new(0x90, nameof(shl))
		{
			Cost = new []{-1, -1, -1, 1, 1},
			Pops = new [] {StackType.Uint64, StackType.Uint64},
			Pushes = new [] {StackType.Uint64}
		};
		
		public static Opcode shr = new(0x91, nameof(shr))
		{
			Cost = new []{-1, -1, -1, 1, 1},
			Pops = new [] {StackType.Uint64, StackType.Uint64},
			Pushes = new [] {StackType.Uint64}
		};
		
		public static Opcode sqrt = new(0x92, nameof(sqrt))
		{
			Cost = new []{-1, -1, -1, 4, 4},
			Pops = new [] {StackType.Uint64},
			Pushes = new [] {StackType.Uint64}
		};
		
		public static Opcode bitlen = new(0x93, nameof(bitlen))
		{
			Cost = new []{-1, -1, -1, 1, 1},
			Pops = new [] {StackType.Any},
			Pushes = new [] {StackType.Uint64}
		};
		
		public static Opcode exp = new(0x94, nameof(exp))
		{
			Cost = new []{-1, -1, -1, 1, 1},
			Pops = new [] {StackType.Uint64, StackType.Uint64},
			Pushes = new [] {StackType.Uint64}
		};
		
		public static Opcode expw = new(0x95, nameof(expw))
		{
			Cost = new []{-1, -1, -1, 10, 10},
			Pops = new [] {StackType.Uint64, StackType.Uint64},
			Pushes = new [] {StackType.Uint64, StackType.Uint64}
		};
		
		public static Opcode badd = new(0xa0, "b+")
		{
			Cost = new []{-1, -1, -1, 10, 10},
			Pops = new [] {StackType.Bytes, StackType.Bytes},
			Pushes = new [] {StackType.Bytes}
		};
		
		public static Opcode bsub = new(0xa1, "b-")
		{
			Cost = new []{-1, -1, -1, 10, 10},
			Pops = new [] {StackType.Bytes, StackType.Bytes},
			Pushes = new [] {StackType.Bytes}
		};
		
		public static Opcode bdiv = new(0xa2, "b/")
		{
			Cost = new []{-1, -1, -1, 20, 20},
			Pops = new [] {StackType.Bytes, StackType.Bytes},
			Pushes = new [] {StackType.Bytes}
		};
		
		public static Opcode bmul = new(0xa3, "b*")
		{
			Cost = new []{-1, -1, -1, 20, 20},
			Pops = new [] {StackType.Bytes, StackType.Bytes},
			Pushes = new [] {StackType.Bytes}
		};
		
		public static Opcode blesser_than = new(0xa4, "b<")
		{
			Cost = new []{-1, -1, -1, 1, 1},
			Pops = new [] {StackType.Bytes, StackType.Bytes},
			Pushes = new [] {StackType.Uint64}
		};
		
		public static Opcode bgreater_than = new(0xa5, "b>")
		{
			Cost = new []{-1, -1, -1, 1, 1},
			Pops = new [] {StackType.Bytes, StackType.Bytes},
			Pushes = new [] {StackType.Uint64}
		};
		
		public static Opcode bequal_or_lesser_than = new(0xa6, "b<=")
		{
			Cost = new []{-1, -1, -1, 1, 1},
			Pops = new [] {StackType.Bytes, StackType.Bytes},
			Pushes = new [] {StackType.Uint64}
		};
		
		public static Opcode bequal_or_greater_than = new(0xa7, "b>=")
		{
			Cost = new []{-1, -1, -1, 1, 1},
			Pops = new [] {StackType.Bytes, StackType.Bytes},
			Pushes = new [] {StackType.Uint64}
		};
		
		public static Opcode bequal = new(0xa8, "b==")
		{
			Cost = new []{-1, -1, -1, 1, 1},
			Pops = new [] {StackType.Bytes, StackType.Bytes},
			Pushes = new [] {StackType.Uint64}
		};
		
		public static Opcode bnotequal = new(0xa9, "b!=")
		{
			Cost = new []{-1, -1, -1, 1, 1},
			Pops = new [] {StackType.Bytes, StackType.Bytes},
			Pushes = new [] {StackType.Uint64}
		};
		
		public static Opcode bmod = new(0xaa, "b%")
		{
			Cost = new []{-1, -1, -1, 20, 20},
			Pops = new [] {StackType.Bytes, StackType.Bytes},
			Pushes = new [] {StackType.Bytes}
		};
		
		public static Opcode bor = new(0xab, "b|")
		{
			Cost = new []{-1, -1, -1, 6, 6},
			Pops = new [] {StackType.Bytes, StackType.Bytes},
			Pushes = new [] {StackType.Bytes}
		};
		
		public static Opcode band = new(0xac, "b&")
		{
			Cost = new []{-1, -1, -1, 6, 6},
			Pops = new [] {StackType.Bytes, StackType.Bytes},
			Pushes = new [] {StackType.Bytes}
		};
		
		public static Opcode bxor = new(0xad, "b^")
		{
			Cost = new []{-1, -1, -1, 6, 6},
			Pops = new [] {StackType.Bytes, StackType.Bytes},
			Pushes = new [] {StackType.Bytes}
		};
		
		public static Opcode borcomplement = new(0xae, "b~")
		{
			Cost = new []{-1, -1, -1, 4, 4},
			Pops = new [] {StackType.Bytes},
			Pushes = new [] {StackType.Bytes}
		};
		
		public static Opcode bzero = new(0xaf, nameof(bzero))
		{
			Cost = new []{-1, -1, -1, 4, 4},
			Pops = new [] {StackType.Uint64},
			Pushes = new [] {StackType.Bytes}
		};
		
		public static Opcode log = new(0xb0, nameof(log))
		{
			Cost = new []{-1, -1, -1, -1, 1},
			Pops = new [] {StackType.Bytes},
			Mode = Mode.Application
		};
		
		public static Opcode itxn_begin = new(0xb1, nameof(itxn_begin))
		{
			Cost = new []{-1, -1, -1, -1, 1},
			Mode = Mode.Application
		};
		
		public static Opcode itxn_field = new(0xb2, nameof(itxn_field))
		{
			Cost = new []{-1, -1, -1, -1, 1},
			Pops = new [] {StackType.Any},
			Mode = Mode.Application
		};
		
		public static Opcode itxn_submit = new(0xb3, nameof(itxn_submit))
		{
			Cost = new []{-1, -1, -1, -1, 1},
			Mode = Mode.Application
		};
		
		public static Opcode itxn = new(0xb4, nameof(itxn))
		{
			Cost = new []{-1, -1, -1, -1, 1},
			Params = new [] {ParamType.Uint},
			Pushes = new [] {StackType.Any},
			Mode = Mode.Application
		};
		
		public static Opcode itxna = new(0xb5, nameof(itxna))
		{
			Cost = new []{-1, -1, -1, -1, 1},
			Params = new [] {ParamType.Uint, ParamType.Uint},
			Pops = new [] {StackType.Any},
			Mode = Mode.Application
		};
		
		public static Opcode txnas = new(0xc0, nameof(txnas))
		{
			Cost = new []{-1, -1, -1, -1, 1},
			Params = new [] {ParamType.Uint},
			Pops = new [] {StackType.Uint64},
			Pushes = new [] {StackType.Any},
			Mode = Mode.Application
		};
		
		public static Opcode gtxnas = new(0xc1, nameof(gtxnas))
		{
			Cost = new []{-1, -1, -1, -1, 1},
			Params = new [] {ParamType.Uint, ParamType.Uint},
			Pops = new [] {StackType.Uint64},
			Pushes = new [] {StackType.Any},
			Mode = Mode.Application
		};
		
		public static Opcode gtxnsas = new(0xc2, nameof(gtxnsas))
		{
			Cost = new []{-1, -1, -1, -1, 1},
			Params = new [] {ParamType.Uint},
			Pops = new [] {StackType.Uint64, StackType.Uint64},
			Pushes = new [] {StackType.Any},
			Mode = Mode.Application
		};
		
		public static Opcode args = new(0xc3, nameof(args))
		{
			Cost = new []{-1, -1, -1, -1, 1},
			Pops = new [] {StackType.Uint64},
			Pushes = new [] {StackType.Bytes},
			Mode = Mode.Signature
		};
	}
}