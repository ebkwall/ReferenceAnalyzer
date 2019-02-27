namespace ReferenceAnalyzerTool
{
    partial class Form1
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.lvProjectReferences = new System.Windows.Forms.ListView();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.lvFileReferences = new System.Windows.Forms.ListView();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.tvwProposedFixes = new CustomizedTreeView();
            this.lvLacking = new System.Windows.Forms.ListView();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer1.Panel2.Controls.Add(this.textBox3);
            this.splitContainer1.Size = new System.Drawing.Size(800, 450);
            this.splitContainer1.SplitterDistance = 405;
            this.splitContainer1.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.lvProjectReferences);
            this.splitContainer2.Panel1.Controls.Add(this.textBox1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.lvFileReferences);
            this.splitContainer2.Panel2.Controls.Add(this.textBox2);
            this.splitContainer2.Size = new System.Drawing.Size(405, 450);
            this.splitContainer2.SplitterDistance = 221;
            this.splitContainer2.TabIndex = 0;
            // 
            // lvProjectReferences
            // 
            this.lvProjectReferences.CheckBoxes = true;
            this.lvProjectReferences.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvProjectReferences.Location = new System.Drawing.Point(0, 20);
            this.lvProjectReferences.Name = "lvProjectReferences";
            this.lvProjectReferences.Size = new System.Drawing.Size(405, 201);
            this.lvProjectReferences.TabIndex = 1;
            this.lvProjectReferences.UseCompatibleStateImageBehavior = false;
            this.lvProjectReferences.View = System.Windows.Forms.View.List;
            // 
            // textBox1
            // 
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.textBox1.Location = new System.Drawing.Point(0, 0);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(405, 20);
            this.textBox1.TabIndex = 0;
            this.textBox1.Text = "Project References - If unchecked do not use as reference";
            // 
            // lvFileReferences
            // 
            this.lvFileReferences.CheckBoxes = true;
            this.lvFileReferences.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvFileReferences.Location = new System.Drawing.Point(0, 20);
            this.lvFileReferences.Name = "lvFileReferences";
            this.lvFileReferences.Size = new System.Drawing.Size(405, 205);
            this.lvFileReferences.TabIndex = 1;
            this.lvFileReferences.UseCompatibleStateImageBehavior = false;
            this.lvFileReferences.View = System.Windows.Forms.View.Details;
            // 
            // textBox2
            // 
            this.textBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.textBox2.Location = new System.Drawing.Point(0, 0);
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.Size = new System.Drawing.Size(405, 20);
            this.textBox2.TabIndex = 0;
            this.textBox2.Text = "Solution File References - If unchecked do not use as reference";
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 20);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.tvwProposedFixes);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.lvLacking);
            this.splitContainer3.Panel2.Controls.Add(this.textBox4);
            this.splitContainer3.Size = new System.Drawing.Size(391, 430);
            this.splitContainer3.SplitterDistance = 245;
            this.splitContainer3.TabIndex = 1;
            // 
            // tvwProposedFixes
            // 
            this.tvwProposedFixes.BackColor = System.Drawing.SystemColors.Window;
            this.tvwProposedFixes.CheckBoxes = true;
            this.tvwProposedFixes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvwProposedFixes.FullRowSelect = true;
            this.tvwProposedFixes.HotTracking = true;
            this.tvwProposedFixes.Indent = 19;
            this.tvwProposedFixes.Location = new System.Drawing.Point(0, 0);
            this.tvwProposedFixes.Name = "tvwProposedFixes";
            this.tvwProposedFixes.ShowLines = false;
            this.tvwProposedFixes.ShowPlusMinus = false;
            this.tvwProposedFixes.ShowRootLines = false;
            this.tvwProposedFixes.Size = new System.Drawing.Size(391, 245);
            this.tvwProposedFixes.TabIndex = 2;
            // 
            // lvLacking
            // 
            this.lvLacking.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvLacking.Location = new System.Drawing.Point(0, 20);
            this.lvLacking.Name = "lvLacking";
            this.lvLacking.Size = new System.Drawing.Size(391, 161);
            this.lvLacking.TabIndex = 2;
            this.lvLacking.UseCompatibleStateImageBehavior = false;
            this.lvLacking.View = System.Windows.Forms.View.Details;
            // 
            // textBox4
            // 
            this.textBox4.Dock = System.Windows.Forms.DockStyle.Top;
            this.textBox4.Location = new System.Drawing.Point(0, 0);
            this.textBox4.Name = "textBox4";
            this.textBox4.ReadOnly = true;
            this.textBox4.Size = new System.Drawing.Size(391, 20);
            this.textBox4.TabIndex = 1;
            this.textBox4.Text = "Need these references to fix this solution";
            // 
            // textBox3
            // 
            this.textBox3.Dock = System.Windows.Forms.DockStyle.Top;
            this.textBox3.Location = new System.Drawing.Point(0, 0);
            this.textBox3.Name = "textBox3";
            this.textBox3.ReadOnly = true;
            this.textBox3.Size = new System.Drawing.Size(391, 20);
            this.textBox3.TabIndex = 0;
            this.textBox3.Text = "Projects with Reference Issues - If unchecked do not modify";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "Form1";
            this.Text = "Reference Analyzer";
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.ListView lvProjectReferences;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ListView lvFileReferences;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private CustomizedTreeView tvwProposedFixes;
        private System.Windows.Forms.ListView lvLacking;
        private System.Windows.Forms.TextBox textBox4;
    }
}