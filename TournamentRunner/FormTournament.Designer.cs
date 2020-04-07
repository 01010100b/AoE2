namespace TournamentRunner
{
    partial class FormTournament
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
            this.TextMap = new System.Windows.Forms.TextBox();
            this.TextGames = new System.Windows.Forms.TextBox();
            this.Check1v1 = new System.Windows.Forms.CheckBox();
            this.Check2v2 = new System.Windows.Forms.CheckBox();
            this.Check3v3 = new System.Windows.Forms.CheckBox();
            this.Check4v4 = new System.Windows.Forms.CheckBox();
            this.ButtonLaunch = new System.Windows.Forms.Button();
            this.LabelMap = new System.Windows.Forms.Label();
            this.LabelGames = new System.Windows.Forms.Label();
            this.TextAIName = new System.Windows.Forms.TextBox();
            this.TextAICivs = new System.Windows.Forms.TextBox();
            this.TextOpponent1Civs = new System.Windows.Forms.TextBox();
            this.TextOpponent1Name = new System.Windows.Forms.TextBox();
            this.TextOpponent2Civs = new System.Windows.Forms.TextBox();
            this.TextOpponent2Name = new System.Windows.Forms.TextBox();
            this.TextOpponent3Civs = new System.Windows.Forms.TextBox();
            this.TextOpponent3Name = new System.Windows.Forms.TextBox();
            this.TextOpponent4Civs = new System.Windows.Forms.TextBox();
            this.TextOpponent4Name = new System.Windows.Forms.TextBox();
            this.TextOpponent5Civs = new System.Windows.Forms.TextBox();
            this.TextOpponent5Name = new System.Windows.Forms.TextBox();
            this.LabelAI = new System.Windows.Forms.Label();
            this.LabelCivs = new System.Windows.Forms.Label();
            this.LabelOpponents = new System.Windows.Forms.Label();
            this.RichOutput = new System.Windows.Forms.RichTextBox();
            this.LabelFormat = new System.Windows.Forms.Label();
            this.CheckRecord = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // TextMap
            // 
            this.TextMap.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextMap.Location = new System.Drawing.Point(136, 49);
            this.TextMap.Name = "TextMap";
            this.TextMap.Size = new System.Drawing.Size(118, 26);
            this.TextMap.TabIndex = 0;
            this.TextMap.Text = "9";
            // 
            // TextGames
            // 
            this.TextGames.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextGames.Location = new System.Drawing.Point(136, 81);
            this.TextGames.Name = "TextGames";
            this.TextGames.Size = new System.Drawing.Size(118, 26);
            this.TextGames.TabIndex = 1;
            this.TextGames.Text = "5";
            // 
            // Check1v1
            // 
            this.Check1v1.AutoSize = true;
            this.Check1v1.Checked = true;
            this.Check1v1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Check1v1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Check1v1.Location = new System.Drawing.Point(136, 113);
            this.Check1v1.Name = "Check1v1";
            this.Check1v1.Size = new System.Drawing.Size(53, 24);
            this.Check1v1.TabIndex = 2;
            this.Check1v1.Text = "1v1";
            this.Check1v1.UseVisualStyleBackColor = true;
            // 
            // Check2v2
            // 
            this.Check2v2.AutoSize = true;
            this.Check2v2.Checked = true;
            this.Check2v2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Check2v2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Check2v2.Location = new System.Drawing.Point(195, 115);
            this.Check2v2.Name = "Check2v2";
            this.Check2v2.Size = new System.Drawing.Size(53, 24);
            this.Check2v2.TabIndex = 3;
            this.Check2v2.Text = "2v2";
            this.Check2v2.UseVisualStyleBackColor = true;
            // 
            // Check3v3
            // 
            this.Check3v3.AutoSize = true;
            this.Check3v3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Check3v3.Location = new System.Drawing.Point(136, 145);
            this.Check3v3.Name = "Check3v3";
            this.Check3v3.Size = new System.Drawing.Size(53, 24);
            this.Check3v3.TabIndex = 4;
            this.Check3v3.Text = "3v3";
            this.Check3v3.UseVisualStyleBackColor = true;
            // 
            // Check4v4
            // 
            this.Check4v4.AutoSize = true;
            this.Check4v4.Checked = true;
            this.Check4v4.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Check4v4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Check4v4.Location = new System.Drawing.Point(195, 145);
            this.Check4v4.Name = "Check4v4";
            this.Check4v4.Size = new System.Drawing.Size(53, 24);
            this.Check4v4.TabIndex = 5;
            this.Check4v4.Text = "4v4";
            this.Check4v4.UseVisualStyleBackColor = true;
            // 
            // ButtonLaunch
            // 
            this.ButtonLaunch.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonLaunch.Location = new System.Drawing.Point(39, 242);
            this.ButtonLaunch.Name = "ButtonLaunch";
            this.ButtonLaunch.Size = new System.Drawing.Size(215, 59);
            this.ButtonLaunch.TabIndex = 6;
            this.ButtonLaunch.Text = "Launch";
            this.ButtonLaunch.UseVisualStyleBackColor = true;
            this.ButtonLaunch.Click += new System.EventHandler(this.ButtonLaunch_Click);
            // 
            // LabelMap
            // 
            this.LabelMap.AutoSize = true;
            this.LabelMap.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelMap.Location = new System.Drawing.Point(90, 52);
            this.LabelMap.Name = "LabelMap";
            this.LabelMap.Size = new System.Drawing.Size(40, 20);
            this.LabelMap.TabIndex = 7;
            this.LabelMap.Text = "Map";
            // 
            // LabelGames
            // 
            this.LabelGames.AutoSize = true;
            this.LabelGames.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelGames.Location = new System.Drawing.Point(69, 84);
            this.LabelGames.Name = "LabelGames";
            this.LabelGames.Size = new System.Drawing.Size(61, 20);
            this.LabelGames.TabIndex = 8;
            this.LabelGames.Text = "Games";
            // 
            // TextAIName
            // 
            this.TextAIName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextAIName.Location = new System.Drawing.Point(285, 49);
            this.TextAIName.Name = "TextAIName";
            this.TextAIName.Size = new System.Drawing.Size(268, 26);
            this.TextAIName.TabIndex = 9;
            this.TextAIName.Text = "Binary";
            // 
            // TextAICivs
            // 
            this.TextAICivs.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextAICivs.Location = new System.Drawing.Point(559, 49);
            this.TextAICivs.Name = "TextAICivs";
            this.TextAICivs.Size = new System.Drawing.Size(118, 26);
            this.TextAICivs.TabIndex = 10;
            this.TextAICivs.Text = "19";
            // 
            // TextOpponent1Civs
            // 
            this.TextOpponent1Civs.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextOpponent1Civs.Location = new System.Drawing.Point(559, 111);
            this.TextOpponent1Civs.Name = "TextOpponent1Civs";
            this.TextOpponent1Civs.Size = new System.Drawing.Size(118, 26);
            this.TextOpponent1Civs.TabIndex = 12;
            this.TextOpponent1Civs.Text = "3,5,15,16";
            // 
            // TextOpponent1Name
            // 
            this.TextOpponent1Name.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextOpponent1Name.Location = new System.Drawing.Point(285, 113);
            this.TextOpponent1Name.Name = "TextOpponent1Name";
            this.TextOpponent1Name.Size = new System.Drawing.Size(268, 26);
            this.TextOpponent1Name.TabIndex = 11;
            this.TextOpponent1Name.Text = "Meleon";
            // 
            // TextOpponent2Civs
            // 
            this.TextOpponent2Civs.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextOpponent2Civs.Location = new System.Drawing.Point(559, 143);
            this.TextOpponent2Civs.Name = "TextOpponent2Civs";
            this.TextOpponent2Civs.Size = new System.Drawing.Size(118, 26);
            this.TextOpponent2Civs.TabIndex = 14;
            this.TextOpponent2Civs.Text = "19";
            // 
            // TextOpponent2Name
            // 
            this.TextOpponent2Name.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextOpponent2Name.Location = new System.Drawing.Point(285, 143);
            this.TextOpponent2Name.Name = "TextOpponent2Name";
            this.TextOpponent2Name.Size = new System.Drawing.Size(268, 26);
            this.TextOpponent2Name.TabIndex = 13;
            this.TextOpponent2Name.Text = "UlyssesWK";
            // 
            // TextOpponent3Civs
            // 
            this.TextOpponent3Civs.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextOpponent3Civs.Location = new System.Drawing.Point(559, 175);
            this.TextOpponent3Civs.Name = "TextOpponent3Civs";
            this.TextOpponent3Civs.Size = new System.Drawing.Size(118, 26);
            this.TextOpponent3Civs.TabIndex = 16;
            this.TextOpponent3Civs.Text = "16";
            // 
            // TextOpponent3Name
            // 
            this.TextOpponent3Name.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextOpponent3Name.Location = new System.Drawing.Point(285, 175);
            this.TextOpponent3Name.Name = "TextOpponent3Name";
            this.TextOpponent3Name.Size = new System.Drawing.Size(268, 26);
            this.TextOpponent3Name.TabIndex = 15;
            this.TextOpponent3Name.Text = "Strong Bow v1b";
            // 
            // TextOpponent4Civs
            // 
            this.TextOpponent4Civs.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextOpponent4Civs.Location = new System.Drawing.Point(559, 207);
            this.TextOpponent4Civs.Name = "TextOpponent4Civs";
            this.TextOpponent4Civs.Size = new System.Drawing.Size(118, 26);
            this.TextOpponent4Civs.TabIndex = 18;
            this.TextOpponent4Civs.Text = "19";
            // 
            // TextOpponent4Name
            // 
            this.TextOpponent4Name.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextOpponent4Name.Location = new System.Drawing.Point(285, 207);
            this.TextOpponent4Name.Name = "TextOpponent4Name";
            this.TextOpponent4Name.Size = new System.Drawing.Size(268, 26);
            this.TextOpponent4Name.TabIndex = 17;
            this.TextOpponent4Name.Text = "Kosmos3.00beta2";
            // 
            // TextOpponent5Civs
            // 
            this.TextOpponent5Civs.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextOpponent5Civs.Location = new System.Drawing.Point(559, 239);
            this.TextOpponent5Civs.Name = "TextOpponent5Civs";
            this.TextOpponent5Civs.Size = new System.Drawing.Size(118, 26);
            this.TextOpponent5Civs.TabIndex = 20;
            this.TextOpponent5Civs.Text = "13";
            // 
            // TextOpponent5Name
            // 
            this.TextOpponent5Name.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextOpponent5Name.Location = new System.Drawing.Point(285, 239);
            this.TextOpponent5Name.Name = "TextOpponent5Name";
            this.TextOpponent5Name.Size = new System.Drawing.Size(268, 26);
            this.TextOpponent5Name.TabIndex = 19;
            this.TextOpponent5Name.Text = "InFamous-Celtic";
            // 
            // LabelAI
            // 
            this.LabelAI.AutoSize = true;
            this.LabelAI.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelAI.Location = new System.Drawing.Point(281, 20);
            this.LabelAI.Name = "LabelAI";
            this.LabelAI.Size = new System.Drawing.Size(51, 20);
            this.LabelAI.TabIndex = 21;
            this.LabelAI.Text = "Name";
            // 
            // LabelCivs
            // 
            this.LabelCivs.AutoSize = true;
            this.LabelCivs.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelCivs.Location = new System.Drawing.Point(555, 20);
            this.LabelCivs.Name = "LabelCivs";
            this.LabelCivs.Size = new System.Drawing.Size(38, 20);
            this.LabelCivs.TabIndex = 22;
            this.LabelCivs.Text = "Civs";
            // 
            // LabelOpponents
            // 
            this.LabelOpponents.AutoSize = true;
            this.LabelOpponents.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelOpponents.Location = new System.Drawing.Point(281, 84);
            this.LabelOpponents.Name = "LabelOpponents";
            this.LabelOpponents.Size = new System.Drawing.Size(88, 20);
            this.LabelOpponents.TabIndex = 23;
            this.LabelOpponents.Text = "Opponents";
            // 
            // RichOutput
            // 
            this.RichOutput.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RichOutput.Location = new System.Drawing.Point(40, 331);
            this.RichOutput.Name = "RichOutput";
            this.RichOutput.ReadOnly = true;
            this.RichOutput.Size = new System.Drawing.Size(665, 270);
            this.RichOutput.TabIndex = 24;
            this.RichOutput.Text = "";
            // 
            // LabelFormat
            // 
            this.LabelFormat.AutoSize = true;
            this.LabelFormat.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelFormat.Location = new System.Drawing.Point(281, 281);
            this.LabelFormat.Name = "LabelFormat";
            this.LabelFormat.Size = new System.Drawing.Size(298, 20);
            this.LabelFormat.TabIndex = 25;
            this.LabelFormat.Text = "Result format: \"name - opponent: W L D\"";
            // 
            // CheckRecord
            // 
            this.CheckRecord.AutoSize = true;
            this.CheckRecord.Checked = true;
            this.CheckRecord.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CheckRecord.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CheckRecord.Location = new System.Drawing.Point(136, 177);
            this.CheckRecord.Name = "CheckRecord";
            this.CheckRecord.Size = new System.Drawing.Size(80, 24);
            this.CheckRecord.TabIndex = 26;
            this.CheckRecord.Text = "Record";
            this.CheckRecord.UseVisualStyleBackColor = true;
            // 
            // FormTournament
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(783, 628);
            this.Controls.Add(this.CheckRecord);
            this.Controls.Add(this.LabelFormat);
            this.Controls.Add(this.RichOutput);
            this.Controls.Add(this.LabelOpponents);
            this.Controls.Add(this.LabelCivs);
            this.Controls.Add(this.LabelAI);
            this.Controls.Add(this.TextOpponent5Civs);
            this.Controls.Add(this.TextOpponent5Name);
            this.Controls.Add(this.TextOpponent4Civs);
            this.Controls.Add(this.TextOpponent4Name);
            this.Controls.Add(this.TextOpponent3Civs);
            this.Controls.Add(this.TextOpponent3Name);
            this.Controls.Add(this.TextOpponent2Civs);
            this.Controls.Add(this.TextOpponent2Name);
            this.Controls.Add(this.TextOpponent1Civs);
            this.Controls.Add(this.TextOpponent1Name);
            this.Controls.Add(this.TextAICivs);
            this.Controls.Add(this.TextAIName);
            this.Controls.Add(this.LabelGames);
            this.Controls.Add(this.LabelMap);
            this.Controls.Add(this.ButtonLaunch);
            this.Controls.Add(this.Check4v4);
            this.Controls.Add(this.Check3v3);
            this.Controls.Add(this.Check2v2);
            this.Controls.Add(this.Check1v1);
            this.Controls.Add(this.TextGames);
            this.Controls.Add(this.TextMap);
            this.Name = "FormTournament";
            this.Text = "FormTournament";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TextMap;
        private System.Windows.Forms.TextBox TextGames;
        private System.Windows.Forms.CheckBox Check1v1;
        private System.Windows.Forms.CheckBox Check2v2;
        private System.Windows.Forms.CheckBox Check3v3;
        private System.Windows.Forms.CheckBox Check4v4;
        private System.Windows.Forms.Button ButtonLaunch;
        private System.Windows.Forms.Label LabelMap;
        private System.Windows.Forms.Label LabelGames;
        private System.Windows.Forms.TextBox TextAIName;
        private System.Windows.Forms.TextBox TextAICivs;
        private System.Windows.Forms.TextBox TextOpponent1Civs;
        private System.Windows.Forms.TextBox TextOpponent1Name;
        private System.Windows.Forms.TextBox TextOpponent2Civs;
        private System.Windows.Forms.TextBox TextOpponent2Name;
        private System.Windows.Forms.TextBox TextOpponent3Civs;
        private System.Windows.Forms.TextBox TextOpponent3Name;
        private System.Windows.Forms.TextBox TextOpponent4Civs;
        private System.Windows.Forms.TextBox TextOpponent4Name;
        private System.Windows.Forms.TextBox TextOpponent5Civs;
        private System.Windows.Forms.TextBox TextOpponent5Name;
        private System.Windows.Forms.Label LabelAI;
        private System.Windows.Forms.Label LabelCivs;
        private System.Windows.Forms.Label LabelOpponents;
        private System.Windows.Forms.RichTextBox RichOutput;
        private System.Windows.Forms.Label LabelFormat;
        private System.Windows.Forms.CheckBox CheckRecord;
    }
}