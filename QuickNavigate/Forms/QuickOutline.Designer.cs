using System;
using System.Windows.Forms;

namespace QuickNavigate.Forms
{
    sealed partial class QuickOutline
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
            this.input = new System.Windows.Forms.TextBox();
            this.tree = new System.Windows.Forms.TreeView();
            this.classes = new System.Windows.Forms.Button();
            this.fields = new System.Windows.Forms.Button();
            this.properties = new System.Windows.Forms.Button();
            this.methods = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // input
            // 
            this.input.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.input.BackColor = System.Drawing.SystemColors.Control;
            this.input.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.input.Location = new System.Drawing.Point(12, 12);
            this.input.Name = "input";
            this.input.Size = new System.Drawing.Size(305, 21);
            this.input.TabIndex = 0;
            this.input.TextChanged += new System.EventHandler(this.OnInputTextChanged);
            this.input.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnInputKeyDown);
            this.input.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.OnInputPreviewKeyDown);
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
            this.tree.Location = new System.Drawing.Point(12, 40);
            this.tree.Name = "tree";
            this.tree.ShowPlusMinus = false;
            this.tree.ShowRootLines = false;
            this.tree.Size = new System.Drawing.Size(305, 158);
            this.tree.TabIndex = 1;
            this.tree.DrawNode += new System.Windows.Forms.DrawTreeNodeEventHandler(this.OnTreeDrawNode);
            this.tree.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.OnTreeNodeMouseClick);
            this.tree.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.OnTreeNodeMouseDoubleClick);
            // 
            // classes
            // 
            this.classes.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.classes.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.classes.Location = new System.Drawing.Point(112, 202);
            this.classes.Margin = new System.Windows.Forms.Padding(0);
            this.classes.Name = "classes";
            this.classes.Size = new System.Drawing.Size(24, 24);
            this.classes.TabIndex = 2;
            this.classes.UseVisualStyleBackColor = true;
            this.classes.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OnFilterMouseClick);
            this.classes.MouseLeave += new System.EventHandler(this.OnFilterMouseLeave);
            this.classes.MouseHover += new System.EventHandler(this.OnFilterMouseHover);
            // 
            // fields
            // 
            this.fields.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.fields.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.fields.Location = new System.Drawing.Point(139, 202);
            this.fields.Margin = new System.Windows.Forms.Padding(0);
            this.fields.Name = "fields";
            this.fields.Size = new System.Drawing.Size(24, 24);
            this.fields.TabIndex = 3;
            this.fields.UseVisualStyleBackColor = true;
            this.fields.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OnFilterMouseClick);
            this.fields.MouseLeave += new System.EventHandler(this.OnFilterMouseLeave);
            this.fields.MouseHover += new System.EventHandler(this.OnFilterMouseHover);
            // 
            // properties
            // 
            this.properties.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.properties.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.properties.Location = new System.Drawing.Point(166, 202);
            this.properties.Margin = new System.Windows.Forms.Padding(0);
            this.properties.Name = "properties";
            this.properties.Size = new System.Drawing.Size(24, 24);
            this.properties.TabIndex = 4;
            this.properties.UseVisualStyleBackColor = true;
            this.properties.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OnFilterMouseClick);
            this.properties.MouseLeave += new System.EventHandler(this.OnFilterMouseLeave);
            this.properties.MouseHover += new System.EventHandler(this.OnFilterMouseHover);
            // 
            // methods
            // 
            this.methods.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.methods.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.methods.Location = new System.Drawing.Point(194, 202);
            this.methods.Margin = new System.Windows.Forms.Padding(0);
            this.methods.Name = "methods";
            this.methods.Size = new System.Drawing.Size(24, 24);
            this.methods.TabIndex = 5;
            this.methods.UseVisualStyleBackColor = true;
            this.methods.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OnFilterMouseClick);
            this.methods.MouseLeave += new System.EventHandler(this.OnFilterMouseLeave);
            this.methods.MouseHover += new System.EventHandler(this.OnFilterMouseHover);
            // 
            // QuickOutline
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(330, 230);
            this.Controls.Add(this.methods);
            this.Controls.Add(this.properties);
            this.Controls.Add(this.fields);
            this.Controls.Add(this.classes);
            this.Controls.Add(this.tree);
            this.Controls.Add(this.input);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(320, 200);
            this.Name = "QuickOutline";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Quick Outline";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox input;
        private System.Windows.Forms.TreeView tree;
        private System.Windows.Forms.Button classes;
        private System.Windows.Forms.Button fields;
        private System.Windows.Forms.Button properties;
        private System.Windows.Forms.Button methods;
    }
}