namespace LuminaAVA.Views;

public sealed record Disciplina(string Nome, int Presenca, double Media);

public static class AlunoData
{
    public const string Nome = "Gabriel Silva";
    public const string Curso = "Engenharia de Software";
    public const string Matricula = "2024013847";
    public const string Polo = "Polo Central";
    public const string Turno = "Noturno";
    public const string Matriz = "Matriz 2024";
    public const int Semestre = 4;
    public const double MediaGeral = 9.2;
    public const double Cr = 8.9;
    public const int CargaConcluida = 1480;
    public const int CargaTotal = 3200;

    public static string SemestreTexto => $"{Semestre}º Semestre";

    public static string CrTexto => Cr.ToString("0.0", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));

    public static string CargaTexto => $"{CargaConcluida:N0}h / {CargaTotal:N0}h";

    public static readonly Disciplina[] Disciplinas =
    {
        new("Arquitetura de Microsserviços", 100, 9.4),
        new("Banco de Dados NoSQL", 96, 8.7),
        new("Dev Mobile com .NET MAUI", 100, 9.8),
        new("Segurança & DevSecOps", 94, 8.5),
    };
}
