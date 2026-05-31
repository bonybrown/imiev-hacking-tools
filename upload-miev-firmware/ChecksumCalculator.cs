using System.Buffers.Binary;

static class ChecksumCalculator
{
    public const int ChecksumOffset = 0xFFFF8;
    public const uint TargetChecksum = 0x5AA55AA5;

    /// <summary>
    /// Calculates the 32-bit unsigned sum of all 32-bit big-endian values in the data.
    /// When <paramref name="includeChecksumOffset"/> is false, the value at offset 0xFFFF8
    /// is excluded from the sum.
    /// </summary>
    public static uint Checksum(ReadOnlySpan<byte> data, bool includeChecksumOffset = true)
    {
        uint sum = 0;
        for (int offset = 0; offset + 4 <= data.Length; offset += 4)
        {
            if (!includeChecksumOffset && offset == ChecksumOffset)
                continue;
            sum += BinaryPrimitives.ReadUInt32BigEndian(data.Slice(offset, 4));
        }
        return sum;
    }

    /// <summary>
    /// Returns the fill value that must be stored at offset 0xFFFF8 so that
    /// the full checksum (including that value) equals 0x5AA55AA5.
    /// </summary>
    public static uint RequiredFillValue(ReadOnlySpan<byte> data)
    {
        uint sumWithout = Checksum(data, includeChecksumOffset: false);
        return TargetChecksum - sumWithout;
    }
}
