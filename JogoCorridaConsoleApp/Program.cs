using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text;
using JogoCorrida;

[SupportedOSPlatform("windows")]
class Program
{
    private const int Colunas = 45;
    private const int Linhas = 26;

    private static readonly char[,] tela = new char[Linhas, Colunas];
    private static readonly ConsoleColor[,] cores = new ConsoleColor[Linhas, Colunas];

    private static readonly Jogo jogo = new();
    private static DateTime proximoSom = DateTime.MinValue;

    static void Main()
    {
        PreparaConsole();
        ConfiguraJogo();

        jogo.Som += TocarSom;
        jogo.IniciaJogo();

        var relogio = Stopwatch.StartNew();
        var ultimoQuadro = relogio.Elapsed.TotalMilliseconds;
        var rodando = true;

        while (rodando)
        {
            var agora = relogio.Elapsed.TotalMilliseconds;
            var delta = agora - ultimoQuadro;
            ultimoQuadro = agora;

            rodando = LeTeclado();
            jogo.Atualizar(delta);

            DesenhaCenario();
            DesenhaElementos();
            DesenhaCarro();
            DesenhaMensagens();
            MostraTela();
            DesenhaPainel();

            Thread.Sleep(16);
        }

        Console.CursorVisible = true;
        Console.ResetColor();
        Console.SetCursorPosition(0, Linhas + 4);
    }

    private static void PreparaConsole()
    {
        Console.CursorVisible = false;
        Console.OutputEncoding = Encoding.UTF8;
        Console.Title = "Jogo Corrida - IFSP";

        try
        {
            Console.SetWindowSize(Colunas + 2, Linhas + 6);
            Console.SetBufferSize(Colunas + 2, Linhas + 6);
        }
        catch
        {
        }

        Console.Clear();
    }

    private static void ConfiguraJogo()
    {
        var cfg = jogo.Config;
        cfg.Largura = Colunas;
        cfg.Altura = Linhas;
        cfg.QtdFaixas = 3;
        cfg.MargemPista = 3;
        cfg.LarguraCarro = 5;
        cfg.AlturaCarro = 3;
        cfg.LarguraMoeda = 1;
        cfg.AlturaMoeda = 1;
        cfg.RecuoCarro = 2;
        cfg.VelocidadeLateral = 26;
        cfg.FolgaColisao = 0.6;
        cfg.DuracaoExplosaoMs = 700;
    }

    private static bool LeTeclado()
    {
        while (Console.KeyAvailable)
        {
            var tecla = Console.ReadKey(true);
            switch (tecla.Key)
            {
                case ConsoleKey.LeftArrow:
                case ConsoleKey.A:
                    jogo.MoverEsquerda();
                    break;

                case ConsoleKey.RightArrow:
                case ConsoleKey.D:
                    jogo.MoverDireita();
                    break;

                case ConsoleKey.Enter:
                case ConsoleKey.Spacebar:
                    jogo.Confirmar();
                    break;

                case ConsoleKey.P:
                    jogo.AlternarPausa();
                    break;

                case ConsoleKey.Escape:
                    if (jogo.Estado == EstadoJogo.Jogando) jogo.AlternarPausa();
                    else return false;
                    break;
            }
        }
        return true;
    }

    private static void DesenhaCenario()
    {
        var cfg = jogo.Config;

        for (int l = 0; l < Linhas; l++)
        {
            for (int c = 0; c < Colunas; c++)
            {
                var naPista = c >= cfg.MargemPista && c < cfg.MargemPista + cfg.LarguraPista;
                tela[l, c] = naPista ? ' ' : '.';
                cores[l, c] = naPista ? ConsoleColor.Gray : ConsoleColor.DarkGreen;
            }
        }

        for (int l = 0; l < Linhas; l++)
        {
            Escreve(l, cfg.MargemPista, '|', ConsoleColor.White);
            Escreve(l, cfg.MargemPista + cfg.LarguraPista - 1, '|', ConsoleColor.White);
        }

        var deslocamento = (int)(jogo.DistanciaPercorrida % 4);
        for (int i = 1; i < cfg.QtdFaixas; i++)
        {
            var coluna = cfg.MargemPista + cfg.LarguraFaixa * i;
            for (int l = -deslocamento; l < Linhas; l += 4)
            {
                Escreve(l, coluna, '|', ConsoleColor.DarkYellow);
                Escreve(l + 1, coluna, '|', ConsoleColor.DarkYellow);
            }
        }
    }

    private static void DesenhaElementos()
    {
        foreach (var el in jogo.Elementos)
        {
            var (simbolo, cor) = el.Tipo == TipoElemento.Moeda
                ? ('$', ConsoleColor.Yellow)
                : el.Obstaculo switch
                {
                    TipoObstaculo.CarroLento => ('#', ConsoleColor.Red),
                    TipoObstaculo.Caminhao => ('H', ConsoleColor.White),
                    TipoObstaculo.Cone => ('^', ConsoleColor.DarkYellow),
                    TipoObstaculo.Buraco => ('O', ConsoleColor.DarkGray),
                    TipoObstaculo.Oleo => ('~', ConsoleColor.DarkMagenta),
                    _ => ('?', ConsoleColor.White)
                };

            Preenche(el, simbolo, cor);
        }
    }

    private static void DesenhaCarro()
    {
        if (jogo.Invencivel && (int)(jogo.TempoInvencivelMs / 120) % 2 == 0) return;

        var explodindo = jogo.Estado == EstadoJogo.Colidindo;
        Preenche(jogo.Carro, explodindo ? '*' : '8',
                 explodindo ? ConsoleColor.Yellow : ConsoleColor.Cyan);

        if (explodindo)
        {
            var raio = (int)((1 - jogo.IntensidadeExplosao) * 6) + 1;
            var centroL = (int)jogo.Carro.CentroY;
            var centroC = (int)jogo.Carro.CentroX;

            for (int a = 0; a < 12; a++)
            {
                var angulo = a * Math.PI / 6;
                Escreve(centroL + (int)(Math.Sin(angulo) * raio),
                        centroC + (int)(Math.Cos(angulo) * raio * 1.8),
                        a % 2 == 0 ? '*' : '+',
                        a % 3 == 0 ? ConsoleColor.Red : ConsoleColor.Yellow);
            }
        }
    }

    private static void DesenhaMensagens()
    {
        switch (jogo.Estado)
        {
            case EstadoJogo.Menu:
                Centraliza(Linhas / 2 - 4, "=== JOGO CORRIDA - IFSP ===", ConsoleColor.Yellow);
                Centraliza(Linhas / 2 - 2, "SETAS: trocar de faixa", ConsoleColor.White);
                Centraliza(Linhas / 2 - 1, "P: pausar    ESC: sair", ConsoleColor.White);
                Centraliza(Linhas / 2 + 1, "Desvie, pegue as moedas", ConsoleColor.Gray);
                Centraliza(Linhas / 2 + 2, $"e vença as {jogo.Fases.Count} fases!", ConsoleColor.Gray);
                Centraliza(Linhas / 2 + 4, "ENTER para começar", ConsoleColor.Green);
                break;

            case EstadoJogo.Pausado:
                Centraliza(Linhas / 2, ">>> PAUSA <<<", ConsoleColor.Yellow);
                Centraliza(Linhas / 2 + 2, "ENTER para continuar", ConsoleColor.White);
                break;

            case EstadoJogo.TrocandoFase:
                Centraliza(Linhas / 2 - 1, $"FASE {jogo.NumeroFase}", ConsoleColor.Yellow);
                Centraliza(Linhas / 2 + 1, jogo.FaseAtual.Nome, ConsoleColor.White);
                break;

            case EstadoJogo.GameOver:
            case EstadoJogo.Vitoria:
                var titulo = jogo.Estado == EstadoJogo.Vitoria ? "*** VOCÊ VENCEU! ***" : "*** GAME OVER ***";
                Centraliza(Linhas / 2 - 4, titulo,
                           jogo.Estado == EstadoJogo.Vitoria ? ConsoleColor.Green : ConsoleColor.Red);
                Centraliza(Linhas / 2 - 2, $"Pontuação: {jogo.Pontuacao}", ConsoleColor.Yellow);
                Centraliza(Linhas / 2 - 1, $"Tempo: {(int)jogo.PontosTempo} pts", ConsoleColor.White);
                Centraliza(Linhas / 2, $"Obstáculos: {jogo.ObstaculosSuperados} ({(int)jogo.PontosObstaculos} pts)", ConsoleColor.White);
                Centraliza(Linhas / 2 + 1, $"Moedas: {jogo.Moedas} ({(int)jogo.PontosMoedas} pts)", ConsoleColor.White);
                Centraliza(Linhas / 2 + 2, $"Recorde: {jogo.MelhorPontuacao}", ConsoleColor.Yellow);
                Centraliza(Linhas / 2 + 4, "ENTER joga de novo | ESC sai", ConsoleColor.Green);
                break;
        }
    }

    private static void MostraTela()
    {
        Console.SetCursorPosition(0, 0);

        var linha = new StringBuilder(Colunas);
        for (int l = 0; l < Linhas; l++)
        {
            var corAtual = cores[l, 0];
            linha.Clear();

            for (int c = 0; c < Colunas; c++)
            {
                if (cores[l, c] != corAtual)
                {
                    Console.ForegroundColor = corAtual;
                    Console.Write(linha.ToString());
                    linha.Clear();
                    corAtual = cores[l, c];
                }
                linha.Append(tela[l, c]);
            }

            Console.ForegroundColor = corAtual;
            Console.WriteLine(linha.ToString());
        }
    }

    private static void DesenhaPainel()
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($" PONTOS: {jogo.Pontuacao,-7} VIDAS: {new string('*', Math.Max(0, jogo.Vidas)),-5} MOEDAS: {jogo.Moedas,-4}".PadRight(Colunas));
        Console.WriteLine($" FASE {jogo.NumeroFase}/{jogo.Fases.Count} - {jogo.FaseAtual.Nome}".PadRight(Colunas));

        var largura = 20;
        var cheio = (int)(largura * jogo.ProgressoFase);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($" [{new string('=', cheio)}{new string(' ', largura - cheio)}] {jogo.ProgressoFase * 100:F0}%".PadRight(Colunas));
        Console.ResetColor();
    }

    private static void Preenche(Elemento el, char simbolo, ConsoleColor cor)
    {
        for (int l = 0; l < el.Altura; l++)
        {
            for (int c = 0; c < el.Largura; c++)
            {
                Escreve((int)Math.Round(el.PosicaoY) + l, (int)Math.Round(el.PosicaoX) + c, simbolo, cor);
            }
        }
    }

    private static void Escreve(int linha, int coluna, char simbolo, ConsoleColor cor)
    {
        if (linha < 0 || linha >= Linhas || coluna < 0 || coluna >= Colunas) return;
        tela[linha, coluna] = simbolo;
        cores[linha, coluna] = cor;
    }

    private static void Centraliza(int linha, string texto, ConsoleColor cor)
    {
        var inicio = (Colunas - texto.Length) / 2;
        for (int i = 0; i < texto.Length; i++)
        {
            Escreve(linha, inicio + i, texto[i], cor);
        }
    }

    private static void TocarSom(EventoSom evento)
    {
        if (DateTime.Now < proximoSom) return;

        var notas = evento switch
        {
            EventoSom.Moeda => new[] { (988, 60), (1319, 70) },
            EventoSom.Colisao => [(220, 120), (110, 200)],
            EventoSom.GameOver => [(440, 180), (330, 180), (220, 320)],
            EventoSom.NovaFase => [(523, 90), (659, 90), (784, 140)],
            EventoSom.Vitoria => [(523, 100), (659, 100), (784, 100), (1047, 250)],
            EventoSom.Derrapagem => [(180, 100), (140, 100)],
            EventoSom.Inicio => [(392, 80), (784, 120)],
            _ => []
        };

        if (notas.Length == 0) return;

        proximoSom = DateTime.Now.AddMilliseconds(notas.Sum(n => n.Item2) + 40);

        Task.Run(() =>
        {
            try
            {
                foreach (var (frequencia, duracao) in notas) Console.Beep(frequencia, duracao);
            }
            catch
            {
            }
        });
    }
}
