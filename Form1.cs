using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Encoder {
    public partial class Form1 : Form {
        Encoder encoder;
        Decoder decoder;
        Bitmap bitmap;
        bool busy = false;

        public Form1() {
            InitializeComponent();
        }

        private void Compress(object sender, EventArgs e) {
            if (busy)
                return;

            OpenFileDialog fd = new OpenFileDialog();

            if (fd.ShowDialog() != DialogResult.OK)
                return;

            bitmap = new Bitmap(fd.FileName);
            encoder = new Encoder();

            if (!compressionWorker.IsBusy)
                compressionWorker.RunWorkerAsync();
        }
        
        private void Decompress(object sender, EventArgs e) {
            decoder = new Decoder();

            if (!decompressionWorker.IsBusy && !busy)
                decompressionWorker.RunWorkerAsync();
        }
        
        new private void Resize(int wide, int high) {
            table.RowStyles.Clear();
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, high));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));

            Width =  2 * wide;
            Height = high + 96;

            Update();
        }

        private void DoCompressionWork(object sender, DoWorkEventArgs e) {
            if (bitmap == null)
                return;

            busy = true;
            encoder.Start(bitmap);
        }

        private void CompressionWorkCompleted(object sender, RunWorkerCompletedEventArgs e) {
            Resize(encoder.bitmap.Width, encoder.bitmap.Height);
            pic1.Image = encoder.bitmap;
            busy = false;
        }

        private void DoDecompressionWork(object sender, DoWorkEventArgs e) {
            busy = true;
            decoder.Start();
        }
        private void DecompressionWorkCompleted(object sender, RunWorkerCompletedEventArgs e) {
            Resize(decoder.bm.Width, decoder.bm.Height);
            pic2.Image = decoder.bm;
            busy = false;
        }

        private void GenerateVectors(object sender, EventArgs e) {
            encoder = new Encoder();
            encoder.StartVectors();
            pic1.Image = encoder.bitmap;
            pic1.Invalidate();
        }

        private void DecodeVectors(object sender, EventArgs e) {

        }
    }
}
