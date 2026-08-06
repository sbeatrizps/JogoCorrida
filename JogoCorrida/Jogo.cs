namespace JogoCorrida
{
    public class Jogo
    {
        public Elemento Carro {  get; set; }
        public List<Elemento> Obstaculos { get; set; }
        public int Velocidade { get; set; }
        public int Pontuacao { get; set; }
        public int Tempo { get; set; }
        public int MelhorPontuacao { get; set; }
        public int ColisoesPermitidas { get; set; }
        public int Faixa1Inicio {  get; set; }
        public int Faixa1Fim { get; set; }
        public int Faixa2Inicio { get; set; }
        public int Faixa2Fim { get; set; }
        public int YMaximo { get; set; } = 50;

        public void IniciaJogo()
        {
            Carro = new Elemento();
            Carro.Tipo = TipoElemento.Carro;
            Carro.PosicaoX = PosicionaObjeto(1);
            Carro.PosicaoY = YMaximo - 10;

            Obstaculos = FabricaObstaculos(3, 10, 50);
        }

        public List<Elemento> FabricaObstaculos(int qtd, int dmin, int dmax)
        {
            var y_inicial = 0;
            var rnd = new Random();
            var obstaculos = new List<Elemento>();

            for (int i = 0; i < qtd; i++)
            {
                if (i != 0)
                {
                    y_inicial -= rnd.Next(dmin, dmax);
                }
                var ob = new Elemento();
                ob.Tipo = TipoElemento.Obstaculo;
                var faixa = rnd.Next(1, 2);
                ob.PosicaoX = PosicionaObjeto(faixa);
                ob.PosicaoY = y_inicial;
            }
            return obstaculos;
        }

        public int PosicionaObjeto(int faixa)
        {
            if(faixa == 1)
            {
                return (Faixa1Fim + Faixa1Inicio) / 2;
            }
            else
            {
                return (Faixa2Fim + Faixa2Inicio) / 2;
            }
        }

        public void Acelerar(int incremento)
        {
            Velocidade += incremento;
        }
        private int ChecaFaixaElemento(Elemento elemento)
        {
            if(elemento.PosicaoX >= Faixa1Inicio &&
                elemento.PosicaoX <= Faixa1Fim)
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }
        public bool ChecarColisao()
        {
            foreach(var ob in Obstaculos)
            {
                if(ChecaFaixaElemento(Carro) == ChecaFaixaElemento(ob))
                {
                    if(Math.Abs(Carro.PosicaoY-ob.PosicaoY) <= 10)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public bool VerificaFimJogo()
        {
            return true;
        }
    }
}
