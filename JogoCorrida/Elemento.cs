namespace JogoCorrida
{
    public class Elemento
    {
        public TipoElemento Tipo { get; set; }
        public TipoObstaculo Obstaculo { get; set; } = TipoObstaculo.Nenhum;
        public double PosicaoX { get; set; }
        public double PosicaoY { get; set; }
        public int Faixa { get; set; }
        public int Altura { get; set; }
        public int Largura { get; set; }

        public double FatorVelocidade { get; set; } = 1.0;

        public int Pontos { get; set; }

        public bool Contabilizado { get; set; }
        public bool Ativo { get; set; } = true;

        public double Animacao { get; set; }

        public double CentroX => PosicaoX + Largura / 2.0;
        public double CentroY => PosicaoY + Altura / 2.0;

        public void Movimentar(double x, double y)
        {
            PosicaoX = x;
            PosicaoY = y;
        }

        public bool Colide(Elemento outro, double folga = 0)
        {
            return PosicaoX + folga < outro.PosicaoX + outro.Largura - folga
                && PosicaoX + Largura - folga > outro.PosicaoX + folga
                && PosicaoY + folga < outro.PosicaoY + outro.Altura - folga
                && PosicaoY + Altura - folga > outro.PosicaoY + folga;
        }

        public static Elemento CriarObstaculo(TipoObstaculo tipo, Configuracao cfg)
        {
            var ob = new Elemento
            {
                Tipo = TipoElemento.Obstaculo,
                Obstaculo = tipo
            };

            switch (tipo)
            {
                case TipoObstaculo.CarroLento:
                    ob.Largura = Escala(cfg.LarguraCarro, 1.0);
                    ob.Altura = Escala(cfg.AlturaCarro, 0.8);
                    ob.FatorVelocidade = 0.60;
                    ob.Pontos = 10;
                    break;

                case TipoObstaculo.Caminhao:
                    ob.Largura = Escala(cfg.LarguraCarro, 1.15);
                    ob.Altura = Escala(cfg.AlturaCarro, 1.75);
                    ob.FatorVelocidade = 0.45;
                    ob.Pontos = 20;
                    break;

                case TipoObstaculo.Cone:
                    ob.Largura = Escala(cfg.LarguraCarro, 0.50);
                    ob.Altura = Escala(cfg.AlturaCarro, 0.45);
                    ob.FatorVelocidade = 1.0;
                    ob.Pontos = 5;
                    break;

                case TipoObstaculo.Buraco:
                    ob.Largura = Escala(cfg.LarguraCarro, 0.50);
                    ob.Altura = Escala(cfg.AlturaCarro, 0.45);
                    ob.FatorVelocidade = 1.0;
                    ob.Pontos = 8;
                    break;

                case TipoObstaculo.Oleo:
                    ob.Largura = Escala(cfg.LarguraCarro, 0.50);
                    ob.Altura = Escala(cfg.AlturaCarro, 0.45);
                    ob.FatorVelocidade = 1.0;
                    ob.Pontos = 12;
                    break;
            }
            return ob;
        }

        public static Elemento CriarMoeda(Configuracao cfg)
        {
            return new Elemento
            {
                Tipo = TipoElemento.Moeda,
                Largura = cfg.LarguraMoeda,
                Altura = cfg.AlturaMoeda,
                FatorVelocidade = 1.0,
                Pontos = cfg.PontosPorMoeda
            };
        }

        private static int Escala(int medida, double fator) => Math.Max(1, (int)Math.Round(medida * fator));
    }
}
