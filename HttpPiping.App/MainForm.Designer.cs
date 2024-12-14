namespace HttpPiping.App
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            label1 = new Label();
            StartButton = new Button();
            StopButton = new Button();
            PortInput = new NumericUpDown();
            LogBox = new TextBox();
            ClearLogsButton = new Button();
            TraceEnabledCheckBox = new CheckBox();
            ((System.ComponentModel.ISupportInitialize)PortInput).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 19);
            label1.Name = "label1";
            label1.Size = new Size(42, 15);
            label1.TabIndex = 0;
            label1.Text = "Port:";
            // 
            // StartButton
            // 
            StartButton.FlatStyle = FlatStyle.Flat;
            StartButton.Font = new Font("Consolas", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            StartButton.ForeColor = Color.Green;
            StartButton.Location = new Point(137, 13);
            StartButton.Name = "StartButton";
            StartButton.Size = new Size(75, 28);
            StartButton.TabIndex = 2;
            StartButton.Text = "Start";
            StartButton.UseVisualStyleBackColor = true;
            StartButton.Click += StartButton_Click;
            // 
            // StopButton
            // 
            StopButton.FlatStyle = FlatStyle.Flat;
            StopButton.Font = new Font("Consolas", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            StopButton.ForeColor = Color.Red;
            StopButton.Location = new Point(218, 13);
            StopButton.Name = "StopButton";
            StopButton.Size = new Size(75, 28);
            StopButton.TabIndex = 2;
            StopButton.Text = "Stop";
            StopButton.UseVisualStyleBackColor = true;
            StopButton.Click += StopButton_Click;
            // 
            // PortInput
            // 
            PortInput.Location = new Point(60, 15);
            PortInput.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            PortInput.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            PortInput.Name = "PortInput";
            PortInput.Size = new Size(71, 23);
            PortInput.TabIndex = 3;
            PortInput.TextAlign = HorizontalAlignment.Center;
            PortInput.Value = new decimal(new int[] { 2080, 0, 0, 0 });
            // 
            // LogBox
            // 
            LogBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            LogBox.BackColor = Color.FromArgb(64, 64, 64);
            LogBox.BorderStyle = BorderStyle.None;
            LogBox.ForeColor = SystemColors.Info;
            LogBox.Location = new Point(12, 47);
            LogBox.Multiline = true;
            LogBox.Name = "LogBox";
            LogBox.ScrollBars = ScrollBars.Both;
            LogBox.Size = new Size(882, 579);
            LogBox.TabIndex = 4;
            // 
            // ClearLogsButton
            // 
            ClearLogsButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ClearLogsButton.FlatStyle = FlatStyle.Flat;
            ClearLogsButton.Location = new Point(802, 12);
            ClearLogsButton.Name = "ClearLogsButton";
            ClearLogsButton.Size = new Size(92, 28);
            ClearLogsButton.TabIndex = 2;
            ClearLogsButton.Text = "Clear Logs";
            ClearLogsButton.UseVisualStyleBackColor = true;
            ClearLogsButton.Click += ClearLogsButton_Click;
            // 
            // TraceEnabledCheckBox
            // 
            TraceEnabledCheckBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            TraceEnabledCheckBox.AutoSize = true;
            TraceEnabledCheckBox.Location = new Point(735, 17);
            TraceEnabledCheckBox.Name = "TraceEnabledCheckBox";
            TraceEnabledCheckBox.Size = new Size(61, 19);
            TraceEnabledCheckBox.TabIndex = 5;
            TraceEnabledCheckBox.Text = "Trace";
            TraceEnabledCheckBox.UseVisualStyleBackColor = true;
            TraceEnabledCheckBox.CheckedChanged += TraceEnabledCheckBox_CheckedChanged;
            // 
            // MainForm
            // 
            AutoScaleMode = AutoScaleMode.None;
            BackColor = Color.DarkGray;
            ClientSize = new Size(906, 638);
            Controls.Add(TraceEnabledCheckBox);
            Controls.Add(LogBox);
            Controls.Add(PortInput);
            Controls.Add(ClearLogsButton);
            Controls.Add(StopButton);
            Controls.Add(StartButton);
            Controls.Add(label1);
            Font = new Font("Consolas", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "MainForm";
            Text = "Http Piping";
            ((System.ComponentModel.ISupportInitialize)PortInput).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Button StartButton;
        private Button StopButton;
        private NumericUpDown PortInput;
        private TextBox LogBox;
        private Button ClearLogsButton;
        private CheckBox TraceEnabledCheckBox;
    }
}
