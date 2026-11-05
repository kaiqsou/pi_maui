using European.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace European.Data
{
    /// <summary>
    /// Acesso ao MongoDB. A conexão padrão é o servidor local; para usar outro, defina a variável
    /// de ambiente EUROPEAN_MONGO (ex.: mongodb+srv://usuario:senha@cluster/...) antes de abrir o app.
    /// </summary>
    public static class MongoContext
    {
        public const string ConexaoPadrao = "mongodb://localhost:27017";
        public const string BancoPadrao = "european";

        private static readonly Lazy<IMongoDatabase> _banco = new(Criar);
        private static readonly SemaphoreSlim _inicializacao = new(1, 1);
        private static bool _inicializado;

        public static IMongoDatabase Banco => _banco.Value;

        public static IMongoCollection<Usuario> Usuarios => Banco.GetCollection<Usuario>("usuarios");
        public static IMongoCollection<Curso> Cursos => Banco.GetCollection<Curso>("cursos");
        public static IMongoCollection<Matricula> Matriculas => Banco.GetCollection<Matricula>("matriculas");
        public static IMongoCollection<ProgressoAula> Progressos => Banco.GetCollection<ProgressoAula>("progressos");
        public static IMongoCollection<MaterialComplementar> Materiais => Banco.GetCollection<MaterialComplementar>("materiais");
        public static IMongoCollection<Prova> Provas => Banco.GetCollection<Prova>("provas");
        public static IMongoCollection<ResultadoProva> Resultados => Banco.GetCollection<ResultadoProva>("resultados");
        public static IMongoCollection<Certificado> Certificados => Banco.GetCollection<Certificado>("certificados");
        public static IMongoCollection<Sala> Salas => Banco.GetCollection<Sala>("salas");
        public static IMongoCollection<SolicitacaoMatricula> Solicitacoes => Banco.GetCollection<SolicitacaoMatricula>("solicitacoes");

        public static GridFSBucket Arquivos => new(Banco, new GridFSBucketOptions { BucketName = "arquivos" });

        public static string NovoId() => ObjectId.GenerateNewId().ToString();

        /// <summary>Cria os índices e os cursos iniciais na primeira execução.</summary>
        public static async Task InicializarAsync()
        {
            if (_inicializado)
                return;

            await _inicializacao.WaitAsync();
            try
            {
                if (_inicializado)
                    return;

                await Usuarios.Indexes.CreateOneAsync(new CreateIndexModel<Usuario>(
                    Builders<Usuario>.IndexKeys.Ascending(u => u.Email),
                    new CreateIndexOptions { Unique = true }));
                await Solicitacoes.Indexes.CreateOneAsync(new CreateIndexModel<SolicitacaoMatricula>(
                    Builders<SolicitacaoMatricula>.IndexKeys.Ascending(s => s.Email),
                    new CreateIndexOptions { Unique = true }));
                await Matriculas.Indexes.CreateOneAsync(new CreateIndexModel<Matricula>(
                    Builders<Matricula>.IndexKeys.Ascending(m => m.AlunoId).Ascending(m => m.CursoId)));
                await Progressos.Indexes.CreateOneAsync(new CreateIndexModel<ProgressoAula>(
                    Builders<ProgressoAula>.IndexKeys.Ascending(p => p.MatriculaId).Ascending(p => p.AulaId),
                    new CreateIndexOptions { Unique = true }));
                await Resultados.Indexes.CreateOneAsync(new CreateIndexModel<ResultadoProva>(
                    Builders<ResultadoProva>.IndexKeys.Ascending(r => r.AlunoId).Ascending(r => r.ProvaId)));
                await Materiais.Indexes.CreateOneAsync(new CreateIndexModel<MaterialComplementar>(
                    Builders<MaterialComplementar>.IndexKeys.Ascending(m => m.AulaId)));

                if (await Cursos.EstimatedDocumentCountAsync() == 0)
                    await Cursos.InsertManyAsync(CursosIniciais.Criar());

                _inicializado = true;
            }
            finally
            {
                _inicializacao.Release();
            }
        }

        private static IMongoDatabase Criar()
        {
            RegistrarMapeamentos();

            var conexao = Environment.GetEnvironmentVariable("EUROPEAN_MONGO");
            var settings = MongoClientSettings.FromConnectionString(string.IsNullOrWhiteSpace(conexao) ? ConexaoPadrao : conexao);
            settings.ServerSelectionTimeout = TimeSpan.FromSeconds(5);
            settings.ConnectTimeout = TimeSpan.FromSeconds(5);

            var url = MongoUrl.Create(string.IsNullOrWhiteSpace(conexao) ? ConexaoPadrao : conexao);
            return new MongoClient(settings).GetDatabase(url.DatabaseName ?? BancoPadrao);
        }

        private static void RegistrarMapeamentos()
        {
            ConventionRegistry.Register("european", new ConventionPack
            {
                new IgnoreExtraElementsConvention(true),
                new EnumRepresentationConvention(BsonType.String)
            }, _ => true);

            BsonSerializer.RegisterSerializer(new DateTimeSerializer(DateTimeKind.Local));
            BsonSerializer.RegisterSerializer(new DecimalSerializer(BsonType.Decimal128));

            // Referências de navegação são montadas em memória (Curso.Vincular etc.) e não vão para o banco.
            BsonClassMap.RegisterClassMap<Modulo>(cm => { cm.AutoMap(); cm.UnmapMember(m => m.Curso); cm.UnmapMember(m => m.CursoId); });
            BsonClassMap.RegisterClassMap<Materia>(cm => { cm.AutoMap(); cm.UnmapMember(m => m.Modulo); cm.UnmapMember(m => m.ModuloId); });
            BsonClassMap.RegisterClassMap<Aula>(cm => { cm.AutoMap(); cm.UnmapMember(a => a.Materia); cm.UnmapMember(a => a.MateriaId); });
            BsonClassMap.RegisterClassMap<Matricula>(cm => { cm.AutoMap(); cm.UnmapMember(m => m.Curso); });
            BsonClassMap.RegisterClassMap<Prova>(cm => { cm.AutoMap(); cm.UnmapMember(p => p.Materia); });
        }
    }
}
