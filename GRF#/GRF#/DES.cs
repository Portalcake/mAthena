using System;
using System.Security.Cryptography;
using System.IO;

namespace SAIB.SharpGRF
{
    public static class DES
    {
        #region local variables
        static int[] BitMaskTable = new int[8] { 
			0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01
		};

        static int[] BitSwapTable1 = new int[64] {
			58, 50, 42, 34, 26, 18, 10,  2, 60, 52, 44, 36, 28, 20, 12,  4,
			62, 54, 46, 38, 30, 22, 14,  6, 64, 56, 48, 40, 32, 24, 16,  8,
			57, 49, 41, 33, 25, 17,  9,  1, 59, 51, 43, 35, 27, 19, 11,  3,
			61, 53, 45, 37, 29, 21, 13,  5, 63, 55, 47, 39, 31, 23, 15,  7
		};

        static int[] BitSwapTable2 = new int[64] {
			40,  8, 48, 16, 56, 24, 64, 32, 39,  7, 47, 15, 55, 23, 63, 31,
			38,  6, 46, 14, 54, 22, 62, 30, 37,  5, 45, 13, 53, 21, 61, 29,
			36,  4, 44, 12, 52, 20, 60, 28, 35,  3, 43, 11, 51, 19, 59, 27,
			34,  2, 42, 10, 50, 18, 58, 26, 33,  1, 41,  9, 49, 17, 57, 25
		};

        static int[] BitSwapTable3 = new int[32]{
			16, 7, 20, 21, 29, 12, 28, 17,  1, 15, 23, 26,  5, 18, 31, 10,
		    2,  8, 24, 14, 32, 27,  3,  9, 19, 13, 30,  6, 22, 11,  4, 25
		};

        static int[,] NibbleData = new int[4, 64] {
			{
				0xef, 0x03, 0x41, 0xfd, 0xd8, 0x74, 0x1e, 0x47,  0x26, 0xef, 0xfb, 0x22, 0xb3, 0xd8, 0x84, 0x1e,
				0x39, 0xac, 0xa7, 0x60, 0x62, 0xc1, 0xcd, 0xba,  0x5c, 0x96, 0x90, 0x59, 0x05, 0x3b, 0x7a, 0x85,
				0x40, 0xfd, 0x1e, 0xc8, 0xe7, 0x8a, 0x8b, 0x21,  0xda, 0x43, 0x64, 0x9f, 0x2d, 0x14, 0xb1, 0x72,
				0xf5, 0x5b, 0xc8, 0xb6, 0x9c, 0x37, 0x76, 0xec,  0x39, 0xa0, 0xa3, 0x05, 0x52, 0x6e, 0x0f, 0xd9,
			}, {
				0xa7, 0xdd, 0x0d, 0x78, 0x9e, 0x0b, 0xe3, 0x95,  0x60, 0x36, 0x36, 0x4f, 0xf9, 0x60, 0x5a, 0xa3,
				0x11, 0x24, 0xd2, 0x87, 0xc8, 0x52, 0x75, 0xec,  0xbb, 0xc1, 0x4c, 0xba, 0x24, 0xfe, 0x8f, 0x19,
				0xda, 0x13, 0x66, 0xaf, 0x49, 0xd0, 0x90, 0x06,  0x8c, 0x6a, 0xfb, 0x91, 0x37, 0x8d, 0x0d, 0x78,
				0xbf, 0x49, 0x11, 0xf4, 0x23, 0xe5, 0xce, 0x3b,  0x55, 0xbc, 0xa2, 0x57, 0xe8, 0x22, 0x74, 0xce,
			}, {
				0x2c, 0xea, 0xc1, 0xbf, 0x4a, 0x24, 0x1f, 0xc2,  0x79, 0x47, 0xa2, 0x7c, 0xb6, 0xd9, 0x68, 0x15,
				0x80, 0x56, 0x5d, 0x01, 0x33, 0xfd, 0xf4, 0xae,  0xde, 0x30, 0x07, 0x9b, 0xe5, 0x83, 0x9b, 0x68,
				0x49, 0xb4, 0x2e, 0x83, 0x1f, 0xc2, 0xb5, 0x7c,  0xa2, 0x19, 0xd8, 0xe5, 0x7c, 0x2f, 0x83, 0xda,
				0xf7, 0x6b, 0x90, 0xfe, 0xc4, 0x01, 0x5a, 0x97,  0x61, 0xa6, 0x3d, 0x40, 0x0b, 0x58, 0xe6, 0x3d,
			}, {
				0x4d, 0xd1, 0xb2, 0x0f, 0x28, 0xbd, 0xe4, 0x78,  0xf6, 0x4a, 0x0f, 0x93, 0x8b, 0x17, 0xd1, 0xa4,
				0x3a, 0xec, 0xc9, 0x35, 0x93, 0x56, 0x7e, 0xcb,  0x55, 0x20, 0xa0, 0xfe, 0x6c, 0x89, 0x17, 0x62,
				0x17, 0x62, 0x4b, 0xb1, 0xb4, 0xde, 0xd1, 0x87,  0xc9, 0x14, 0x3c, 0x4a, 0x7e, 0xa8, 0xe2, 0x7d,
				0xa0, 0x9f, 0xf6, 0x5c, 0x6a, 0x09, 0x8d, 0xf0,  0x0f, 0xe3, 0x53, 0x25, 0x95, 0x36, 0x28, 0xcb,
			}
		};

        #endregion

        #region private functions
        private static void NibbleSwap(int[] Src, int index, int len)
        {

            for (; 0 < len; len--, index++)
                Src[index] = ((int)Src[index] >> 4) | ((int)Src[index] << 4);
        }

        private static void BitConvert(int[] Src, int index, int[] BitSwapTable)
        {
            int lop, prm;
            int[] tmp = new int[8];

            for (lop = 0; lop != 64; lop++)
            {
                prm = BitSwapTable[lop] - 1;
                if ((Src[index + (prm >> 3) & 7] & BitMaskTable[prm & 7]) != 0)
                {
                    tmp[(lop >> 3) & 7] |= BitMaskTable[lop & 7];
                }
            }

            Array.Copy(tmp, 0, Src, index, 8);
        }

        private static void BitConvert4(int[] Src, int index)
        {
            int lop, prm;
            int[] tmp = new int[8];
            tmp[0] = ((Src[index + 7] << 5) | (Src[index + 4] >> 3)) & 0x3f;	// ..0 vutsr
            tmp[1] = ((Src[index + 4] << 1) | (Src[index + 5] >> 7)) & 0x3f;	// ..srqpo n
            tmp[2] = ((Src[index + 4] << 5) | (Src[index + 5] >> 3)) & 0x3f;	// ..o nmlkj
            tmp[3] = ((Src[index + 5] << 1) | (Src[index + 6] >> 7)) & 0x3f;	// ..kjihg f
            tmp[4] = ((Src[index + 5] << 5) | (Src[index + 6] >> 3)) & 0x3f;	// ..g fedcb
            tmp[5] = ((Src[index + 6] << 1) | (Src[index + 7] >> 7)) & 0x3f;	// ..cba98 7
            tmp[6] = ((Src[index + 6] << 5) | (Src[index + 7] >> 3)) & 0x3f;	// ..8 76543
            tmp[7] = ((Src[index + 7] << 1) | (Src[index + 4] >> 7)) & 0x3f;	// ..43210 v

            for (lop = 0; lop != 4; lop++)
            {
                tmp[lop] = (NibbleData[lop, tmp[lop * 2]] & 0xf0)
                | (NibbleData[lop, tmp[lop * 2 + 1]] & 0x0f);
            }

            Array.Copy(new int[] { 0, 0, 0, 0 }, 0, tmp, 4, 4);

            for (lop = 0; lop != 32; lop++)
            {
                prm = BitSwapTable3[lop] - 1;
                if ((tmp[prm >> 3] & BitMaskTable[prm & 7]) != 0)
                {
                    tmp[(lop >> 3) + 4] |= BitMaskTable[lop & 7];
                }
            }
            Src[index + 0] ^= tmp[4];
            Src[index + 1] ^= tmp[5];
            Src[index + 2] ^= tmp[6];
            Src[index + 3] ^= tmp[7];

        }

        public static byte[] Decode(byte[] data, int len, int cycle)
        {

            int[] buf = Array.ConvertAll(data, i => (int)i);

            int lop, cnt = 0;
            int type = (cycle == 0 ? 1 : 0);

            int bufIdx = 0;

            if (cycle < 3) cycle = 3;
            else if (cycle < 5) cycle++;
            else if (cycle < 7) cycle += 9;
            else cycle += 15;

            for (lop = 0; lop * 8 < len; lop++, bufIdx += 8)
            {
                if (lop < 20 || (type == 0 && lop % cycle == 0))
                { // des
                    BitConvert(buf, bufIdx, BitSwapTable1);
                    BitConvert4(buf, bufIdx);
                    BitConvert(buf, bufIdx, BitSwapTable2);
                }
                else
                {
                    if (cnt == 7 && type == 0)
                    {
                        int a;
                        int[] tmp = new int[8];
                        Array.Copy(buf, bufIdx, tmp, 0, 8);     
                        cnt = 0;
                        buf[bufIdx + 0] = tmp[3];
                        buf[bufIdx + 1] = tmp[4];
                        buf[bufIdx + 2] = tmp[6];
                        buf[bufIdx + 3] = tmp[0];
                        buf[bufIdx + 4] = tmp[1];
                        buf[bufIdx + 5] = tmp[2];
                        buf[bufIdx + 6] = tmp[5];
                        a = tmp[7];
                        if (a == 0x00) a = 0x2b;
                        else if (a == 0x2b) a = 0x00;
                        else if (a == 0x01) a = 0x68;
                        else if (a == 0x68) a = 0x01;
                        else if (a == 0x48) a = 0x77;
                        else if (a == 0x77) a = 0x48;
                        else if (a == 0x60) a = 0xff;
                        else if (a == 0xff) a = 0x60;
                        else if (a == 0x6c) a = 0x80;
                        else if (a == 0x80) a = 0x6c;
                        else if (a == 0xb9) a = 0xc0;
                        else if (a == 0xc0) a = 0xb9;
                        else if (a == 0xeb) a = 0xfe;
                        else if (a == 0xfe) a = 0xeb;
                        buf[bufIdx + 7] = a;
                    }
                    cnt++;
                }
            }

            return Array.ConvertAll(buf, i => (byte)i);
        }
        #endregion
    }
}

