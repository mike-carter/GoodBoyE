namespace GBE
{
    partial class GBEDebugger
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
            System.Windows.Forms.Label labelAF;
            System.Windows.Forms.Label labelHL;
            System.Windows.Forms.Label labelBC;
            System.Windows.Forms.Label labelPC;
            System.Windows.Forms.Label labelDE;
            System.Windows.Forms.Label labelSP;
            this.disassemblyBox = new System.Windows.Forms.ListBox();
            this.startButton = new System.Windows.Forms.Button();
            this.selectButton = new System.Windows.Forms.Button();
            this.aButton = new System.Windows.Forms.Button();
            this.bButton = new System.Windows.Forms.Button();
            this.downButton = new System.Windows.Forms.Button();
            this.rightButton = new System.Windows.Forms.Button();
            this.leftButton = new System.Windows.Forms.Button();
            this.upButton = new System.Windows.Forms.Button();
            this.cbHold = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.spValue = new System.Windows.Forms.Label();
            this.pcValue = new System.Windows.Forms.Label();
            this.hlValue = new System.Windows.Forms.Label();
            this.deValue = new System.Windows.Forms.Label();
            this.bcValue = new System.Windows.Forms.Label();
            this.afValue = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.cbZero = new System.Windows.Forms.CheckBox();
            this.cbOp = new System.Windows.Forms.CheckBox();
            this.cbHalfCarry = new System.Windows.Forms.CheckBox();
            this.cbCarry = new System.Windows.Forms.CheckBox();
            this.runButton = new System.Windows.Forms.Button();
            this.stepButton = new System.Windows.Forms.Button();
            this.emulator = new GBE.Emulation.GameBoyEmulator();
            this.nextLineButton = new System.Windows.Forms.Button();
            this.breakpointsBox = new System.Windows.Forms.ListBox();
            this.pauseButton = new System.Windows.Forms.Button();
            labelAF = new System.Windows.Forms.Label();
            labelHL = new System.Windows.Forms.Label();
            labelBC = new System.Windows.Forms.Label();
            labelPC = new System.Windows.Forms.Label();
            labelDE = new System.Windows.Forms.Label();
            labelSP = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelAF
            // 
            labelAF.AutoSize = true;
            labelAF.Location = new System.Drawing.Point(3, 0);
            labelAF.Name = "labelAF";
            labelAF.Size = new System.Drawing.Size(28, 14);
            labelAF.TabIndex = 0;
            labelAF.Text = "AF:";
            // 
            // labelHL
            // 
            labelHL.AutoSize = true;
            labelHL.Location = new System.Drawing.Point(3, 54);
            labelHL.Name = "labelHL";
            labelHL.Size = new System.Drawing.Size(28, 14);
            labelHL.TabIndex = 1;
            labelHL.Text = "HL:";
            // 
            // labelBC
            // 
            labelBC.AutoSize = true;
            labelBC.Location = new System.Drawing.Point(3, 18);
            labelBC.Name = "labelBC";
            labelBC.Size = new System.Drawing.Size(28, 14);
            labelBC.TabIndex = 2;
            labelBC.Text = "BC:";
            // 
            // labelPC
            // 
            labelPC.AutoSize = true;
            labelPC.Location = new System.Drawing.Point(3, 72);
            labelPC.Name = "labelPC";
            labelPC.Size = new System.Drawing.Size(28, 14);
            labelPC.TabIndex = 3;
            labelPC.Text = "PC:";
            // 
            // labelDE
            // 
            labelDE.AutoSize = true;
            labelDE.Location = new System.Drawing.Point(3, 36);
            labelDE.Name = "labelDE";
            labelDE.Size = new System.Drawing.Size(28, 14);
            labelDE.TabIndex = 4;
            labelDE.Text = "DE:";
            // 
            // labelSP
            // 
            labelSP.AutoSize = true;
            labelSP.Location = new System.Drawing.Point(3, 90);
            labelSP.Name = "labelSP";
            labelSP.Size = new System.Drawing.Size(28, 14);
            labelSP.TabIndex = 5;
            labelSP.Text = "SP:";
            // 
            // disassemblyBox
            // 
            this.disassemblyBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.disassemblyBox.FormattingEnabled = true;
            this.disassemblyBox.ItemHeight = 14;
            this.disassemblyBox.Location = new System.Drawing.Point(12, 12);
            this.disassemblyBox.Name = "disassemblyBox";
            this.disassemblyBox.Size = new System.Drawing.Size(283, 396);
            this.disassemblyBox.TabIndex = 0;
            this.disassemblyBox.DoubleClick += new System.EventHandler(this.disassemblyBox_DoubleClick);
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(581, 122);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(75, 23);
            this.startButton.TabIndex = 24;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            // 
            // selectButton
            // 
            this.selectButton.Location = new System.Drawing.Point(500, 122);
            this.selectButton.Name = "selectButton";
            this.selectButton.Size = new System.Drawing.Size(75, 23);
            this.selectButton.TabIndex = 23;
            this.selectButton.Text = "Select";
            this.selectButton.UseVisualStyleBackColor = true;
            // 
            // aButton
            // 
            this.aButton.Location = new System.Drawing.Point(641, 50);
            this.aButton.Name = "aButton";
            this.aButton.Size = new System.Drawing.Size(30, 30);
            this.aButton.TabIndex = 22;
            this.aButton.Text = "A";
            this.aButton.UseVisualStyleBackColor = true;
            // 
            // bButton
            // 
            this.bButton.Location = new System.Drawing.Point(605, 50);
            this.bButton.Name = "bButton";
            this.bButton.Size = new System.Drawing.Size(30, 30);
            this.bButton.TabIndex = 21;
            this.bButton.Text = "B";
            this.bButton.UseVisualStyleBackColor = true;
            // 
            // downButton
            // 
            this.downButton.Location = new System.Drawing.Point(509, 86);
            this.downButton.Name = "downButton";
            this.downButton.Size = new System.Drawing.Size(30, 30);
            this.downButton.TabIndex = 20;
            this.downButton.Text = "\\/";
            this.downButton.UseVisualStyleBackColor = true;
            // 
            // rightButton
            // 
            this.rightButton.Location = new System.Drawing.Point(545, 50);
            this.rightButton.Name = "rightButton";
            this.rightButton.Size = new System.Drawing.Size(30, 30);
            this.rightButton.TabIndex = 19;
            this.rightButton.Text = ">";
            this.rightButton.UseVisualStyleBackColor = true;
            // 
            // leftButton
            // 
            this.leftButton.Location = new System.Drawing.Point(473, 50);
            this.leftButton.Name = "leftButton";
            this.leftButton.Size = new System.Drawing.Size(30, 30);
            this.leftButton.TabIndex = 18;
            this.leftButton.Text = "<";
            this.leftButton.UseVisualStyleBackColor = true;
            // 
            // upButton
            // 
            this.upButton.Location = new System.Drawing.Point(509, 14);
            this.upButton.Name = "upButton";
            this.upButton.Size = new System.Drawing.Size(30, 30);
            this.upButton.TabIndex = 17;
            this.upButton.Text = "/\\";
            this.upButton.UseVisualStyleBackColor = true;
            // 
            // cbHold
            // 
            this.cbHold.AutoSize = true;
            this.cbHold.Location = new System.Drawing.Point(605, 22);
            this.cbHold.Name = "cbHold";
            this.cbHold.Size = new System.Drawing.Size(48, 17);
            this.cbHold.TabIndex = 25;
            this.cbHold.Text = "Hold";
            this.cbHold.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.spValue, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.pcValue, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.hlValue, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.deValue, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.bcValue, 1, 1);
            this.tableLayoutPanel1.Controls.Add(labelDE, 0, 2);
            this.tableLayoutPanel1.Controls.Add(labelBC, 0, 1);
            this.tableLayoutPanel1.Controls.Add(labelAF, 0, 0);
            this.tableLayoutPanel1.Controls.Add(labelHL, 0, 3);
            this.tableLayoutPanel1.Controls.Add(labelPC, 0, 4);
            this.tableLayoutPanel1.Controls.Add(labelSP, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.afValue, 1, 0);
            this.tableLayoutPanel1.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tableLayoutPanel1.Location = new System.Drawing.Point(301, 162);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(120, 109);
            this.tableLayoutPanel1.TabIndex = 26;
            // 
            // spValue
            // 
            this.spValue.AutoSize = true;
            this.spValue.Location = new System.Drawing.Point(37, 90);
            this.spValue.Name = "spValue";
            this.spValue.Size = new System.Drawing.Size(35, 14);
            this.spValue.TabIndex = 11;
            this.spValue.Text = "0000";
            // 
            // pcValue
            // 
            this.pcValue.AutoSize = true;
            this.pcValue.Location = new System.Drawing.Point(37, 72);
            this.pcValue.Name = "pcValue";
            this.pcValue.Size = new System.Drawing.Size(35, 14);
            this.pcValue.TabIndex = 10;
            this.pcValue.Text = "0000";
            // 
            // hlValue
            // 
            this.hlValue.AutoSize = true;
            this.hlValue.Location = new System.Drawing.Point(37, 54);
            this.hlValue.Name = "hlValue";
            this.hlValue.Size = new System.Drawing.Size(35, 14);
            this.hlValue.TabIndex = 9;
            this.hlValue.Text = "0000";
            // 
            // deValue
            // 
            this.deValue.AutoSize = true;
            this.deValue.Location = new System.Drawing.Point(37, 36);
            this.deValue.Name = "deValue";
            this.deValue.Size = new System.Drawing.Size(35, 14);
            this.deValue.TabIndex = 8;
            this.deValue.Text = "0000";
            // 
            // bcValue
            // 
            this.bcValue.AutoSize = true;
            this.bcValue.Location = new System.Drawing.Point(37, 18);
            this.bcValue.Name = "bcValue";
            this.bcValue.Size = new System.Drawing.Size(35, 14);
            this.bcValue.TabIndex = 7;
            this.bcValue.Text = "0000";
            // 
            // afValue
            // 
            this.afValue.AutoSize = true;
            this.afValue.Location = new System.Drawing.Point(37, 0);
            this.afValue.Name = "afValue";
            this.afValue.Size = new System.Drawing.Size(35, 14);
            this.afValue.TabIndex = 6;
            this.afValue.Text = "0000";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 427);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(683, 22);
            this.statusStrip1.TabIndex = 27;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // cbZero
            // 
            this.cbZero.AutoSize = true;
            this.cbZero.Location = new System.Drawing.Point(427, 161);
            this.cbZero.Name = "cbZero";
            this.cbZero.Size = new System.Drawing.Size(33, 17);
            this.cbZero.TabIndex = 28;
            this.cbZero.Text = "Z";
            this.cbZero.UseVisualStyleBackColor = true;
            // 
            // cbOp
            // 
            this.cbOp.AutoSize = true;
            this.cbOp.Location = new System.Drawing.Point(427, 179);
            this.cbOp.Name = "cbOp";
            this.cbOp.Size = new System.Drawing.Size(34, 17);
            this.cbOp.TabIndex = 29;
            this.cbOp.Text = "N";
            this.cbOp.UseVisualStyleBackColor = true;
            // 
            // cbHalfCarry
            // 
            this.cbHalfCarry.AutoSize = true;
            this.cbHalfCarry.Location = new System.Drawing.Point(427, 197);
            this.cbHalfCarry.Name = "cbHalfCarry";
            this.cbHalfCarry.Size = new System.Drawing.Size(34, 17);
            this.cbHalfCarry.TabIndex = 30;
            this.cbHalfCarry.Text = "H";
            this.cbHalfCarry.UseVisualStyleBackColor = true;
            // 
            // cbCarry
            // 
            this.cbCarry.AutoSize = true;
            this.cbCarry.Location = new System.Drawing.Point(427, 215);
            this.cbCarry.Name = "cbCarry";
            this.cbCarry.Size = new System.Drawing.Size(33, 17);
            this.cbCarry.TabIndex = 31;
            this.cbCarry.Text = "C";
            this.cbCarry.UseVisualStyleBackColor = true;
            // 
            // runButton
            // 
            this.runButton.Location = new System.Drawing.Point(301, 277);
            this.runButton.Name = "runButton";
            this.runButton.Size = new System.Drawing.Size(120, 23);
            this.runButton.TabIndex = 33;
            this.runButton.Text = "Run";
            this.runButton.UseVisualStyleBackColor = true;
            this.runButton.Click += new System.EventHandler(this.runButton_Click);
            // 
            // stepButton
            // 
            this.stepButton.Location = new System.Drawing.Point(301, 306);
            this.stepButton.Name = "stepButton";
            this.stepButton.Size = new System.Drawing.Size(120, 23);
            this.stepButton.TabIndex = 34;
            this.stepButton.Text = "Step";
            this.stepButton.UseVisualStyleBackColor = true;
            this.stepButton.Click += new System.EventHandler(this.stepButton_Click);
            // 
            // emulator
            // 
            this.emulator.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.emulator.DebugMode = true;
            this.emulator.Location = new System.Drawing.Point(301, 9);
            this.emulator.MinimumSize = new System.Drawing.Size(162, 146);
            this.emulator.Name = "emulator";
            this.emulator.Size = new System.Drawing.Size(162, 146);
            this.emulator.TabIndex = 32;
            // 
            // nextLineButton
            // 
            this.nextLineButton.Location = new System.Drawing.Point(301, 335);
            this.nextLineButton.Name = "nextLineButton";
            this.nextLineButton.Size = new System.Drawing.Size(120, 23);
            this.nextLineButton.TabIndex = 35;
            this.nextLineButton.Text = "Run to Next Line";
            this.nextLineButton.UseVisualStyleBackColor = true;
            this.nextLineButton.Click += new System.EventHandler(this.nextLineButton_Click);
            // 
            // breakpointsBox
            // 
            this.breakpointsBox.FormattingEnabled = true;
            this.breakpointsBox.Location = new System.Drawing.Point(581, 238);
            this.breakpointsBox.Name = "breakpointsBox";
            this.breakpointsBox.Size = new System.Drawing.Size(90, 186);
            this.breakpointsBox.TabIndex = 36;
            this.breakpointsBox.DoubleClick += new System.EventHandler(this.breakpointsBox_DoubleClick);
            // 
            // pauseButton
            // 
            this.pauseButton.Location = new System.Drawing.Point(301, 364);
            this.pauseButton.Name = "pauseButton";
            this.pauseButton.Size = new System.Drawing.Size(120, 23);
            this.pauseButton.TabIndex = 37;
            this.pauseButton.Text = "Pause";
            this.pauseButton.UseVisualStyleBackColor = true;
            this.pauseButton.Click += new System.EventHandler(this.pauseButton_Click);
            // 
            // GBEDebugger
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(683, 449);
            this.Controls.Add(this.pauseButton);
            this.Controls.Add(this.breakpointsBox);
            this.Controls.Add(this.nextLineButton);
            this.Controls.Add(this.stepButton);
            this.Controls.Add(this.runButton);
            this.Controls.Add(this.emulator);
            this.Controls.Add(this.cbCarry);
            this.Controls.Add(this.cbHalfCarry);
            this.Controls.Add(this.cbOp);
            this.Controls.Add(this.cbZero);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.cbHold);
            this.Controls.Add(this.startButton);
            this.Controls.Add(this.selectButton);
            this.Controls.Add(this.aButton);
            this.Controls.Add(this.bButton);
            this.Controls.Add(this.downButton);
            this.Controls.Add(this.rightButton);
            this.Controls.Add(this.leftButton);
            this.Controls.Add(this.upButton);
            this.Controls.Add(this.disassemblyBox);
            this.Name = "GBEDebugger";
            this.Text = "GBEDebugger";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox disassemblyBox;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Button selectButton;
        private System.Windows.Forms.Button aButton;
        private System.Windows.Forms.Button bButton;
        private System.Windows.Forms.Button downButton;
        private System.Windows.Forms.Button rightButton;
        private System.Windows.Forms.Button leftButton;
        private System.Windows.Forms.Button upButton;
        private System.Windows.Forms.CheckBox cbHold;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label spValue;
        private System.Windows.Forms.Label pcValue;
        private System.Windows.Forms.Label hlValue;
        private System.Windows.Forms.Label deValue;
        private System.Windows.Forms.Label bcValue;
        private System.Windows.Forms.Label afValue;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.CheckBox cbZero;
        private System.Windows.Forms.CheckBox cbOp;
        private System.Windows.Forms.CheckBox cbHalfCarry;
        private System.Windows.Forms.CheckBox cbCarry;
        private Emulation.GameBoyEmulator emulator;
        private System.Windows.Forms.Button runButton;
        private System.Windows.Forms.Button stepButton;
        private System.Windows.Forms.Button nextLineButton;
        private System.Windows.Forms.ListBox breakpointsBox;
        private System.Windows.Forms.Button pauseButton;
    }
}