namespace RobotCC
{
    partial class OptionSetting
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
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.robotName = new System.Windows.Forms.TextBox();
            this.LSize = new System.Windows.Forms.TextBox();
            this.RSize = new System.Windows.Forms.TextBox();
            this.radioButtonV = new System.Windows.Forms.RadioButton();
            this.radioButtonH = new System.Windows.Forms.RadioButton();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("나눔스퀘어OTF Bold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label1.Location = new System.Drawing.Point(46, 164);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "가로 길이(m)";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("나눔스퀘어OTF Bold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label3.Location = new System.Drawing.Point(46, 213);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(96, 18);
            this.label3.TabIndex = 2;
            this.label3.Text = "세로 길이(m)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("나눔스퀘어OTF Bold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label2.Location = new System.Drawing.Point(50, 115);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 18);
            this.label2.TabIndex = 1;
            this.label2.Text = "로봇이름";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("나눔스퀘어OTF Bold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label4.Location = new System.Drawing.Point(50, 265);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(72, 18);
            this.label4.TabIndex = 3;
            this.label4.Text = "운용 방향";
            // 
            // robotName
            // 
            this.robotName.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.robotName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.robotName.Enabled = false;
            this.robotName.Font = new System.Drawing.Font("나눔스퀘어OTF", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.robotName.Location = new System.Drawing.Point(190, 115);
            this.robotName.Margin = new System.Windows.Forms.Padding(5, 3, 3, 3);
            this.robotName.Name = "robotName";
            this.robotName.ReadOnly = true;
            this.robotName.Size = new System.Drawing.Size(164, 26);
            this.robotName.TabIndex = 5;
            // 
            // LSize
            // 
            this.LSize.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LSize.Font = new System.Drawing.Font("나눔스퀘어OTF", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LSize.Location = new System.Drawing.Point(192, 160);
            this.LSize.Margin = new System.Windows.Forms.Padding(5, 3, 3, 3);
            this.LSize.Name = "LSize";
            this.LSize.Size = new System.Drawing.Size(162, 26);
            this.LSize.TabIndex = 6;
            this.LSize.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.LSize_KeyPress);
            // 
            // RSize
            // 
            this.RSize.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.RSize.Font = new System.Drawing.Font("나눔스퀘어OTF", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.RSize.Location = new System.Drawing.Point(192, 209);
            this.RSize.Margin = new System.Windows.Forms.Padding(5, 3, 3, 3);
            this.RSize.Name = "RSize";
            this.RSize.Size = new System.Drawing.Size(162, 26);
            this.RSize.TabIndex = 7;
            this.RSize.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.RSize_KeyPress);
            // 
            // radioButtonV
            // 
            this.radioButtonV.AutoSize = true;
            this.radioButtonV.Font = new System.Drawing.Font("나눔스퀘어OTF", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.radioButtonV.Location = new System.Drawing.Point(192, 263);
            this.radioButtonV.Name = "radioButtonV";
            this.radioButtonV.Size = new System.Drawing.Size(86, 22);
            this.radioButtonV.TabIndex = 8;
            this.radioButtonV.TabStop = true;
            this.radioButtonV.Text = "수직방향";
            this.radioButtonV.UseVisualStyleBackColor = true;
            this.radioButtonV.CheckedChanged += new System.EventHandler(this.radioButtonV_CheckedChanged);
            // 
            // radioButtonH
            // 
            this.radioButtonH.AutoSize = true;
            this.radioButtonH.Font = new System.Drawing.Font("나눔스퀘어OTF", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.radioButtonH.Location = new System.Drawing.Point(192, 295);
            this.radioButtonH.Name = "radioButtonH";
            this.radioButtonH.Size = new System.Drawing.Size(86, 22);
            this.radioButtonH.TabIndex = 9;
            this.radioButtonH.TabStop = true;
            this.radioButtonH.Text = "수평방향";
            this.radioButtonH.UseVisualStyleBackColor = true;
            this.radioButtonH.CheckedChanged += new System.EventHandler(this.radioButtonH_CheckedChanged);
            // 
            // buttonSave
            // 
            this.buttonSave.Font = new System.Drawing.Font("나눔스퀘어OTF Bold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.buttonSave.Location = new System.Drawing.Point(50, 365);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(198, 36);
            this.buttonSave.TabIndex = 10;
            this.buttonSave.Text = "저장";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Font = new System.Drawing.Font("나눔스퀘어OTF Bold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.buttonCancel.Location = new System.Drawing.Point(279, 365);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(169, 36);
            this.buttonCancel.TabIndex = 11;
            this.buttonCancel.Text = "취소";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("나눔스퀘어OTF ExtraBold", 22.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label6.ForeColor = System.Drawing.SystemColors.MenuText;
            this.label6.Location = new System.Drawing.Point(184, 31);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(193, 33);
            this.label6.TabIndex = 13;
            this.label6.Text = "로봇 상세 설정";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Font = new System.Drawing.Font("나눔스퀘어OTF Bold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.checkBox1.Location = new System.Drawing.Point(386, 264);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(91, 22);
            this.checkBox1.TabIndex = 14;
            this.checkBox1.Text = "자동 시작";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // OptionSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(568, 450);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.radioButtonH);
            this.Controls.Add(this.radioButtonV);
            this.Controls.Add(this.RSize);
            this.Controls.Add(this.LSize);
            this.Controls.Add(this.robotName);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "OptionSetting";
            this.Text = "OPTION 설정";
            this.Load += new System.EventHandler(this.OptionFormLoad);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox robotName;
        private System.Windows.Forms.TextBox LSize;
        private System.Windows.Forms.TextBox RSize;
        private System.Windows.Forms.RadioButton radioButtonV;
        private System.Windows.Forms.RadioButton radioButtonH;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox checkBox1;
    }
}