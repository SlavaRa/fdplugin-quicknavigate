namespace QuickNavigate.Forms
{
    sealed partial class TypeExplorer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.infoLabel = new System.Windows.Forms.Label();
            this.input = new System.Windows.Forms.TextBox();
            this.tree = new System.Windows.Forms.TreeView();
            this.searchingInExternalClasspaths = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // infoLabel
            // 
            this.infoLabel.AutoSize = true;
            this.infoLabel.Location = new System.Drawing.Point(12, 9);
            this.infoLabel.Name = "infoLabel";
            this.infoLabel.Size = new System.Drawing.Size(271, 13);
            this.infoLabel.TabIndex = 2;
            this.infoLabel.Text = "Search string: (UPPERCASE for search by abbreviation)";
            // 
            // input
            // 
            this.input.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.input.BackColor = System.Drawing.SystemColors.Control;
            this.input.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.input.Location = new System.Drawing.Point(12, 26);
            this.input.Name = "input";
            this.input.Size = new System.Drawing.Size(365, 21);
            this.input.TabIndex = 0;
            this.input.TextChanged += new System.EventHandler(this.OnInputTextChanged);
            this.input.PreviewKeyDown += OnInputPreviewKeyDown;
            this.input.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnInputKeyDown);
            // 
            // searchingInExternalClasspaths
            // 
            this.searchingInExternalClasspaths.AutoSize = true;
            this.searchingInExternalClasspaths.Checked = true;
            this.searchingInExternalClasspaths.CheckState = System.Windows.Forms.CheckState.Checked;
            this.searchingInExternalClasspaths.Location = new System.Drawing.Point(12, 54);
            this.searchingInExternalClasspaths.Name = "searchingInExternalClasspaths";
            this.searchingInExternalClasspaths.Size = new System.Drawing.Size(246, 17);
            this.searchingInExternalClasspaths.TabIndex = 1;
            this.searchingInExternalClasspaths.Text = "Searching types in external classpaths(Ctrl+E)";
            this.searchingInExternalClasspaths.UseVisualStyleBackColor = true;
            this.searchingInExternalClasspaths.CheckStateChanged += new System.EventHandler(OnSearchingModeCheckStateChanged);
            // 
            // tree
            // 
            this.tree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tree.BackColor = System.Drawing.SystemColors.Control;
            this.tree.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tree.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
            this.tree.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tree.HideSelection = false;
            this.tree.ItemHeight = 16;
            this.tree.Location = new System.Drawing.Point(12, 77);
            this.tree.Name = "tree";
            this.tree.ShowLines = false;
            this.tree.ShowPlusMinus = false;
            this.tree.ShowRootLines = false;
            this.tree.Size = new System.Drawing.Size(365, 175);
            this.tree.TabIndex = 2;
            this.tree.DrawNode += new System.Windows.Forms.DrawTreeNodeEventHandler(this.OnTreeDrawNode);
            this.tree.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.OnTreeNodeMouseClick);
            this.tree.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.OnTreeNodeMouseDoubleClick);
            // 
            // TypeExplorer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(390, 255);
            this.Controls.Add(this.searchingInExternalClasspaths);
            this.Controls.Add(this.tree);
            this.Controls.Add(this.input);
            this.Controls.Add(this.infoLabel);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(320, 200);
            this.Name = "TypeExplorer";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Type Explorer";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label infoLabel;
        private System.Windows.Forms.TextBox input;
        private System.Windows.Forms.TreeView tree;
        private System.Windows.Forms.CheckBox searchingInExternalClasspaths;
    }
}