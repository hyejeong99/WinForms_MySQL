namespace RobotCC
{
    partial class SerialPortSetting
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
            this.cBoxPort = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cancelBtn = new System.Windows.Forms.Button();
            this.saveAndExitBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cBoxPort
            // 
            this.cBoxPort.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.cBoxPort.Font = new System.Drawing.Font("나눔스퀘어OTF", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.cBoxPort.FormattingEnabled = true;
            this.cBoxPort.Location = new System.Drawing.Point(281, 106);
            this.cBoxPort.Name = "cBoxPort";
            this.cBoxPort.Size = new System.Drawing.Size(111, 26);
            this.cBoxPort.TabIndex = 97;
            this.cBoxPort.SelectedIndexChanged += new System.EventHandler(this.cBox_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("나눔스퀘어OTF Bold", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label3.Location = new System.Drawing.Point(135, 106);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(122, 21);
            this.label3.TabIndex = 96;
            this.label3.Text = "통신 포트 선택";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("나눔스퀘어OTF ExtraBold", 22.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label1.ForeColor = System.Drawing.SystemColors.MenuText;
            this.label1.Location = new System.Drawing.Point(186, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(193, 33);
            this.label1.TabIndex = 95;
            this.label1.Text = "통신 설정 변경";
            // 
            // cancelBtn
            // 
            this.cancelBtn.Font = new System.Drawing.Font("나눔스퀘어OTF Bold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.cancelBtn.Location = new System.Drawing.Point(281, 173);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(172, 38);
            this.cancelBtn.TabIndex = 99;
            this.cancelBtn.Text = "취소";
            this.cancelBtn.UseVisualStyleBackColor = true;
            this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
            // 
            // saveAndExitBtn
            // 
            this.saveAndExitBtn.Font = new System.Drawing.Font("나눔스퀘어OTF Bold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.saveAndExitBtn.Location = new System.Drawing.Point(94, 173);
            this.saveAndExitBtn.Name = "saveAndExitBtn";
            this.saveAndExitBtn.Size = new System.Drawing.Size(172, 38);
            this.saveAndExitBtn.TabIndex = 98;
            this.saveAndExitBtn.Text = "설정 저장 및 종료";
            this.saveAndExitBtn.UseVisualStyleBackColor = true;
            this.saveAndExitBtn.Click += new System.EventHandler(this.saveAndExitBtn_Click);
            // 
            // SerialPortSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(565, 266);
            this.Controls.Add(this.cBoxPort);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this.saveAndExitBtn);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SerialPortSetting";
            this.Text = "SerialPortSetting";
            this.Load += new System.EventHandler(this.Form_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cBoxPort;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button cancelBtn;
        private System.Windows.Forms.Button saveAndExitBtn;
    }
}