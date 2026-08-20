using JogoCorrida;
using JogoCorridaWinFormsApp.Properties;

namespace JogoCorridaWinFormsApp
{
    public partial class FormJogoCorrida : Form
    {
        Jogo jogo;
        DateTime tempoUltimaMovimentacao = DateTime.Now;
        List<PictureBox> pictureBoxes = [];
        public FormJogoCorrida()
        {
            InitializeComponent();

            jogo = new Jogo
            {
                Faixa1Inicio = -50,
                Faixa1Fim = 198,
                Faixa2Inicio = 50,
                Faixa2Fim = 302
            };
            jogo.IniciaJogo();
            jogo.Carro.PosicaoX = jogo.PosicionaObjeto(1);
            jogo.Velocidade = 300;

            foreach (var ob in jogo.Obstaculos)
            {
                var picOb = new PictureBox();
                picOb.BackColor = Color.Transparent;
                picOb.BackgroundImage = Properties.Resources.depositphotos_357517002_stock_illustration_car_top_view_cute_cartoon;
                picOb.BackgroundImageLayout = ImageLayout.Stretch;
                pictureBoxes.Add(picOb);
                this.Controls.Add(picOb);
            }
            timerJogo.Enabled = true;
        }

        private void FormJogoCorrida_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left)
            {
                jogo.Carro.PosicaoX = jogo.PosicionaObjeto(1);
            }
            if (e.KeyCode == Keys.Right)
            {
                jogo.Carro.PosicaoX = jogo.PosicionaObjeto(2);
            }
        }

        private void timerJogo_Tick(object sender, EventArgs e)
        {
            picCarro.Location = new Point(jogo.Carro.PosicaoX, jogo.Carro.PosicaoY);
            var i = 0;
            foreach (var ob in jogo.Obstaculos)
            {
                if (ob.PosicaoY >= 0)
                {
                    pictureBoxes[i].Location = new Point(ob.PosicaoX, ob.PosicaoY);
                }
                i++;
            }

            if ((DateTime.Now - tempoUltimaMovimentacao).Microseconds >= jogo.Velocidade)
            {
                tempoUltimaMovimentacao = DateTime.Now;
                jogo.MovimentaObstaculos();
            }
            
            if (jogo.ChecarColisao())
            {
                //GameOver();
                //TocarSom();
                Application.Exit();
            }
            Application.DoEvents();
        }
    }
}
