namespace JogoCorridaWinFormsApp
{
    partial class FormJogoCorrida
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormJogoCorrida));
            picIFSP = new PictureBox();
            picCarro = new PictureBox();
            picObstaculo1 = new PictureBox();
            timerJogo = new System.Windows.Forms.Timer(components);
            ((System.ComponentModel.ISupportInitialize)picIFSP).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picCarro).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picObstaculo1).BeginInit();
            SuspendLayout();
            // 
            // picIFSP
            // 
            picIFSP.BackgroundImage = (Image)resources.GetObject("picIFSP.BackgroundImage");
            picIFSP.BackgroundImageLayout = ImageLayout.Stretch;
            picIFSP.ImageLocation = "";
            picIFSP.Location = new Point(330, 12);
            picIFSP.Name = "picIFSP";
            picIFSP.Size = new Size(42, 54);
            picIFSP.TabIndex = 0;
            picIFSP.TabStop = false;
            // 
            // picCarro
            // 
            picCarro.BackColor = Color.Transparent;
            picCarro.BackgroundImage = Properties.Resources._3767626_carro_com_vista_de_cima_gratis_vetor;
            picCarro.BackgroundImageLayout = ImageLayout.Stretch;
            picCarro.Location = new Point(0, 0);
            picCarro.Name = "picCarro";
            picCarro.Size = new Size(140, 130);
            picCarro.TabIndex = 1;
            picCarro.TabStop = false;
            // 
            // picObstaculo1
            // 
            picObstaculo1.BackColor = Color.Transparent;
            picObstaculo1.BackgroundImage = Properties.Resources.depositphotos_357517002_stock_illustration_car_top_view_cute_cartoon;
            picObstaculo1.BackgroundImageLayout = ImageLayout.Stretch;
            picObstaculo1.Location = new Point(0, 0);
            picObstaculo1.Name = "picObstaculo1";
            picObstaculo1.Size = new Size(140, 130);
            picObstaculo1.TabIndex = 2;
            picObstaculo1.TabStop = false;
            // 
            // timerJogo
            // 
            timerJogo.Interval = 20;
            timerJogo.Tick += timerJogo_Tick;
            // 
            // FormJogoCorrida
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(384, 511);
            Controls.Add(picObstaculo1);
            Controls.Add(picCarro);
            Controls.Add(picIFSP);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Name = "FormJogoCorrida";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Jogo Corrida - IFSP";
            KeyDown += FormJogoCorrida_KeyDown;
            ((System.ComponentModel.ISupportInitialize)picIFSP).EndInit();
            ((System.ComponentModel.ISupportInitialize)picCarro).EndInit();
            ((System.ComponentModel.ISupportInitialize)picObstaculo1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox picIFSP;
        private PictureBox picCarro;
        private PictureBox picObstaculo1;
        private System.Windows.Forms.Timer timerJogo;
    }
}
