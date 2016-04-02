using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using static System.Math;

namespace Encoder {
    class Decoder {
        public Bitmap bm { get; private set; } // bitmap we are working on
        private int width, height, index = 0;  // dimension of the bitmap
        private int[,] Y, Cb, Cr;              // chroma channels
        private List<int> decoded;

        // luma quantization table
        protected readonly int[,] luminance = {
            { 16, 11, 10, 16, 24, 40, 51, 61 },
            { 12, 12, 14, 19, 26, 58, 60, 55 },
            { 14, 13, 16, 24, 40, 57, 69, 56 },
            { 14, 17, 22, 29, 51, 87, 80, 62 },
            { 18, 22, 37, 56, 68, 109, 103, 77 },
            { 24, 35, 55, 64, 81, 104, 113, 92 },
            { 49, 64, 78, 87, 103, 121, 120, 101 },
            { 72, 92, 95, 98, 112, 100, 103, 99 }};

        // chroma quantization table
        protected readonly int[,] chrominance = {
            { 17, 18, 24, 27, 47, 99, 99, 99 },
            { 18, 21, 26, 66, 99, 99, 99, 99 },
            { 24, 26, 56, 99, 99, 99, 99, 99 },
            { 47, 66, 99, 99, 99, 99, 99, 99 },
            { 99, 99, 99, 99, 99, 99, 99, 99 },
            { 99, 99, 99, 99, 99, 99, 99, 99 },
            { 99, 99, 99, 99, 99, 99, 99, 99 },
            { 99, 99, 99, 99, 99, 99, 99, 99 }};

        public void Start() {
            byte[] bytes = File.ReadAllBytes("saved");

            width = BitConverter.ToInt32(bytes, 0);
            height = BitConverter.ToInt32(bytes, 4);
            bm = new Bitmap(width, height);

            int dictLen = BitConverter.ToInt32(bytes, 8);
            byte[] temp = new byte[bytes.Length - 12];
            Array.Copy(bytes, 12, temp, 0, bytes.Length - 12);
            bytes = temp;

            int width2 = (int)Ceiling(width / 2.0), height2 = (int)Ceiling(height / 2.0);
            Y = new int[width, height];
            Cb = new int[width, height];
            Cr = new int[width, height];

            int YLen = (int)(Ceiling(width / 8.0) * Ceiling(height / 8.0) * 64);
            int CbLen = (int)(Ceiling(width / 16.0) * Ceiling(height / 16.0) * 64);
            int CrLen = (int)(Ceiling(width / 16.0) * Ceiling(height / 16.0) * 64);

            // huffman frequency table the list is symbol and  
            var frequencies = new Dictionary<List<bool>, int>(new BoolListComparer());
            
            List<bool> symbol;
            for (int i = 0; i < dictLen; i++) {
                int symlen = bytes[i++];               // length of next symbol in bits
                int len = (int) Ceiling(symlen / 8.0); // length of next symbol in bytes

                temp = new byte[len];
                for (int j = 1; j <= len; j++)
                    temp[j - 1] = bytes[i++];

                symbol = ReadSymbol(temp, symlen);
                int value = (bytes[i] > 127) ? bytes[i] - 256 : bytes[i];

                frequencies.Add(symbol, value);
            }

            temp = new byte[bytes.Length - dictLen];
            Array.Copy(bytes, dictLen, temp, 0, bytes.Length - dictLen);
            bytes = temp;
            BitArray encoded = new BitArray(bytes);

            // flip every 8 bits
            for (int i = 0; i < encoded.Length / 8; i++) {
                bool[] flip = new bool[8];

                for (int j = 0; j < 8; j++)
                    flip[j] = encoded[i * 8 + j];

                for (int j = 7; j >= 0; j--)
                    encoded[i * 8 + 7 - j] = flip[j];
            }

            int val;
            symbol = new List<bool>();
            decoded = new List<int>();
            for (int i = 0; i < encoded.Length; i++) {
                symbol.Add(encoded[i]);

                if (frequencies.ContainsKey(symbol)) {
                    val = 0;
                    frequencies.TryGetValue(symbol, out val);

                    decoded.Add(val);
                    symbol = new List<bool>();
                }
            }    
                 
            DecodeChannel(Y, YLen, width, height, luminance);
            DecodeChannel(Cb, CbLen, width2, height2, chrominance);
            DecodeChannel(Cr, CrLen, width2, height2, chrominance);

            SuperSample();
            UpdateBitmap();
        }
        
        private List<bool> ReadSymbol(byte[] bits, int len) {
            List<bool> result = new List<bool>(len);
            byte[] temp = new byte[bits.Length];
            for (int i = bits.Length - 1; i >= 0; i--)
                temp[bits.Length - 1 - i] = bits[i];

            int off = (len < 8) ? 8 - len: 0;
            BitArray cur = new BitArray(temp);
            for (int i = 0; i < len; i++)
                result.Add(cur[cur.Count - 1 - off - i]);

            return result;
        }

        private void UpdateBitmap() {
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    int luma = Y[x, y];
                    int cb = Cb[x, y];
                    int cr = Cr[x, y];

                    int r = (int)(1.164 * (luma - 16) + 1.596 * (cr - 128));
                    int g = (int)(1.164 * (luma - 16) - 0.813 * (cr - 128) - 0.392 * (cb - 128));
                    int b = (int)(1.164 * (luma - 16) + 2.017 * (cb - 128));

                    r = Max(0, Min(255, r));
                    g = Max(0, Min(255, g));
                    b = Max(0, Min(255, b));

                    bm.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }
        }

        private void SuperSample() {
            int[,] temp1 = new int[width, height];
            int[,] temp2 = new int[width, height];
            int xin = 0, yin = 0;

            for (int x = 0; x < width; x += 2) {
                for (int y = 0; y < height; y += 2) {
                    temp1[x, y] = Cr[xin, yin];
                    temp2[x, y] = Cb[xin, yin];

                    if (x < width - 1) {
                        temp1[x + 1, y] = Cr[xin, yin];
                        temp2[x + 1, y] = Cb[xin, yin];
                    }

                    if (y < height - 1) {
                        temp1[x, y + 1] = Cr[xin, yin];
                        temp2[x, y + 1] = Cb[xin, yin];
                    }

                    if (x < width - 1 && y < height - 1) {
                        temp1[x + 1, y + 1] = Cr[xin, yin];
                        temp2[x + 1, y + 1] = Cb[xin, yin++];
                    }
                }

                xin++;
                yin = 0;
            }

            Cb = temp2;
            Cr = temp1;
        }

        private void DecodeChannel(int[,] channel, int len, int w, int h, int[,] table) {
            List<int> list = new List<int>();
            
            while (list.Count < len) {
                if (decoded[index] == 127) {
                    for (int x = 0; x < decoded[index + 1]; x++)
                        list.Add(decoded[index + 2]);
                    index += 2;
                } else
                    list.Add(decoded[index]);

                index++;
            }
            
            RebuildChannel(list, channel, w, h, table);
        }

        private void RebuildChannel(List<int> channel, int[,] dst, int width, int height, int[,] table) {
            int[] temp = new int[64], src = channel.ToArray();
            int startx = 0, starty = 0, m = 0, n = 0;

            for (int i = 0; i < channel.Count; i += 64) {
                Array.Copy(src, i, temp, 0, 64);
                int[,] block = IQuantize(temp, table);
                block = IDCT(block);
                for (int x = 0; x < 8; x++)
                    for (int y = 0; y < 8; y++)
                        block[x, y] += 128;

                for (int x = startx; x < startx + 8; x++) {
                    for (int y = starty; y < starty + 8; y++)
                        if (x < width && y < height)
                            dst[x, y] = block[m, n++];

                    n = 0;
                    m++;
                }

                m = 0;
                n = 0;

                starty += 8;
                if (starty >= height) {
                    startx += 8;
                    starty = 0;
                }
            }
        }

        public static int[,] IQuantize(int[] src, int[,] table) {
            int i = 0, j = 0, ind = 0, n = 8;
            int start = 0, end = n * n - 1, d = -1;
            int[,] result = new int[8, 8];

            do {
                result[i, j] = (int)Round((double)(src[ind] * table[i, j]));
                result[n-i-1, n-j-1] = (int)Round((double)(src[63-ind] * table[n-i-1, n-j-1]));
                ind++;

                start++;
                end--;

                i += d;
                j -= d;

                if (i < 0) {
                    i++;
                    d = -d; // top reached, reverse
                } else if (j < 0) {
                    j++;
                    d = -d; // left reached, reverse
                }
            } while (start < end);

            return result;
        }

        private int[,] IDCT(int[,] F) {
            int[,] f = new int[8, 8];

            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    double val = 0;
                    for (int u = 0; u < 8; u++) {
                        double cu = u == 0 ? Sqrt(2) / 2 : 1;
                        for (int v = 0; v < 8; v++) {
                            double cv = v == 0 ? Sqrt(2) / 2 : 1;
                            
                            var v1 = Cos(((2 * i + 1) * u * PI) / 16);
                            var v2 = Cos(((2 * j + 1) * v * PI) / 16);

                            val += v1 * v2 * F[u, v] * cu * cv / 4;
                        }
                    }

                   f[i, j] = val > 0 ? (int)Round(val) : (int)Floor(val);
                }
            }

            return f;
        }

        public class BoolListComparer : IEqualityComparer<List<bool>> {
            public bool Equals(List<bool> left, List<bool> right) {
                if (ReferenceEquals(left, right)) {
                    return true;
                }

                if ((left == null) || (right == null)) {
                    return false;
                }

                return left.SequenceEqual(right);
            }

            public int GetHashCode(List<bool> obj) {
                return obj.Aggregate(17, (res, item) => unchecked(res * 23 + item.GetHashCode()));
            }
        }
    }
}
