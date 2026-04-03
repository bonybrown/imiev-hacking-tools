/// <summary>
/// Represents a CAN address as both a numeric value and its 4-byte big-endian representation.
/// </summary>
class CanAddress(int value)
{
    /// <summary>The numeric CAN ID value.</summary>
    public int Value { get; } = value;

    /// <summary>The 4-byte big-endian representation of the CAN ID.</summary>
    public byte[] Bytes { get; } =
    [
        (byte)((value >> 24) & 0xFF),
        (byte)((value >> 16) & 0xFF),
        (byte)((value >> 8) & 0xFF),
        (byte)(value & 0xFF)
    ];

    public override string ToString() => $"0x{Value:X3}";
}
