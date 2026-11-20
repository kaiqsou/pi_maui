using European.Enums;
using European.Models;

namespace European.Data
{
    /// <summary>
    /// Grade dos dois mestrados, gravada no MongoDB apenas quando a coleção de cursos está vazia.
    /// Aulas, provas e materiais são cadastrados depois pelos professores.
    /// </summary>
    public static class CursosIniciais
    {
        public static IEnumerable<Curso> Criar()
        {
            yield return Montar(
                "Mestrado em Direito Penal Econômico",
                "Mestrado internacional (3.000 h · 120 ECTS) em direito penal econômico.",
                CategoriaEnum.Direito,
                new (string, string[])[]
                {
                    ("Módulo 1 — Parte Geral", new[]
                    {
                        "Teoria Geral da Infração no Direito Penal Econômico",
                        "Análise Criminológica do Crime Econômico",
                        "Direito Societário Aplicado ao Direito Penal Econômico",
                        "Teoria da Pena e Política Criminal",
                        "Bem Jurídico Penal Econômico",
                        "Responsabilidade Penal da Pessoa Coletiva",
                        "Processo Penal Econômico e Pessoa Coletiva",
                        "Responsabilidade Penal de Administradores e Dirigentes",
                        "Direito Penal Internacional e Cooperação Judiciária",
                        "Direito Penal da União Europeia"
                    }),
                    ("Módulo 2 — Parte Especial", new[]
                    {
                        "Branqueamento de Capitais / Lavagem de Dinheiro",
                        "Crimes Tributários / Infrações Fiscais",
                        "Corrupção e Programas de Compliance",
                        "Criminalidade Organizada Transnacional",
                        "Crimes Informáticos e Prova Digital",
                        "Crimes contra Mercado, Concorrência e Consumidor",
                        "Crime Previdenciário",
                        "Crimes Empresariais e de Licitação Pública",
                        "Perda de Bens e Recuperação de Ativos",
                        "Direito Penal Ambiental Econômico",
                        "Proteção de Dados e Cibersegurança",
                        "ESG e Responsabilidade Penal Empresarial"
                    }),
                    ("Módulo 3 — Investigação e Processo", new[]
                    {
                        "Investigação Criminal nos Crimes Econômicos",
                        "Meios Especiais de Obtenção de Prova",
                        "Prova Digital Avançada, Blockchain e Criptoativos",
                        "Medidas Cautelares Reais",
                        "Justiça Penal Consensual",
                        "Cooperação Judiciária Internacional Penal",
                        "Investigação Financeira e Análise Patrimonial"
                    }),
                    ("Módulo 4 — Seminário de Dissertação", new[]
                    {
                        "Metodologia da Investigação Jurídica",
                        "Seminários Interdisciplinares de Dissertação"
                    }),
                    ("Módulo 5 — Dissertação", new[]
                    {
                        "Dissertação do Mestrado"
                    })
                });

            yield return Montar(
                "Mestrado em Harmonização Orofacial",
                "Mestrado internacional (3.000 h · 120 ECTS) em harmonização orofacial.",
                CategoriaEnum.Saude,
                new (string, string[])[]
                {
                    ("Módulo 1 — Introdução e Anatomia", new[] { "Introdução e Planejamento em Harmonização Orofacial", "Anatomia Aplicada" }),
                    ("Módulo 2 — Anatomia Estética e Fisiologia", new[] { "Face Anatomy Esthetic", "Fisiologia Aplicada" }),
                    ("Módulo 3 — Neurofisiologia e Peeling", new[] { "Neurofisiologia Aplicada", "Peeling Físico e Químico" }),
                    ("Módulo 4 — Ética e Medicina Forense", new[] { "Ética e Deontologia Profissional", "Medicina Forense" }),
                    ("Módulo 5 — Toxina Botulínica e Biossegurança", new[] { "Toxina Botulínica Estética e Funcional", "Biossegurança" }),
                    ("Módulo 6 — Bioestimuladores e Anestesia", new[] { "Bioestimuladores Sintéticos e Autólogos", "Técnicas Anestésicas" }),
                    ("Módulo 7 — Farmacologia e Emergências", new[] { "Farmacologia e Terapia Medicinal", "Emergências e Urgências Médicas" }),
                    ("Módulo 8 — Metodologia Científica I e Preenchimentos", new[] { "Metodologia de Trabalho Científico I", "Preenchimentos Faciais" }),
                    ("Módulo 9 — Ultrassom e Dermocosméticos", new[] { "Ultrassom com Endolaser", "Dermocosméticos Aplicados" }),
                    ("Módulo 10 — Marketing e Fios Lisos", new[] { "Marketing Aplicado", "Fios Lisos e Tensores Absorvíveis" }),
                    ("Módulo 11 — Fios e Lipoplastia Química", new[] { "Fios Não Absorvíveis", "Lipoplastia Química Facial e de Pescoço" }),
                    ("Módulo 12 — Lipoplastia Cirúrgica e Ultrassom", new[] { "Lipoplastia Cirúrgica Facial e de Pescoço", "Ultrassom Microfocalizado" }),
                    ("Módulo 13 — Metodologia Científica II", new[] { "Metodologia de Trabalho Científico II", "Módulo Presencial Obrigatório" }),
                    ("Módulo 14 — Metodologia na Prática", new[] { "Metodologia Científica na Prática", "Módulo Presencial Obrigatório" }),
                    ("Módulo 15 — Dissertação e Conclusão", new[] { "Desenvolvimento da Dissertação", "Qualificação e Defesa" })
                });
        }

        private static Curso Montar(string nome, string descricao, CategoriaEnum categoria, (string Titulo, string[] Materias)[] grade)
        {
            var curso = new Curso
            {
                Id = MongoContext.NovoId(),
                Nome = nome,
                Descricao = descricao,
                Categoria = categoria,
                CargaHorariaHoras = 3000,
                IsPublicado = true
            };

            for (var i = 0; i < grade.Length; i++)
            {
                var modulo = new Modulo { Id = MongoContext.NovoId(), Titulo = grade[i].Titulo, Ordem = i + 1 };
                for (var j = 0; j < grade[i].Materias.Length; j++)
                    modulo.Materias.Add(new Materia { Id = MongoContext.NovoId(), Nome = grade[i].Materias[j], Ordem = j + 1 });

                curso.Modulos.Add(modulo);
            }

            return curso;
        }
    }
}
