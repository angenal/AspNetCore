namespace WebCore.Security
{
    /// <summary>
    /// This is a Crc16 algorithm.
    /// </summary>
    public static class Crc16Algorithm
    {
        const ushort POLYNOMIAL = 0x8408;
        const ushort PRESET_VALUE = 0xFFFF;

        /// <summary>
        /// Crc16
        /// </summary>
        public static ushort Crc16(byte[] data) => Calc(data);
        /// <summary>
        /// Crc16
        /// </summary>
        public static byte[] Crc16(string s) => System.BitConverter.GetBytes(Calc(System.Text.Encoding.ASCII.GetBytes(s)));

        static ushort Calc(byte[] buffer)
        {
            ushort value = PRESET_VALUE;
            for (int i = 0; i < buffer.Length; i++)
            {
                value = (ushort)(value ^ buffer[i]);
                for (int ucJ = 0; ucJ < 8; ucJ++)
                {
                    value = (value & 0x0001) != 0 ? (ushort)((value >> 1) ^ POLYNOMIAL) : (ushort)(value >> 1);
                }
            }
            return value;
        }
    }
}
