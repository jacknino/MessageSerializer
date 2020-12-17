namespace MessageSerializerClassFileCreator
{
    partial class FormMain
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
            this.splitContainerDllToRest = new System.Windows.Forms.SplitContainer();
            this.buttonSelectDll = new System.Windows.Forms.Button();
            this.textBoxDll = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.splitContainerClassesToRest = new System.Windows.Forms.SplitContainer();
            this.groupBoxClasses = new System.Windows.Forms.GroupBox();
            this.listBoxClasses = new System.Windows.Forms.ListBox();
            this.splitContainerOutputDetailsToRest = new System.Windows.Forms.SplitContainer();
            this.textBoxOutputFilename = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonOutputDirectory = new System.Windows.Forms.Button();
            this.textBoxOutputDirectory = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.splitContainerStatusToRest = new System.Windows.Forms.SplitContainer();
            this.richTextBoxStatus = new System.Windows.Forms.RichTextBox();
            this.buttonProcess = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerDllToRest)).BeginInit();
            this.splitContainerDllToRest.Panel1.SuspendLayout();
            this.splitContainerDllToRest.Panel2.SuspendLayout();
            this.splitContainerDllToRest.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerClassesToRest)).BeginInit();
            this.splitContainerClassesToRest.Panel1.SuspendLayout();
            this.splitContainerClassesToRest.Panel2.SuspendLayout();
            this.splitContainerClassesToRest.SuspendLayout();
            this.groupBoxClasses.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerOutputDetailsToRest)).BeginInit();
            this.splitContainerOutputDetailsToRest.Panel1.SuspendLayout();
            this.splitContainerOutputDetailsToRest.Panel2.SuspendLayout();
            this.splitContainerOutputDetailsToRest.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerStatusToRest)).BeginInit();
            this.splitContainerStatusToRest.Panel1.SuspendLayout();
            this.splitContainerStatusToRest.Panel2.SuspendLayout();
            this.splitContainerStatusToRest.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainerDllToRest
            // 
            this.splitContainerDllToRest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerDllToRest.IsSplitterFixed = true;
            this.splitContainerDllToRest.Location = new System.Drawing.Point(0, 0);
            this.splitContainerDllToRest.Name = "splitContainerDllToRest";
            this.splitContainerDllToRest.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerDllToRest.Panel1
            // 
            this.splitContainerDllToRest.Panel1.Controls.Add(this.buttonSelectDll);
            this.splitContainerDllToRest.Panel1.Controls.Add(this.textBoxDll);
            this.splitContainerDllToRest.Panel1.Controls.Add(this.label1);
            // 
            // splitContainerDllToRest.Panel2
            // 
            this.splitContainerDllToRest.Panel2.Controls.Add(this.splitContainerClassesToRest);
            this.splitContainerDllToRest.Size = new System.Drawing.Size(1305, 629);
            this.splitContainerDllToRest.SplitterDistance = 28;
            this.splitContainerDllToRest.TabIndex = 0;
            // 
            // buttonSelectDll
            // 
            this.buttonSelectDll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSelectDll.Location = new System.Drawing.Point(1278, 2);
            this.buttonSelectDll.Name = "buttonSelectDll";
            this.buttonSelectDll.Size = new System.Drawing.Size(24, 23);
            this.buttonSelectDll.TabIndex = 2;
            this.buttonSelectDll.Text = "&...";
            this.buttonSelectDll.UseVisualStyleBackColor = true;
            this.buttonSelectDll.Click += new System.EventHandler(this.buttonSelectDll_Click);
            // 
            // textBoxDll
            // 
            this.textBoxDll.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxDll.Location = new System.Drawing.Point(95, 4);
            this.textBoxDll.Name = "textBoxDll";
            this.textBoxDll.Size = new System.Drawing.Size(1177, 20);
            this.textBoxDll.TabIndex = 1;
            this.textBoxDll.Enter += new System.EventHandler(this.textBoxDll_Enter);
            this.textBoxDll.Leave += new System.EventHandler(this.textBoxDll_Leave);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(62, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(27, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "DLL";
            // 
            // splitContainerClassesToRest
            // 
            this.splitContainerClassesToRest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerClassesToRest.Location = new System.Drawing.Point(0, 0);
            this.splitContainerClassesToRest.Name = "splitContainerClassesToRest";
            this.splitContainerClassesToRest.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerClassesToRest.Panel1
            // 
            this.splitContainerClassesToRest.Panel1.Controls.Add(this.groupBoxClasses);
            // 
            // splitContainerClassesToRest.Panel2
            // 
            this.splitContainerClassesToRest.Panel2.Controls.Add(this.splitContainerOutputDetailsToRest);
            this.splitContainerClassesToRest.Size = new System.Drawing.Size(1305, 597);
            this.splitContainerClassesToRest.SplitterDistance = 157;
            this.splitContainerClassesToRest.TabIndex = 0;
            // 
            // groupBoxClasses
            // 
            this.groupBoxClasses.Controls.Add(this.listBoxClasses);
            this.groupBoxClasses.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxClasses.Location = new System.Drawing.Point(0, 0);
            this.groupBoxClasses.Name = "groupBoxClasses";
            this.groupBoxClasses.Size = new System.Drawing.Size(1305, 157);
            this.groupBoxClasses.TabIndex = 0;
            this.groupBoxClasses.TabStop = false;
            this.groupBoxClasses.Text = "Classes";
            // 
            // listBoxClasses
            // 
            this.listBoxClasses.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxClasses.FormattingEnabled = true;
            this.listBoxClasses.Location = new System.Drawing.Point(3, 16);
            this.listBoxClasses.Name = "listBoxClasses";
            this.listBoxClasses.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBoxClasses.Size = new System.Drawing.Size(1299, 138);
            this.listBoxClasses.TabIndex = 0;
            // 
            // splitContainerOutputDetailsToRest
            // 
            this.splitContainerOutputDetailsToRest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerOutputDetailsToRest.IsSplitterFixed = true;
            this.splitContainerOutputDetailsToRest.Location = new System.Drawing.Point(0, 0);
            this.splitContainerOutputDetailsToRest.Name = "splitContainerOutputDetailsToRest";
            this.splitContainerOutputDetailsToRest.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerOutputDetailsToRest.Panel1
            // 
            this.splitContainerOutputDetailsToRest.Panel1.Controls.Add(this.textBoxOutputFilename);
            this.splitContainerOutputDetailsToRest.Panel1.Controls.Add(this.label3);
            this.splitContainerOutputDetailsToRest.Panel1.Controls.Add(this.buttonOutputDirectory);
            this.splitContainerOutputDetailsToRest.Panel1.Controls.Add(this.textBoxOutputDirectory);
            this.splitContainerOutputDetailsToRest.Panel1.Controls.Add(this.label2);
            // 
            // splitContainerOutputDetailsToRest.Panel2
            // 
            this.splitContainerOutputDetailsToRest.Panel2.Controls.Add(this.splitContainerStatusToRest);
            this.splitContainerOutputDetailsToRest.Size = new System.Drawing.Size(1305, 436);
            this.splitContainerOutputDetailsToRest.SplitterDistance = 52;
            this.splitContainerOutputDetailsToRest.TabIndex = 0;
            // 
            // textBoxOutputFilename
            // 
            this.textBoxOutputFilename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxOutputFilename.Location = new System.Drawing.Point(95, 30);
            this.textBoxOutputFilename.Name = "textBoxOutputFilename";
            this.textBoxOutputFilename.Size = new System.Drawing.Size(1177, 20);
            this.textBoxOutputFilename.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 33);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(84, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Output Filename";
            // 
            // buttonOutputDirectory
            // 
            this.buttonOutputDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOutputDirectory.Location = new System.Drawing.Point(1278, 2);
            this.buttonOutputDirectory.Name = "buttonOutputDirectory";
            this.buttonOutputDirectory.Size = new System.Drawing.Size(24, 23);
            this.buttonOutputDirectory.TabIndex = 3;
            this.buttonOutputDirectory.Text = "&...";
            this.buttonOutputDirectory.UseVisualStyleBackColor = true;
            this.buttonOutputDirectory.Click += new System.EventHandler(this.buttonOutputDirectory_Click);
            // 
            // textBoxOutputDirectory
            // 
            this.textBoxOutputDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxOutputDirectory.Location = new System.Drawing.Point(95, 4);
            this.textBoxOutputDirectory.Name = "textBoxOutputDirectory";
            this.textBoxOutputDirectory.Size = new System.Drawing.Size(1177, 20);
            this.textBoxOutputDirectory.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(84, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Output Directory";
            // 
            // splitContainerStatusToRest
            // 
            this.splitContainerStatusToRest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerStatusToRest.IsSplitterFixed = true;
            this.splitContainerStatusToRest.Location = new System.Drawing.Point(0, 0);
            this.splitContainerStatusToRest.Name = "splitContainerStatusToRest";
            this.splitContainerStatusToRest.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerStatusToRest.Panel1
            // 
            this.splitContainerStatusToRest.Panel1.Controls.Add(this.richTextBoxStatus);
            // 
            // splitContainerStatusToRest.Panel2
            // 
            this.splitContainerStatusToRest.Panel2.Controls.Add(this.buttonProcess);
            this.splitContainerStatusToRest.Size = new System.Drawing.Size(1305, 380);
            this.splitContainerStatusToRest.SplitterDistance = 347;
            this.splitContainerStatusToRest.TabIndex = 0;
            // 
            // richTextBoxStatus
            // 
            this.richTextBoxStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxStatus.Location = new System.Drawing.Point(0, 0);
            this.richTextBoxStatus.Name = "richTextBoxStatus";
            this.richTextBoxStatus.Size = new System.Drawing.Size(1305, 347);
            this.richTextBoxStatus.TabIndex = 0;
            this.richTextBoxStatus.Text = "";
            // 
            // buttonProcess
            // 
            this.buttonProcess.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonProcess.Location = new System.Drawing.Point(1230, 6);
            this.buttonProcess.Name = "buttonProcess";
            this.buttonProcess.Size = new System.Drawing.Size(75, 23);
            this.buttonProcess.TabIndex = 0;
            this.buttonProcess.Text = "&Process";
            this.buttonProcess.UseVisualStyleBackColor = true;
            this.buttonProcess.Click += new System.EventHandler(this.buttonProcess_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1305, 629);
            this.Controls.Add(this.splitContainerDllToRest);
            this.Name = "FormMain";
            this.Text = "MessageSerializer Class File Creator";
            this.Shown += new System.EventHandler(this.FormMain_Shown);
            this.splitContainerDllToRest.Panel1.ResumeLayout(false);
            this.splitContainerDllToRest.Panel1.PerformLayout();
            this.splitContainerDllToRest.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerDllToRest)).EndInit();
            this.splitContainerDllToRest.ResumeLayout(false);
            this.splitContainerClassesToRest.Panel1.ResumeLayout(false);
            this.splitContainerClassesToRest.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerClassesToRest)).EndInit();
            this.splitContainerClassesToRest.ResumeLayout(false);
            this.groupBoxClasses.ResumeLayout(false);
            this.splitContainerOutputDetailsToRest.Panel1.ResumeLayout(false);
            this.splitContainerOutputDetailsToRest.Panel1.PerformLayout();
            this.splitContainerOutputDetailsToRest.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerOutputDetailsToRest)).EndInit();
            this.splitContainerOutputDetailsToRest.ResumeLayout(false);
            this.splitContainerStatusToRest.Panel1.ResumeLayout(false);
            this.splitContainerStatusToRest.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerStatusToRest)).EndInit();
            this.splitContainerStatusToRest.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainerDllToRest;
        private System.Windows.Forms.Button buttonSelectDll;
        private System.Windows.Forms.TextBox textBoxDll;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.SplitContainer splitContainerClassesToRest;
        private System.Windows.Forms.GroupBox groupBoxClasses;
        private System.Windows.Forms.ListBox listBoxClasses;
        private System.Windows.Forms.SplitContainer splitContainerOutputDetailsToRest;
        private System.Windows.Forms.Button buttonOutputDirectory;
        private System.Windows.Forms.TextBox textBoxOutputDirectory;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxOutputFilename;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.SplitContainer splitContainerStatusToRest;
        private System.Windows.Forms.RichTextBox richTextBoxStatus;
        private System.Windows.Forms.Button buttonProcess;
    }
}

