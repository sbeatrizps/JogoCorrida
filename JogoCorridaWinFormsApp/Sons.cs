using System.Runtime.InteropServices;
using JogoCorrida;

namespace JogoCorridaWinFormsApp
{
    internal static class Sons
    {
        private enum Onda { Senoide, Quadrada, Serra, Ruido }

        private readonly record struct Nota(double Freq, double FreqFinal, double DuracaoMs, double Volume, Onda Forma);

        [DllImport("winmm.dll", EntryPoint = "PlaySoundW", CharSet = CharSet.Unicode)]
        private static extern bool PlaySound(IntPtr dados, IntPtr modulo, uint opcoes);

        private const uint SND_ASYNC = 0x0001;
        private const uint SND_NODEFAULT = 0x0002;
        private const uint SND_MEMORY = 0x0004;
        private const uint SND_PURGE = 0x0040;

        private const int TaxaAmostragem = 22050;

        private static readonly Dictionary<EventoSom, IntPtr> buffers = [];
        private static readonly Dictionary<EventoSom, double> duracoes = [];
        private static readonly Random rnd = new();

        private static DateTime fimDoSomAtual = DateTime.MinValue;
        private static int prioridadeAtual;

        public static bool Ligado { get; set; } = true;

        public static void Inicializar()
        {
            if (buffers.Count > 0) return;

            Registrar(EventoSom.Inicio,
            [
                new Nota(392, 523, 90, 0.22, Onda.Quadrada),
                new Nota(659, 784, 140, 0.22, Onda.Quadrada)
            ]);

            Registrar(EventoSom.TrocaFaixa,
            [
                new Nota(520, 700, 45, 0.10, Onda.Quadrada)
            ]);

            Registrar(EventoSom.Moeda,
            [
                new Nota(988, 988, 55, 0.22, Onda.Quadrada),
                new Nota(1319, 1319, 110, 0.22, Onda.Quadrada)
            ]);

            Registrar(EventoSom.ObstaculoSuperado,
            [
                new Nota(760, 920, 40, 0.09, Onda.Senoide)
            ]);

            Registrar(EventoSom.Colisao,
            [
                new Nota(1, 1, 260, 0.40, Onda.Ruido),
                new Nota(180, 55, 240, 0.32, Onda.Quadrada)
            ]);

            Registrar(EventoSom.Derrapagem,
            [
                new Nota(1, 1, 200, 0.16, Onda.Ruido),
                new Nota(900, 380, 160, 0.14, Onda.Senoide)
            ]);

            Registrar(EventoSom.NovaFase,
            [
                new Nota(523, 523, 110, 0.22, Onda.Quadrada),
                new Nota(659, 659, 110, 0.22, Onda.Quadrada),
                new Nota(784, 784, 110, 0.22, Onda.Quadrada),
                new Nota(1047, 1047, 220, 0.22, Onda.Quadrada)
            ]);

            Registrar(EventoSom.GameOver,
            [
                new Nota(440, 440, 190, 0.26, Onda.Serra),
                new Nota(392, 392, 190, 0.26, Onda.Serra),
                new Nota(330, 330, 190, 0.26, Onda.Serra),
                new Nota(262, 200, 420, 0.26, Onda.Serra)
            ]);

            Registrar(EventoSom.Vitoria,
            [
                new Nota(523, 523, 110, 0.24, Onda.Quadrada),
                new Nota(659, 659, 110, 0.24, Onda.Quadrada),
                new Nota(784, 784, 110, 0.24, Onda.Quadrada),
                new Nota(1047, 1047, 160, 0.24, Onda.Quadrada),
                new Nota(784, 784, 90, 0.24, Onda.Quadrada),
                new Nota(1047, 1319, 420, 0.24, Onda.Quadrada)
            ]);
        }

        public static void Tocar(EventoSom evento)
        {
            if (!Ligado || !buffers.TryGetValue(evento, out var buffer)) return;

            var prioridade = Prioridade(evento);
            if (DateTime.Now < fimDoSomAtual && prioridade < prioridadeAtual) return;

            System.IO.File.AppendAllText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "sons.txt"), DateTime.Now.ToString("HH:mm:ss.fff") + " antes " + evento + Environment.NewLine);
            try
            {
                PlaySound(buffer, IntPtr.Zero, SND_MEMORY | SND_ASYNC | SND_NODEFAULT);
                System.IO.File.AppendAllText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "sons.txt"), DateTime.Now.ToString("HH:mm:ss.fff") + " depois " + evento + Environment.NewLine);
                prioridadeAtual = prioridade;
                fimDoSomAtual = DateTime.Now.AddMilliseconds(duracoes[evento]);
            }
            catch
            {
                Ligado = false; 
            }
        }

        public static void Parar()
        {
            try
            {
                PlaySound(IntPtr.Zero, IntPtr.Zero, SND_PURGE);
            }
            catch
            {
            }
        }

        private static int Prioridade(EventoSom evento) => evento switch
        {
            EventoSom.Colisao or EventoSom.GameOver or EventoSom.Vitoria or EventoSom.NovaFase => 3,
            EventoSom.Moeda or EventoSom.Derrapagem or EventoSom.Inicio => 2,
            EventoSom.TrocaFaixa => 1,
            _ => 0
        };

        private static void Registrar(EventoSom evento, Nota[] notas)
        {
            var wav = GerarWav(notas);
            var buffer = Marshal.AllocHGlobal(wav.Length);
            Marshal.Copy(wav, 0, buffer, wav.Length);

            buffers[evento] = buffer;
            duracoes[evento] = notas.Sum(n => n.DuracaoMs);
        }

        private static byte[] GerarWav(Nota[] notas)
        {
            var amostras = new List<short>();
            foreach (var nota in notas) GerarNota(nota, amostras);

            var dados = new byte[amostras.Count * 2];
            for (int i = 0; i < amostras.Count; i++)
            {
                dados[i * 2] = (byte)(amostras[i] & 0xFF);
                dados[i * 2 + 1] = (byte)((amostras[i] >> 8) & 0xFF);
            }

            using var memoria = new MemoryStream();
            using var escritor = new BinaryWriter(memoria);

            escritor.Write("RIFF"u8.ToArray());
            escritor.Write(36 + dados.Length);
            escritor.Write("WAVE"u8.ToArray());
            escritor.Write("fmt "u8.ToArray());
            escritor.Write(16);                      
            escritor.Write((short)1);            
            escritor.Write((short)1);        
            escritor.Write(TaxaAmostragem);
            escritor.Write(TaxaAmostragem * 2);   
            escritor.Write((short)2);    
            escritor.Write((short)16);        
            escritor.Write("data"u8.ToArray());
            escritor.Write(dados.Length);
            escritor.Write(dados);
            escritor.Flush();

            return memoria.ToArray();
        }

        private static void GerarNota(Nota nota, List<short> saida)
        {
            var total = (int)(TaxaAmostragem * nota.DuracaoMs / 1000.0);
            double fase = 0;

            for (int i = 0; i < total; i++)
            {
                var avanco = total <= 1 ? 0 : (double)i / (total - 1);
                var frequencia = nota.Freq + (nota.FreqFinal - nota.Freq) * avanco;
                fase += 2 * Math.PI * frequencia / TaxaAmostragem;

                var valor = nota.Forma switch
                {
                    Onda.Senoide => Math.Sin(fase),
                    Onda.Quadrada => Math.Sin(fase) >= 0 ? 1 : -1,
                    Onda.Serra => 2 * (fase / (2 * Math.PI) % 1) - 1,
                    _ => rnd.NextDouble() * 2 - 1
                };

                saida.Add((short)(valor * nota.Volume * Envelope(avanco) * short.MaxValue));
            }
        }

        private static double Envelope(double avanco)
        {
            const double ataque = 0.03;
            const double queda = 0.35;

            if (avanco < ataque) return avanco / ataque;
            if (avanco > 1 - queda) return (1 - avanco) / queda;
            return 1;
        }
    }
}
