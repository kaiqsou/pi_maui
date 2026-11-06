namespace European.Models
{
    /// <summary>Documento do MongoDB identificado pelo Id (ObjectId em texto).</summary>
    public interface IEntidade
    {
        string Id { get; set; }
    }

    /// <summary>Documento com datas de criação e alteração, preenchidas pelos repositórios.</summary>
    public interface IAuditavel : IEntidade
    {
        DateTime CreatedAt { get; set; }
        DateTime UpdatedAt { get; set; }
    }

    /// <summary>Item da árvore do curso (módulo, matéria ou aula), exibido na ordem definida pelo campo Ordem.</summary>
    public interface IOrdenavel : IEntidade
    {
        int Ordem { get; set; }
    }
}
