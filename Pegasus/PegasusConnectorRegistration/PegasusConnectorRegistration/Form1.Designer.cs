namespace PegasusConnectorRegistration
{
    partial class PegasusConnectorRegistrationForm
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
            this.APIUriLabel = new System.Windows.Forms.Label();
            this.NotificationURILabel = new System.Windows.Forms.Label();
            this.TenantIDLabel = new System.Windows.Forms.Label();
            this.APIUriTextBox = new System.Windows.Forms.TextBox();
            this.NotificationUriTextBox = new System.Windows.Forms.TextBox();
            this.TenantIDTextBox = new System.Windows.Forms.TextBox();
            this.RegisterBtn = new System.Windows.Forms.Button();
            this.ResultsTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // APIUriLabel
            // 
            this.APIUriLabel.AutoSize = true;
            this.APIUriLabel.Location = new System.Drawing.Point(13, 13);
            this.APIUriLabel.Name = "APIUriLabel";
            this.APIUriLabel.Size = new System.Drawing.Size(64, 13);
            this.APIUriLabel.TabIndex = 0;
            this.APIUriLabel.Text = "API address";
            // 
            // NotificationURILabel
            // 
            this.NotificationURILabel.AutoSize = true;
            this.NotificationURILabel.Location = new System.Drawing.Point(12, 39);
            this.NotificationURILabel.Name = "NotificationURILabel";
            this.NotificationURILabel.Size = new System.Drawing.Size(100, 13);
            this.NotificationURILabel.TabIndex = 1;
            this.NotificationURILabel.Text = "Notification address";
            // 
            // TenantIDLabel
            // 
            this.TenantIDLabel.AutoSize = true;
            this.TenantIDLabel.Location = new System.Drawing.Point(13, 65);
            this.TenantIDLabel.Name = "TenantIDLabel";
            this.TenantIDLabel.Size = new System.Drawing.Size(55, 13);
            this.TenantIDLabel.TabIndex = 2;
            this.TenantIDLabel.Text = "Tenant ID";
            // 
            // APIUriTextBox
            // 
            this.APIUriTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.APIUriTextBox.Location = new System.Drawing.Point(113, 10);
            this.APIUriTextBox.Name = "APIUriTextBox";
            this.APIUriTextBox.Size = new System.Drawing.Size(159, 20);
            this.APIUriTextBox.TabIndex = 3;
            this.APIUriTextBox.Text = "http://127.0.0.1:81/";
            // 
            // NotificationUriTextBox
            // 
            this.NotificationUriTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NotificationUriTextBox.Location = new System.Drawing.Point(113, 36);
            this.NotificationUriTextBox.Name = "NotificationUriTextBox";
            this.NotificationUriTextBox.Size = new System.Drawing.Size(159, 20);
            this.NotificationUriTextBox.TabIndex = 4;
            this.NotificationUriTextBox.Text = "http://127.0.0.1:8080/";
            // 
            // TenantIDTextBox
            // 
            this.TenantIDTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TenantIDTextBox.Location = new System.Drawing.Point(74, 62);
            this.TenantIDTextBox.Name = "TenantIDTextBox";
            this.TenantIDTextBox.Size = new System.Drawing.Size(198, 20);
            this.TenantIDTextBox.TabIndex = 5;
            this.TenantIDTextBox.Text = "3813fccf-4946-43e8-ac72-0c00d2df9f6f";
            // 
            // RegisterBtn
            // 
            this.RegisterBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.RegisterBtn.Location = new System.Drawing.Point(197, 88);
            this.RegisterBtn.Name = "RegisterBtn";
            this.RegisterBtn.Size = new System.Drawing.Size(75, 23);
            this.RegisterBtn.TabIndex = 6;
            this.RegisterBtn.Text = "Register";
            this.RegisterBtn.UseVisualStyleBackColor = true;
            this.RegisterBtn.Click += new System.EventHandler(this.RegisterBtn_Click);
            // 
            // ResultsTextBox
            // 
            this.ResultsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ResultsTextBox.Location = new System.Drawing.Point(16, 127);
            this.ResultsTextBox.Multiline = true;
            this.ResultsTextBox.Name = "ResultsTextBox";
            this.ResultsTextBox.ReadOnly = true;
            this.ResultsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.ResultsTextBox.Size = new System.Drawing.Size(255, 117);
            this.ResultsTextBox.TabIndex = 7;
            // 
            // PegasusConnectorRegistrationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.ResultsTextBox);
            this.Controls.Add(this.RegisterBtn);
            this.Controls.Add(this.TenantIDTextBox);
            this.Controls.Add(this.NotificationUriTextBox);
            this.Controls.Add(this.APIUriTextBox);
            this.Controls.Add(this.TenantIDLabel);
            this.Controls.Add(this.NotificationURILabel);
            this.Controls.Add(this.APIUriLabel);
            this.MinimumSize = new System.Drawing.Size(300, 300);
            this.Name = "PegasusConnectorRegistrationForm";
            this.Text = "Pegasus Connector Registration";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label APIUriLabel;
        private System.Windows.Forms.Label NotificationURILabel;
        private System.Windows.Forms.Label TenantIDLabel;
        private System.Windows.Forms.TextBox APIUriTextBox;
        private System.Windows.Forms.TextBox NotificationUriTextBox;
        private System.Windows.Forms.TextBox TenantIDTextBox;
        private System.Windows.Forms.Button RegisterBtn;
        private System.Windows.Forms.TextBox ResultsTextBox;
    }
}

