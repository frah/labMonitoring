namespace LabMonitoring
{
    partial class OAuthForm
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
            this.label2 = new System.Windows.Forms.Label();
            this.secretTextBox = new System.Windows.Forms.TextBox();
            this.keyTextBox = new System.Windows.Forms.TextBox();
            this.authButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.pinTextBox = new System.Windows.Forms.TextBox();
            this.webBrowser = new System.Windows.Forms.WebBrowser();
            this.okButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "Consumer Key";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(93, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "Consumer Secret";
            // 
            // secretTextBox
            // 
            this.secretTextBox.Location = new System.Drawing.Point(112, 38);
            this.secretTextBox.Name = "secretTextBox";
            this.secretTextBox.Size = new System.Drawing.Size(263, 19);
            this.secretTextBox.TabIndex = 2;
            // 
            // keyTextBox
            // 
            this.keyTextBox.Location = new System.Drawing.Point(112, 10);
            this.keyTextBox.Name = "keyTextBox";
            this.keyTextBox.Size = new System.Drawing.Size(263, 19);
            this.keyTextBox.TabIndex = 3;
            // 
            // authButton
            // 
            this.authButton.Location = new System.Drawing.Point(381, 8);
            this.authButton.Name = "authButton";
            this.authButton.Size = new System.Drawing.Size(184, 49);
            this.authButton.TabIndex = 4;
            this.authButton.Text = "Authenticate";
            this.authButton.UseVisualStyleBackColor = true;
            this.authButton.Click += new System.EventHandler(this.authButton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label3.Location = new System.Drawing.Point(12, 79);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(32, 16);
            this.label3.TabIndex = 5;
            this.label3.Text = "PIN";
            // 
            // pinTextBox
            // 
            this.pinTextBox.Enabled = false;
            this.pinTextBox.Font = new System.Drawing.Font("MS UI Gothic", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.pinTextBox.Location = new System.Drawing.Point(112, 74);
            this.pinTextBox.Name = "pinTextBox";
            this.pinTextBox.Size = new System.Drawing.Size(263, 26);
            this.pinTextBox.TabIndex = 6;
            // 
            // webBrowser
            // 
            this.webBrowser.Location = new System.Drawing.Point(13, 106);
            this.webBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser.Name = "webBrowser";
            this.webBrowser.Size = new System.Drawing.Size(552, 267);
            this.webBrowser.TabIndex = 7;
            // 
            // okButton
            // 
            this.okButton.Enabled = false;
            this.okButton.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.okButton.Location = new System.Drawing.Point(382, 74);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(183, 26);
            this.okButton.TabIndex = 8;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // OAuthForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(577, 385);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.webBrowser);
            this.Controls.Add(this.pinTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.authButton);
            this.Controls.Add(this.keyTextBox);
            this.Controls.Add(this.secretTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OAuthForm";
            this.Text = "OAuthForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox secretTextBox;
        private System.Windows.Forms.TextBox keyTextBox;
        private System.Windows.Forms.Button authButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox pinTextBox;
        private System.Windows.Forms.WebBrowser webBrowser;
        private System.Windows.Forms.Button okButton;
    }
}