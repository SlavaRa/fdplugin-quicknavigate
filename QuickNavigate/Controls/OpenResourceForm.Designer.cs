namespace QuickNavigate
{
    partial class OpenResourceForm
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
            this.input = new System.Windows.Forms.TextBox();
            this.tree = new System.Windows.Forms.ListBox();
            this.refreshButton = new System.Windows.Forms.Button();
            this.worker = new System.ComponentModel.BackgroundWorker();
            this.messageLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // input
            // 
            this.input.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.input.BackColor = System.Drawing.SystemColors.Control;
            this.input.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.input.Location = new System.Drawing.Point(12, 26);
            this.input.Name = "input";
            this.input.Size = new System.Drawing.Size(338, 20);
            this.input.TabIndex = 1;
            this.input.TextChanged += new System.EventHandler(this.TextBox_TextChanged);
            this.input.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBox_KeyDown);
            // 
            // listBox
            // 
            this.tree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tree.BackColor = System.Drawing.SystemColors.Control;
            this.tree.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tree.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.tree.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tree.FormattingEnabled = true;
            this.tree.ItemHeight = 16;
            this.tree.Location = new System.Drawing.Point(12, 53);
            this.tree.Name = "listBox";
            this.tree.Size = new System.Drawing.Size(365, 194);
            this.tree.TabIndex = 2;
            this.tree.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.ListBox_DrawItem);
            this.tree.DoubleClick += new System.EventHandler(this.ListBox_DoubleClick);
            this.tree.Resize += new System.EventHandler(this.ListBox_Resize);
            // 
            // refreshButton
            // 
            this.refreshButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.refreshButton.FlatAppearance.BorderSize = 0;
            this.refreshButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.refreshButton.Location = new System.Drawing.Point(356, 26);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(22, 22);
            this.refreshButton.TabIndex = 3;
            this.refreshButton.UseVisualStyleBackColor = true;
            this.refreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
            // 
            // worker
            // 
            this.worker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.Worker_DoWork);
            this.worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.Worker_RunWorkerCompleted);
            // 
            // messageLabel
            // 
            this.messageLabel.AutoSize = true;
            this.messageLabel.Location = new System.Drawing.Point(12, 9);
            this.messageLabel.Name = "messageLabel";
            this.messageLabel.Size = new System.Drawing.Size(271, 13);
            this.messageLabel.TabIndex = 4;
            this.messageLabel.Text = "Search string: (UPPERCASE for search by abbreviation)";
            // 
            // OpenResourceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(389, 255);
            this.Controls.Add(this.messageLabel);
            this.Controls.Add(this.refreshButton);
            this.Controls.Add(this.tree);
            this.Controls.Add(this.input);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(320, 200);
            this.Name = "OpenResourceForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Open Resource";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OpenResourceForm_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OpenResourceForm_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox input;
        private System.Windows.Forms.ListBox tree;
        private System.Windows.Forms.Button refreshButton;
        private System.ComponentModel.BackgroundWorker worker;
        private System.Windows.Forms.Label messageLabel;
    }
}