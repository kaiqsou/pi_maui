using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace European.ViewModels
{
    /// <summary>Paginação "‹ 1 / 3 ›" das listas (módulos do curso, cursos do cadastro). Exibida pelo controle BarraPaginacao.</summary>
    public partial class Paginador : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Indicador))]
        [NotifyCanExecuteChangedFor(nameof(AnteriorCommand), nameof(ProximaCommand))]
        public partial int Indice { get; private set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Indicador), nameof(TemPaginacao))]
        [NotifyCanExecuteChangedFor(nameof(AnteriorCommand), nameof(ProximaCommand))]
        public partial int Total { get; private set; } = 1;

        public bool TemPaginacao => Total > 1;
        public string Indicador => $"{Indice + 1} / {Total}";

        /// <summary>Disparado quando a página muda (pelos botões ou por <see cref="Reiniciar"/>).</summary>
        public event Action? Mudou;

        /// <summary>Define quantas páginas existem e vai para a página indicada.</summary>
        public void Reiniciar(int total, int indice = 0)
        {
            Total = Math.Max(1, total);
            Indice = Math.Clamp(indice, 0, Total - 1);
            Mudou?.Invoke();
        }

        private bool PodeAnterior() => Indice > 0;
        private bool PodeProxima() => Indice < Total - 1;

        [RelayCommand(CanExecute = nameof(PodeAnterior))]
        private void Anterior() => IrPara(Indice - 1);

        [RelayCommand(CanExecute = nameof(PodeProxima))]
        private void Proxima() => IrPara(Indice + 1);

        private void IrPara(int indice)
        {
            if (indice < 0 || indice >= Total)
                return;

            Indice = indice;
            Mudou?.Invoke();
        }
    }
}
