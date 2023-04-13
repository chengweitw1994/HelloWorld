namespace MySocketServer
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btn_sendMessage = new System.Windows.Forms.Button();
            this.tx_sendMessage = new System.Windows.Forms.TextBox();
            this.gb_sendMessage = new System.Windows.Forms.GroupBox();
            this.gb_sendMessage.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_sendMessage
            // 
            this.btn_sendMessage.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btn_sendMessage.Location = new System.Drawing.Point(406, 20);
            this.btn_sendMessage.Name = "btn_sendMessage";
            this.btn_sendMessage.Size = new System.Drawing.Size(114, 36);
            this.btn_sendMessage.TabIndex = 0;
            this.btn_sendMessage.Text = "發送訊息";
            this.btn_sendMessage.UseVisualStyleBackColor = true;
            // 
            // tx_sendMessage
            // 
            this.tx_sendMessage.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.tx_sendMessage.Location = new System.Drawing.Point(6, 22);
            this.tx_sendMessage.Name = "tx_sendMessage";
            this.tx_sendMessage.Size = new System.Drawing.Size(394, 35);
            this.tx_sendMessage.TabIndex = 1;
            // 
            // gb_sendMessage
            // 
            this.gb_sendMessage.Controls.Add(this.btn_sendMessage);
            this.gb_sendMessage.Controls.Add(this.tx_sendMessage);
            this.gb_sendMessage.Location = new System.Drawing.Point(12, 21);
            this.gb_sendMessage.Name = "gb_sendMessage";
            this.gb_sendMessage.Size = new System.Drawing.Size(532, 68);
            this.gb_sendMessage.TabIndex = 4;
            this.gb_sendMessage.TabStop = false;
            this.gb_sendMessage.Text = "發送訊息";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(604, 130);
            this.Controls.Add(this.gb_sendMessage);
            this.Name = "Form1";
            this.Text = "Form1";
            this.gb_sendMessage.ResumeLayout(false);
            this.gb_sendMessage.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Button btn_sendMessage;
        private TextBox tx_sendMessage;
        private GroupBox gb_sendMessage;
    }
}