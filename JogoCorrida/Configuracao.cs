namespace JogoCorrida
{
    public class Configuracao
    {
        public int Largura { get; set; } = 480;
        public int Altura { get; set; } = 640;

        public int MargemPista { get; set; } = 40;
        public int QtdFaixas { get; set; } = 3;

        public int LarguraCarro { get; set; } = 100;
        public int AlturaCarro { get; set; } = 100;
        public int LarguraMoeda { get; set; } = 25;
        public int AlturaMoeda { get; set; } = 25;

        public int RecuoCarro { get; set; } = 10;

        public double VelocidadeLateral { get; set; } = 500;

        public double FolgaColisao { get; set; } = 10;

        public double DuracaoExplosaoMs { get; set; } = 800;
        public double DuracaoInvencivelMs { get; set; } = 2400;
        public double DuracaoTrocaFaseMs { get; set; } = 4800;
        public double DuracaoDerrapagemMs { get; set; } = 600;

        public int VidasIniciais { get; set; } = 3;
        public int VidasMaximas { get; set; } = 5;
        public int PontosPorMoeda { get; set; } = 50;

        public int LarguraPista => Largura - 2 * MargemPista;
        public int LarguraFaixa => LarguraPista / QtdFaixas;

        public double CentroFaixa(int faixa)
        {
            faixa = Math.Clamp(faixa, 0, QtdFaixas - 1);
            return MargemPista + LarguraFaixa * faixa + LarguraFaixa / 2.0;
        }
    }
}
