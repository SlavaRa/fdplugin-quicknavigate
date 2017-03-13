// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Windows.Forms;

namespace QuickNavigate.Forms
{
    sealed partial class OpenRecentProjectsForm
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
            this.tree = new System.Windows.Forms.TreeView();
            this.open = new System.Windows.Forms.Button();
            this.cancel = new System.Windows.Forms.Button();
            this.openInNewWindow = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // input
            // 
            this.input.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.input.Location = new System.Drawing.Point(12, 12);
            this.input.Name = "input";
            this.input.Size = new System.Drawing.Size(305, 20);
            this.input.TabIndex = 0;
            this.input.TextChanged += new System.EventHandler(this.OnInputTextChanged);
            this.input.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnInputKeyDown);
            // 
            // tree
            // 
            this.tree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tree.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tree.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
            this.tree.HideSelection = false;
            this.tree.Location = new System.Drawing.Point(12, 40);
            this.tree.Name = "tree";
            this.tree.ShowLines = false;
            this.tree.ShowPlusMinus = false;
            this.tree.ShowRootLines = false;
            this.tree.Size = new System.Drawing.Size(305, 182);
            this.tree.TabIndex = 1;
            this.tree.DrawNode += new System.Windows.Forms.DrawTreeNodeEventHandler(this.OnTreeDrawNode);
            this.tree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.OnTreeAfterSelect);
            this.tree.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.OnTreeMouseDoubleClick);
            // 
            // open
            // 
            this.open.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.open.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.open.Location = new System.Drawing.Point(161, 225);
            this.open.Name = "open";
            this.open.Size = new System.Drawing.Size(75, 23);
            this.open.TabIndex = 3;
            this.open.Text = "Open";
            this.open.UseVisualStyleBackColor = true;
            // 
            // cancel
            // 
            this.cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancel.Location = new System.Drawing.Point(242, 225);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(75, 23);
            this.cancel.TabIndex = 4;
            this.cancel.Text = "Cancel";
            this.cancel.UseVisualStyleBackColor = true;
            // 
            // openInNewWindow
            // 
            this.openInNewWindow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.openInNewWindow.Location = new System.Drawing.Point(12, 225);
            this.openInNewWindow.Name = "openInNewWindow";
            this.openInNewWindow.Size = new System.Drawing.Size(143, 23);
            this.openInNewWindow.TabIndex = 2;
            this.openInNewWindow.Text = "Open in new Window";
            this.openInNewWindow.UseVisualStyleBackColor = true;
            this.openInNewWindow.Click += new System.EventHandler(this.OnOpenInNewWindowClick);
            // 
            // OpenRecentProjectsForm
            // 
            this.AcceptButton = this.open;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancel;
            this.ClientSize = new System.Drawing.Size(330, 251);
            this.Controls.Add(this.openInNewWindow);
            this.Controls.Add(this.open);
            this.Controls.Add(this.cancel);
            this.Controls.Add(this.tree);
            this.Controls.Add(this.input);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(320, 200);
            this.Name = "OpenRecentProjectsForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Recent Projects";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox input;
        private System.Windows.Forms.TreeView tree;
        private System.Windows.Forms.Button open;
        private System.Windows.Forms.Button cancel;
        private Button openInNewWindow;
    }
}