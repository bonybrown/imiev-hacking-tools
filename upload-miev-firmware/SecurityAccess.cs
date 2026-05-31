/// <summary>
/// Mitsubishi ECU security access seed-to-key computation.
/// </summary>
static class SecurityAccess
{
    /// <summary>
    /// Computes the key from the seed for the given access level.
    /// </summary>
    /// <param name="seed">The 4-byte seed received from the ECU.</param>
    /// <param name="accessLevel">The security access level (odd = seed request, even = key send).</param>
    /// <returns>The computed 4-byte key.</returns>
    public static byte[] ComputeKey(ReadOnlySpan<byte> seed, byte accessLevel)
    {
        if (seed.Length != 4)
            throw new ArgumentException($"Expected 4-byte seed, got {seed.Length}");

        if (accessLevel == 0x05)
        {
            // Two big-endian uint16 values: out = (in * 0xE0) + 0x2AB6
            // Multiply via repeated addition to ensure pure 16-bit wrapping
            ushort s0 = (ushort)((seed[0] << 8) | seed[1]);
            ushort s1 = (ushort)((seed[2] << 8) | seed[3]);

            ushort k0 = Kdf(s0);
            ushort k1 = Kdf(s1);

            return [(byte)(k0 >> 8), (byte)(k0 & 0xFF), (byte)(k1 >> 8), (byte)(k1 & 0xFF)];

            static ushort Kdf(ushort input)
            {
                ushort acc = 0;
                for (int i = 0; i < 0xB1; i++)
                    acc = ((ushort)(acc + input));
                acc = ((ushort)(acc + 0xCB14));
                return acc;
            }
        }

        throw new NotImplementedException(
            $"Security Access level 0x{accessLevel:X2} not implemented. " +
            $"Seed: {BitConverter.ToString(seed.ToArray())}");
    }
}
