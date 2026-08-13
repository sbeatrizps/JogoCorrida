using JogoCorrida;

class Program
{
    static void Main()
    {
        Jogo jogo = new Jogo
        {
            Faixa1Inicio = 2,
            Faixa1Fim = 12,
            Faixa2Inicio = 14,
            Faixa2Fim = 24
        };
        jogo.YMaximo = 11;
        jogo.IniciaJogo();
        jogo.Carro.PosicaoX = jogo.PosicionaObjeto(1);
        
        jogo.Velocidade = 10;
        var tempoUltimaMovimentacao = DateTime.Now;

        for(; ; )
        {
            DesenhaCenario();
            DesenhaElemento(jogo.Carro.PosicaoY, jogo.Carro.PosicaoX, '8');

            foreach (var ob in jogo.Obstaculos) 
            {
                if (ob.PosicaoY >= 0)
                {
                    DesenhaElemento(ob.PosicaoY, ob.PosicaoX, '0');
                }
            }

            if((DateTime.Now - tempoUltimaMovimentacao).Microseconds >= jogo.Velocidade)
            {
                tempoUltimaMovimentacao = DateTime.Now;
                jogo.MovimentaObstaculos();
            }

            if (Console.KeyAvailable)
            {
                var tecla = Console.ReadKey();
                if(tecla.Key == ConsoleKey.LeftArrow)
                {
                    jogo.Carro.PosicaoX = jogo.PosicionaObjeto(1);
                }
                else if (tecla.Key == ConsoleKey.RightArrow)
                {
                    jogo.Carro.PosicaoX = jogo.PosicionaObjeto(2);
                }

                TocarSom();
            }
            if (jogo.ChecarColisao())
            {
                GameOver();
                TocarSom();
                break;
            }
            Thread.Sleep(100);
        }
    }

    public static void DesenhaElemento(int linha, int coluna, char simbolo)
    {
        var xOriginal = Console.CursorLeft;
        var yOriginal = Console.CursorTop;
        Console.SetCursorPosition(coluna, linha);
        Console.Write(simbolo.ToString());
        Console.SetCursorPosition(xOriginal, yOriginal);
    }

    public static void DesenhaCenario()
    {
        Console.Clear();
        Console.WriteLine("+-----------+-----------+");
        Console.WriteLine("|           |           |");
        Console.WriteLine("|           |           |");
        Console.WriteLine("|           |           |");
        Console.WriteLine("|           |           |");
        Console.WriteLine("|           |           |");
        Console.WriteLine("|           |           |");
        Console.WriteLine("|           |           |");
        Console.WriteLine("|           |           |");
        Console.WriteLine("|           |           |");
        Console.WriteLine("|           |           |");
        Console.WriteLine("+-----------+-----------+");
    }

    public static void GameOver()
    {
        Console.Clear();
        Console.WriteLine("  GAME OVER  ");
    }

    static void TocarSom()
    {
        Console.Beep();
    }
}



