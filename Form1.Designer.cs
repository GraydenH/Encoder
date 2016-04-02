namespace Encoder {
    partial class Form1 {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.pic1 = new System.Windows.Forms.PictureBox();
            this.compressbutton = new System.Windows.Forms.Button();
            this.pic2 = new System.Windows.Forms.PictureBox();
            this.decompressbutton = new System.Windows.Forms.Button();
            this.table = new System.Windows.Forms.TableLayoutPanel();
            this.generateVectorsButton = new System.Windows.Forms.Button();
            this.decodeVectorsButton = new System.Windows.Forms.Button();
            this.compressionWorker = new System.ComponentModel.BackgroundWorker();
            this.decompressionWorker = new System.ComponentModel.BackgroundWorker();
            ((System.ComponentModel.ISupportInitialize)(this.pic1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pic2)).BeginInit();
            this.table.SuspendLayout();
            this.SuspendLayout();
            // 
            // pic1
            // 
            this.pic1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pic1.Location = new System.Drawing.Point(3, 3);
            this.pic1.Name = "pic1";
            this.pic1.Size = new System.Drawing.Size(110, 34);
            this.pic1.TabIndex = 0;
            this.pic1.TabStop = false;
            // 
            // compressbutton
            // 
            this.compressbutton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.compressbutton.Location = new System.Drawing.Point(3, 43);
            this.compressbutton.Name = "compressbutton";
            this.compressbutton.Size = new System.Drawing.Size(75, 20);
            this.compressbutton.TabIndex = 2;
            this.compressbutton.Text = "Compress";
            this.compressbutton.UseVisualStyleBackColor = true;
            this.compressbutton.Click += new System.EventHandler(this.Compress);
            // 
            // pic2
            // 
            this.pic2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pic2.Location = new System.Drawing.Point(129, 3);
            this.pic2.Name = "pic2";
            this.pic2.Size = new System.Drawing.Size(110, 34);
            this.pic2.TabIndex = 3;
            this.pic2.TabStop = false;
            // 
            // decompressbutton
            // 
            this.decompressbutton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.decompressbutton.Location = new System.Drawing.Point(164, 43);
            this.decompressbutton.Name = "decompressbutton";
            this.decompressbutton.Size = new System.Drawing.Size(75, 20);
            this.decompressbutton.TabIndex = 4;
            this.decompressbutton.Text = "Decompress";
            this.decompressbutton.UseVisualStyleBackColor = true;
            this.decompressbutton.Click += new System.EventHandler(this.Decompress);
            // 
            // table
            // 
            this.table.ColumnCount = 3;
            this.table.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.00001F));
            this.table.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.table.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 49.99999F));
            this.table.Controls.Add(this.compressbutton, 0, 1);
            this.table.Controls.Add(this.pic1, 0, 0);
            this.table.Controls.Add(this.pic2, 2, 0);
            this.table.Controls.Add(this.decompressbutton, 2, 1);
            this.table.Controls.Add(this.generateVectorsButton, 0, 2);
            this.table.Controls.Add(this.decodeVectorsButton, 2, 2);
            this.table.Dock = System.Windows.Forms.DockStyle.Fill;
            this.table.Location = new System.Drawing.Point(0, 0);
            this.table.Name = "table";
            this.table.RowCount = 3;
            this.table.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.table.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.table.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.table.Size = new System.Drawing.Size(242, 92);
            this.table.TabIndex = 5;
            // 
            // generateVectorsButton
            // 
            this.generateVectorsButton.Location = new System.Drawing.Point(3, 69);
            this.generateVectorsButton.Name = "generateVectorsButton";
            this.generateVectorsButton.Size = new System.Drawing.Size(110, 20);
            this.generateVectorsButton.TabIndex = 5;
            this.generateVectorsButton.Text = "Generate Vectors";
            this.generateVectorsButton.UseVisualStyleBackColor = true;
            this.generateVectorsButton.Click += new System.EventHandler(this.GenerateVectors);
            // 
            // decodeVectorsButton
            // 
            this.decodeVectorsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.decodeVectorsButton.Location = new System.Drawing.Point(129, 69);
            this.decodeVectorsButton.Name = "decodeVectorsButton";
            this.decodeVectorsButton.Size = new System.Drawing.Size(110, 20);
            this.decodeVectorsButton.TabIndex = 6;
            this.decodeVectorsButton.Text = "Decode Vectors";
            this.decodeVectorsButton.UseVisualStyleBackColor = true;
            this.decodeVectorsButton.Click += new System.EventHandler(this.DecodeVectors);
            // 
            // compressionWorker
            // 
            this.compressionWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.DoCompressionWork);
            this.compressionWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.CompressionWorkCompleted);
            // 
            // decompressionWorker
            // 
            this.decompressionWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.DoDecompressionWork);
            this.decompressionWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.DecompressionWorkCompleted);
            // 
            // EncoderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(242, 92);
            this.Controls.Add(this.table);
            this.Name = "EncoderForm";
            this.Text = "A2";
            ((System.ComponentModel.ISupportInitialize)(this.pic1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pic2)).EndInit();
            this.table.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pic1;
        private System.Windows.Forms.Button compressbutton;
        private System.Windows.Forms.PictureBox pic2;
        private System.Windows.Forms.Button decompressbutton;
        private System.Windows.Forms.TableLayoutPanel table;
        private System.ComponentModel.BackgroundWorker compressionWorker;
        private System.ComponentModel.BackgroundWorker decompressionWorker;
        private System.Windows.Forms.Button generateVectorsButton;
        private System.Windows.Forms.Button decodeVectorsButton;
    }
}

