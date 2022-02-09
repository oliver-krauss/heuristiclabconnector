#region License Information
/* HeuristicLab
 * Copyright (C) 2002-2018 Heuristic and Evolutionary Algorithms Laboratory (HEAL)
 *
 * This file is part of HeuristicLab.
 *
 * HeuristicLab is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * HeuristicLab is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with HeuristicLab. If not, see <http://www.gnu.org/licenses/>.
 */
#endregion

namespace HeuristicLab.TruffleConnector.Views {
  partial class TruffleTestCaseView {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.functionNameGroupBox = new System.Windows.Forms.GroupBox();
            this.functionNameViewHost = new HeuristicLab.MainForm.WindowsForms.ViewHost();
            this.outputGroupBox = new System.Windows.Forms.GroupBox();
            this.outputViewHost = new HeuristicLab.MainForm.WindowsForms.ViewHost();
            this.inputGroupBox = new System.Windows.Forms.GroupBox();
            this.inputViewHost = new HeuristicLab.MainForm.WindowsForms.ViewHost();
            this.sourceCodeGoupBox = new System.Windows.Forms.GroupBox();
            this.sourceCodeViewHost = new HeuristicLab.MainForm.WindowsForms.ViewHost();
            this.tableLayoutPanel.SuspendLayout();
            this.functionNameGroupBox.SuspendLayout();
            this.outputGroupBox.SuspendLayout();
            this.inputGroupBox.SuspendLayout();
            this.sourceCodeGoupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.ColumnCount = 3;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel.Controls.Add(this.functionNameGroupBox, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.outputGroupBox, 2, 1);
            this.tableLayoutPanel.Controls.Add(this.inputGroupBox, 1, 1);
            this.tableLayoutPanel.Controls.Add(this.sourceCodeGoupBox, 0, 1);
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 2;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(652, 413);
            this.tableLayoutPanel.TabIndex = 0;
            // 
            // functionNameGroupBox
            // 
            this.tableLayoutPanel.SetColumnSpan(this.functionNameGroupBox, 3);
            this.functionNameGroupBox.Controls.Add(this.functionNameViewHost);
            this.functionNameGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.functionNameGroupBox.Location = new System.Drawing.Point(3, 3);
            this.functionNameGroupBox.Name = "functionNameGroupBox";
            this.functionNameGroupBox.Size = new System.Drawing.Size(646, 44);
            this.functionNameGroupBox.TabIndex = 6;
            this.functionNameGroupBox.TabStop = false;
            this.functionNameGroupBox.Text = "Function Name";
            // 
            // functionNameViewHost
            // 
            this.functionNameViewHost.Caption = "View";
            this.functionNameViewHost.Content = null;
            this.functionNameViewHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.functionNameViewHost.Enabled = false;
            this.functionNameViewHost.Location = new System.Drawing.Point(3, 16);
            this.functionNameViewHost.Name = "functionNameViewHost";
            this.functionNameViewHost.ReadOnly = false;
            this.functionNameViewHost.Size = new System.Drawing.Size(640, 25);
            this.functionNameViewHost.TabIndex = 2;
            this.functionNameViewHost.ViewsLabelVisible = false;
            this.functionNameViewHost.ViewType = null;
            // 
            // outputGroupBox
            // 
            this.outputGroupBox.Controls.Add(this.outputViewHost);
            this.outputGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outputGroupBox.Location = new System.Drawing.Point(437, 53);
            this.outputGroupBox.Name = "outputGroupBox";
            this.outputGroupBox.Size = new System.Drawing.Size(212, 357);
            this.outputGroupBox.TabIndex = 5;
            this.outputGroupBox.TabStop = false;
            this.outputGroupBox.Text = "Output";
            // 
            // outputViewHost
            // 
            this.outputViewHost.Caption = "View";
            this.outputViewHost.Content = null;
            this.outputViewHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outputViewHost.Enabled = false;
            this.outputViewHost.Location = new System.Drawing.Point(3, 16);
            this.outputViewHost.Name = "outputViewHost";
            this.outputViewHost.ReadOnly = false;
            this.outputViewHost.Size = new System.Drawing.Size(206, 338);
            this.outputViewHost.TabIndex = 2;
            this.outputViewHost.ViewsLabelVisible = false;
            this.outputViewHost.ViewType = null;
            // 
            // inputGroupBox
            // 
            this.inputGroupBox.Controls.Add(this.inputViewHost);
            this.inputGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputGroupBox.Location = new System.Drawing.Point(220, 53);
            this.inputGroupBox.Name = "inputGroupBox";
            this.inputGroupBox.Size = new System.Drawing.Size(211, 357);
            this.inputGroupBox.TabIndex = 4;
            this.inputGroupBox.TabStop = false;
            this.inputGroupBox.Text = "Input";
            // 
            // inputViewHost
            // 
            this.inputViewHost.Caption = "View";
            this.inputViewHost.Content = null;
            this.inputViewHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputViewHost.Enabled = false;
            this.inputViewHost.Location = new System.Drawing.Point(3, 16);
            this.inputViewHost.Name = "inputViewHost";
            this.inputViewHost.ReadOnly = false;
            this.inputViewHost.Size = new System.Drawing.Size(205, 338);
            this.inputViewHost.TabIndex = 2;
            this.inputViewHost.ViewsLabelVisible = false;
            this.inputViewHost.ViewType = null;
            // 
            // sourceCodeGoupBox
            // 
            this.sourceCodeGoupBox.Controls.Add(this.sourceCodeViewHost);
            this.sourceCodeGoupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sourceCodeGoupBox.Location = new System.Drawing.Point(3, 53);
            this.sourceCodeGoupBox.Name = "sourceCodeGoupBox";
            this.sourceCodeGoupBox.Size = new System.Drawing.Size(211, 357);
            this.sourceCodeGoupBox.TabIndex = 3;
            this.sourceCodeGoupBox.TabStop = false;
            this.sourceCodeGoupBox.Text = "Source Code";
            // 
            // sourceCodeViewHost
            // 
            this.sourceCodeViewHost.Caption = "View";
            this.sourceCodeViewHost.Content = null;
            this.sourceCodeViewHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sourceCodeViewHost.Enabled = false;
            this.sourceCodeViewHost.Location = new System.Drawing.Point(3, 16);
            this.sourceCodeViewHost.Name = "sourceCodeViewHost";
            this.sourceCodeViewHost.ReadOnly = false;
            this.sourceCodeViewHost.Size = new System.Drawing.Size(205, 338);
            this.sourceCodeViewHost.TabIndex = 2;
            this.sourceCodeViewHost.ViewsLabelVisible = false;
            this.sourceCodeViewHost.ViewType = null;
            // 
            // TruffleTestCaseView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel);
            this.Name = "TruffleTestCaseView";
            this.Size = new System.Drawing.Size(652, 413);
            this.tableLayoutPanel.ResumeLayout(false);
            this.functionNameGroupBox.ResumeLayout(false);
            this.outputGroupBox.ResumeLayout(false);
            this.inputGroupBox.ResumeLayout(false);
            this.sourceCodeGoupBox.ResumeLayout(false);
            this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
    private System.Windows.Forms.GroupBox outputGroupBox;
    private MainForm.WindowsForms.ViewHost outputViewHost;
    private System.Windows.Forms.GroupBox inputGroupBox;
    private MainForm.WindowsForms.ViewHost inputViewHost;
    private System.Windows.Forms.GroupBox sourceCodeGoupBox;
    private MainForm.WindowsForms.ViewHost sourceCodeViewHost;
    private System.Windows.Forms.GroupBox functionNameGroupBox;
    private MainForm.WindowsForms.ViewHost functionNameViewHost;
  }
}
