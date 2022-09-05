namespace RobotCC
{
    partial class ReportForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReportForm));
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.printBtn = new System.Windows.Forms.Button();
            this.emailBtn = new System.Windows.Forms.Button();
            this.exitBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("나눔스퀘어OTF ExtraBold", 22.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label1.ForeColor = System.Drawing.SystemColors.MenuText;
            this.label1.Location = new System.Drawing.Point(364, 54);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(433, 33);
            this.label1.TabIndex = 70;
            this.label1.Text = "보고서 작성 및 이메일 발송 시스템";
            // 
            // comboBox1
            // 
            this.comboBox1.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.comboBox1.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.ItemHeight = 16;
            this.comboBox1.Items.AddRange(new object[] {
            "DB로 추가 예정",
            "나중에 ",
            "발전소1",
            "발전소2"});
            this.comboBox1.Location = new System.Drawing.Point(129, 148);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(355, 24);
            this.comboBox1.Sorted = true;
            this.comboBox1.TabIndex = 72;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label2.Location = new System.Drawing.Point(21, 148);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 21);
            this.label2.TabIndex = 71;
            this.label2.Text = "발전소명";
            // 
            // printBtn
            // 
            this.printBtn.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.printBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("printBtn.BackgroundImage")));
            this.printBtn.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.printBtn.FlatAppearance.BorderSize = 13;
            this.printBtn.Font = new System.Drawing.Font("나눔스퀘어OTF Bold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.printBtn.ForeColor = System.Drawing.SystemColors.MenuText;
            this.printBtn.Location = new System.Drawing.Point(841, 148);
            this.printBtn.Name = "printBtn";
            this.printBtn.Size = new System.Drawing.Size(96, 38);
            this.printBtn.TabIndex = 73;
            this.printBtn.Text = "인쇄";
            this.printBtn.UseVisualStyleBackColor = false;
            this.printBtn.Click += new System.EventHandler(this.printBtn_Click);
            // 
            // emailBtn
            // 
            this.emailBtn.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.emailBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("emailBtn.BackgroundImage")));
            this.emailBtn.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.emailBtn.FlatAppearance.BorderSize = 13;
            this.emailBtn.Font = new System.Drawing.Font("나눔스퀘어OTF Bold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.emailBtn.ForeColor = System.Drawing.SystemColors.MenuText;
            this.emailBtn.Location = new System.Drawing.Point(943, 148);
            this.emailBtn.Name = "emailBtn";
            this.emailBtn.Size = new System.Drawing.Size(120, 38);
            this.emailBtn.TabIndex = 74;
            this.emailBtn.Text = "이메일 발송";
            this.emailBtn.UseVisualStyleBackColor = false;
            this.emailBtn.Click += new System.EventHandler(this.emailBtn_Click);
            // 
            // exitBtn
            // 
            this.exitBtn.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.exitBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("exitBtn.BackgroundImage")));
            this.exitBtn.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.exitBtn.FlatAppearance.BorderSize = 13;
            this.exitBtn.Font = new System.Drawing.Font("나눔스퀘어OTF Bold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.exitBtn.ForeColor = System.Drawing.SystemColors.MenuText;
            this.exitBtn.Location = new System.Drawing.Point(1070, 148);
            this.exitBtn.Name = "exitBtn";
            this.exitBtn.Size = new System.Drawing.Size(90, 38);
            this.exitBtn.TabIndex = 75;
            this.exitBtn.Text = "종료";
            this.exitBtn.UseVisualStyleBackColor = false;
            this.exitBtn.Click += new System.EventHandler(this.exitBtn_Click);
            // 
            // ReportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1182, 753);
            this.Controls.Add(this.exitBtn);
            this.Controls.Add(this.emailBtn);
            this.Controls.Add(this.printBtn);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "ReportForm";
            this.Text = "Report";
            this.Load += new System.EventHandler(this.Form_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button printBtn;
        private System.Windows.Forms.Button emailBtn;
        private System.Windows.Forms.Button exitBtn;
    }
}