using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using static System.Math;

namespace Encoder {
    class Encoder {
        public Bitmap bitmap { get; private set; }  // bitmap we are working on
        private int[,] Y, Cb, Cr;  // chroma channels
        private int width, height; // dimension of the bitmap

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

        /// <summary>
        /// begin image compression, Saved as "saved"
        /// in the same directory as the executable
        /// </summary>
        public void Start(Bitmap bm) {
            if (bm == null)
                return;

            this.bitmap = bm;
            width = bm.Width;
            height = bm.Height;

            // RGB to YCrCb
            ChangeColourSpace();
            // 4:2:0 scheme
            SubSample();

            // generate the blocks for each channel
            var l1 = GenerateBlocks(Y, width, height, luminance);
            var l2 = GenerateBlocks(Cb, (int) Ceiling(width / 2.0), (int) Ceiling(height / 2.0), chrominance);
            var l3 = GenerateBlocks(Cr, (int) Ceiling(width / 2.0), (int) Ceiling(height / 2.0), chrominance);

            // predetermined file where the compressed image is saved
            FileStream stream = new FileStream("saved", FileMode.Create);

            // write the width, height, and length of each channel to a file
            stream.Write(BitConverter.GetBytes(width), 0, 4);
            stream.Write(BitConverter.GetBytes(height), 0, 4);

            l1.AddRange(l2);
            l1.AddRange(l3);

            HuffmanTree tree = new HuffmanTree();
            tree.Build(l1);
            stream.Write(BitConverter.GetBytes(0), 0, 4); // save space for dictionary count

            int count = 0;
            byte[] symbol;
            List<bool> list;
            foreach (KeyValuePair<int, int> pair in tree.Frequencies) {
                list = tree.Root.Traverse(pair.Key, new List<bool>());
                symbol = ToBytes(list.ToArray());
                stream.WriteByte((byte) list.Count);
                stream.Write(symbol, 0, symbol.Length);
                count += symbol.Length + 1;

                stream.WriteByte((byte) pair.Key);
                count++;
            }

            int pos = (int) stream.Position;
            stream.Seek(8, SeekOrigin.Begin);
            stream.Write(BitConverter.GetBytes(count), 0, 4); // overwrite saved area

            BitArray encoded = tree.Encode(l1);
            bool[] bits = new bool[encoded.Length];
            encoded.CopyTo(bits, 0);
            byte[] bytes = ToBytes(bits);

            stream.Seek(pos, SeekOrigin.Begin);
            stream.Write(bytes, 0, bytes.Length);

            stream.Dispose();
        }

        private byte[] ToBytes(bool[] bits) {
            List<byte> list = new List<byte>();
            byte b = 0;

            for (int i = 0; i < bits.Length; i++) {
                if (i % 8 == 0 && i > 0) {
                    list.Add(b);
                    b = 0;
                }

                b <<= 1;
                b |= (byte) ((bits[i]) ? 1 : 0);
            }

            if (bits.Length > 8 && bits.Length % 8 != 0)
                b <<= 8 - bits.Length % 8;
            list.Add(b);

            byte[] result = new byte[list.Count];
            for (int i = 0; i < list.Count; i++)
                result[i] = list[i];

            return result;
        }

        /// <summary>
        /// change color space from rgb to YCrCb
        /// </summary>
        protected void ChangeColourSpace() {
            Y = new int[width, height];
            Cb = new int[width, height];
            Cr = new int[width, height];

            BitmapData bmdata = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            byte[] data = new byte[bmdata.Height * bmdata.Stride]; // stride is number of bytes per pixel
            Marshal.Copy(bmdata.Scan0, data, 0, bmdata.Height * bmdata.Stride); // convert intptr to byte[]

            // Convert to YCbCr
            for (int y = 0; y < bmdata.Height; y++) {
                for (int x = 0; x < width; x++) {
                    byte[] curr = new byte[3];
                    Array.Copy(data, y * bmdata.Stride + x * 3, curr, 0, 3);

                    float b = curr[0];
                    float g = curr[1];
                    float r = curr[2];

                    Y[x, y] = (int)(16 + (0.257 * r) + (0.504 * g) + (0.0988 * b));
                    Cb[x, y] = (int)(128 - (0.148 * r) - (0.2916 * g) + (0.4398 * b));
                    Cr[x, y] = (int)(128 + (0.439 * r) - (0.368 * g) - (0.0718 * b));
                }
            }

            bitmap.UnlockBits(bmdata);
        }

        /// <summary>
        /// Reduce the chroma channels (cb and cr)
        /// by the 4:2:0 scheme.
        /// Only keep every second pixel in each
        /// row and each column
        /// </summary>
        private void SubSample() {
            int[,] temp1 = new int[(int)Ceiling(width / 2.0), (int)Ceiling((height / 2.0))];
            int[,] temp2 = new int[(int)Ceiling(width / 2.0), (int)Ceiling((height / 2.0))];

            for (int u = 0, uin = 0; u < width; u += 2, uin++) {
                for (int v = 0, vin = 0; v < height; v += 2, vin++) {
                    temp1[uin, vin] = Cr[u, v];
                    temp2[uin, vin] = Cb[u, v];
                }
            }

            Cb = temp2;
            Cr = temp1;
        }

        /// <summary>
        /// generates encoded values for a channel
        /// </summary>
        /// <param name="src">source channel of the blocks</param>
        /// <param name="w">width of this channel</param>
        /// <param name="h">height of this channel</param>
        /// <param name="table">quantization table, chrominance or luminance</param>
        /// <returns>List of encoded values</returns>
        private List<int> GenerateBlocks(int[,] src, int w, int h, int[,] table) {
            List<int> result = new List<int>();
            int[,] block = new int[8, 8];
            int val = 0;

            for (int u = 0; u < w; u += 8) {
                for (int v = 0; v < h; v += 8) {
                    for (int i = u; i < u + 8; i++) {
                        for (int j = v; j < v + 8; j++) {
                            val = 0;

                            if (i < w && j < h)
                                val = src[i, j];

                            block[i - u, j - v] = val;
                        }
                    }

                    for (int x = 0; x < 8; x++)
                        for (int y = 0; y < 8; y++)
                            block[x, y] -= 128;

                    block = DCT(block);
                    var quantized = Quantize(block, table);
                    var encoded = MRLE(quantized);
                    for (int i = 0; i < encoded.Length; i++)
                        result.Add(encoded[i]);
                }
            }

            return result;
        }

        /// <summary>
        /// Discrete Cosine Transform, finds the
        /// frequency of changes in a block
        /// </summary>
        /// <param name="f">source block</param>
        /// <returns>
        /// resulting block containing frequencies of 
        /// changes in the source block
        /// </returns>
        private int[,] DCT(int[,] f) {
            int M = f.GetLength(0), N = f.GetLength(1);
            int[,] F = new int[M, N];
            double Cu, Cv;

            for (int u = 0; u < M; u++) {
                Cu = (u == 0) ? Sqrt(2) / 2 : 1;

                for (int v = 0; v < N; v++) {
                    Cv = (v == 0) ? Sqrt(2) / 2 : 1;
                    double val = 0;

                    for (int i = 0; i < M; i++) {
                        for (int j = 0; j < N; j++) {
                            var cos1 = Cos(((2 * i + 1) * u * PI) / (2 * M));
                            var cos2 = Cos(((2 * j + 1) * v * PI) / (2 * N));
                            val += cos1 * cos2 * f[i, j];
                        }
                    }

                    F[u, v] = (int) (val * Cu * Cv * 2 / Sqrt(M * N));
                }
            }

            return F;
        }
        
        /// <summary>
        /// Reduces less important information in a block
        /// by dividing each value in the block by it's corresponding
        /// value in the quantization table in a zig-zag pattern
        /// </summary>
        /// <param name="matrix">source matrix</param>
        /// <param name="table">quantization table</param>
        /// <returns>1D array of quantized values</returns>
        private int[] Quantize(int[,] matrix, int[,] table) {
            int i = 0, j = 0, n = matrix.GetLength(0);
            int d = -1; // -1 for top-right move, +1 for bottom-left move
            int start = 0, end = n * n - 1, ind = 0;
            int[] result = new int[matrix.Length];

            do {
                result[ind] = (int) Round((double) (matrix[i, j] / table[i, j]));
                result[matrix.Length-ind-1] = (int) Round((double) (matrix[n-i-1, n-j-1] / table[n-i-1, n-j-1]));

                start++;
                end--;
                ind++;

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

        /// <summary>
        /// Modified Run Length Encodes an array using
        /// 127 as a special symbol for runs. the value
        /// 127 is encoded as a run of 1 127s
        /// </summary>
        /// <param name="src">1D array of quantized values</param>
        /// <returns>Run Length Encoded values</returns>
        private int[] MRLE(int[] src) {
            int  n = src.Length, end = n * n - 1, symbol = 127;
            int[] result;
            List<int> list = new List<int>();
            
            int c = 1, cur = src[0];
            for (int k = 1; k < src.Length; k++) {
                if (cur == src[k]) {
                    c++;
                } else {
                    if (c > 1 || cur == symbol) { // found our run symbol or a run occured
                        list.Add(symbol);
                        list.Add(c);
                        list.Add(cur);
                    } else { // no run, add single number
                        list.Add(cur);
                    }

                    c = 1;
                    cur = src[k];
                }
            }

            // repeat adding logic one last time
            if (c > 1 || cur == symbol) {
                list.Add(symbol);
                list.Add(c);
                list.Add(cur);
            } else {
                list.Add(cur);
            }

            // change our variable length list into a static array
            result = new int[list.Count];
            for (int k = 0; k < list.Count; k++)
                result[k] = list[k];

            return result;
        }

        public void StartVectors() {
            Bitmap source = new Bitmap("frame1.jpg");
            bitmap = new Bitmap("frame2.jpg");

            width = bitmap.Width;
            height = bitmap.Height;
            Bitmap result = (Bitmap)bitmap.Clone();
            
            for (int i = 0; i < (int)Ceiling(width / 8.0); i++) {
                for (int j = 0; j < (int)Ceiling(height / 8.0); j++) {
                    int[,] target = new int[8, 8], reference;
                    int refx = (i * 8) + 4, refy = (j * 8) + 4;

                    var graphics = Graphics.FromImage(result);
                    graphics.DrawRectangle(Pens.Black, refx - 1, refy - 1, 3, 3);

                    for (int x = -4; x < 3; x++) {
                        for (int y = -4; y < 3; y++) {
                            int reachx = (refx + x < width && refx + x > 0) ? refx + x : 0;
                            int reachy = (refy + y < height && refy + y > 0) ? refy + y : 0;
                            target[x + 4, y + 4] = bitmap.GetPixel(reachx, reachy).ToArgb();
                        }
                    }

                    int match = int.MaxValue, avg = 0, mvx = 0, mvy = 0, p = 12;
                    for (int x = -p; x < p + 1 && refx + x < width; x++) {
                        if (refx + x < 0) x += p - refx;
                        for (int y = -p; y < p + 1 && refy + y < height; y++) {
                            if (refy + y < 0) y += p - refy;
                            reference = new int[8, 8];

                            for (int u = -4; u < 3; u++) {
                                for (int v = -4; v < 3; v++) {
                                    int reachx = (refx + x < width && refx + x > 0) ? refx + x : 0;
                                    int reachy = (refy + y < height && refy + y > 0) ? refy + y : 0;
                                    if (refx + x + u < width && refx + x + u > 0) reachx = refx + x + u;
                                    if (refy + y + v < height && refy + y + v > 0) reachy = refy + y + v;
                                    reference[u + 4, v + 4] = source.GetPixel(reachx, reachy).ToArgb();
                                }
                            }

                            for (int u = 0; u < 8; u++)
                                for (int v = 0; v < 8; v++)
                                    avg += Abs(target[u, v] - reference[u, v]);

                            avg = avg / 64;
                            if (avg < match) {
                                match = avg;
                                mvx = x;
                                mvy = y;
                            }
                        }

                    }

                    reference = new int[8, 8];
                    for (int u = -4; u < 3; u++) {
                        for (int v = -4; v < 3; v++) {
                            int reachx = (refx + u < width && refx + u > 0) ? refx + u : 0;
                            int reachy = (refy + v < height && refy + v > 0) ? refy + v : 0;
                            reference[u + 4, v + 4] = source.GetPixel(reachx, reachy).ToArgb();
                        }
                    }

                    avg = 0;
                    for (int u = 0; u < 8; u++)
                        for (int v = 0; v < 8; v++)
                            avg += Abs(target[u, v] - reference[u, v]);

                    avg = avg / 64;
                    if (avg <= match) {
                        match = avg;
                        mvx = 0;
                        mvy = 0;
                    }

                    graphics.DrawLine(Pens.Red, refx, refy, refx + mvx, refy + mvy);
                }
            }

            bitmap = result;
        }
    }
}
