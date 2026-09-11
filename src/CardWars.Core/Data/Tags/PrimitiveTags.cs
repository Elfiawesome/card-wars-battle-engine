namespace CardWars.Core.Data.Tags;

public sealed class IntTag(int value) : DataTag
{
	public int Value { get; set; } = value;
	public override DataTagKind Type => DataTagKind.Int;
	public override DataTag Clone() => new IntTag(Value);

	// Operator overload
	public static IntTag operator +(IntTag v1, IntTag v2) => new(v1.Value + v2.Value);
	public static IntTag operator -(IntTag v1, IntTag v2) => new(v1.Value - v2.Value);
	public static IntTag operator *(IntTag v1, IntTag v2) => new(v1.Value * v2.Value);
	public static IntTag operator /(IntTag v1, IntTag v2) => new(v1.Value / v2.Value);
	public static IntTag operator ++(IntTag v1) { v1.Value += 1; return v1; }
	public static IntTag operator --(IntTag v1) { v1.Value -= 1; return v1; }

	public static IntTag operator +(IntTag v1, int v2) => new(v1.Value + v2);
	public static IntTag operator -(IntTag v1, int v2) => new(v1.Value - v2);
	public static IntTag operator *(IntTag v1, int v2) => new(v1.Value * v2);
	public static IntTag operator /(IntTag v1, int v2) => new(v1.Value / v2);

	// public static implicit operator int(IntTag v) => v.Value;
	// public static implicit operator IntTag(int v) => new(v);
}

public sealed class FloatTag(float value) : DataTag
{
	public float Value { get; set; } = value;
	public override DataTagKind Type => DataTagKind.Float;
	public override DataTag Clone() => new FloatTag(Value);

	// Operator overload
	public static FloatTag operator +(FloatTag v1, FloatTag v2) => new(v1.Value + v2.Value);
	public static FloatTag operator -(FloatTag v1, FloatTag v2) => new(v1.Value - v2.Value);
	public static FloatTag operator *(FloatTag v1, FloatTag v2) => new(v1.Value * v2.Value);
	public static FloatTag operator /(FloatTag v1, FloatTag v2) => new(v1.Value / v2.Value);
	public static FloatTag operator ++(FloatTag v1) { v1.Value += 1; return v1; }
	public static FloatTag operator --(FloatTag v1) { v1.Value -= 1; return v1; }

	public static FloatTag operator +(FloatTag v1, float v2) => new(v1.Value + v2);
	public static FloatTag operator -(FloatTag v1, float v2) => new(v1.Value - v2);
	public static FloatTag operator *(FloatTag v1, float v2) => new(v1.Value * v2);
	public static FloatTag operator /(FloatTag v1, float v2) => new(v1.Value / v2);

	// public static implicit operator float(FloatTag v) => v.Value;
	// public static implicit operator FloatTag(float v) => new(v);
}

public sealed class StringTag(string value) : DataTag
{
	public string Value { get; set; } = value;
	public override DataTagKind Type => DataTagKind.String;
	public override DataTag Clone() => new StringTag(Value);

	// Operator overload
	public static StringTag operator +(StringTag v1, StringTag v2) => new(v1.Value + v2.Value);
	public static StringTag operator ++(StringTag v1) { v1.Value += 1; return v1; }

	public static StringTag operator +(StringTag v1, string v2) => new(v1.Value + v2);

	// public static implicit operator string(StringTag v) => v.Value;
	// public static implicit operator StringTag(string v) => new(v);
}

public sealed class BoolTag(bool value) : DataTag
{
	public bool Value { get; set; } = value;
	public override DataTagKind Type => DataTagKind.Bool;
	public override DataTag Clone() => new BoolTag(Value);

	// Operator overload
	public static bool operator true(BoolTag v1) => v1.Value;
	public static bool operator false(BoolTag v1) => v1.Value;

	// public static implicit operator bool(BoolTag v) => v.Value;
	// public static implicit operator BoolTag(bool v) => new(v);
}

public sealed class GuidTag(Guid value) : DataTag
{
	public Guid Value { get; set; } = value;
	public override DataTagKind Type => DataTagKind.Guid;
	public override DataTag Clone() => new GuidTag(Value);

	// public static implicit operator Guid(GuidTag v) => v.Value;
	// public static implicit operator GuidTag(Guid v) => new(v);
}