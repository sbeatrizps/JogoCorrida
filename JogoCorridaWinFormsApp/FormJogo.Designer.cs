namespace JogoCorridaWinFormsApp
{
    partial class FormJogoCorrida
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            if (disposing)
            {
                LiberarRecursos();
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
            components = new System.ComponentModel.Container();
            timerJogo = new System.Windows.Forms.Timer(components);
            SuspendLayout();
            //
            // timerJogo
            //
            timerJogo.Interval = 15;
            timerJogo.Tick += timerJogo_Tick;
            //
            // FormJogoCorrida
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(24, 26, 32);
            ClientSize = new Size(480, 660);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            KeyPreview = true;
            MaximizeBox = false;
            Name = "FormJogoCorrida";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Jogo Corrida - IFSP";
            KeyDown += FormJogoCorrida_KeyDown;
            KeyUp += FormJogoCorrida_KeyUp;
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Timer timerJogo;
    }
}
