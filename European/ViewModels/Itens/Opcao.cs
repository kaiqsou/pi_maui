using European.Models;

namespace European.ViewModels
{
    /// <summary>Item de Picker: o valor escolhido e o texto exibido.</summary>
    public class Opcao<T>
    {
        public T? Valor { get; init; }
        public string Texto { get; init; } = string.Empty;
    }

    // Tipos concretos para usar com x:DataType no XAML (que não aceita tipos genéricos).
    public class CursoOpcao : Opcao<Curso> { }
    public class MateriaOpcao : Opcao<Materia> { }
    public class SalaOpcao : Opcao<Sala> { }
}
