namespace ScriptCompiler
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
            this.ButtonAnalyze = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ButtonAnalyze
            // 
            this.ButtonAnalyze.Location = new System.Drawing.Point(42, 40);
            this.ButtonAnalyze.Name = "ButtonAnalyze";
            this.ButtonAnalyze.Size = new System.Drawing.Size(299, 60);
            this.ButtonAnalyze.TabIndex = 0;
            this.ButtonAnalyze.Text = "button1";
            this.ButtonAnalyze.UseVisualStyleBackColor = true;
            this.ButtonAnalyze.Click += new System.EventHandler(this.ButtonAnalyze_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.ButtonAnalyze);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button ButtonAnalyze;
    }
}

