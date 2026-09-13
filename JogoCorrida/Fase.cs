namespace JogoCorrida
{
    public class Fase
    {
        public int Numero { get; set; }
        public string Nome { get; set; } = "";
        public string Descricao { get; set; } = "";

        public double VelocidadeInicial { get; set; } = 0.40;
        public double VelocidadeMaxima { get; set; } = 0.60;
        public double AceleracaoPorSegundo { get; set; } = 0.012;

        public double PontosPorSegundo { get; set; } = 8;
        public int PontosParaConcluir { get; set; } = 800;

        public double DistanciaMinima { get; set; } = 3.0;
        public double DistanciaMaxima { get; set; } = 5.0;

        public double ChanceMoeda { get; set; } = 0.50;
        public double ChanceObstaculoDuplo { get; set; } = 0.15;

        public TipoObstaculo[] Obstaculos { get; set; } = [TipoObstaculo.CarroLento, TipoObstaculo.Cone];

        public bool Noite { get; set; }

        public uint CorGrama { get; set; } = 0xFF3E8E4E;
        public uint CorAsfalto { get; set; } = 0xFF4A4A52;
        public uint CorFaixa { get; set; } = 0xFFF2E85C;

        public static List<Fase> FasesPadrao() =>
        [
            new Fase
            {
                Numero = 1,
                Nome = "AVENIDA CENTRAL",
                Descricao = "Transito leve. Desvie dos carros e pegue as moedas!",
                VelocidadeInicial = 0.42,
                VelocidadeMaxima = 0.62,
                AceleracaoPorSegundo = 0.012,
                PontosPorSegundo = 12,
                PontosParaConcluir = 450,
                DistanciaMinima = 3.2,
                DistanciaMaxima = 4.8,
                ChanceMoeda = 0.55,
                ChanceObstaculoDuplo = 0.15,
                Obstaculos = [TipoObstaculo.CarroLento, TipoObstaculo.Cone],
                CorGrama = 0xFF3E8E4E,
                CorAsfalto = 0xFF4A4A52,
                CorFaixa = 0xFFF2E85C
            },
            new Fase
            {
                Numero = 2,
                Nome = "RODOVIA",
                Descricao = "Caminhoes e buracos no asfalto. Atenção redobrada!",
                VelocidadeInicial = 0.55,
                VelocidadeMaxima = 0.80,
                AceleracaoPorSegundo = 0.016,
                PontosPorSegundo = 16,
                PontosParaConcluir = 700,
                DistanciaMinima = 3.0,
                DistanciaMaxima = 4.4,
                ChanceMoeda = 0.60,
                ChanceObstaculoDuplo = 0.35,
                Obstaculos =
                [
                    TipoObstaculo.CarroLento, TipoObstaculo.Cone,
                    TipoObstaculo.Buraco, TipoObstaculo.Caminhao
                ],
                CorGrama = 0xFFC2A24A,
                CorAsfalto = 0xFF5A5048,
                CorFaixa = 0xFFFFF3C4
            },
            new Fase
            {
                Numero = 3,
                Nome = "RODOVIA NOTURNA",
                Descricao = "Pista molhada, oleo escorregadio e velocidade maxima!",
                VelocidadeInicial = 0.70,
                VelocidadeMaxima = 0.95,
                AceleracaoPorSegundo = 0.020,
                PontosPorSegundo = 20,
                PontosParaConcluir = 950,
                DistanciaMinima = 2.9,
                DistanciaMaxima = 4.2,
                ChanceMoeda = 0.70,
                ChanceObstaculoDuplo = 0.50,
                Obstaculos =
                [
                    TipoObstaculo.CarroLento, TipoObstaculo.Cone, TipoObstaculo.Buraco,
                    TipoObstaculo.Caminhao, TipoObstaculo.Oleo, TipoObstaculo.Oleo
                ],
                Noite = true,
                CorGrama = 0xFF16203A,
                CorAsfalto = 0xFF25252E,
                CorFaixa = 0xFFEDEDED
            }
        ];
    }
}
