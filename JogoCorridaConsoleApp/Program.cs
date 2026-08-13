class Program
{
    static void Main()
    {
        //Thread keyThread = new Thread(ListenerForKeys);
        Console.WriteLine("IFSP");
        var posicao = "Left";
        for(; ; )
        {
            DesenhaCenario();
            DesenhaElemento(2, 6, '0');
            DesenhaElemento(4, 18, '0');
            DesenhaElemento(10, posicao == "Left" ? 6 : 18, '8');
            if (Console.KeyAvailable)
            {
                var tecla = Console.ReadKey();
                Console.WriteLine(tecla.Key.ToString());
                if(tecla.Key == ConsoleKey.LeftArrow)
                {
                    posicao = "Left";
                }
                else if (tecla.Key == ConsoleKey.RightArrow)
                {
                    posicao = "Right";
                }
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
}



