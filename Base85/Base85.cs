using System;
using System.Text;
using System.Collections.Generic;

namespace Base85Project
{
    public static class Base85
    {
        #region EncodeToString
        public static string EncodeToString(byte[] binary) => Encoding.ASCII.GetString(Encode(binary));
        #endregion

        #region Encode
        public static byte[] Encode(byte[] binary)
        {
            if(binary == null) { throw new ArgumentNullException(); }
            const int ASCII_OFFSET = 33;
            const byte Z = 0x7a;        // 'z'
            var loopCount = (binary.Length + 3) / 4;
            int paddingCount = (binary.Length % 4 == 0) ? 0 : 4 - binary.Length % 4;      // 0 ~ 3
            var result = new List<byte>(loopCount * 5 + 4 - paddingCount);
            result.Add(0x3c);           // '<'
            result.Add(0x7e);           // '~'
            for(int i = 0; i < loopCount; i++) {
                int j = i * 4;
                // ----------------------------
                uint block;
                byte ans0;
                byte ans1;
                byte? ans2 = null;
                byte? ans3 = null;
                byte? ans4 = null;
                if(i != loopCount - 1) {
                    block = (uint)((binary[j] << 24) + (binary[j + 1] << 16) + (binary[j + 2] << 8) + binary[j + 3]);
                }
                else {
                    block = (paddingCount == 0) ? (uint)((binary[j] << 24) + (binary[j + 1] << 16) + (binary[j + 2] << 8) + binary[j + 3]) :
                            (paddingCount == 1) ? (uint)((binary[j] << 24) + (binary[j + 1] << 16) + (binary[j + 2] << 8)) :
                            (paddingCount == 2) ? (uint)((binary[j] << 24) + (binary[j + 1] << 16)) :
                                                  (uint)((binary[j] << 24));
                }
                if(block == 0) {
                    result.Add(Z);
                    continue;
                }
                uint tmp = (block / 85) * 85;
                if(i != loopCount - 1 || paddingCount < 1) {
                    ans4 = (byte)(block - tmp + ASCII_OFFSET);
                }
                // ----------------------------
                block = tmp / 85;
                tmp = (block / 85) * 85;
                if(i != loopCount - 1 || paddingCount < 2) {
                    ans3 = (byte)(block - tmp + ASCII_OFFSET);
                }
                // ----------------------------
                block = tmp / 85;
                tmp = (block / 85) * 85;
                if(i != loopCount - 1 || paddingCount < 3) {
                    ans2 = (byte)(block - tmp + ASCII_OFFSET);
                }
                // ----------------------------
                block = tmp / 85;
                tmp = (block / 85) * 85;
                ans1 = (byte)(block - tmp + ASCII_OFFSET);
                // ----------------------------
                block = tmp / 85;
                tmp = (block / 85) * 85;
                ans0 = (byte)(block - tmp + ASCII_OFFSET);
                // ----------------------------
                result.Add(ans0);
                result.Add(ans1);
                if(ans2 != null) { result.Add(ans2.Value); }
                if(ans3 != null) { result.Add(ans3.Value); }
                if(ans4 != null) { result.Add(ans4.Value); }
            }
            result.Add(0x7e);   // '~'
            result.Add(0x3e);   // '>'
            return result.ToArray();
        }
        #endregion

        #region DecodeFromString
        public static byte[] DecodeFromString(string str) => Decode(Encoding.ASCII.GetBytes(str));
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
            const byte PADDING_ASCII = 117;     // 'u'
            const byte Z = 0x7a;                // 'z'
            var len = ascii.Length - 4;
            var skipCount = 0;
            var result = new List<byte>(ascii.Length / 5 * 4);
            int i = 0;
            while(true) {
                int j = i * 5 + INDEX_OFFSET - skipCount * 4;
                if(j >= ascii.Length - INDEX_OFFSET) { break; }
                if(ascii[j] == Z) {
                    result.Add(0x00);
                    result.Add(0x00);
                    result.Add(0x00);
                    result.Add(0x00);
                    skipCount++;
                }
                else {
                    bool isLastLoop = (j >= ascii.Length - 5 - INDEX_OFFSET);
                    uint block;
                    int paddingCount = 0;
                    if(!isLastLoop) {
                        block = (uint)(ascii[j] - ASCII_OFFSET) * 52200625 +
                                (uint)(ascii[j + 1] - ASCII_OFFSET) * 614125 +
                                (uint)(ascii[j + 2] - ASCII_OFFSET) * 7225 +
                                (uint)(ascii[j + 3] - ASCII_OFFSET) * 85 +
                                (uint)(ascii[j + 4] - ASCII_OFFSET);
                    }
                    else {
                        paddingCount = 5 - (ascii.Length - INDEX_OFFSET - j);       // 0 ~ 4
                        block = (paddingCount == 0) ? (uint)(ascii[j] - ASCII_OFFSET) * 52200625 + (uint)(ascii[j + 1] - ASCII_OFFSET) * 614125 + (uint)(ascii[j + 2] - ASCII_OFFSET) * 7225 + (uint)(ascii[j + 3] - ASCII_OFFSET) * 85 + (uint)(ascii[j + 4] - ASCII_OFFSET) :
                                (paddingCount == 1) ? (uint)(ascii[j] - ASCII_OFFSET) * 52200625 + (uint)(ascii[j + 1] - ASCII_OFFSET) * 614125 + (uint)(ascii[j + 2] - ASCII_OFFSET) * 7225 + (uint)(ascii[j + 3] - ASCII_OFFSET) * 85 + (uint)(PADDING_ASCII - ASCII_OFFSET) :
                                (paddingCount == 2) ? (uint)(ascii[j] - ASCII_OFFSET) * 52200625 + (uint)(ascii[j + 1] - ASCII_OFFSET) * 614125 + (uint)(ascii[j + 2] - ASCII_OFFSET) * 7225 + (uint)(PADDING_ASCII - ASCII_OFFSET) * 85 + (uint)(PADDING_ASCII - ASCII_OFFSET) :
                                (paddingCount == 3) ? (uint)(ascii[j] - ASCII_OFFSET) * 52200625 + (uint)(ascii[j + 1] - ASCII_OFFSET) * 614125 + (uint)(PADDING_ASCII - ASCII_OFFSET) * 7225 + (uint)(PADDING_ASCII - ASCII_OFFSET) * 85 + (uint)(PADDING_ASCII - ASCII_OFFSET) :
                                                      (uint)(ascii[j] - ASCII_OFFSET) * 52200625 + (uint)(PADDING_ASCII - ASCII_OFFSET) * 614125 + (uint)(PADDING_ASCII - ASCII_OFFSET) * 7225 + (uint)(PADDING_ASCII - ASCII_OFFSET) * 85 + (uint)(PADDING_ASCII - ASCII_OFFSET);
                    }
                    result.Add((byte)((block & 0xff000000) >> 24));
                    if(!isLastLoop || paddingCount <= 2) {
                        result.Add((byte)((block & 0x00ff0000) >> 16));
                    }
                    if(!isLastLoop || paddingCount <= 1) {
                        result.Add((byte)((block & 0x0000ff00) >> 8));
                    }
                    if(!isLastLoop || paddingCount <= 0) {
                        result.Add((byte)(block & 0x000000ff));
                    }
                }
                i++;
            }
            return result.ToArray();
        }
        #endregion
    }
}
