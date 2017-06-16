namespace GBE
{
    partial class GoodBoyEUI
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
            this.runButton = new System.Windows.Forms.Button();
            this.restartButton = new System.Windows.Forms.Button();
            this.upButton = new System.Windows.Forms.Button();
            this.leftButton = new System.Windows.Forms.Button();
            this.rightButton = new System.Windows.Forms.Button();
            this.downButton = new System.Windows.Forms.Button();
            this.bButton = new System.Windows.Forms.Button();
            this.aButton = new System.Windows.Forms.Button();
            this.selectButton = new System.Windows.Forms.Button();
            this.startButton = new System.Windows.Forms.Button();
            this.emulator = new GBE.Emulation.GameBoyEmulator();
            this.SuspendLayout();
            // 
            // runButton
            // 
            this.runButton.Location = new System.Drawing.Point(180, 12);
            this.runButton.Name = "runButton";
            this.runButton.Size = new System.Drawing.Size(75, 23);
            this.runButton.TabIndex = 1;
            this.runButton.Text = "Run";
            this.runButton.UseVisualStyleBackColor = true;
            this.runButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // restartButton
            // 
            this.restartButton.Location = new System.Drawing.Point(180, 41);
            this.restartButton.Name = "restartButton";
            this.restartButton.Size = new System.Drawing.Size(75, 23);
            this.restartButton.TabIndex = 2;
            this.restartButton.Text = "Restart";
            this.restartButton.UseVisualStyleBackColor = true;
            this.restartButton.Click += new System.EventHandler(this.restartButton_Click);
            // 
            // upButton
            // 
            this.upButton.Location = new System.Drawing.Point(48, 164);
            this.upButton.Name = "upButton";
            this.upButton.Size = new System.Drawing.Size(30, 30);
            this.upButton.TabIndex = 3;
            this.upButton.Text = "/\\";
            this.upButton.UseVisualStyleBackColor = true;
            this.upButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.controlButton_MouseDown);
            this.upButton.MouseUp += new System.Windows.Forms.MouseEventHandler(this.controlButton_MouseUp);
            // 
            // leftButton
            // 
            this.leftButton.Location = new System.Drawing.Point(12, 200);
            this.leftButton.Name = "leftButton";
            this.leftButton.Size = new System.Drawing.Size(30, 30);
            this.leftButton.TabIndex = 7;
            this.leftButton.Text = "<";
            this.leftButton.UseVisualStyleBackColor = true;
            this.leftButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.controlButton_MouseDown);
            this.leftButton.MouseUp += new System.Windows.Forms.MouseEventHandler(this.controlButton_MouseUp);
            // 
            // rightButton
            // 
            this.rightButton.Location = new System.Drawing.Point(84, 200);
            this.rightButton.Name = "rightButton";
            this.rightButton.Size = new System.Drawing.Size(30, 30);
            this.rightButton.TabIndex = 9;
            this.rightButton.Text = ">";
            this.rightButton.UseVisualStyleBackColor = true;
            this.rightButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.controlButton_MouseDown);
            this.rightButton.MouseUp += new System.Windows.Forms.MouseEventHandler(this.controlButton_MouseUp);
            // 
            // downButton
            // 
            this.downButton.Location = new System.Drawing.Point(48, 236);
            this.downButton.Name = "downButton";
            this.downButton.Size = new System.Drawing.Size(30, 30);
            this.downButton.TabIndex = 11;
            this.downButton.Text = "\\/";
            this.downButton.UseVisualStyleBackColor = true;
            this.downButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.controlButton_MouseDown);
            this.downButton.MouseUp += new System.Windows.Forms.MouseEventHandler(this.controlButton_MouseUp);
            // 
            // bButton
            // 
            this.bButton.Location = new System.Drawing.Point(144, 200);
            this.bButton.Name = "bButton";
            this.bButton.Size = new System.Drawing.Size(30, 30);
            this.bButton.TabIndex = 13;
            this.bButton.Text = "B";
            this.bButton.UseVisualStyleBackColor = true;
            this.bButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.controlButton_MouseDown);
            this.bButton.MouseUp += new System.Windows.Forms.MouseEventHandler(this.controlButton_MouseUp);
            // 
            // aButton
            // 
            this.aButton.Location = new System.Drawing.Point(180, 200);
            this.aButton.Name = "aButton";
            this.aButton.Size = new System.Drawing.Size(30, 30);
            this.aButton.TabIndex = 14;
            this.aButton.Text = "A";
            this.aButton.UseVisualStyleBackColor = true;
            this.aButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.controlButton_MouseDown);
            this.aButton.MouseUp += new System.Windows.Forms.MouseEventHandler(this.controlButton_MouseUp);
            // 
            // selectButton
            // 
            this.selectButton.Location = new System.Drawing.Point(39, 272);
            this.selectButton.Name = "selectButton";
            this.selectButton.Size = new System.Drawing.Size(75, 23);
            this.selectButton.TabIndex = 15;
            this.selectButton.Text = "Select";
            this.selectButton.UseVisualStyleBackColor = true;
            this.selectButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.controlButton_MouseDown);
            this.selectButton.MouseUp += new System.Windows.Forms.MouseEventHandler(this.controlButton_MouseUp);
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(120, 272);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(75, 23);
            this.startButton.TabIndex = 16;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.controlButton_MouseDown);
            this.startButton.MouseUp += new System.Windows.Forms.MouseEventHandler(this.controlButton_MouseUp);
            // 
            // emulator
            // 
            this.emulator.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.emulator.DebugMode = true;
            this.emulator.Location = new System.Drawing.Point(12, 12);
            this.emulator.MinimumSize = new System.Drawing.Size(162, 146);
            this.emulator.Name = "emulator";
            this.emulator.Size = new System.Drawing.Size(162, 146);
            this.emulator.TabIndex = 0;
            // 
            // GoodBoyEUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(266, 325);
            this.Controls.Add(this.startButton);
            this.Controls.Add(this.selectButton);
            this.Controls.Add(this.aButton);
            this.Controls.Add(this.bButton);
            this.Controls.Add(this.downButton);
            this.Controls.Add(this.rightButton);
            this.Controls.Add(this.leftButton);
            this.Controls.Add(this.upButton);
            this.Controls.Add(this.restartButton);
            this.Controls.Add(this.runButton);
            this.Controls.Add(this.emulator);
            this.KeyPreview = true;
            this.Name = "GoodBoyEUI";
            this.Text = "GBE";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GoodBoyEUI_FormClosing);
            this.Load += new System.EventHandler(this.GoodBoyEUI_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private Emulation.GameBoyEmulator emulator;
        private System.Windows.Forms.Button runButton;
        private System.Windows.Forms.Button restartButton;
        private System.Windows.Forms.Button upButton;
        private System.Windows.Forms.Button leftButton;
        private System.Windows.Forms.Button rightButton;
        private System.Windows.Forms.Button downButton;
        private System.Windows.Forms.Button bButton;
        private System.Windows.Forms.Button aButton;
        private System.Windows.Forms.Button selectButton;
        private System.Windows.Forms.Button startButton;
    }
}

