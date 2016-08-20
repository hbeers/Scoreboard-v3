namespace Scoreboard
{
    partial class Matrix
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
            this.Stop_button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Stop_button
            // 
            this.Stop_button.BackColor = System.Drawing.Color.Gold;
            this.Stop_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Stop_button.Location = new System.Drawing.Point(1218, 652);
            this.Stop_button.MaximumSize = new System.Drawing.Size(100, 50);
            this.Stop_button.MinimumSize = new System.Drawing.Size(100, 50);
            this.Stop_button.Name = "Stop_button";
            this.Stop_button.Size = new System.Drawing.Size(100, 50);
            this.Stop_button.TabIndex = 183;
            this.Stop_button.Text = "Stop programma";
            this.Stop_button.UseVisualStyleBackColor = false;
            this.Stop_button.Click += new System.EventHandler(this.Stop_button_Click);
            // 
            // Matrix
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.DarkGreen;
            this.ClientSize = new System.Drawing.Size(1604, 882);
            this.Controls.Add(this.Stop_button);
            this.Name = "Matrix";
            this.Text = "Scoreboard Gasterij de Merode";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Shown += new System.EventHandler(this.Matrix_Shown);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button Stop_button;

    }
}

