namespace JogoCorrida
{
    public class Jogo
    {
        private readonly Random rnd = new();

        private int indiceFase;
        private double pontosDaFase;
        private double distanciaProximaOnda;
        private double tempoEfeitoMs;
        private double tempoInvencivelMs;
        private double tempoDerrapagemMs;

        public Configuracao Config { get; set; } = new();
        public List<Fase> Fases { get; set; } = Fase.FasesPadrao();

        public Elemento Carro { get; private set; } = new();
        public List<Elemento> Elementos { get; } = [];

        public IEnumerable<Elemento> Obstaculos => Elementos.Where(e => e.Tipo == TipoElemento.Obstaculo);

        public EstadoJogo Estado { get; private set; } = EstadoJogo.Menu;
        public int Vidas { get; private set; }
        public double VelocidadeEstrada { get; private set; }
        public double DistanciaPercorrida { get; private set; }
        public double TempoJogoMs { get; private set; }
        public int FaixaAlvo { get; private set; }

        public double PontosTempo { get; private set; }
        public double PontosObstaculos { get; private set; }
        public double PontosMoedas { get; private set; }
        public int Pontuacao => (int)(PontosTempo + PontosObstaculos + PontosMoedas);
        public int Moedas { get; private set; }
        public int ObstaculosSuperados { get; private set; }
        public int MelhorPontuacao { get; set; }

        public Fase FaseAtual => Fases[Math.Clamp(indiceFase, 0, Fases.Count - 1)];
        public int NumeroFase => indiceFase + 1;
        public double ProgressoFase => Math.Clamp(pontosDaFase / FaseAtual.PontosParaConcluir, 0, 1);

        public bool Invencivel => tempoInvencivelMs > 0;
        public bool Derrapando => tempoDerrapagemMs > 0;
        public double TempoInvencivelMs => tempoInvencivelMs;
        public double TempoEfeitoMs => tempoEfeitoMs;

        public double IntensidadeExplosao => Estado == EstadoJogo.Colidindo
            ? Math.Clamp(tempoEfeitoMs / Config.DuracaoExplosaoMs, 0, 1)
            : 0;

        public Elemento? ElementoAtingido { get; private set; }

        public event Action<EventoSom>? Som;

        public void IniciaJogo()
        {
            indiceFase = 0;
            pontosDaFase = 0;
            PontosTempo = PontosObstaculos = PontosMoedas = 0;
            Moedas = ObstaculosSuperados = 0;
            TempoJogoMs = 0;
            DistanciaPercorrida = 0;
            tempoEfeitoMs = tempoInvencivelMs = tempoDerrapagemMs = 0;
            ElementoAtingido = null;
            Vidas = Config.VidasIniciais;
            VelocidadeEstrada = FaseAtual.VelocidadeInicial * Config.Altura;

            Elementos.Clear();

            FaixaAlvo = Config.QtdFaixas / 2;
            Carro = new Elemento
            {
                Tipo = TipoElemento.Carro,
                Largura = Config.LarguraCarro,
                Altura = Config.AlturaCarro,
                Faixa = FaixaAlvo
            };
            Carro.PosicaoX = PosicionaObjeto(FaixaAlvo);
            Carro.PosicaoY = Config.Altura - Config.RecuoCarro - Carro.Altura;

            distanciaProximaOnda = Config.Altura * 0.6;
            Estado = EstadoJogo.Menu;
        }

        public void Comecar()
        {
            if (Estado != EstadoJogo.Menu) IniciaJogo();
            Estado = EstadoJogo.Jogando;
            Som?.Invoke(EventoSom.Inicio);
        }

        public void Confirmar()
        {
            switch (Estado)
            {
                case EstadoJogo.Menu:
                    Comecar();
                    break;
                case EstadoJogo.GameOver:
                case EstadoJogo.Vitoria:
                    IniciaJogo();
                    Comecar();
                    break;
                case EstadoJogo.Pausado:
                    Estado = EstadoJogo.Jogando;
                    break;
            }
        }

        public void AlternarPausa()
        {
            if (Estado == EstadoJogo.Jogando) Estado = EstadoJogo.Pausado;
            else if (Estado == EstadoJogo.Pausado) Estado = EstadoJogo.Jogando;
        }

        public void Atualizar(double deltaMs)
        {
            deltaMs = Math.Clamp(deltaMs, 0, 60);
            var dt = deltaMs / 1000.0;

            foreach (var e in Elementos) e.Animacao += deltaMs;

            if (Estado == EstadoJogo.Colidindo)
            {
                tempoEfeitoMs -= deltaMs;
                if (tempoEfeitoMs <= 0) TerminaExplosao();
                return;
            }

            if (Estado == EstadoJogo.TrocandoFase)
            {
                tempoEfeitoMs -= deltaMs;
                if (tempoEfeitoMs <= 0) Estado = EstadoJogo.Jogando;
                return;
            }

            if (Estado != EstadoJogo.Jogando) return;

            TempoJogoMs += deltaMs;
            if (tempoInvencivelMs > 0) tempoInvencivelMs -= deltaMs;
            if (tempoDerrapagemMs > 0) tempoDerrapagemMs -= deltaMs;

            Acelerar(FaseAtual.AceleracaoPorSegundo * Config.Altura * dt);
            DistanciaPercorrida += VelocidadeEstrada * dt;

            var ganho = FaseAtual.PontosPorSegundo * dt;
            PontosTempo += ganho;
            pontosDaFase += ganho;

            MovimentaCarro(dt);
            MovimentaObstaculos(dt);
            GeraElementos();
            VerificaColetas();
            VerificaColisao();
            VerificaFimDaFase();
        }

        public void Acelerar(double incremento)
        {
            var maxima = FaseAtual.VelocidadeMaxima * Config.Altura;
            VelocidadeEstrada = Math.Min(maxima, VelocidadeEstrada + incremento);
        }

        private void MovimentaCarro(double dt)
        {
            var alvo = PosicionaObjeto(FaixaAlvo);
            var passo = Config.VelocidadeLateral * dt;
            var diferenca = alvo - Carro.PosicaoX;

            if (Math.Abs(diferenca) <= passo)
            {
                Carro.PosicaoX = alvo;
                Carro.Faixa = FaixaAlvo;
            }
            else
            {
                Carro.PosicaoX += Math.Sign(diferenca) * passo;
            }
        }

        public void MovimentaObstaculos(double dt)
        {
            for (int i = Elementos.Count - 1; i >= 0; i--)
            {
                var e = Elementos[i];
                e.PosicaoY += VelocidadeEstrada * e.FatorVelocidade * dt;

                if (e.Ativo && !e.Contabilizado && e.Tipo == TipoElemento.Obstaculo
                    && e.PosicaoY > Carro.PosicaoY + Carro.Altura)
                {
                    e.Contabilizado = true;
                    ObstaculosSuperados++;
                    PontosObstaculos += e.Pontos;
                    pontosDaFase += e.Pontos;
                    Som?.Invoke(EventoSom.ObstaculoSuperado);
                }

                if (!e.Ativo || e.PosicaoY > Config.Altura) Elementos.RemoveAt(i);
            }
        }

        private void GeraElementos()
        {
            if (DistanciaPercorrida < distanciaProximaOnda) return;

            var fase = FaseAtual;
            var vao = fase.DistanciaMinima
                    + rnd.NextDouble() * (fase.DistanciaMaxima - fase.DistanciaMinima);
            distanciaProximaOnda = DistanciaPercorrida + vao * Config.AlturaCarro;

            var maximo = Math.Max(1, Config.QtdFaixas - 1);
            var quantidade = (maximo > 1 && rnd.NextDouble() < fase.ChanceObstaculoDuplo) ? 2 : 1;

            var livres = Enumerable.Range(0, Config.QtdFaixas).ToList();
            for (int i = 0; i < quantidade; i++)
            {
                var faixa = livres[rnd.Next(livres.Count)];
                livres.Remove(faixa);

                var tipo = fase.Obstaculos[rnd.Next(fase.Obstaculos.Length)];
                var ob = Elemento.CriarObstaculo(tipo, Config);
                ob.Faixa = faixa;
                ob.PosicaoX = Config.CentroFaixa(faixa) - ob.Largura / 2.0;
                ob.PosicaoY = -ob.Altura - rnd.Next(0, Config.AlturaCarro);
                Elementos.Add(ob);
            }

            if (livres.Count > 0 && rnd.NextDouble() < fase.ChanceMoeda)
            {
                var faixa = livres[rnd.Next(livres.Count)];
                var quantas = rnd.Next(1, 4);
                var espaco = Config.AlturaMoeda * 1.8;
                var topo = -Config.AlturaMoeda - rnd.Next(0, Config.AlturaCarro);

                for (int i = 0; i < quantas; i++)
                {
                    var moeda = Elemento.CriarMoeda(Config);
                    moeda.Faixa = faixa;
                    moeda.PosicaoX = Config.CentroFaixa(faixa) - moeda.Largura / 2.0;
                    moeda.PosicaoY = topo - i * espaco;
                    moeda.Animacao = i * 120;
                    Elementos.Add(moeda);
                }
            }
        }

        public double PosicionaObjeto(int faixa) => Config.CentroFaixa(faixa) - Config.LarguraCarro / 2.0;

        public void MoverEsquerda() => MoverParaFaixa(FaixaAlvo - 1);
        public void MoverDireita() => MoverParaFaixa(FaixaAlvo + 1);

        public void MoverParaFaixa(int faixa)
        {
            if (Estado != EstadoJogo.Jogando || Derrapando) return;

            faixa = Math.Clamp(faixa, 0, Config.QtdFaixas - 1);
            if (faixa == FaixaAlvo) return;

            FaixaAlvo = faixa;
            Som?.Invoke(EventoSom.TrocaFaixa);
        }

        private void VerificaColetas()
        {
            foreach (var moeda in Elementos)
            {
                if (moeda.Tipo != TipoElemento.Moeda || !moeda.Ativo) continue;
                if (!Carro.Colide(moeda, 2)) continue;

                moeda.Ativo = false;
                Moedas++;
                PontosMoedas += moeda.Pontos;
                pontosDaFase += moeda.Pontos;
                Som?.Invoke(EventoSom.Moeda);
            }
        }

        public bool ChecarColisao()
        {
            foreach (var ob in Elementos)
            {
                if (ob.Tipo == TipoElemento.Obstaculo && ob.Ativo
                    && Carro.Colide(ob, Config.FolgaColisao))
                {
                    return true;
                }
            }
            return false;
        }

        private void VerificaColisao()
        {
            foreach (var ob in Elementos)
            {
                if (ob.Tipo != TipoElemento.Obstaculo || !ob.Ativo) continue;
                if (!Carro.Colide(ob, Config.FolgaColisao)) continue;

                if (ob.Obstaculo == TipoObstaculo.Oleo)
                {
                    ob.Ativo = false;
                    Derrapar();
                    continue;
                }

                if (Invencivel) continue;

                Bater(ob);
                return;
            }
        }

        private void Derrapar()
        {
            tempoDerrapagemMs = Config.DuracaoDerrapagemMs;

            var destino = FaixaAlvo + (rnd.Next(2) == 0 ? -1 : 1);
            if (destino < 0 || destino > Config.QtdFaixas - 1)
            {
                destino = FaixaAlvo - (destino - FaixaAlvo); 
            }

            FaixaAlvo = Math.Clamp(destino, 0, Config.QtdFaixas - 1);
            Som?.Invoke(EventoSom.Derrapagem);
        }

        private void Bater(Elemento ob)
        {
            ob.Ativo = false;
            ElementoAtingido = ob;
            Vidas--;
            Estado = EstadoJogo.Colidindo;
            tempoEfeitoMs = Config.DuracaoExplosaoMs;
            Som?.Invoke(EventoSom.Colisao);
        }

        private void TerminaExplosao()
        {
            tempoEfeitoMs = 0;

            if (Vidas <= 0)
            {
                Estado = EstadoJogo.GameOver;
                AtualizaRecorde();
                Som?.Invoke(EventoSom.GameOver);
                return;
            }

            Elementos.RemoveAll(e => e.Tipo == TipoElemento.Obstaculo
                                  && e.PosicaoY + e.Altura > Carro.PosicaoY - Config.AlturaCarro * 2);

            tempoInvencivelMs = Config.DuracaoInvencivelMs;
            VelocidadeEstrada = FaseAtual.VelocidadeInicial * Config.Altura;
            Estado = EstadoJogo.Jogando;
        }


        private void VerificaFimDaFase()
        {
            if (pontosDaFase < FaseAtual.PontosParaConcluir) return;

            if (indiceFase >= Fases.Count - 1)
            {
                Estado = EstadoJogo.Vitoria;
                AtualizaRecorde();
                Som?.Invoke(EventoSom.Vitoria);
                return;
            }

            indiceFase++;
            pontosDaFase = 0;
            Elementos.Clear();
            VelocidadeEstrada = FaseAtual.VelocidadeInicial * Config.Altura;
            distanciaProximaOnda = DistanciaPercorrida + Config.Altura;
            tempoInvencivelMs = Config.DuracaoInvencivelMs;
            Vidas = Math.Min(Config.VidasMaximas, Vidas + 1); 
            Estado = EstadoJogo.TrocandoFase;
            tempoEfeitoMs = Config.DuracaoTrocaFaseMs;
            Som?.Invoke(EventoSom.NovaFase);
        }

        public bool VerificaFimJogo() => Estado is EstadoJogo.GameOver or EstadoJogo.Vitoria;

        private void AtualizaRecorde()
        {
            if (Pontuacao > MelhorPontuacao) MelhorPontuacao = Pontuacao;
        }
    }
}
