using System;

namespace Base85Project
{
    public static class Base85
    {
        #region Encode
        public static byte[] Encode(byte[] binary)
        {
            if(binary == null) { throw new ArgumentNullException(); }
            const int ASCII_OFFSET = 33;
            const int INDEX_OFFSET = 2;
            var loopCount = (binary.Length + 3) / 4;
            int paddingCount = (binary.Length % 4 == 0) ? 0 : 4 - binary.Length % 4;      // 0 ~ 3
            var result = new byte[loopCount * 5 + 4 - paddingCount];
            result[0] = 0x3c;                       // '<'
            result[1] = 0x7e;                       // '~'
            result[result.Length - 2] = 0x7e;       // '~'
            result[result.Length - 1] = 0x3e;       // '>'
            for(int i = 0; i < loopCount; i++) {
                int j = i * 4;
                // ----------------------------
                uint block;
                if(i != loopCount - 1) {
                    block = (uint)((binary[j] << 24) + (binary[j + 1] << 16) + (binary[j + 2] << 8) + binary[j + 3]);
                }
                else {
                    block = (paddingCount == 0) ? (uint)((binary[j] << 24) + (binary[j + 1] << 16) + (binary[j + 2] << 8) + binary[j + 3]) :
                            (paddingCount == 1) ? (uint)((binary[j] << 24) + (binary[j + 1] << 16) + (binary[j + 2] << 8)) :
                            (paddingCount == 2) ? (uint)((binary[j] << 24) + (binary[j + 1] << 16)) :
                                                  (uint)((binary[j] << 24));
                }
                uint tmp = (block / 85) * 85;
                if(i != loopCount - 1 || paddingCount < 1) {
                    result[i * 5 + 4 + INDEX_OFFSET] = (byte)(block - tmp + ASCII_OFFSET);
                }
                // ----------------------------
                block = tmp / 85;
                tmp = (block / 85) * 85;
                if(i != loopCount - 1 || paddingCount < 2) {
                    result[i * 5 + 3 + INDEX_OFFSET] = (byte)(block - tmp + ASCII_OFFSET);
                }
                // ----------------------------
                block = tmp / 85;
                tmp = (block / 85) * 85;
                if(i != loopCount - 1 || paddingCount < 3) {
                    result[i * 5 + 2 + INDEX_OFFSET] = (byte)(block - tmp + ASCII_OFFSET);
                }
                // ----------------------------
                block = tmp / 85;
                tmp = (block / 85) * 85;
                result[i * 5 + 1 + INDEX_OFFSET] = (byte)(block - tmp + ASCII_OFFSET);
                // ----------------------------
                block = tmp / 85;
                tmp = (block / 85) * 85;
                result[i * 5 + INDEX_OFFSET] = (byte)(block - tmp + ASCII_OFFSET);
                // ----------------------------
            }
            return result;
        }
        #endregion

        #region Decode
        public static byte[] Decode(byte[] ascii)
        {
            if(ascii == null) { throw new ArgumentNullException(nameof(ascii)); }
            if(ascii[0] != 0x3c || ascii[1] != 0x7e || ascii[ascii.Length - 2] != 0x7e || ascii[ascii.Length - 1] != 0x3e) {
                throw new FormatException();
            }
            const int ASCII_OFFSET = 33;
            const int INDEX_OFFSET = 2;
            const byte PADDING_ASCII = 117;      // 'u'
            var len = ascii.Length - 4;
            var loopCount = (len + 4) / 5;
            var paddingCount = (len % 5 == 0) ? 0 : 5 - len % 5;        // 0 ~ 4
            var result = new byte[loopCount * 4 - paddingCount];
            for(int i = 0; i < loopCount; i++) {
                int j = i * 5 + INDEX_OFFSET;
                uint block;
                if(i != loopCount - 1) {
                    block = (uint)(ascii[j] - ASCII_OFFSET) * 52200625 +
                            (uint)(ascii[j + 1] - ASCII_OFFSET) * 614125 +
                            (uint)(ascii[j + 2] - ASCII_OFFSET) * 7225 +
                            (uint)(ascii[j + 3] - ASCII_OFFSET) * 85 +
                            (uint)(ascii[j + 4] - ASCII_OFFSET);
                }
                else {
                    block = (paddingCount == 0) ? (uint)(ascii[j] - ASCII_OFFSET) * 52200625 + (uint)(ascii[j + 1] - ASCII_OFFSET) * 614125 + (uint)(ascii[j + 2] - ASCII_OFFSET) * 7225 + (uint)(ascii[j + 3] - ASCII_OFFSET) * 85 + (uint)(ascii[j + 4] - ASCII_OFFSET) :
                            (paddingCount == 1) ? (uint)(ascii[j] - ASCII_OFFSET) * 52200625 + (uint)(ascii[j + 1] - ASCII_OFFSET) * 614125 + (uint)(ascii[j + 2] - ASCII_OFFSET) * 7225 + (uint)(ascii[j + 3] - ASCII_OFFSET) * 85 + (uint)(PADDING_ASCII - ASCII_OFFSET) :
                            (paddingCount == 2) ? (uint)(ascii[j] - ASCII_OFFSET) * 52200625 + (uint)(ascii[j + 1] - ASCII_OFFSET) * 614125 + (uint)(ascii[j + 2] - ASCII_OFFSET) * 7225 + (uint)(PADDING_ASCII - ASCII_OFFSET) * 85 + (uint)(PADDING_ASCII - ASCII_OFFSET) :
                            (paddingCount == 3) ? (uint)(ascii[j] - ASCII_OFFSET) * 52200625 + (uint)(ascii[j + 1] - ASCII_OFFSET) * 614125 + (uint)(PADDING_ASCII - ASCII_OFFSET) * 7225 + (uint)(PADDING_ASCII - ASCII_OFFSET) * 85 + (uint)(PADDING_ASCII - ASCII_OFFSET) :
                                                  (uint)(ascii[j] - ASCII_OFFSET) * 52200625 + (uint)(PADDING_ASCII - ASCII_OFFSET) * 614125 + (uint)(PADDING_ASCII - ASCII_OFFSET) * 7225 + (uint)(PADDING_ASCII - ASCII_OFFSET) * 85 + (uint)(PADDING_ASCII - ASCII_OFFSET);
                }
                result[i * 4] = (byte)((block & 0xff000000) >> 24);
                if(i != loopCount - 1 || paddingCount <= 2) {
                    result[i * 4 + 1] = (byte)((block & 0x00ff0000) >> 16);
                }
                if(i != loopCount - 1 || paddingCount <= 1) {
                    result[i * 4 + 2] = (byte)((block & 0x0000ff00) >> 8);
                }
                if(i != loopCount - 1 || paddingCount <= 0) {
                    result[i * 4 + 3] = (byte)(block & 0x000000ff);
                }
            }
            return result;
        }
        #endregion
    }
}
