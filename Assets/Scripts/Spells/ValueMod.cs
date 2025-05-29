public enum ModOp { Add, Mul }

public readonly struct ValueMod
{
    public ModOp Op { get; }
    public float Value { get; }

    public ValueMod(ModOp op, float value)
    {
        Op = op;
        Value = value;
    }
}