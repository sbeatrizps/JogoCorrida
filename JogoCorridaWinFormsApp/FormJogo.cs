using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using JogoCorrida;

namespace JogoCorridaWinFormsApp
{
    public partial class FormJogoCorrida : Form
    {
        private readonly Jogo jogo = new();
        private readonly Efeitos efeitos = new();
        private readonly Stopwatch relogio = Stopwatch.StartNew();
        private readonly HashSet<Keys> teclasPressionadas = [];
        private readonly Random rnd = new();

        private double ultimoQuadroMs;

        private Image? imgCarro;
        private Image? imgObstaculo;
        private Image? imgLogo;

        private Font fonteTitulo = null!;
        private Font fonteGrande = null!;
        private Font fonteMedia = null!;
        private Font fontePequena = null!;
        private Font fonteMini = null!;

        private const float AlturaPainel = 78f;

        private static readonly string ArquivoRecorde = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "JogoCorrida", "recorde.txt");

        public FormJogoCorrida()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.UserPaint
                   | ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();

            CriaFontes();
            CarregaImagens();
            Sons.Inicializar();

            jogo.Som += Sons.Tocar;
            jogo.MelhorPontuacao = CarregaRecorde();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ConfiguraAreaDeJogo();
            jogo.IniciaJogo();

            ultimoQuadroMs = relogio.Elapsed.TotalMilliseconds;
            timerJogo.Enabled = true;
        }

        private void ConfiguraAreaDeJogo()
        {
            var cfg = jogo.Config;
            cfg.Largura = ClientSize.Width;
            cfg.Altura = ClientSize.Height;
            cfg.QtdFaixas = 3;
            cfg.MargemPista = (int)(cfg.Largura * 0.09);

            var faixa = cfg.LarguraFaixa;
            cfg.LarguraCarro = (int)(faixa * 0.8);
            cfg.AlturaCarro = (int)(cfg.LarguraCarro * 1.3);
            cfg.LarguraMoeda = cfg.AlturaMoeda = (int)(faixa * 0.36);
            cfg.RecuoCarro = (int)(cfg.Altura * 0.15);
            cfg.VelocidadeLateral = faixa * 5.5;
            cfg.FolgaColisao = cfg.LarguraCarro * 0.14;
        }

        private void CriaFontes()
        {
            fonteTitulo = new Font("Segoe UI", 30F, FontStyle.Bold);
            fonteGrande = new Font("Segoe UI", 19F, FontStyle.Bold);
            fonteMedia = new Font("Segoe UI", 13F, FontStyle.Bold);
            fontePequena = new Font("Segoe UI", 10F, FontStyle.Bold);
            fonteMini = new Font("Segoe UI", 8F, FontStyle.Bold);
        }

        private void CarregaImagens()
        {
            imgCarro = Properties.Resources._3767626_carro_com_vista_de_cima_gratis_vetor;
            imgObstaculo = Properties.Resources.depositphotos_357517002_stock_illustration_car_top_view_cute_cartoon;

            try
            {
                var recursos = new ComponentResourceManager(typeof(FormJogoCorrida));
                imgLogo = recursos.GetObject("picIFSP.BackgroundImage") as Image;
            }
            catch
            {
                imgLogo = null; 
            }
        }

        private void LiberarRecursos()
        {
            imgCarro?.Dispose();
            imgObstaculo?.Dispose();
            imgLogo?.Dispose();
            fonteTitulo?.Dispose();
            fonteGrande?.Dispose();
            fonteMedia?.Dispose();
            fontePequena?.Dispose();
            fonteMini?.Dispose();
        }

        private void timerJogo_Tick(object? sender, EventArgs e)
        {
            var agora = relogio.Elapsed.TotalMilliseconds;
            var delta = agora - ultimoQuadroMs;
            ultimoQuadroMs = agora;

            if (jogo.Config.Largura != ClientSize.Width || jogo.Config.Altura != ClientSize.Height)
            {
                ConfiguraAreaDeJogo();
                jogo.IniciaJogo();
                efeitos.Limpar();
            }

            var estadoAnterior = jogo.Estado;
            var moedasAnteriores = jogo.Moedas;

            jogo.Atualizar(delta);
            efeitos.Atualizar(delta);

            if (jogo.Moedas > moedasAnteriores)
            {
                efeitos.Texto((float)jogo.Carro.CentroX, (float)jogo.Carro.PosicaoY - 10,
                              $"+{jogo.Config.PontosPorMoeda}", Color.Gold);
            }

            if (estadoAnterior != EstadoJogo.Colidindo && jogo.Estado == EstadoJogo.Colidindo)
            {
                var atingido = jogo.ElementoAtingido;
                var x = atingido is null ? jogo.Carro.CentroX : (atingido.CentroX + jogo.Carro.CentroX) / 2;
                var y = atingido is null ? jogo.Carro.CentroY : (atingido.CentroY + jogo.Carro.PosicaoY) / 2;
                efeitos.Explosao((float)x, (float)y, jogo.Config.LarguraCarro / 46f);
            }

            if (estadoAnterior != jogo.Estado && jogo.VerificaFimJogo())
            {
                SalvaRecorde(jogo.MelhorPontuacao);
            }

            Invalidate();
        }


        private void FormJogoCorrida_KeyDown(object sender, KeyEventArgs e)
        {
            var repetida = !teclasPressionadas.Add(e.KeyCode);

            if (!repetida)
            {
                switch (e.KeyCode)
                {
                    case Keys.Left:
                    case Keys.A:
                        jogo.MoverEsquerda();
                        break;

                    case Keys.Right:
                    case Keys.D:
                        jogo.MoverDireita();
                        break;

                    case Keys.Enter:
                    case Keys.Space:
                        efeitos.Limpar();
                        jogo.Confirmar();
                        break;

                    case Keys.P:
                        jogo.AlternarPausa();
                        break;

                    case Keys.M:
                        Sons.Ligado = !Sons.Ligado;
                        break;

                    case Keys.Escape:
                        if (jogo.Estado == EstadoJogo.Jogando) jogo.AlternarPausa();
                        else Close();
                        break;
                }
            }

            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        private void FormJogoCorrida_KeyUp(object sender, KeyEventArgs e)
        {
            teclasPressionadas.Remove(e.KeyCode);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            System.IO.File.AppendAllText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "sons.txt"), "FECHANDO motivo=" + e.CloseReason + " estado=" + jogo.Estado + Environment.NewLine + Environment.StackTrace + Environment.NewLine);
            timerJogo.Enabled = false;
            Sons.Parar();
            SalvaRecorde(jogo.MelhorPontuacao);
            base.OnFormClosing(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            var estadoGraficos = g.Save();
            AplicaTremorDaBatida(g);

            DesenhaCenario(g);
            DesenhaElementos(g);
            DesenhaCarroDoJogador(g);
            if (jogo.FaseAtual.Noite) DesenhaNoite(g);
            efeitos.Desenhar(g, fonteMedia);
            DesenhaClaraoDaBatida(g);

            g.Restore(estadoGraficos);

            DesenhaPainel(g);
            DesenhaAvisos(g);
        }

        private void AplicaTremorDaBatida(Graphics g)
        {
            if (jogo.Estado != EstadoJogo.Colidindo) return;

            var forca = (float)(jogo.IntensidadeExplosao * jogo.Config.Largura * 0.03);
            g.TranslateTransform((float)(rnd.NextDouble() * 2 - 1) * forca,
                                 (float)(rnd.NextDouble() * 2 - 1) * forca);
        }

        private void DesenhaCenario(Graphics g)
        {
            var cfg = jogo.Config;
            var fase = jogo.FaseAtual;

            using (var grama = new SolidBrush(Cor(fase.CorGrama)))
            {
                g.FillRectangle(grama, -40, -40,
                                Math.Max(cfg.Largura, ClientSize.Width) + 80,
                                Math.Max(cfg.Altura, ClientSize.Height) + 80);
            }

            using (var asfalto = new SolidBrush(Cor(fase.CorAsfalto)))
            {
                g.FillRectangle(asfalto, cfg.MargemPista, -40, cfg.LarguraPista, cfg.Altura + 80);
            }

            DesenhaPaisagem(g);

            using (var borda = new Pen(Color.FromArgb(220, 245, 245, 245), 4))
            {
                g.DrawLine(borda, cfg.MargemPista + 3, -40, cfg.MargemPista + 3, cfg.Altura + 40);
                g.DrawLine(borda, cfg.MargemPista + cfg.LarguraPista - 3, -40,
                                  cfg.MargemPista + cfg.LarguraPista - 3, cfg.Altura + 40);
            }

            var comprimento = cfg.Altura * 0.055f;
            var vao = comprimento * 0.85f;
            var periodo = comprimento + vao;
            var deslocamento = (float)(jogo.DistanciaPercorrida % periodo);

            using var pincelFaixa = new SolidBrush(Cor(fase.CorFaixa));
            for (int i = 1; i < cfg.QtdFaixas; i++)
            {
                var x = cfg.MargemPista + cfg.LarguraFaixa * i;
                for (var y = -periodo + deslocamento; y < cfg.Altura + periodo; y += periodo)
                {
                    g.FillRectangle(pincelFaixa, x - 3, y, 6, comprimento);
                }
            }
        }

        private void DesenhaPaisagem(Graphics g)
        {
            var cfg = jogo.Config;
            var fase = jogo.FaseAtual;

            var periodo = cfg.Altura * 0.34f;
            var deslocamento = (float)(jogo.DistanciaPercorrida % periodo);
            var larguraMargem = cfg.MargemPista;

            using var poste = new SolidBrush(Color.FromArgb(210, 60, 60, 68));
            using var lampada = new SolidBrush(fase.Noite ? Color.FromArgb(235, 255, 236, 160)
                                                          : Color.FromArgb(200, 220, 220, 225));
            using var mato = new SolidBrush(Color.FromArgb(80, 0, 0, 0));

            var indice = 0;
            for (var y = -periodo + deslocamento; y < cfg.Altura + periodo; y += periodo)
            {
                var esquerda = indice % 2 == 0;
                var x = esquerda ? larguraMargem * 0.45f : cfg.Largura - larguraMargem * 0.45f;

                g.FillRectangle(poste, x - 3, y, 6, cfg.Altura * 0.075f);
                g.FillEllipse(lampada, x - 8, y - 7, 16, 12);

                if (fase.Noite)
                {
                    using var brilho = new SolidBrush(Color.FromArgb(45, 255, 236, 160));
                    g.FillEllipse(brilho, x - 26, y - 24, 52, 46);
                }

                var xArbusto = esquerda ? cfg.Largura - larguraMargem * 0.5f : larguraMargem * 0.5f;
                g.FillEllipse(mato, xArbusto - 12, y + periodo * 0.45f, 24, 16);

                indice++;
            }
        }

        private void DesenhaElementos(Graphics g)
        {
            foreach (var el in jogo.Elementos)
            {
                var r = new RectangleF((float)el.PosicaoX, (float)el.PosicaoY, el.Largura, el.Altura);
                if (r.Bottom < -10 || r.Top > jogo.Config.Altura + 10) continue;

                if (el.Tipo == TipoElemento.Moeda)
                {
                    DesenhaMoeda(g, r, el.Animacao);
                    continue;
                }

                switch (el.Obstaculo)
                {
                    case TipoObstaculo.CarroLento: DesenhaCarroInimigo(g, r); break;
                    case TipoObstaculo.Caminhao: DesenhaCaminhao(g, r); break;
                    case TipoObstaculo.Cone: DesenhaCone(g, r); break;
                    case TipoObstaculo.Buraco: DesenhaBuraco(g, r); break;
                    case TipoObstaculo.Oleo: DesenhaOleo(g, r); break;
                }
            }
        }

        private static void DesenhaSombra(Graphics g, RectangleF r)
        {
            using var sombra = new SolidBrush(Color.FromArgb(70, 0, 0, 0));
            g.FillEllipse(sombra, r.X + 2, r.Bottom - r.Height * 0.22f, r.Width - 4, r.Height * 0.26f);
        }

        private void DesenhaCarroInimigo(Graphics g, RectangleF r)
        {
            DesenhaSombra(g, r);

            if (imgObstaculo is not null)
            {
                g.DrawImage(imgObstaculo, r);
            }
            else
            {
                using var pincel = new SolidBrush(Color.FromArgb(215, 70, 70));
                using var caminho = Arredondado(r, r.Width * 0.28f);
                g.FillPath(pincel, caminho);
            }
        }

        private void DesenhaCaminhao(Graphics g, RectangleF r)
        {
            var state = g.Save();

            var centroX = r.X + r.Width / 2f;
            var centroY = r.Y + r.Height / 2f;

            g.TranslateTransform(centroX, centroY);
            g.RotateTransform(180);
            g.TranslateTransform(-centroX, -centroY);

            DesenhaSombra(g, r);

            var cabine = new RectangleF(r.X + r.Width * 0.08f, r.Bottom - r.Height * 0.26f,
                                        r.Width * 0.84f, r.Height * 0.24f);
            var bau = new RectangleF(r.X, r.Y, r.Width, r.Height * 0.72f);

            using (var pincelBau = new LinearGradientBrush(bau,
                       Color.FromArgb(238, 240, 245), Color.FromArgb(186, 192, 205), 0f))
            using (var caminho = Arredondado(bau, r.Width * 0.12f))
            {
                g.FillPath(pincelBau, caminho);
            }

            using (var linhas = new Pen(Color.FromArgb(120, 90, 96, 110), 2))
            {
                for (int i = 1; i <= 3; i++)
                {
                    var y = bau.Y + bau.Height * i / 4f;
                    g.DrawLine(linhas, bau.X + 4, y, bau.Right - 4, y);
                }
            }

            using (var pincelCabine = new SolidBrush(Color.FromArgb(48, 92, 168)))
            using (var caminho = Arredondado(cabine, r.Width * 0.16f))
            {
                g.FillPath(pincelCabine, caminho);
            }

            using var luz = new SolidBrush(Color.FromArgb(230, 240, 70, 60));
            g.FillEllipse(luz, cabine.X + 3, cabine.Bottom - 8, 8, 6);
            g.FillEllipse(luz, cabine.Right - 11, cabine.Bottom - 8, 8, 6);

            g.Restore(state);
        }

        private static void DesenhaCone(Graphics g, RectangleF r)
        {
            DesenhaSombra(g, r);

            var pontos = new PointF[]
            {
                new(r.X + r.Width / 2, r.Y),
                new(r.Right, r.Bottom - r.Height * 0.16f),
                new(r.X, r.Bottom - r.Height * 0.16f)
            };

            using (var laranja = new SolidBrush(Color.FromArgb(240, 118, 32)))
            {
                g.FillPolygon(laranja, pontos);
            }

            using (var faixa = new Pen(Color.FromArgb(250, 250, 250), Math.Max(2f, r.Height * 0.16f)))
            {
                var y = r.Y + r.Height * 0.52f;
                g.DrawLine(faixa, r.X + r.Width * 0.22f, y, r.Right - r.Width * 0.22f, y);
            }

            using var baseCone = new SolidBrush(Color.FromArgb(210, 88, 20));
            g.FillRectangle(baseCone, r.X, r.Bottom - r.Height * 0.18f, r.Width, r.Height * 0.18f);
        }

        private static void DesenhaBuraco(Graphics g, RectangleF r)
        {
            using (var contorno = new SolidBrush(Color.FromArgb(120, 30, 30, 34)))
            {
                g.FillEllipse(contorno, r);
            }

            var interno = new RectangleF(r.X + r.Width * 0.09f, r.Y + r.Height * 0.14f,
                                         r.Width * 0.82f, r.Height * 0.72f);
            using (var buraco = new SolidBrush(Color.FromArgb(245, 16, 16, 20)))
            {
                g.FillEllipse(buraco, interno);
            }

            using var pedras = new Pen(Color.FromArgb(150, 120, 120, 128), 2);
            g.DrawEllipse(pedras, r);
        }

        private static void DesenhaOleo(Graphics g, RectangleF r)
        {
            using (var oleo = new SolidBrush(Color.FromArgb(215, 22, 20, 30)))
            {
                g.FillEllipse(oleo, r);
            }

            using (var respingo = new SolidBrush(Color.FromArgb(180, 22, 20, 30)))
            {
                g.FillEllipse(respingo, r.X - r.Width * 0.12f, r.Y + r.Height * 0.45f,
                              r.Width * 0.4f, r.Height * 0.4f);
                g.FillEllipse(respingo, r.Right - r.Width * 0.3f, r.Y - r.Height * 0.18f,
                              r.Width * 0.36f, r.Height * 0.36f);
            }

            using var brilho = new SolidBrush(Color.FromArgb(90, 120, 90, 200));
            g.FillEllipse(brilho, r.X + r.Width * 0.22f, r.Y + r.Height * 0.2f,
                          r.Width * 0.34f, r.Height * 0.3f);
        }

        private void DesenhaMoeda(Graphics g, RectangleF r, double animacao)
        {
            // "giro" da moeda: a largura vai e volta
            var giro = (float)Math.Abs(Math.Cos(animacao / 260.0));
            var largura = Math.Max(3f, r.Width * giro);
            var face = new RectangleF(r.X + (r.Width - largura) / 2, r.Y, largura, r.Height);

            if (jogo.FaseAtual.Noite)
            {
                using var brilho = new SolidBrush(Color.FromArgb(60, 255, 226, 120));
                g.FillEllipse(brilho, r.X - r.Width * 0.35f, r.Y - r.Height * 0.35f,
                              r.Width * 1.7f, r.Height * 1.7f);
            }

            using (var ouro = new LinearGradientBrush(
                       new RectangleF(face.X, face.Y, Math.Max(1f, face.Width), Math.Max(1f, face.Height)),
                       Color.FromArgb(255, 226, 120), Color.FromArgb(214, 152, 24), 60f))
            {
                g.FillEllipse(ouro, face);
            }

            using (var aro = new Pen(Color.FromArgb(150, 110, 20), 2))
            {
                g.DrawEllipse(aro, face);
            }

            if (giro > 0.55f)
            {
                using var formato = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                using var texto = new SolidBrush(Color.FromArgb(150, 110, 20));
                g.DrawString("$", fonteMini, texto, face, formato);
            }
        }

        private void DesenhaCarroDoJogador(Graphics g)
        {
            var cfg = jogo.Config;
            var carro = jogo.Carro;

            if (jogo.Invencivel && jogo.Estado == EstadoJogo.Jogando
                && (int)(jogo.TempoInvencivelMs / 110) % 2 == 0)
            {
                return;
            }

            var r = new RectangleF((float)carro.PosicaoX, (float)carro.PosicaoY, carro.Largura, carro.Altura);
            var alvo = jogo.PosicionaObjeto(jogo.FaixaAlvo);
            var inclinacao = (float)Math.Clamp((alvo - carro.PosicaoX) / cfg.LarguraFaixa * 16, -14, 14);

            var estadoGraficos = g.Save();
            g.TranslateTransform(r.X + r.Width / 2, r.Y + r.Height / 2);
            g.RotateTransform(inclinacao);
            g.TranslateTransform(-(r.X + r.Width / 2), -(r.Y + r.Height / 2));

            DesenhaSombra(g, r);

            if (imgCarro is not null)
            {
                g.DrawImage(imgCarro, r);
            }
            else
            {
                using var pincel = new SolidBrush(Color.FromArgb(60, 140, 230));
                using var caminho = Arredondado(r, r.Width * 0.28f);
                g.FillPath(pincel, caminho);
            }

            g.Restore(estadoGraficos);

            if (jogo.Derrapando)
            {
                using var fumaca = new SolidBrush(Color.FromArgb(90, 235, 235, 235));
                g.FillEllipse(fumaca, r.X - 6, r.Bottom - 10, r.Width + 12, 20);
            }
        }

        private void DesenhaNoite(Graphics g)
        {
            var cfg = jogo.Config;

            using (var escuro = new SolidBrush(Color.FromArgb(130, 6, 8, 22)))
            {
                g.FillRectangle(escuro, -40, -40, cfg.Largura + 80, cfg.Altura + 80);
            }

            var carro = jogo.Carro;
            var topo = (float)carro.PosicaoY + 6;
            var alcance = cfg.Altura * 0.55f;

            var farol = new PointF[]
            {
                new((float)carro.PosicaoX + 4, topo),
                new((float)carro.PosicaoX + carro.Largura - 4, topo),
                new((float)carro.CentroX + carro.Largura * 1.7f, topo - alcance),
                new((float)carro.CentroX - carro.Largura * 1.7f, topo - alcance)
            };

            using var luz = new LinearGradientBrush(
                new PointF(0, topo), new PointF(0, topo - alcance),
                Color.FromArgb(85, 255, 246, 200), Color.FromArgb(0, 255, 246, 200));
            g.FillPolygon(luz, farol);
        }

        private void DesenhaClaraoDaBatida(Graphics g)
        {
            var intensidade = jogo.IntensidadeExplosao;
            if (intensidade <= 0) return;

            var cfg = jogo.Config;
            var alfa = (int)(200 * Math.Pow(intensidade, 3));
            if (alfa <= 0) return;

            using var clarao = new SolidBrush(Color.FromArgb(alfa, 255, 232, 180));
            g.FillRectangle(clarao, -40, -40, cfg.Largura + 80, cfg.Altura + 80);
        }

        private void DesenhaPainel(Graphics g)
        {
            var cfg = jogo.Config;
            var largura = cfg.Largura;

            using (var fundo = new SolidBrush(Color.FromArgb(175, 10, 12, 20)))
            {
                g.FillRectangle(fundo, 0, 0, largura, AlturaPainel);
            }

            Texto(g, "PONTOS", fonteMini, Color.FromArgb(170, 200, 210, 230), 14, 7, StringAlignment.Near);
            Texto(g, jogo.Pontuacao.ToString("N0"), fonteGrande, Color.White, 12, 18, StringAlignment.Near);

            Texto(g, "RECORDE", fonteMini, Color.FromArgb(170, 200, 210, 230),
                  largura / 2f + 10, 7, StringAlignment.Center);
            Texto(g, jogo.MelhorPontuacao.ToString("N0"), fontePequena, Color.FromArgb(255, 214, 90),
                  largura / 2f + 10, 22, StringAlignment.Center);

            var x = largura - 16f;
            for (int i = 0; i < jogo.Config.VidasMaximas; i++)
            {
                if (i >= jogo.Vidas) continue;
                DesenhaCoracao(g, x - 18, 10, 18);
                x -= 24;
            }

            var fase = jogo.FaseAtual;
            Texto(g, $"FASE {jogo.NumeroFase}/{jogo.Fases.Count}  {fase.Nome}", fonteMini,
                  Color.FromArgb(225, 235, 245), 14, 50, StringAlignment.Near);

            var tempo = TimeSpan.FromMilliseconds(jogo.TempoJogoMs);
            Texto(g, $"TEMPO {tempo:mm\\:ss}", fonteMini, Color.FromArgb(190, 200, 215, 235),
                  largura - 14, 50, StringAlignment.Far);

            using (var ouro = new SolidBrush(Color.FromArgb(255, 214, 90)))
            {
                g.FillEllipse(ouro, largura / 2f + 26, 50, 12, 12);
            }
            Texto(g, $"x {jogo.Moedas}", fonteMini, Color.FromArgb(255, 214, 90),
                  largura / 2f + 42, 50, StringAlignment.Near);

            var y = AlturaPainel - 7;
            using (var trilho = new SolidBrush(Color.FromArgb(120, 255, 255, 255)))
            {
                g.FillRectangle(trilho, 0, y, largura, 5);
            }
            using (var preenchido = new SolidBrush(Cor(fase.CorFaixa)))
            {
                g.FillRectangle(preenchido, 0, y, (float)(largura * jogo.ProgressoFase), 5);
            }
        }

        private static void DesenhaCoracao(Graphics g, float x, float y, float tamanho)
        {
            using var caminho = new GraphicsPath();
            var metade = tamanho / 2;

            caminho.AddArc(x, y, metade, metade, 180, 180);
            caminho.AddArc(x + metade, y, metade, metade, 180, 180);
            caminho.AddLine(x + tamanho, y + metade * 0.6f, x + metade, y + tamanho);
            caminho.AddLine(x + metade, y + tamanho, x, y + metade * 0.6f);
            caminho.CloseFigure();

            using var pincel = new SolidBrush(Color.FromArgb(235, 72, 88));
            using var contorno = new Pen(Color.FromArgb(120, 0, 0, 0), 1.5f);
            g.FillPath(pincel, caminho);
            g.DrawPath(contorno, caminho);
        }

        private void DesenhaAvisos(Graphics g)
        {
            switch (jogo.Estado)
            {
                case EstadoJogo.Menu:
                    DesenhaMenu(g);
                    break;

                case EstadoJogo.Pausado:
                    DesenhaCaixa(g, "PAUSA", Color.White,
                    [
                        "ENTER  continuar",
                        "ESC  sair do jogo",
                        Sons.Ligado ? "M  desligar o som" : "M  ligar o som"
                    ]);
                    break;

                case EstadoJogo.TrocandoFase:
                    DesenhaTrocaDeFase(g);
                    break;

                case EstadoJogo.GameOver:
                    DesenhaFimDeJogo(g, "GAME OVER", Color.FromArgb(255, 96, 86));
                    break;

                case EstadoJogo.Vitoria:
                    DesenhaFimDeJogo(g, "VOCÊ VENCEU!", Color.FromArgb(126, 232, 148));
                    break;
            }
        }

        private void DesenhaMenu(Graphics g)
        {
            var cfg = jogo.Config;
            EscureceTela(g, 175);

            var y = AlturaPainel + 14;

            if (imgLogo is not null)
            {
                var altura = cfg.Altura * 0.10f;
                var largura = altura * imgLogo.Width / imgLogo.Height;
                g.DrawImage(imgLogo, cfg.Largura / 2f - largura / 2, y, largura, altura);
                y += altura + 14;
            }

            Texto(g, "JOGO CORRIDA", fonteTitulo, Color.White, cfg.Largura / 2f, y, StringAlignment.Center);
            Texto(g, "IFSP", fontePequena, Color.FromArgb(255, 214, 90),
                  cfg.Largura / 2f, y + 46, StringAlignment.Center);

            string[] instrucoes =
            [
                "SETAS  ou  A / D     trocar de faixa",
                "ENTER  ou  ESPAÇO    começar",
                "P  pausar        M  som        ESC  sair",
                "",
                "Desvie dos obstáculos, pegue as moedas",
                $"e sobreviva às {jogo.Fases.Count} fases!"
            ];

            var linha = y + 100;
            foreach (var texto in instrucoes)
            {
                Texto(g, texto, fontePequena, Color.FromArgb(230, 235, 245),
                      cfg.Largura / 2f, linha, StringAlignment.Center);
                linha += 26;
            }

            DesenhaLegendaDosItens(g, linha + 10);

            var piscando = (int)(relogio.Elapsed.TotalMilliseconds / 500) % 2 == 0;
            if (piscando)
            {
                Texto(g, "PRESSIONE ENTER PARA COMEÇAR", fonteMedia, Color.FromArgb(255, 214, 90),
                      cfg.Largura / 2f, cfg.Altura * 0.86f, StringAlignment.Center);
            }

            if (jogo.MelhorPontuacao > 0)
            {
                Texto(g, $"Recorde: {jogo.MelhorPontuacao:N0}", fontePequena, Color.White,
                      cfg.Largura / 2f, cfg.Altura * 0.92f, StringAlignment.Center);
            }
        }

        private void DesenhaLegendaDosItens(Graphics g, float y)
        {
            var cfg = jogo.Config;
            var tamanho = 26f;
            var espaco = cfg.Largura / 7f;  
            var x = espaco;

            var itens = new (TipoObstaculo Tipo, string Texto)[]
            {
                (TipoObstaculo.CarroLento, "10"),
                (TipoObstaculo.Caminhao, "20"),
                (TipoObstaculo.Cone, "5"),
                (TipoObstaculo.Buraco, "8"),
                (TipoObstaculo.Oleo, "esc.")
            };

            foreach (var (tipo, texto) in itens)
            {
                var r = new RectangleF(x - tamanho / 2, y, tamanho, tamanho);
                switch (tipo)
                {
                    case TipoObstaculo.CarroLento: DesenhaCarroInimigo(g, r); break;
                    case TipoObstaculo.Caminhao: DesenhaCaminhao(g, r); break;
                    case TipoObstaculo.Cone: DesenhaCone(g, r); break;
                    case TipoObstaculo.Buraco: DesenhaBuraco(g, r); break;
                    case TipoObstaculo.Oleo: DesenhaOleo(g, r); break;
                }
                Texto(g, texto, fonteMini, Color.FromArgb(225, 235, 245), x, y + tamanho + 4, StringAlignment.Center);
                x += espaco;
            }

            var moeda = new RectangleF(x - tamanho / 2, y, tamanho, tamanho);
            DesenhaMoeda(g, moeda, 0);
            Texto(g, $"+{jogo.Config.PontosPorMoeda}", fonteMini, Color.FromArgb(255, 214, 90),
                  x, y + tamanho + 4, StringAlignment.Center);
        }

        private void DesenhaTrocaDeFase(Graphics g)
        {
            var cfg = jogo.Config;
            var fase = jogo.FaseAtual;

            var restante = jogo.TempoEfeitoMs;
            var total = cfg.DuracaoTrocaFaseMs;
            var transparencia = Math.Clamp(Math.Min((total - restante) / 250.0, restante / 400.0), 0, 1);

            EscureceTela(g, (int)(165 * transparencia));

            var cor = Color.FromArgb((int)(255 * transparencia), 255, 255, 255);
            var destaque = Color.FromArgb((int)(255 * transparencia), 255, 214, 90);

            Texto(g, $"FASE {jogo.NumeroFase}", fonteTitulo, destaque,
                  cfg.Largura / 2f, cfg.Altura * 0.38f, StringAlignment.Center);
            Texto(g, fase.Nome, fonteMedia, cor,
                  cfg.Largura / 2f, cfg.Altura * 0.48f, StringAlignment.Center);
            Texto(g, fase.Descricao, fontePequena, cor,
                  cfg.Largura / 2f, cfg.Altura * 0.54f, StringAlignment.Center);
            Texto(g, $"Objetivo: {fase.PontosParaConcluir:N0} pontos   •   +1 vida de bônus", fonteMini, cor,
                  cfg.Largura / 2f, cfg.Altura * 0.60f, StringAlignment.Center);
        }

        private void DesenhaFimDeJogo(Graphics g, string titulo, Color corTitulo)
        {
            var novoRecorde = jogo.Pontuacao >= jogo.MelhorPontuacao && jogo.Pontuacao > 0;

            string[] linhas =
            [
                $"PONTUAÇÃO FINAL:  {jogo.Pontuacao:N0}",
                "",
                $"Tempo:  {(int)jogo.PontosTempo:N0} pts",
                $"Obstáculos superados:  {jogo.ObstaculosSuperados}  ({(int)jogo.PontosObstaculos:N0} pts)",
                $"Moedas:  {jogo.Moedas}  ({(int)jogo.PontosMoedas:N0} pts)",
                $"Fase alcançada:  {jogo.NumeroFase} de {jogo.Fases.Count}",
                "",
                novoRecorde ? "NOVO RECORDE!" : $"Recorde: {jogo.MelhorPontuacao:N0}",
                "",
                "ENTER  jogar de novo        ESC  sair"
            ];

            DesenhaCaixa(g, titulo, corTitulo, linhas);
        }

        private void DesenhaCaixa(Graphics g, string titulo, Color corTitulo, string[] linhas)
        {
            var cfg = jogo.Config;
            EscureceTela(g, 185);

            var alturaCaixa = 110 + linhas.Length * 24;
            var caixa = new RectangleF(cfg.Largura * 0.06f,
                                       (cfg.Altura - alturaCaixa) / 2f,
                                       cfg.Largura * 0.88f,
                                       alturaCaixa);

            using (var fundo = new SolidBrush(Color.FromArgb(225, 18, 20, 30)))
            using (var borda = new Pen(Color.FromArgb(120, 255, 255, 255), 2))
            using (var caminho = Arredondado(caixa, 18))
            {
                g.FillPath(fundo, caminho);
                g.DrawPath(borda, caminho);
            }

            Texto(g, titulo, fonteTitulo, corTitulo, cfg.Largura / 2f, caixa.Y + 24, StringAlignment.Center);

            var y = caixa.Y + 92;
            foreach (var linha in linhas)
            {
                var destaque = linha.StartsWith("PONTUAÇÃO") || linha == "NOVO RECORDE!";
                Texto(g, linha, destaque ? fonteMedia : fontePequena,
                      destaque ? Color.FromArgb(255, 214, 90) : Color.FromArgb(230, 235, 245),
                      cfg.Largura / 2f, y, StringAlignment.Center);
                y += 24;
            }
        }

        private void EscureceTela(Graphics g, int alfa)
        {
            using var escuro = new SolidBrush(Color.FromArgb(Math.Clamp(alfa, 0, 255), 6, 8, 16));
            g.FillRectangle(escuro, 0, 0, jogo.Config.Largura, jogo.Config.Altura);
        }

        private static void Texto(Graphics g, string texto, Font fonte, Color cor,
                                  float x, float y, StringAlignment alinhamento)
        {
            using var formato = new StringFormat { Alignment = alinhamento };
            using var sombra = new SolidBrush(Color.FromArgb(140, 0, 0, 0));
            using var pincel = new SolidBrush(cor);
            g.DrawString(texto, fonte, sombra, x + 1.5f, y + 1.5f, formato);
            g.DrawString(texto, fonte, pincel, x, y, formato);
        }

        private static Color Cor(uint argb) => Color.FromArgb(unchecked((int)argb));

        private static GraphicsPath Arredondado(RectangleF r, float raio)
        {
            raio = Math.Max(1, Math.Min(raio, Math.Min(r.Width, r.Height) / 2));
            var diametro = raio * 2;
            var caminho = new GraphicsPath();

            caminho.AddArc(r.X, r.Y, diametro, diametro, 180, 90);
            caminho.AddArc(r.Right - diametro, r.Y, diametro, diametro, 270, 90);
            caminho.AddArc(r.Right - diametro, r.Bottom - diametro, diametro, diametro, 0, 90);
            caminho.AddArc(r.X, r.Bottom - diametro, diametro, diametro, 90, 90);
            caminho.CloseFigure();

            return caminho;
        }

        private static int CarregaRecorde()
        {
            try
            {
                if (File.Exists(ArquivoRecorde)
                    && int.TryParse(File.ReadAllText(ArquivoRecorde).Trim(), out var valor))
                {
                    return valor;
                }
            }
            catch
            {
            }
            return 0;
        }

        private static void SalvaRecorde(int valor)
        {
            if (valor <= 0) return;

            try
            {
                var pasta = Path.GetDirectoryName(ArquivoRecorde);
                if (!string.IsNullOrEmpty(pasta)) Directory.CreateDirectory(pasta);
                File.WriteAllText(ArquivoRecorde, valor.ToString());
            }
            catch
            {
            }
        }
    }
}
