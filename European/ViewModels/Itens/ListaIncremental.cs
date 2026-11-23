using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace European.ViewModels
{
    /// <summary>
    /// Lista longa mostrada aos poucos, como nos apps de celular: começa com alguns itens e acrescenta mais
    /// ao chegar perto do fim (carregar ao rolar) ou no botão "Ver mais". Acompanha a lista de origem.
    ///
    /// Na tela: ItemsSource="{Binding X.Visiveis}" e, para carregar ao rolar, RemainingItemsThreshold="3"
    /// RemainingItemsThresholdReachedCommand="{Binding X.CarregarMaisCommand}"; para o botão, X.TemMais e X.VerMaisTexto.
    /// </summary>
    public partial class ListaIncremental<T> : ObservableObject
    {
        private readonly int _porVez;
        private IList<T> _fonte = new List<T>();
        private int _quantidade;

        public ListaIncremental(int porVez, IList<T>? fonte = null)
        {
            _porVez = Math.Max(1, porVez);
            _quantidade = _porVez;
            if (fonte != null)
                Fonte = fonte;
        }

        /// <summary>Itens já mostrados.</summary>
        public ObservableCollection<T> Visiveis { get; } = new();

        public bool TemMais => _fonte.Count > Visiveis.Count;
        public bool Vazia => _fonte.Count == 0;
        public string VerMaisTexto => $"Ver mais ({_fonte.Count - Visiveis.Count})";

        /// <summary>A lista completa. Trocar a lista volta a mostrar só o começo.</summary>
        public IList<T> Fonte
        {
            get => _fonte;
            set
            {
                if (_fonte is INotifyCollectionChanged antiga)
                    antiga.CollectionChanged -= AoMudarFonte;
                _fonte = value ?? new List<T>();
                if (_fonte is INotifyCollectionChanged nova)
                    nova.CollectionChanged += AoMudarFonte;
                _quantidade = _porVez;
                Reaplicar();
            }
        }

        /// <summary>
        /// A lista de origem mudou (filtro, reordenação...): mantém quantos itens o usuário já tinha aberto. As mudanças
        /// em sequência (ex.: esvaziar e preencher de novo) são juntadas numa só atualização, para a tela não ficar vazia
        /// por um instante e a rolagem não voltar ao topo.
        /// </summary>
        private void AoMudarFonte(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (_atualizacaoAgendada)
                return;

            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null)
            {
                Reaplicar();
                return;
            }

            _atualizacaoAgendada = true;
            dispatcher.Dispatch(() =>
            {
                _atualizacaoAgendada = false;
                Reaplicar();
            });
        }

        private bool _atualizacaoAgendada;

        /// <summary>Atualiza os itens mostrados no lugar (troca só o que mudou), sem esvaziar a lista.</summary>
        private void Reaplicar()
        {
            var alvo = _fonte.Take(_quantidade).ToList();
            for (var i = 0; i < alvo.Count; i++)
            {
                if (i >= Visiveis.Count)
                    Visiveis.Add(alvo[i]);
                else if (!EqualityComparer<T>.Default.Equals(Visiveis[i], alvo[i]))
                    Visiveis[i] = alvo[i];
            }

            while (Visiveis.Count > alvo.Count)
                Visiveis.RemoveAt(Visiveis.Count - 1);
            Avisar();
        }

        [RelayCommand]
        private void CarregarMais()
        {
            if (!TemMais)
                return;

            _quantidade = Visiveis.Count + _porVez;
            // acrescenta no fim, sem refazer a lista (a rolagem fica onde está)
            foreach (var item in _fonte.Skip(Visiveis.Count).Take(_porVez))
                Visiveis.Add(item);
            Avisar();
        }

        private void Avisar()
        {
            OnPropertyChanged(nameof(TemMais));
            OnPropertyChanged(nameof(VerMaisTexto));
            OnPropertyChanged(nameof(Vazia));
        }
    }
}
