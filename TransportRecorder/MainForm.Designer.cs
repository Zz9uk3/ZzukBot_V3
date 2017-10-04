namespace TransportRecorder
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.tbRelative1 = new System.Windows.Forms.TextBox();
            this.tbRest = new System.Windows.Forms.TextBox();
            this.tbWait1 = new System.Windows.Forms.TextBox();
            this.tbArrived = new System.Windows.Forms.TextBox();
            this.tbEnd1 = new System.Windows.Forms.TextBox();
            this.bWait1 = new System.Windows.Forms.Button();
            this.bRest = new System.Windows.Forms.Button();
            this.bRelative1 = new System.Windows.Forms.Button();
            this.bArrived = new System.Windows.Forms.Button();
            this.bEnd1 = new System.Windows.Forms.Button();
            this.bSave = new System.Windows.Forms.Button();
            this.tbFileName = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Wait1";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 95);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(72, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "OnTransport1";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 56);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(74, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "TransportRest";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 173);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(32, 13);
            this.label7.TabIndex = 5;
            this.label7.Text = "End1";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(12, 134);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(85, 13);
            this.label8.TabIndex = 4;
            this.label8.Text = "TransportArrived";
            // 
            // tbRelative1
            // 
            this.tbRelative1.Location = new System.Drawing.Point(119, 92);
            this.tbRelative1.Name = "tbRelative1";
            this.tbRelative1.Size = new System.Drawing.Size(100, 20);
            this.tbRelative1.TabIndex = 7;
            // 
            // tbRest
            // 
            this.tbRest.Location = new System.Drawing.Point(119, 53);
            this.tbRest.Name = "tbRest";
            this.tbRest.Size = new System.Drawing.Size(100, 20);
            this.tbRest.TabIndex = 8;
            // 
            // tbWait1
            // 
            this.tbWait1.Location = new System.Drawing.Point(119, 18);
            this.tbWait1.Name = "tbWait1";
            this.tbWait1.Size = new System.Drawing.Size(100, 20);
            this.tbWait1.TabIndex = 10;
            // 
            // tbArrived
            // 
            this.tbArrived.Location = new System.Drawing.Point(119, 131);
            this.tbArrived.Name = "tbArrived";
            this.tbArrived.Size = new System.Drawing.Size(100, 20);
            this.tbArrived.TabIndex = 11;
            // 
            // tbEnd1
            // 
            this.tbEnd1.Location = new System.Drawing.Point(119, 170);
            this.tbEnd1.Name = "tbEnd1";
            this.tbEnd1.Size = new System.Drawing.Size(100, 20);
            this.tbEnd1.TabIndex = 12;
            // 
            // bWait1
            // 
            this.bWait1.Location = new System.Drawing.Point(241, 18);
            this.bWait1.Name = "bWait1";
            this.bWait1.Size = new System.Drawing.Size(99, 20);
            this.bWait1.TabIndex = 14;
            this.bWait1.Text = "button1";
            this.bWait1.UseVisualStyleBackColor = true;
            this.bWait1.Click += new System.EventHandler(this.bWait1_Click);
            // 
            // bRest
            // 
            this.bRest.Location = new System.Drawing.Point(241, 53);
            this.bRest.Name = "bRest";
            this.bRest.Size = new System.Drawing.Size(99, 20);
            this.bRest.TabIndex = 16;
            this.bRest.Text = "button3";
            this.bRest.UseVisualStyleBackColor = true;
            this.bRest.Click += new System.EventHandler(this.bRest_Click);
            // 
            // bRelative1
            // 
            this.bRelative1.Location = new System.Drawing.Point(241, 91);
            this.bRelative1.Name = "bRelative1";
            this.bRelative1.Size = new System.Drawing.Size(99, 20);
            this.bRelative1.TabIndex = 17;
            this.bRelative1.Text = "button4";
            this.bRelative1.UseVisualStyleBackColor = true;
            this.bRelative1.Click += new System.EventHandler(this.bRelative_Click);
            // 
            // bArrived
            // 
            this.bArrived.Location = new System.Drawing.Point(241, 130);
            this.bArrived.Name = "bArrived";
            this.bArrived.Size = new System.Drawing.Size(99, 20);
            this.bArrived.TabIndex = 18;
            this.bArrived.Text = "button5";
            this.bArrived.UseVisualStyleBackColor = true;
            this.bArrived.Click += new System.EventHandler(this.bArrived_Click);
            // 
            // bEnd1
            // 
            this.bEnd1.Location = new System.Drawing.Point(241, 170);
            this.bEnd1.Name = "bEnd1";
            this.bEnd1.Size = new System.Drawing.Size(99, 20);
            this.bEnd1.TabIndex = 19;
            this.bEnd1.Text = "button6";
            this.bEnd1.UseVisualStyleBackColor = true;
            this.bEnd1.Click += new System.EventHandler(this.bEnd1_Click);
            // 
            // bSave
            // 
            this.bSave.Location = new System.Drawing.Point(119, 222);
            this.bSave.Name = "bSave";
            this.bSave.Size = new System.Drawing.Size(100, 30);
            this.bSave.TabIndex = 21;
            this.bSave.Text = "Save";
            this.bSave.UseVisualStyleBackColor = true;
            this.bSave.Click += new System.EventHandler(this.bSave_Click);
            // 
            // tbFileName
            // 
            this.tbFileName.Location = new System.Drawing.Point(15, 228);
            this.tbFileName.Name = "tbFileName";
            this.tbFileName.Size = new System.Drawing.Size(100, 20);
            this.tbFileName.TabIndex = 22;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(240, 222);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 30);
            this.button1.TabIndex = 28;
            this.button1.Text = "Reset";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(352, 284);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.tbFileName);
            this.Controls.Add(this.bSave);
            this.Controls.Add(this.bEnd1);
            this.Controls.Add(this.bArrived);
            this.Controls.Add(this.bRelative1);
            this.Controls.Add(this.bRest);
            this.Controls.Add(this.bWait1);
            this.Controls.Add(this.tbEnd1);
            this.Controls.Add(this.tbArrived);
            this.Controls.Add(this.tbWait1);
            this.Controls.Add(this.tbRest);
            this.Controls.Add(this.tbRelative1);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label1);
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox tbRelative1;
        private System.Windows.Forms.TextBox tbRest;
        private System.Windows.Forms.TextBox tbWait1;
        private System.Windows.Forms.TextBox tbArrived;
        private System.Windows.Forms.TextBox tbEnd1;
        private System.Windows.Forms.Button bWait1;
        private System.Windows.Forms.Button bRest;
        private System.Windows.Forms.Button bRelative1;
        private System.Windows.Forms.Button bArrived;
        private System.Windows.Forms.Button bEnd1;
        private System.Windows.Forms.Button bSave;
        private System.Windows.Forms.TextBox tbFileName;
        private System.Windows.Forms.Button button1;
    }
}