namespace SirenSettingInstaller
{
    partial class MainWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            title = new Label();
            subtitle = new Label();
            mainPG = new ProgressBar();
            stateLabel = new Label();
            versionLabel = new Label();
            SuspendLayout();
            // 
            // title
            // 
            title.AutoSize = true;
            title.Font = new Font("Segoe UI", 20.25F, FontStyle.Bold, GraphicsUnit.Point);
            title.Location = new Point(196, 9);
            title.Name = "title";
            title.Size = new Size(476, 37);
            title.TabIndex = 0;
            title.Text = "SirenSetting Limit Adjuster Installer";
            // 
            // subtitle
            // 
            subtitle.AutoSize = true;
            subtitle.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point);
            subtitle.ForeColor = Color.Red;
            subtitle.Location = new Point(281, 46);
            subtitle.Name = "subtitle";
            subtitle.Size = new Size(293, 25);
            subtitle.TabIndex = 1;
            subtitle.Text = "Please do not close this window";
            // 
            // mainPG
            // 
            mainPG.Location = new Point(196, 89);
            mainPG.Name = "mainPG";
            mainPG.Size = new Size(476, 23);
            mainPG.TabIndex = 2;
            // 
            // stateLabel
            // 
            stateLabel.AutoSize = true;
            stateLabel.Location = new Point(399, 115);
            stateLabel.Name = "stateLabel";
            stateLabel.Size = new Size(74, 15);
            stateLabel.TabIndex = 3;
            stateLabel.Text = "Please wait...";
            // 
            // versionLabel
            // 
            versionLabel.AutoSize = true;
            versionLabel.Location = new Point(661, 9);
            versionLabel.Name = "versionLabel";
            versionLabel.Size = new Size(28, 15);
            versionLabel.TabIndex = 4;
            versionLabel.Text = "v1.0";
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(848, 136);
            ControlBox = false;
            Controls.Add(versionLabel);
            Controls.Add(stateLabel);
            Controls.Add(mainPG);
            Controls.Add(subtitle);
            Controls.Add(title);
            HelpButton = true;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "MainWindow";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SirenSetting Limit Adjuster Installer";
            FormClosed += MainWindow_FormClosed;
            Shown += MainWindow_Shown;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label title;
        private Label subtitle;
        private ProgressBar mainPG;
        private Label stateLabel;
        private Label versionLabel;
    }
}