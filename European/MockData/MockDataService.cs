using System.Text.Json;
using System.Text.Json.Serialization;
using European.Enums;
using European.Models;
using European.Services;

namespace European.MockData
{
    public static class MockDataService
    {
        private static List<Usuario> _usuarios;
        private static readonly List<Curso> _cursos;
        private static List<Matricula> _matriculas;
        private static List<ProgressoAula> _progressoAulas;
        private static readonly List<MaterialComplementar> _materiais;
        private static readonly List<Prova> _provas;
        private static List<ResultadoProva> _resultados;
        private static List<Certificado> _certificados;
        private static readonly Dictionary<string, double> _duracaoVideosRealPorAula = new();

        private const int DuracaoAulaPadraoSegundos = 826;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        static MockDataService()
        {
            _usuarios = CriarUsuariosSemente();
            var professor = _usuarios.First(u => u.Id == "usr-prof-1");
            var aluno = _usuarios.First(u => u.Id == "usr-aluno-1");
            var aluna = _usuarios.First(u => u.Id == "usr-aluno-2");

            var curso = new Curso
            {
                Id = "curso-1",
                Nome = "Mestrado em Direito Penal Econômico",
                Descricao = "Mestrado internacional com 3.000 horas e 120 ECTS. Dogmática, parte especial e processo penal econômico com corpo docente internacional (Brasil, Portugal e Espanha).",
                Categoria = CategoriaEnum.Direito,
                Preco = 9690m,
                CargaHorariaHoras = 3000,
                IsPublicado = true,
                ProfessorId = professor.Id,
                Coordenador = "Prof. Dr. Arthur Pinto de Lemos Junior",
                Docentes = new List<string>
                {
                    "Prof. Dr. Arthur Pinto de Lemos Junior",
                    "Dra. Marcela Camargo",
                    "Dr. Olavo Evangelista Pezzotti",
                    "Dr. Flávio Turessi",
                    "Dr. Eronides dos Santos",
                    "Dra. Júlia Schütt"
                }
            };

            var curso2 = new Curso
            {
                Id = "curso-2",
                Nome = "Mestrado em Harmonização Orofacial",
                Descricao = "Mestrado internacional com 3.000 horas e 120 ECTS. Formação completa em harmonização orofacial com base em evidências científicas, técnicas avançadas e prática clínica supervisionada.",
                Categoria = CategoriaEnum.Saude,
                Preco = 9690m,
                CargaHorariaHoras = 3000,
                IsPublicado = true,
                ProfessorId = professor.Id,
                Coordenador = "Prof. Dr. Paulo Moraes",
                Docentes = new List<string>
                {
                    "Prof. Dr. Paulo Moraes",
                    "Dra. Marcela Patera",
                    "Dra. Adriana Manzoli",
                    "Dra. Andrea Françoso",
                    "Dra. Noemi Borgas",
                    "Dra. Simone Lopes",
                    "Dra. Irina Bernardino"
                }
            };

            var gradeDireito = new (string Titulo, (string Nome, string Descricao)[] Disciplinas)[]
            {
                ("Módulo 1 — Parte Geral do Direito Penal Econômico (1.º Semestre)", new (string Nome, string Descricao)[]
                {
                    ("Teoria Geral da Infração no Direito Penal Econômico", "Abordagem comparada dos modelos de imputação brasileiro e português no âmbito do direito penal econômico."),
                    ("Análise Criminológica do Crime Econômico", "Estudo criminológico dos fenômenos da criminalidade econômica contemporânea."),
                    ("Direito Societário Aplicado ao Direito Penal Econômico", "Compreensão da organização empresarial como base para a responsabilidade penal da pessoa coletiva."),
                    ("Punição nos Crimes Econômicos: Teoria da Pena e Política Criminal", "Teoria da pena e opções de política criminal aplicadas aos crimes econômicos."),
                    ("Bem Jurídico Penal Econômico", "Análise dogmática do bem jurídico na tutela penal da ordem econômica."),
                    ("Responsabilidade Penal da Pessoa Coletiva: Direito Substantivo", "Análise comparada da responsabilidade penal das pessoas coletivas no Brasil, em Portugal e na União Europeia."),
                    ("Direito Processual Penal Econômico e Responsabilidade da Pessoa Coletiva", "Aspectos processuais da responsabilização penal de empresas e pessoas coletivas."),
                    ("Responsabilidade Penal dos Administradores, Dirigentes e Membros de Órgãos Colegiados em Estruturas Empresariais", "Responsabilidade penal individual dos gestores e órgãos colegiados no contexto empresarial."),
                    ("Direito Penal Internacional: Fundamentos e Cooperação Judiciária", "Fundamentos do direito penal internacional e mecanismos de cooperação judiciária."),
                    ("Direito Penal da União Europeia: Competência, Instrumentos e Evolução", "Competência penal da UE, instrumentos de harmonização e a Procuradoria Europeia.")
                }),
                ("Módulo 2 — Parte Especial do Direito Penal Econômico (2.º Semestre)", new (string Nome, string Descricao)[]
                {
                    ("Prevenção e Combate ao Branqueamento de Capitais / Lavagem de Dinheiro", "Cotejo entre o regime brasileiro, o português e as diretivas europeias de combate à lavagem de dinheiro."),
                    ("Crimes Tributários / Infrações Fiscais", "Análise paralela dos crimes tributários no Brasil e em Portugal, com referência ao direito da UE."),
                    ("Corrupção: Regime Jurídico-Penal e Programas de Compliance", "Regime jurídico-penal da corrupção e articulação com programas de compliance."),
                    ("Criminalidade Organizada Transnacional: Aspectos Substantivos e Processuais", "Estrutura e persecução da criminalidade organizada com dimensão transnacional."),
                    ("Crimes Informáticos e Prova Digital", "Crimes informáticos e obtenção transfronteiriça de prova digital."),
                    ("Crimes Econômicos contra Mercado, Concorrência e Consumidor", "Tutela penal do mercado, da concorrência e das relações de consumo."),
                    ("Crime Previdenciário", "Crimes contra a seguridade social e a previdência nos ordenamentos brasileiro e português."),
                    ("Crimes Empresariais e de Licitação Pública", "Crimes falimentares, licitatórios e fraudes em contratos e fundos públicos."),
                    ("Perda de Bens e Recuperação de Ativos (Asset Recovery)", "Instrumentos de localização, apreensão e confisco de ativos provenientes de crimes econômicos."),
                    ("Direito Penal Ambiental Econômico", "Crimes ambientais com repercussão econômica e responsabilização penal e civil."),
                    ("Proteção de Dados, Cibersegurança e Direito Penal Econômico Digital", "Proteção de dados, cibersegurança e impactos penais da economia digital."),
                    ("ESG, Sustentabilidade e Responsabilidade Penal Empresarial", "Governança ESG e responsabilidade penal empresarial em perspectiva comparada.")
                }),
                ("Módulo 3 — Investigação e Processo Penal Econômico (3.º Semestre)", new (string Nome, string Descricao)[]
                {
                    ("Investigação Criminal nos Crimes Econômicos: Modelos e Técnicas", "Cotejo entre o modelo investigatório brasileiro e o português nos crimes econômicos."),
                    ("Meios Especiais de Obtenção de Prova: Interceptações, Buscas e Apreensões e Agente Encoberto", "Análise comparada dos meios especiais de obtenção de prova e suas garantias."),
                    ("Prova Digital Avançada, Blockchain e Criptoativo", "Técnicas avançadas de prova digital, cadeia de custódia, blockchain e criptoativos."),
                    ("Medidas Cautelares Reais: Arresto, Sequestro e Apreensão de Bens", "Medidas cautelares patrimoniais em processos penais econômicos complexos."),
                    ("Justiça Penal Consensual: Acordo de Não Persecução Penal, Colaboração Premiada, Plea Bargaining, Suspensão Provisória e Acordo de Leniência", "Acordos processuais e negociações penais nos sistemas brasileiro, português e comparado."),
                    ("Cooperação Judiciária Internacional Penal: Extradição, MLAT e Mandado de Detenção Europeu", "Mecanismos de cooperação judiciária internacional em matéria penal."),
                    ("Investigação Financeira e Análise Patrimonial", "Técnicas de investigação financeira e análise patrimonial em crimes econômicos.")
                }),
                ("Módulo 4 — Seminário de Dissertação (4.º Semestre)", new (string Nome, string Descricao)[]
                {
                    ("Metodologia da Investigação Jurídica", "Técnicas de pesquisa jurídica e escrita acadêmica para elaboração da dissertação."),
                    ("Seminários Interdisciplinares de Dissertação", "Apresentação progressiva dos projetos de dissertação com avaliação por pares e pela comissão de orientação.")
                }),
                ("Módulo 5 — Dissertação (4.º–5.º Semestres)", new (string Nome, string Descricao)[]
                {
                    ("Dissertação do Mestrado", "Desenvolvimento, orientação, qualificação e defesa pública da dissertação de mestrado.")
                })
            };

            var gradeOrofacial = new (string Titulo, (string Nome, string Descricao)[] Disciplinas)[]
            {
                ("Módulo 1 — Introdução e Anatomia", new (string Nome, string Descricao)[]
                {
                    ("Introdução à Harmonização Orofacial associada ao Planejamento em Harmonização Orofacial", "Fundamentos e planejamento em harmonização orofacial."),
                    ("Anatomia Aplicada à Harmonização Orofacial", "Anatomia aplicada aos procedimentos de harmonização orofacial.")
                }),
                ("Módulo 2 — Face Anatomy e Fisiologia", new (string Nome, string Descricao)[]
                {
                    ("Face Anatomy Esthetic", "Anatomia estética facial."),
                    ("Fisiologia Aplicada à Harmonização Orofacial", "Fisiologia aplicada à harmonização orofacial.")
                }),
                ("Módulo 3 — Neurofisiologia e Peeling", new (string Nome, string Descricao)[]
                {
                    ("Neurofisiologia Aplicada à Harmonização Orofacial", "Neurofisiologia aplicada à harmonização orofacial."),
                    ("Peeling Físico e Químico Aplicado à Harmonização Orofacial", "Peelings físicos e químicos aplicados à harmonização orofacial.")
                }),
                ("Módulo 4 — Ética e Medicina Forense", new (string Nome, string Descricao)[]
                {
                    ("Ética e Deontologia Profissional", "Ética e deontologia profissional."),
                    ("Medicina Forense", "Medicina forense aplicada.")
                }),
                ("Módulo 5 — Toxina Botulínica e Biossegurança", new (string Nome, string Descricao)[]
                {
                    ("Toxina Botulínica Estética e Funcional", "Aplicações estéticas e funcionais da toxina botulínica."),
                    ("Biossegurança em Harmonização Orofacial", "Biossegurança em harmonização orofacial.")
                }),
                ("Módulo 6 — Bioestimuladores e Anestesia", new (string Nome, string Descricao)[]
                {
                    ("Bioestimuladores Sintéticos e Autólogos", "Bioestimuladores sintéticos e autólogos."),
                    ("Técnicas Anestésicas em Harmonização Orofacial", "Técnicas anestésicas em harmonização orofacial.")
                }),
                ("Módulo 7 — Farmacologia e Emergências", new (string Nome, string Descricao)[]
                {
                    ("Farmacologia e Terapia Medicinal Aplicada à Harmonização Orofacial", "Farmacologia e terapia medicinal aplicada à harmonização orofacial."),
                    ("Emergências e Urgências Médicas", "Emergências e urgências médicas.")
                }),
                ("Módulo 8 — Metodologia Científica I e Preenchimentos", new (string Nome, string Descricao)[]
                {
                    ("Metodologia de Trabalho Científico I", "Metodologia de trabalho científico I."),
                    ("Preenchimentos Faciais", "Preenchimentos faciais.")
                }),
                ("Módulo 9 — Ultrassom e Dermocosméticos", new (string Nome, string Descricao)[]
                {
                    ("Ultrassom com Endolaser", "Ultrassom com endolaser."),
                    ("Dermocosméticos Aplicados à Harmonização Orofacial", "Dermocosméticos aplicados à harmonização orofacial.")
                }),
                ("Módulo 10 — Marketing e Fios Lisos", new (string Nome, string Descricao)[]
                {
                    ("Marketing Aplicado à Harmonização Orofacial", "Marketing aplicado à harmonização orofacial."),
                    ("Fios Lisos e Tensores Absorvíveis em Harmonização Orofacial", "Fios lisos e tensores absorvíveis em harmonização orofacial.")
                }),
                ("Módulo 11 — Fios Não Absorvíveis e Lipoplastia Química", new (string Nome, string Descricao)[]
                {
                    ("Fios Não Absorvíveis", "Fios não absorvíveis."),
                    ("Lipoplastia Química Facial e de Pescoço", "Lipoplastia química facial e de pescoço.")
                }),
                ("Módulo 12 — Lipoplastia Cirúrgica e Ultrassom Microfocalizado", new (string Nome, string Descricao)[]
                {
                    ("Lipoplastia Cirúrgica Facial e de Pescoço", "Lipoplastia cirúrgica facial e de pescoço."),
                    ("Ultrassom Microfocalizado", "Ultrassom microfocalizado.")
                }),
                ("Módulo 13 — Metodologia Científica II e Módulo Presencial", new (string Nome, string Descricao)[]
                {
                    ("Metodologia de Trabalho Científico II", "Metodologia de trabalho científico II."),
                    ("Módulo Presencial Obrigatório (Brasil ou Europa)", "Módulo presencial obrigatório no Brasil ou Europa.")
                }),
                ("Módulo 14 — Metodologia Científica na Prática e Módulo Presencial", new (string Nome, string Descricao)[]
                {
                    ("Metodologia Científica na Prática", "Metodologia científica na prática."),
                    ("Módulo Presencial Obrigatório (Brasil ou Europa)", "Módulo presencial obrigatório no Brasil ou Europa.")
                }),
                ("Módulo 15 — Dissertação e Conclusão", new (string Nome, string Descricao)[]
                {
                    ("Período de Desenvolvimento da Dissertação (6 meses)", "Desenvolvimento da dissertação."),
                    ("Conclusão — Qualificação e Defesa (7 meses)", "Qualificação e defesa da dissertação.")
                })
            };

            static void ConstruirGrade(Curso c, string prefixo, (string Titulo, (string Nome, string Descricao)[] Disciplinas)[] grade)
            {
                for (var i = 0; i < grade.Length; i++)
                {
                    var modulo = new Modulo
                    {
                        Id = $"{prefixo}-modulo-{i + 1}",
                        CursoId = c.Id,
                        Curso = c,
                        Titulo = grade[i].Titulo,
                        Ordem = i + 1
                    };
                    c.Modulos.Add(modulo);

                    var disciplinas = grade[i].Disciplinas;
                    for (var j = 0; j < disciplinas.Length; j++)
                    {
                        var materia = new Materia
                        {
                            Id = $"{prefixo}-materia-{i + 1}-{j + 1}",
                            ModuloId = modulo.Id,
                            Modulo = modulo,
                            Nome = disciplinas[j].Nome,
                            Descricao = disciplinas[j].Descricao,
                            Ordem = j + 1
                        };
                        modulo.Materias.Add(materia);

                        materia.Aulas.Add(new Aula
                        {
                            Id = $"{prefixo}-aula-{i + 1}-{j + 1}",
                            MateriaId = materia.Id,
                            Materia = materia,
                            Titulo = disciplinas[j].Nome,
                            Descricao = disciplinas[j].Descricao,
                            DuracaoSegundos = DuracaoAulaPadraoSegundos,
                            Ordem = 1,
                            IsGratuita = i == 0 && j == 0
                        });
                    }
                }
            }

            ConstruirGrade(curso, "dpe", gradeDireito);
            ConstruirGrade(curso2, "hof", gradeOrofacial);

            _cursos = new List<Curso> { curso, curso2 };

            _materiais = new List<MaterialComplementar>();
            var materialId = 1;
            foreach (var aula in _cursos.SelectMany(c => c.Modulos).SelectMany(m => m.Materias).SelectMany(x => x.Aulas))
            {
                _materiais.Add(new MaterialComplementar
                {
                    Id = $"mat-{materialId++}",
                    AulaId = aula.Id,
                    Aula = aula,
                    Titulo = "Slides da disciplina",
                    Url = "https://www.example.com/european/material.pdf",
                    Tipo = TipoMaterial.Pdf,
                    Ordem = 1
                });
            }

            var matriculaAluno = new Matricula
            {
                Id = "matricula-1",
                AlunoId = aluno.Id,
                Aluno = aluno,
                CursoId = curso.Id,
                Curso = curso,
                Status = StatusMatricula.Ativa,
                ProgressoPercentual = 0
            };

            var matriculaAluna = new Matricula
            {
                Id = "matricula-2",
                AlunoId = aluna.Id,
                Aluno = aluna,
                CursoId = curso.Id,
                Curso = curso,
                Status = StatusMatricula.Concluida,
                ProgressoPercentual = 100,
                DataConclusao = DateTime.Now.AddDays(-15)
            };

            _matriculas = new List<Matricula> { matriculaAluno, matriculaAluna };

            _progressoAulas = new List<ProgressoAula>();
            var progressoId = 1;
            foreach (var aula in curso.Modulos.SelectMany(m => m.Materias).SelectMany(x => x.Aulas))
            {
                _progressoAulas.Add(new ProgressoAula
                {
                    Id = $"prog-{progressoId++}",
                    MatriculaId = matriculaAluna.Id,
                    AulaId = aula.Id,
                    PercentualAssistido = 100,
                    Concluida = true
                });
            }

            var primeiraMateriaDireito = curso.Modulos[0].Materias[0];
            var primeiraMateriaOrofacial = curso2.Modulos[0].Materias[0];

            _provas = new List<Prova>
            {
                new()
                {
                    Id = "prova-1",
                    MateriaId = primeiraMateriaDireito.Id,
                    Materia = primeiraMateriaDireito,
                    Titulo = "Avaliação — Teoria Geral da Infração",
                    NotaMaxima = 10,
                    Peso = 1,
                    NotaCorte = 6,
                    Prazo = DateTime.Now.AddDays(7),
                    DuracaoMinutos = 60
                },
                new()
                {
                    Id = "prova-2",
                    MateriaId = primeiraMateriaOrofacial.Id,
                    Materia = primeiraMateriaOrofacial,
                    Titulo = "Avaliação — Introdução à Harmonização Orofacial",
                    NotaMaxima = 10,
                    Peso = 1,
                    NotaCorte = 6,
                    Prazo = DateTime.Now.AddDays(10),
                    DuracaoMinutos = 45
                }
            };

            _resultados = new List<ResultadoProva>();

            _certificados = new List<Certificado>();

            GerarCertificados(matriculaAluna);

            CarregarPersistidos();
            GarantirUsuariosSemente();
        }

        private static List<Usuario> CriarUsuariosSemente() => new()
        {
            new Usuario
            {
                Id = "usr-admin-1",
                Nome = "Admin European",
                Email = "admin@european.com",
                SenhaHash = SenhaService.GerarHash("admin1234"),
                Roles = Roles.Administrador
            },
            new Usuario
            {
                Id = "usr-prof-1",
                Nome = "Marina Frankin",
                Email = "marina.frankin@european.com",
                SenhaHash = SenhaService.GerarHash("professor1234"),
                Roles = Roles.Professor
            },
            new Usuario
            {
                Id = "usr-aluno-1",
                Nome = "Kaique Souza",
                Email = "kaique.souza@european.com",
                SenhaHash = SenhaService.GerarHash("aluno1234"),
                Roles = Roles.Aluno
            },
            new Usuario
            {
                Id = "usr-aluno-2",
                Nome = "Ana Lima",
                Email = "ana.lima@european.com",
                SenhaHash = SenhaService.GerarHash("aluna1234"),
                Roles = Roles.Aluno
            }
        };

        private static void GarantirUsuariosSemente()
        {
            foreach (var semente in CriarUsuariosSemente())
            {
                var existente = _usuarios.FirstOrDefault(u => u.Id == semente.Id);
                if (existente == null)
                {
                    _usuarios.Add(semente);
                    continue;
                }

                existente.Nome = semente.Nome;
                existente.Email = semente.Email;
                existente.SenhaHash = semente.SenhaHash;
                existente.Roles = semente.Roles;
            }
        }

        public static List<Usuario> Usuarios() => _usuarios;
        public static List<Curso> Cursos() => _cursos;
        public static List<Matricula> Matriculas() => _matriculas;
        public static List<ProgressoAula> ProgressoAulas() => _progressoAulas;
        public static List<MaterialComplementar> Materiais() => _materiais;
        public static List<Prova> Provas() => _provas;
        public static List<ResultadoProva> Resultados() => _resultados;
        public static List<Certificado> Certificados() => _certificados;

        public static Matricula? MatriculaDoAluno(string alunoId, string cursoId)
            => _matriculas.FirstOrDefault(m => m.AlunoId == alunoId && m.CursoId == cursoId);

        public static Matricula? MatriculaPorId(string matriculaId)
            => _matriculas.FirstOrDefault(m => m.Id == matriculaId);

        public static List<Certificado> CertificadosDoAluno(string alunoId)
            => _certificados.Where(c => c.AlunoId == alunoId)
                .OrderByDescending(c => c.IsCurso)
                .ThenBy(c => c.EmitidoEm)
                .ToList();

        public static ProgressoAula? ProgressoDaAula(string matriculaId, string aulaId)
            => _progressoAulas.FirstOrDefault(p => p.MatriculaId == matriculaId && p.AulaId == aulaId);

        public static List<ProgressoAula> ProgressosDaMatricula(string matriculaId)
            => _progressoAulas.Where(p => p.MatriculaId == matriculaId).ToList();

        public static Curso? CursoDaAula(Aula aula)
        {
            if (aula.Materia?.Modulo?.Curso != null)
                return aula.Materia.Modulo.Curso;

            return _cursos.FirstOrDefault(c =>
                c.Modulos.Any(m => m.Materias.Any(x => x.Aulas.Any(a => a.Id == aula.Id))));
        }

        public static bool AulaEstaLiberada(string matriculaId, Aula aula)
        {
            var curso = CursoDaAula(aula);
            if (curso == null)
                return true;

            var aulasNaOrdem = curso.Modulos.OrderBy(m => m.Ordem)
                .SelectMany(m => m.Materias.OrderBy(x => x.Ordem))
                .SelectMany(x => x.Aulas.OrderBy(a => a.Ordem))
                .ToList();

            var indice = aulasNaOrdem.FindIndex(a => a.Id == aula.Id);
            if (indice <= 0)
                return true;

            for (var i = 0; i < indice; i++)
            {
                var progresso = _progressoAulas.FirstOrDefault(p =>
                    p.MatriculaId == matriculaId && p.AulaId == aulasNaOrdem[i].Id);

                if (progresso == null || !progresso.Concluida)
                    return false;
            }

            return true;
        }

        public static void RegistrarAcessoAula(string matriculaId, string aulaId)
        {
            var progresso = _progressoAulas.FirstOrDefault(p => p.MatriculaId == matriculaId && p.AulaId == aulaId)
                ?? CriarProgresso(matriculaId, aulaId);

            progresso.AtualizadoEm = DateTime.Now;
            Persistir();
        }

        public static double DuracaoVideosDaAula(Aula aula)
        {
            if (_duracaoVideosRealPorAula.TryGetValue(aula.Id, out var real) && real > 0)
                return real;

            return aula.DuracaoSegundos;
        }

        public static void SalvarDuracaoRealAula(string aulaId, double segundos)
        {
            if (segundos > 0)
            {
                _duracaoVideosRealPorAula[aulaId] = segundos;
                Persistir();
            }
        }

        public static void SalvarPosicaoAula(string matriculaId, string aulaId, double segundos, double duracaoTotal)
        {
            var progresso = _progressoAulas.FirstOrDefault(p => p.MatriculaId == matriculaId && p.AulaId == aulaId)
                ?? CriarProgresso(matriculaId, aulaId);

            progresso.UltimaPosicaoSegundos = Math.Max(0, segundos);

            if (duracaoTotal > 0)
                progresso.PercentualAssistido = Math.Min(100, progresso.UltimaPosicaoSegundos / duracaoTotal * 100);

            progresso.AtualizadoEm = DateTime.Now;
            Persistir();
        }

        public static void ConcluirAula(Matricula matricula, Aula aula)
        {
            var progresso = _progressoAulas.FirstOrDefault(p => p.MatriculaId == matricula.Id && p.AulaId == aula.Id)
                ?? CriarProgresso(matricula.Id, aula.Id);

            progresso.Concluida = true;
            progresso.PercentualAssistido = 100;
            progresso.AtualizadoEm = DateTime.Now;

            AtualizarProgresso(matricula);
            Persistir();
        }

        private static ProgressoAula CriarProgresso(string matriculaId, string aulaId)
        {
            var progresso = new ProgressoAula
            {
                Id = Guid.NewGuid().ToString(),
                MatriculaId = matriculaId,
                AulaId = aulaId
            };
            _progressoAulas.Add(progresso);
            return progresso;
        }

        public static double AtualizarProgresso(Matricula matricula)
        {
            var curso = matricula.Curso ?? _cursos.FirstOrDefault(c => c.Id == matricula.CursoId);
            if (curso == null)
                return matricula.ProgressoPercentual;

            var aulas = curso.Modulos.SelectMany(m => m.Materias).SelectMany(x => x.Aulas).ToList();
            if (aulas.Count == 0)
                return matricula.ProgressoPercentual;

            var concluidas = aulas.Count(a => _progressoAulas.Any(p => p.MatriculaId == matricula.Id && p.AulaId == a.Id && p.Concluida));
            matricula.ProgressoPercentual = Math.Round(concluidas * 100.0 / aulas.Count, 0);

            if (matricula.ProgressoPercentual >= 100)
            {
                matricula.Status = StatusMatricula.Concluida;
                matricula.DataConclusao ??= DateTime.Now;
            }

            GerarCertificados(matricula);
            return matricula.ProgressoPercentual;
        }

        private static void GerarCertificados(Matricula matricula)
        {
            var curso = matricula.Curso ?? _cursos.FirstOrDefault(c => c.Id == matricula.CursoId);
            var aluno = matricula.Aluno ?? _usuarios.FirstOrDefault(u => u.Id == matricula.AlunoId);
            if (curso == null || aluno == null)
                return;

            var materias = curso.Modulos.SelectMany(m => m.Materias).ToList();

            foreach (var materia in materias)
            {
                if (materia.Aulas.Count == 0)
                    continue;

                var concluidas = materia.Aulas.Count(a => _progressoAulas.Any(p => p.MatriculaId == matricula.Id && p.AulaId == a.Id && p.Concluida));
                if (concluidas < materia.Aulas.Count)
                    continue;

                if (_certificados.Any(c => c.MatriculaId == matricula.Id && c.MateriaId == materia.Id))
                    continue;

                _certificados.Add(new Certificado
                {
                    Id = Guid.NewGuid().ToString(),
                    MatriculaId = matricula.Id,
                    CursoId = curso.Id,
                    MateriaId = materia.Id,
                    AlunoId = aluno.Id,
                    Titulo = $"Conclusao em {materia.Nome}",
                    AlunoNome = aluno.Nome,
                    CursoNome = curso.Nome,
                    CargaHorariaHoras = Math.Max(1, materia.Aulas.Sum(a => a.DuracaoSegundos) / 3600),
                    CodigoVerificacao = ProximoCodigo(),
                    EmitidoEm = DateTime.Now
                });
            }

            if (matricula.ProgressoPercentual >= 100 &&
                !_certificados.Any(c => c.MatriculaId == matricula.Id && c.MateriaId == null))
            {
                _certificados.Add(new Certificado
                {
                    Id = Guid.NewGuid().ToString(),
                    MatriculaId = matricula.Id,
                    CursoId = curso.Id,
                    AlunoId = aluno.Id,
                    Titulo = $"Mestrado - {curso.Nome}",
                    AlunoNome = aluno.Nome,
                    CursoNome = curso.Nome,
                    CargaHorariaHoras = curso.CargaHorariaHoras,
                    CodigoVerificacao = ProximoCodigo(),
                    EmitidoEm = DateTime.Now
                });
            }
        }

        private static string ProximoCodigo() => $"EURO-{DateTime.Now:yyyy}-{_certificados.Count + 1:0000}";

        // ---- Curso ----
        public static void AddCurso(Curso curso)
        {
            curso.Id = string.IsNullOrEmpty(curso.Id) ? Guid.NewGuid().ToString() : curso.Id;
            curso.CreatedAt = DateTime.Now;
            curso.UpdatedAt = DateTime.Now;
            _cursos.Add(curso);
        }

        public static void UpdateCurso(Curso curso)
        {
            var index = _cursos.FindIndex(c => c.Id == curso.Id);
            if (index >= 0)
                _cursos[index] = curso;
            curso.UpdatedAt = DateTime.Now;
        }

        public static void RemoveCurso(Curso curso)
        {
            _matriculas.RemoveAll(m => m.CursoId == curso.Id);
            _cursos.Remove(curso);
        }

        // ---- Modulo ----
        public static void AddModulo(Curso curso, Modulo modulo)
        {
            modulo.Id = string.IsNullOrEmpty(modulo.Id) ? Guid.NewGuid().ToString() : modulo.Id;
            modulo.CursoId = curso.Id;
            modulo.Curso = curso;
            modulo.Ordem = curso.Modulos.Count + 1;
            curso.Modulos.Add(modulo);
        }

        public static void UpdateModulo(Modulo modulo)
        {
            var curso = _cursos.FirstOrDefault(c => c.Id == modulo.CursoId);
            var index = curso?.Modulos.FindIndex(m => m.Id == modulo.Id) ?? -1;
            if (curso != null && index >= 0)
                curso.Modulos[index] = modulo;
        }

        public static void RemoveModulo(Modulo modulo)
        {
            var curso = _cursos.FirstOrDefault(c => c.Id == modulo.CursoId);
            curso?.Modulos.Remove(modulo);
        }

        // ---- Materia ----
        public static void AddMateria(Modulo modulo, Materia materia)
        {
            materia.Id = string.IsNullOrEmpty(materia.Id) ? Guid.NewGuid().ToString() : materia.Id;
            materia.ModuloId = modulo.Id;
            materia.Modulo = modulo;
            materia.Ordem = modulo.Materias.Count + 1;
            modulo.Materias.Add(materia);
        }

        public static void UpdateMateria(Materia materia)
        {
            var modulo = _cursos.SelectMany(c => c.Modulos).FirstOrDefault(m => m.Id == materia.ModuloId);
            var index = modulo?.Materias.FindIndex(x => x.Id == materia.Id) ?? -1;
            if (modulo != null && index >= 0)
                modulo.Materias[index] = materia;
        }

        public static void RemoveMateria(Materia materia)
        {
            var modulo = _cursos.SelectMany(c => c.Modulos).FirstOrDefault(m => m.Id == materia.ModuloId);
            modulo?.Materias.Remove(materia);
        }

        // ---- Aula ----
        public static void AddAula(Materia materia, Aula aula)
        {
            aula.Id = string.IsNullOrEmpty(aula.Id) ? Guid.NewGuid().ToString() : aula.Id;
            aula.MateriaId = materia.Id;
            aula.Materia = materia;
            aula.Ordem = materia.Aulas.Count + 1;
            materia.Aulas.Add(aula);
        }

        public static void UpdateAula(Aula aula)
        {
            var materia = _cursos.SelectMany(c => c.Modulos).SelectMany(m => m.Materias).FirstOrDefault(x => x.Id == aula.MateriaId);
            var index = materia?.Aulas.FindIndex(a => a.Id == aula.Id) ?? -1;
            if (materia != null && index >= 0)
                materia.Aulas[index] = aula;
        }

        public static void RemoveAula(Aula aula)
        {
            var materia = _cursos.SelectMany(c => c.Modulos).SelectMany(m => m.Materias).FirstOrDefault(x => x.Id == aula.MateriaId);
            materia?.Aulas.Remove(aula);
        }

        // ---- Prova ----
        public static void AddProva(Prova prova)
        {
            prova.Id = string.IsNullOrEmpty(prova.Id) ? Guid.NewGuid().ToString() : prova.Id;
            _provas.Add(prova);
        }

        public static void UpdateProva(Prova prova)
        {
            var index = _provas.FindIndex(p => p.Id == prova.Id);
            if (index >= 0)
                _provas[index] = prova;
        }

        public static void RemoveProva(Prova prova)
        {
            _resultados.RemoveAll(r => r.ProvaId == prova.Id);
            _provas.Remove(prova);
        }

        // ---- Usuário ----
        public static void AdicionarUsuario(Usuario usuario)
        {
            usuario.Id = string.IsNullOrEmpty(usuario.Id) ? Guid.NewGuid().ToString() : usuario.Id;
            usuario.UpdatedAt = DateTime.Now;
            _usuarios.Add(usuario);
            Persistir();
        }

        // ---- Matrícula ----
        public static void RegistrarMatricula(Usuario aluno, Curso curso)
        {
            _matriculas.Add(new Matricula
            {
                Id = Guid.NewGuid().ToString(),
                AlunoId = aluno.Id,
                Aluno = aluno,
                CursoId = curso.Id,
                Curso = curso,
                Status = StatusMatricula.Ativa,
                ProgressoPercentual = 0,
                DataMatricula = DateTime.Now
            });
            Persistir();
        }

        // ---- Persistência ----
        private sealed class MatriculaPersistida
        {
            public string Id { get; set; } = string.Empty;
            public string AlunoId { get; set; } = string.Empty;
            public string CursoId { get; set; } = string.Empty;
            public StatusMatricula Status { get; set; }
            public double ProgressoPercentual { get; set; }
            public bool IsActive { get; set; } = true;
            public DateTime DataMatricula { get; set; }
            public DateTime? DataConclusao { get; set; }
        }

        private sealed class DadosPersistidos
        {
            public List<Usuario>? Usuarios { get; set; }
            public List<MatriculaPersistida>? Matriculas { get; set; }
            public List<ProgressoAula>? Progressos { get; set; }
            public List<ResultadoProva>? Resultados { get; set; }
            public List<Certificado>? Certificados { get; set; }
            public Dictionary<string, double>? DuracaoRealPorAula { get; set; }
        }

        private static string CaminhoDoArquivo()
        {
            try
            {
                return Path.Combine(FileSystem.AppDataDirectory, "european-data.json");
            }
            catch
            {
                return "european-data.json";
            }
        }

        private static void Persistir()
        {
            try
            {
                var dados = new DadosPersistidos
                {
                    Usuarios = _usuarios,
                    Matriculas = _matriculas.Select(m => new MatriculaPersistida
                    {
                        Id = m.Id,
                        AlunoId = m.AlunoId,
                        CursoId = m.CursoId,
                        Status = m.Status,
                        ProgressoPercentual = m.ProgressoPercentual,
                        IsActive = m.IsActive,
                        DataMatricula = m.DataMatricula,
                        DataConclusao = m.DataConclusao
                    }).ToList(),
                    Progressos = _progressoAulas,
                    Resultados = _resultados,
                    Certificados = _certificados,
                    DuracaoRealPorAula = _duracaoVideosRealPorAula
                };

                File.WriteAllText(CaminhoDoArquivo(), JsonSerializer.Serialize(dados, _jsonOptions));
            }
            catch
            {
                // ambiente sem permissão de escrita não derruba a sessão
            }
        }

        private static void CarregarPersistidos()
        {
            try
            {
                var caminho = CaminhoDoArquivo();
                if (!File.Exists(caminho))
                    return;

                var dados = JsonSerializer.Deserialize<DadosPersistidos>(File.ReadAllText(caminho), _jsonOptions);
                if (dados == null)
                    return;

                if (dados.Usuarios is { Count: > 0 })
                    _usuarios = dados.Usuarios;

                if (dados.Matriculas is { Count: > 0 })
                {
                    _matriculas = dados.Matriculas.Select(m => new Matricula
                    {
                        Id = m.Id,
                        AlunoId = m.AlunoId,
                        CursoId = m.CursoId,
                        Aluno = _usuarios.FirstOrDefault(u => u.Id == m.AlunoId),
                        Curso = _cursos.FirstOrDefault(c => c.Id == m.CursoId),
                        Status = m.Status,
                        ProgressoPercentual = m.ProgressoPercentual,
                        IsActive = m.IsActive,
                        DataMatricula = m.DataMatricula,
                        DataConclusao = m.DataConclusao
                    }).ToList();
                }

                if (dados.Progressos is { Count: > 0 })
                    _progressoAulas = dados.Progressos;

                if (dados.Resultados is { Count: > 0 })
                    _resultados = dados.Resultados;

                if (dados.Certificados is { Count: > 0 })
                    _certificados = dados.Certificados;

                if (dados.DuracaoRealPorAula is { Count: > 0 })
                {
                    _duracaoVideosRealPorAula.Clear();
                    foreach (var item in dados.DuracaoRealPorAula)
                        _duracaoVideosRealPorAula[item.Key] = item.Value;
                }
            }
            catch
            {
                // arquivo corrompido/incompatível: mantém o seed
            }
        }
    }
}
