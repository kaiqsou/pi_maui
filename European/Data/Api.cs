using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using European.Models;

namespace European.Data
{
    /// <summary>A API não respondeu (desligada, sem rede ou tempo esgotado).</summary>
    public class ApiIndisponivelException : Exception
    {
        public ApiIndisponivelException(Exception interna)
            : base($"Não foi possível conectar à API ({Api.Endereco}). Verifique se ela está em execução.", interna) { }
    }

    /// <summary>
    /// Cliente HTTP da API do European (pasta European-API). Os repositórios usam estes métodos no lugar
    /// do driver do MongoDB. Endereço padrão: http://localhost:3000 (no emulador Android, http://10.0.2.2:3000);
    /// para usar outro, defina a variável de ambiente EUROPEAN_API antes de abrir o app.
    /// </summary>
    internal static class Api
    {
        public static string Endereco { get; } = DefinirEndereco();

        private static readonly HttpClient Http = new()
        {
            BaseAddress = new Uri(Endereco),
            Timeout = TimeSpan.FromSeconds(30)
        };

        /// <summary>Mesmo formato que o app gravava no MongoDB: nomes das propriedades e enums como texto.</summary>
        public static readonly JsonSerializerOptions Json = CriarOpcoes();

        private static string DefinirEndereco()
        {
            var configurado = Environment.GetEnvironmentVariable("EUROPEAN_API");
            if (!string.IsNullOrWhiteSpace(configurado))
                return configurado.TrimEnd('/') + "/";

            return DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2:3000/" : "http://localhost:3000/";
        }

        public static string Esc(string valor) => Uri.EscapeDataString(valor ?? string.Empty);

        // ---------------------------------------------------------------- chamadas

        /// <summary>GET; devolve null se a API responder 404.</summary>
        public static async Task<T?> GetOuNullAsync<T>(string url) where T : class
        {
            using var resposta = await EnviarAsync(() => Http.GetAsync(url), HttpStatusCode.NotFound);
            return resposta.StatusCode == HttpStatusCode.NotFound ? null : await LerAsync<T>(resposta);
        }

        public static async Task<T> GetAsync<T>(string url)
        {
            using var resposta = await EnviarAsync(() => Http.GetAsync(url));
            return await LerAsync<T>(resposta);
        }

        public static async Task<T> PostAsync<T>(string url, object? corpo)
        {
            using var resposta = await EnviarAsync(() => Http.PostAsync(url, Conteudo(corpo)));
            return await LerAsync<T>(resposta);
        }

        public static async Task PostAsync(string url, object? corpo)
        {
            using var _ = await EnviarAsync(() => Http.PostAsync(url, Conteudo(corpo)));
        }

        /// <summary>POST de inserção; false se a API responder 409 (ex.: e-mail já cadastrado).</summary>
        public static async Task<bool> InserirAsync(string url, object corpo)
        {
            using var resposta = await EnviarAsync(() => Http.PostAsync(url, Conteudo(corpo)), HttpStatusCode.Conflict);
            return resposta.StatusCode != HttpStatusCode.Conflict;
        }

        public static async Task PutAsync(string url, object corpo)
        {
            using var _ = await EnviarAsync(() => Http.PutAsync(url, Conteudo(corpo)));
        }

        public static async Task PatchAsync(string url, object corpo)
        {
            using var _ = await EnviarAsync(() => Http.PatchAsync(url, Conteudo(corpo)));
        }

        public static async Task DeleteAsync(string url)
        {
            using var _ = await EnviarAsync(() => Http.DeleteAsync(url));
        }

        /// <summary>Busca por lista de ids (POST {prefixo}/ids). Lista vazia nem chama a API.</summary>
        public static async Task<List<T>> PorIdsAsync<T>(string url, IEnumerable<string> ids)
        {
            var lista = ids.Distinct().ToList();
            return lista.Count == 0 ? new List<T>() : await PostAsync<List<T>>(url, new { ids = lista });
        }

        // ---------------------------------------------------------------- arquivos (GridFS na API)

        /// <summary>Envia o arquivo; os metadados vão na URL (ex.: aulaId, solicitacaoId). Devolve o id do arquivo.</summary>
        public static async Task<string> EnviarArquivoAsync(string nomeArquivo, Stream conteudo, IDictionary<string, string> metadados)
        {
            var query = string.Join("&", metadados.Select(m => $"{Esc(m.Key)}={Esc(m.Value)}").Prepend($"nome={Esc(nomeArquivo)}"));
            var corpo = new StreamContent(conteudo);
            corpo.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            var resposta = await PostRawAsync($"arquivos?{query}", corpo);
            return resposta.Id;
        }

        private static async Task<ArquivoEnviado> PostRawAsync(string url, HttpContent corpo)
        {
            using var resposta = await EnviarAsync(() => Http.PostAsync(url, corpo));
            return await LerAsync<ArquivoEnviado>(resposta);
        }

        public static async Task BaixarArquivoAsync(string arquivoId, Stream destino)
        {
            using var resposta = await EnviarAsync(() => Http.GetAsync($"arquivos/{Esc(arquivoId)}", HttpCompletionOption.ResponseHeadersRead),
                HttpStatusCode.NotFound);
            if (resposta.StatusCode == HttpStatusCode.NotFound)
                throw new InvalidOperationException("O arquivo não foi encontrado.");

            await using var origem = await resposta.Content.ReadAsStreamAsync();
            await origem.CopyToAsync(destino);
        }

        // ---------------------------------------------------------------- respostas simples

        internal sealed class RespostaExiste { public bool Existe { get; set; } }
        internal sealed class RespostaTotal { public int Total { get; set; } }
        private sealed class ArquivoEnviado { public string Id { get; set; } = string.Empty; }
        private sealed class RespostaErro { public string? Erro { get; set; } }

        // ---------------------------------------------------------------- infraestrutura

        private static async Task<HttpResponseMessage> EnviarAsync(Func<Task<HttpResponseMessage>> chamada, params HttpStatusCode[] aceitos)
        {
            HttpResponseMessage resposta;
            try
            {
                resposta = await chamada();
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                throw new ApiIndisponivelException(ex);
            }

            if (resposta.IsSuccessStatusCode || aceitos.Contains(resposta.StatusCode))
                return resposta;

            string? mensagem = null;
            try { mensagem = (await resposta.Content.ReadFromJsonAsync<RespostaErro>(Json))?.Erro; } catch { }
            var status = (int)resposta.StatusCode;
            resposta.Dispose();
            throw new InvalidOperationException(mensagem ?? $"A API respondeu com erro {status}.");
        }

        private static async Task<T> LerAsync<T>(HttpResponseMessage resposta)
            => (await resposta.Content.ReadFromJsonAsync<T>(Json))!;

        /// <summary>Corpo JSON da requisição (sem corpo quando for null: a API recusa um JSON "null").</summary>
        private static HttpContent? Conteudo(object? corpo) => corpo == null ? null : JsonContent.Create(corpo, options: Json);

        private static JsonSerializerOptions CriarOpcoes()
        {
            var resolver = new DefaultJsonTypeInfoResolver();
            resolver.Modifiers.Add(IgualAoBanco);

            var opcoes = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                PropertyNameCaseInsensitive = true,
                TypeInfoResolver = resolver
            };
            opcoes.Converters.Add(new JsonStringEnumConverter());
            opcoes.Converters.Add(new DataLocalConverter());
            return opcoes;
        }

        /// <summary>
        /// Nos modelos, envia/recebe só o que era gravado no MongoDB: propriedades com set, sem as referências
        /// de navegação montadas em memória (Modulo.Curso, Materia.Modulo, Aula.Materia, Matricula.Curso, Prova.Materia).
        /// Sem isso o JSON teria campos calculados e laços (Curso → Módulo → Curso...).
        /// </summary>
        private static void IgualAoBanco(JsonTypeInfo tipo)
        {
            if (tipo.Kind != JsonTypeInfoKind.Object || tipo.Type.Namespace != typeof(Curso).Namespace)
                return;

            for (var i = tipo.Properties.Count - 1; i >= 0; i--)
            {
                var propriedade = tipo.Properties[i];
                if (propriedade.Set == null || NaoGravada(tipo.Type, propriedade.Name))
                    tipo.Properties.RemoveAt(i);
            }
        }

        private static bool NaoGravada(Type tipo, string nome) => (tipo.Name, nome) switch
        {
            (nameof(Modulo), nameof(Modulo.Curso) or nameof(Modulo.CursoId)) => true,
            (nameof(Materia), nameof(Materia.Modulo) or nameof(Materia.ModuloId)) => true,
            (nameof(Aula), nameof(Aula.Materia) or nameof(Aula.MateriaId)) => true,
            (nameof(Matricula), nameof(Matricula.Curso)) => true,
            (nameof(Prova), nameof(Prova.Materia)) => true,
            _ => false
        };

        /// <summary>Datas trafegam em ISO com fuso; no app ficam sempre no horário local (como o MongoContext fazia).</summary>
        private sealed class DataLocalConverter : JsonConverter<DateTime>
        {
            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var texto = reader.GetString();
                return string.IsNullOrEmpty(texto)
                    ? default
                    : DateTimeOffset.Parse(texto, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal).LocalDateTime;
            }

            public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            {
                if (value == DateTime.MinValue)
                {
                    writer.WriteStringValue("0001-01-01T00:00:00.000Z");
                    return;
                }

                var local = value.Kind == DateTimeKind.Utc ? value.ToLocalTime() : DateTime.SpecifyKind(value, DateTimeKind.Local);
                writer.WriteStringValue(new DateTimeOffset(local).ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzz", CultureInfo.InvariantCulture));
            }
        }
    }
}
