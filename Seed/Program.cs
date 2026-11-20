// Popula o MongoDB com dados de demonstração do European.
// Uso (na pasta pi_maui):  dotnet run --project Seed
// ATENÇÃO: apaga o banco inteiro antes de gravar. Todas as senhas são "Senha123".

using European.Data;
using European.Enums;
using European.Models;
using European.Services;

const string SenhaPadrao = "Senha123";

var banco = MongoContext.Banco; // registra os mapeamentos do app
var nomeBanco = banco.DatabaseNamespace.DatabaseName;
await banco.Client.DropDatabaseAsync(nomeBanco);
Console.WriteLine($"Banco \"{nomeBanco}\" apagado. Gravando dados de demonstração...");

var usuarios = new List<Usuario>();
var matriculas = new List<Matricula>();
var progressos = new List<ProgressoAula>();
var provas = new List<Prova>();
var resultados = new List<ResultadoProva>();
var materiais = new List<MaterialComplementar>();
var notificacoes = new List<Notificacao>();

// ============================================================ USUÁRIOS

var kaique = NovoUsuario("Kaique", "kaique@european.com", Roles.Coordenador, new DateTime(1990, 4, 12), 700);

var helena = NovoUsuario("Helena Duarte", "helena.duarte@european.com", Roles.Professor, new DateTime(1978, 9, 3), 690);
var ricardo = NovoUsuario("Ricardo Albuquerque", "ricardo.albuquerque@european.com", Roles.Professor, new DateTime(1981, 2, 21), 690);
var camila = NovoUsuario("Camila Rezende", "camila.rezende@european.com", Roles.Professor, new DateTime(1985, 6, 14), 400);
var marcelo = NovoUsuario("Marcelo Tavares", "marcelo.tavares@european.com", Roles.Professor, new DateTime(1979, 11, 30), 400);
var fernanda = NovoUsuario("Fernanda Lopes", "fernanda.lopes@european.com", Roles.Professor, new DateTime(1983, 1, 8), 300);

var ana = NovoAluno("Ana Beatriz Souza", "ana.souza@european.com", new DateTime(1994, 3, 17), 610, FormaPagamento.Pix);
var bruno = NovoAluno("Bruno Carvalho", "bruno.carvalho@european.com", new DateTime(1991, 7, 2), 600, FormaPagamento.CartaoCredito);
var carolina = NovoAluno("Carolina Mendes", "carolina.mendes@european.com", new DateTime(1997, 12, 5), 120, FormaPagamento.Pix);
var diego = NovoAluno("Diego Fernandes", "diego.fernandes@european.com", new DateTime(1989, 5, 23), 115, FormaPagamento.CartaoDebito);
var eduarda = NovoAluno("Eduarda Lima", "eduarda.lima@european.com", new DateTime(1993, 8, 11), 150, FormaPagamento.CartaoCredito);
var felipe = NovoAluno("Felipe Rocha", "felipe.rocha@european.com", new DateTime(1996, 10, 29), 140, FormaPagamento.Pix);
var gabriela = NovoAluno("Gabriela Nunes", "gabriela.nunes@european.com", new DateTime(1992, 4, 4), 150, FormaPagamento.CartaoCredito);
var henrique = NovoAluno("Henrique Alves", "henrique.alves@european.com", new DateTime(1988, 1, 19), 100, FormaPagamento.Pix);
var isabela = NovoAluno("Isabela Martins", "isabela.martins@european.com", new DateTime(1999, 9, 9), 10, FormaPagamento.CartaoDebito);
var joao = NovoAluno("João Pedro Costa", "joao.costa@european.com", new DateTime(1995, 6, 27), 95, FormaPagamento.Pix);

// ============================================================ CURSOS

var direito = NovoCurso(
    "Mestrado em Direito Penal Econômico",
    "Mestrado internacional (3.000 h · 120 ECTS) voltado à criminalidade econômica e empresarial: bem jurídico, responsabilidade da pessoa coletiva, lavagem de dinheiro, crimes tributários e justiça penal consensual.",
    CategoriaEnum.Direito, 28900m, 690, new[] { helena, ricardo },
    Mod("Módulo 1 — Fundamentos", "Bases dogmáticas e político-criminais do Direito Penal Econômico.",
        Mat("Bem Jurídico Penal Econômico", "O bem jurídico como limite ao poder punitivo e a tutela de bens supraindividuais.",
            Au("O bem jurídico como chave do Direito Penal", "Conceito de bem jurídico e sua função crítica e limitadora.", "dzsgd1ybGXo", "6:54"),
            Au("Bem jurídico penal e bens supraindividuais", "Ordem econômica, tributária e financeira como objetos de tutela penal.", "rgnUZsUhBUA", "41:25")),
        Mat("Responsabilidade Penal da Pessoa Coletiva", "Fundamentos, modelos de imputação e consequências jurídicas para empresas.",
            Au("Responsabilidade penal da pessoa jurídica", "Previsão constitucional, Lei 9.605/98 e teoria da dupla imputação.", "CuuC6wz8YsI", "8:52"),
            Au("A responsabilidade da empresa na prática", "Visão institucional e casos concretos de responsabilização.", "gJGq5PGvFoM", "2:32"))),
    Mod("Módulo 2 — Crimes em Espécie e Processo", "Principais tipos penais econômicos e os instrumentos de justiça consensual.",
        Mat("Branqueamento de Capitais / Lavagem de Dinheiro", "Fases da lavagem, crimes antecedentes e novas tecnologias.",
            Au("Lei 9.613/98: estrutura do crime de lavagem", "Tipo penal, fases da lavagem e crimes antecedentes.", "of-ABr6MA3M", "13:24"),
            Au("Lavagem de dinheiro com criptoativos", "Riscos, tipologias e investigação envolvendo criptoativos.", "SmVBHmUxmgQ", "32:08")),
        Mat("Crimes Tributários / Infrações Fiscais", "Lei 8.137/90, Súmula Vinculante 24 e extinção da punibilidade.",
            Au("Crimes contra a ordem tributária", "Visão geral da Lei 8.137/90.", "zdYiGekNIrY", "16:33"),
            Au("Lei 8.137/90 comentada", "Crimes materiais e formais, apropriação indébita tributária.", "PyUX10RjiLc", "30:13")),
        Mat("Justiça Penal Consensual", "Acordo de não persecução penal e colaboração premiada.",
            Au("Acordo de não persecução penal (art. 28-A do CPP)", "Requisitos, condições e controle judicial do ANPP.", "7Pe1KFnLk3s", "29:10"),
            Au("Colaboração premiada na Lei 12.850/13", "Natureza jurídica, benefícios e limites probatórios.", "ZChJDajZ14Y", "11:12"))));

var hof = NovoCurso(
    "Mestrado em Harmonização Orofacial",
    "Mestrado internacional (3.000 h · 120 ECTS) em harmonização orofacial: anatomia aplicada, planejamento, biossegurança, toxina botulínica, preenchedores e bioestimuladores.",
    CategoriaEnum.Saude, 34500m, 400, new[] { camila, marcelo },
    Mod("Módulo 1 — Anatomia e Planejamento", "Bases anatômicas da face e planejamento seguro dos procedimentos.",
        Mat("Anatomia Aplicada à Face", "Musculatura da expressão facial e estruturas de risco.",
            Au("Músculos da expressão facial e análise funcional", "Anatomia muscular aplicada à harmonização orofacial.", "Uq3veLUBinQ", "52:48"),
            Au("Músculos da face em 3D", "Revisão tridimensional da musculatura facial.", "JjzqY4TfQ5Y", "16:58")),
        Mat("Planejamento e Biossegurança", "Avaliação do paciente, documentação e controle de infecção.",
            Au("Planejamento completo em harmonização orofacial", "Da avaliação facial ao plano de tratamento.", "8ksEdN7sdfI", "8:22"),
            Au("Biossegurança no consultório", "Esterilização, EPIs e descarte de resíduos.", "v1V0Ubst6jM", "14:16"))),
    Mod("Módulo 2 — Procedimentos Injetáveis", "Toxina botulínica, preenchedores, bioestimuladores e intercorrências.",
        Mat("Toxina Botulínica Estética e Funcional", "Mecanismo de ação, indicações e técnica de aplicação.",
            Au("Mecanismo de ação da toxina botulínica", "Junção neuromuscular e bloqueio da acetilcolina.", "abcELFM2u4c", "6:35"),
            Au("Aplicação de toxina botulínica na glabela", "Pontos, doses e cuidados na região glabelar.", "8yco8OsbX-I", "6:45")),
        Mat("Preenchedores e Bioestimuladores", "Ácido hialurônico, bioestimuladores de colágeno e manejo de complicações.",
            Au("Bioestimuladores de colágeno: conceitos fundamentais", "Tipos de bioestimuladores e indicações.", "NSGKnXc6aVo", "20:10"),
            Au("Intercorrências com ácido hialurônico", "Reconhecimento e conduta nas complicações.", "RqgGWt0FM9w", "13:30"))));

var educacao = NovoCurso(
    "Mestrado em Ciências da Educação",
    "Mestrado internacional (3.000 h · 120 ECTS) para educadores e gestores: gestão democrática, currículo e BNCC, avaliação, tecnologias digitais e pesquisa em educação.",
    CategoriaEnum.Educacao, 19800m, 300, new[] { fernanda },
    Mod("Módulo 1 — Gestão e Currículo", "Organização da escola e referências curriculares nacionais.",
        Mat("Gestão Escolar Democrática", "Dimensões da gestão escolar e participação da comunidade.",
            Au("Gestão escolar: conceitos e dimensões", "Gestão pedagógica, administrativa e de pessoas.", "VUyHBRVYxAc", "22:57"),
            Au("Gestão democrática na escola", "Conselho escolar, PPP e participação.", "KPh0CsW9XIw", "1:00:08")),
        Mat("Currículo e BNCC", "A Base Nacional Comum Curricular e a construção do currículo.",
            Au("BNCC de forma resumida", "Estrutura geral da BNCC.", "8loI6H4PqMQ", "8:28"),
            Au("BNCC: o que todo educador precisa saber", "Competências gerais e organização por etapas.", "fiPp4JX7Tmw", "11:18"))),
    Mod("Módulo 2 — Avaliação, Tecnologia e Pesquisa", "Práticas avaliativas, tecnologias digitais e metodologia de pesquisa.",
        Mat("Avaliação da Aprendizagem", "Avaliação diagnóstica, formativa e somativa.",
            Au("Avaliação da aprendizagem segundo Luckesi", "A avaliação a serviço da aprendizagem.", "nR3DmB6mKoM", "16:18")),
        Mat("Tecnologias Digitais na Educação", "AVA, ensino híbrido e tecnologias para a aprendizagem ativa.",
            Au("Tecnologias e educação: o essencial", "Panorama das tecnologias na prática docente.", "YlrM1qadXRg", "13:58"),
            Au("Como as tecnologias digitais ajudam a aprender", "Experiências e evidências de uso em sala de aula.", "IcCECS_IDQI", "24:38")),
        Mat("Metodologia da Pesquisa em Educação", "Fundamentos para o projeto de dissertação.",
            Au("O que é metodologia científica", "Método, tipos de pesquisa e delineamento.", "NRt5mnEh0nQ", "15:03"))));

var cursos = new List<Curso> { direito, hof, educacao };

// ============================================================ MATERIAIS COMPLEMENTARES

Material(direito, "Responsabilidade penal da pessoa jurídica", "Lei nº 9.605/1998 — Crimes ambientais", "https://www.planalto.gov.br/ccivil_03/leis/l9605.htm", TipoMaterial.Link);
Material(direito, "A responsabilidade da empresa na prática", "O que é compliance anticorrupção? (vídeo)", "https://www.youtube.com/watch?v=0oubU7flvQw", TipoMaterial.Video);
Material(direito, "A responsabilidade da empresa na prática", "Lei Anticorrupção (Lei 12.846/13) — resumo (vídeo)", "https://www.youtube.com/watch?v=VEOAXMi4e5I", TipoMaterial.Video);
Material(direito, "Lei 9.613/98: estrutura do crime de lavagem", "Lei nº 9.613/1998 — Lavagem de dinheiro", "https://www.planalto.gov.br/ccivil_03/leis/l9613.htm", TipoMaterial.Link);
Material(direito, "Crimes contra a ordem tributária", "Lei nº 8.137/1990 — Crimes contra a ordem tributária", "https://www.planalto.gov.br/ccivil_03/leis/l8137.htm", TipoMaterial.Link);
Material(direito, "Acordo de não persecução penal (art. 28-A do CPP)", "Código de Processo Penal", "https://www.planalto.gov.br/ccivil_03/decreto-lei/del3689.htm", TipoMaterial.Link);
Material(direito, "Colaboração premiada na Lei 12.850/13", "Lei nº 12.850/2013 — Organizações criminosas", "https://www.planalto.gov.br/ccivil_03/_ato2011-2014/2013/lei/l12850.htm", TipoMaterial.Link);
Material(hof, "Biossegurança no consultório", "Biossegurança: conceitos básicos (vídeo)", "https://www.youtube.com/watch?v=3KuMdkvZ6rg", TipoMaterial.Video);
Material(hof, "Bioestimuladores de colágeno: conceitos fundamentais", "Mecanismo de ação dos bioestimuladores (vídeo)", "https://www.youtube.com/watch?v=g6iTkD26O-I", TipoMaterial.Video);
Material(educacao, "Gestão democrática na escola", "Lei nº 9.394/1996 — LDB", "https://www.planalto.gov.br/ccivil_03/leis/l9394.htm", TipoMaterial.Link);
Material(educacao, "BNCC de forma resumida", "Base Nacional Comum Curricular (MEC)", "http://basenacionalcomum.mec.gov.br/", TipoMaterial.Link);

// ============================================================ TURMAS (salas)

var turmaDpe2025 = NovaSala("Turma DPE 2025.1", "Turma de Direito Penal Econômico iniciada no 1º semestre de 2025.", helena, direito, 610, ana, bruno);
var turmaDpe2026 = NovaSala("Turma DPE 2026.1", "Turma de Direito Penal Econômico iniciada no 1º semestre de 2026.", ricardo, direito, 120, carolina, diego);
var turmaHof2026 = NovaSala("Turma HOF 2026.1", "Turma de Harmonização Orofacial — encontros práticos às sextas.", camila, hof, 150, eduarda, felipe, gabriela);
var turmaEdu2026 = NovaSala("Turma EDU 2026.1", "Turma de Ciências da Educação — gestores e coordenadores pedagógicos.", fernanda, educacao, 100, gabriela, henrique, isabela, joao);
var salas = new List<Sala> { turmaDpe2025, turmaDpe2026, turmaHof2026, turmaEdu2026 };

// ============================================================ PROVAS

var pBem = NovaProva(direito, "Bem Jurídico Penal Econômico", helena, "Avaliação — Bem Jurídico Penal Econômico",
    "Prova objetiva sobre a teoria do bem jurídico. Leia cada enunciado com atenção.", Dia(-60, 23, 59), 40, 1, true, 600, null,
    Alt("Segundo a doutrina majoritária, qual a principal função do bem jurídico no Direito Penal?", 0,
        "Limitar o poder punitivo, legitimando apenas a tutela de interesses relevantes",
        "Ampliar a tipificação de condutas meramente imorais",
        "Substituir o princípio da legalidade",
        "Definir a competência dos tribunais"),
    Alt("A ordem econômica, a ordem tributária e o sistema financeiro são exemplos de bens jurídicos:", 1,
        "Individuais disponíveis", "Supraindividuais (coletivos ou difusos)", "Exclusivamente patrimoniais", "Sem relevância penal"),
    Alt("Os crimes de perigo abstrato, frequentes no Direito Penal Econômico, caracterizam-se por:", 1,
        "Exigir dano efetivo ao bem jurídico", "Dispensar a demonstração de perigo concreto no caso",
        "Serem sempre culposos", "Admitirem apenas pena de multa"),
    Alt("O princípio da subsidiariedade (ultima ratio) indica que o Direito Penal deve:", 1,
        "Ser aplicado antes das demais esferas", "Intervir apenas quando os outros ramos do Direito forem insuficientes",
        "Punir toda infração administrativa", "Afastar o Direito Administrativo sancionador"));

var pPessoa = NovaProva(direito, "Responsabilidade Penal da Pessoa Coletiva", helena, "Avaliação — Responsabilidade da Pessoa Coletiva",
    "Três questões objetivas e uma dissertativa. A dissertativa é corrigida pela professora.", Dia(-45, 23, 59), 60, 1, true, 590, null,
    Alt("No Brasil, a Constituição admite expressamente a responsabilidade penal da pessoa jurídica em crimes:", 1,
        "Contra a vida", "Ambientais (art. 225, §3º)", "Eleitorais", "Militares"),
    Alt("Sobre a teoria da dupla imputação, o STF (RE 548.181) entendeu que:", 1,
        "É obrigatória a denúncia simultânea da pessoa física",
        "A pessoa jurídica pode ser processada independentemente da pessoa física",
        "A pessoa jurídica nunca responde penalmente",
        "Somente o diretor-presidente pode ser denunciado"),
    Alt("Qual destas NÃO é uma pena aplicável à pessoa jurídica na Lei 9.605/98?", 2,
        "Multa", "Prestação de serviços à comunidade", "Privativa de liberdade", "Suspensão parcial ou total de atividades"),
    Diss("Explique como um programa de compliance efetivo pode influenciar a responsabilização penal de uma empresa.",
        "Relacionar o compliance à prevenção e detecção de ilícitos, à demonstração de diligência da organização, à possível atenuação de sanções e à delimitação das responsabilidades individuais."));

var pLavagem = NovaProva(direito, "Branqueamento de Capitais / Lavagem de Dinheiro", ricardo, "Avaliação — Lavagem de Dinheiro",
    "Prova objetiva com peso 2 na média da matéria.", Dia(25, 23, 59), 45, 2, true, 560, null,
    Alt("As três fases clássicas da lavagem de dinheiro são:", 0,
        "Colocação, ocultação (dissimulação) e integração", "Captação, investimento e resgate",
        "Fraude, sonegação e evasão", "Depósito, saque e transferência"),
    Alt("Após a Lei 12.683/2012, o crime de lavagem de dinheiro pode ter como antecedente:", 2,
        "Apenas o tráfico de drogas", "Somente crimes contra a administração pública", "Qualquer infração penal", "Apenas crimes hediondos"),
    Alt("O órgão brasileiro de inteligência financeira que recebe comunicações de operações suspeitas é:", 1,
        "Banco Central", "COAF", "CVM", "Receita Federal"),
    Alt("O uso de criptoativos em esquemas de lavagem é atrativo principalmente por:", 1,
        "Exigir identificação biométrica", "Permitir transferências rápidas, transfronteiriças e com pseudoanonimato",
        "Ser proibido em todos os países", "Estar sujeito a controle cambial integral"));

var pTributario = NovaProva(direito, "Crimes Tributários / Infrações Fiscais", ricardo, "Avaliação — Crimes Tributários",
    "Três questões objetivas e uma dissertativa.", Dia(30, 23, 59), 60, 1, true, 540, null,
    Alt("Segundo a Súmula Vinculante 24 do STF, o crime material contra a ordem tributária (art. 1º, I a IV, da Lei 8.137/90) só se tipifica:", 1,
        "Com a instauração do inquérito", "Após o lançamento definitivo do tributo",
        "Com a simples declaração falsa", "Após o trânsito em julgado da execução fiscal"),
    Alt("O pagamento integral do débito tributário, nos crimes da Lei 8.137/90:", 1,
        "Não produz efeito penal", "Extingue a punibilidade", "Apenas reduz a pena pela metade", "Converte o crime em contravenção"),
    Alt("Deixar de recolher ICMS cobrado do adquirente (art. 2º, II) é crime, segundo o STF (RHC 163.334), quando:", 1,
        "Há qualquer inadimplência", "Há dolo de apropriação e contumácia", "O valor é inferior a R$ 20 mil", "O contribuinte é pessoa física"),
    Diss("Diferencie elisão fiscal, evasão fiscal e sonegação, indicando qual delas pode ter relevância penal.",
        "Elisão: planejamento lícito, antes do fato gerador. Evasão: conduta ilícita para reduzir ou suprimir tributo. Sonegação: evasão dolosa por fraude ou omissão, tipificada na Lei 8.137/90 — é a que tem relevância penal."));

var pConsensual = NovaProva(direito, "Justiça Penal Consensual", ricardo, "Avaliação — Justiça Penal Consensual",
    "Prova objetiva sobre ANPP e colaboração premiada.", Dia(35, 23, 59), 40, 1, true, 530, null,
    Alt("O acordo de não persecução penal (art. 28-A do CPP) é cabível quando a pena mínima for:", 0,
        "Inferior a 4 anos, em infração sem violência ou grave ameaça", "Superior a 8 anos",
        "Qualquer uma, inclusive em crimes hediondos", "Inferior a 1 ano, apenas"),
    Alt("Um requisito do ANPP é que o investigado:", 1,
        "Seja reincidente", "Tenha confessado formal e circunstancialmente", "Esteja preso preventivamente", "Renuncie ao direito de defesa"),
    Alt("A colaboração premiada, segundo a Lei 12.850/13, tem natureza de:", 1,
        "Meio de prova", "Negócio jurídico processual e meio de obtenção de prova", "Sentença condenatória", "Medida cautelar real"),
    Alt("A sentença condenatória pode se basear exclusivamente nas declarações do colaborador?", 1,
        "Sim, sempre", "Não, a lei veda condenação fundada apenas nas declarações do colaborador",
        "Sim, se o acordo for homologado pelo STF", "Sim, nos crimes tributários"));

var pCaso = NovaProva(direito, "Branqueamento de Capitais / Lavagem de Dinheiro", ricardo, "Estudo de caso — Operação Fachada",
    "Atividade exclusiva da Turma DPE 2026.1. Responda à dissertativa com base nas aulas de lavagem de dinheiro.", Dia(14, 23, 59), 0, 1, true, 20, turmaDpe2026,
    Diss("Uma construtora emite notas fiscais por serviços não prestados a empresas de fachada e recebe o valor de volta em espécie. Identifique os possíveis crimes e as fases da lavagem envolvidas.",
        "Sonegação/falsidade (Lei 8.137/90) e possível corrupção; lavagem: colocação (pagamentos), ocultação (notas frias e empresas de fachada) e integração (retorno do dinheiro com aparência lícita).", 2),
    Alt("Empresas de fachada são utilizadas, principalmente, na fase de:", 1,
        "Colocação", "Ocultação/dissimulação", "Integração", "Nenhuma das fases"),
    Alt("Qual medida assecuratória recai sobre bens adquiridos com os proventos do crime?", 0,
        "Sequestro", "Busca pessoal", "Prisão temporária", "Interceptação telefônica"));

NovaProva(direito, "Bem Jurídico Penal Econômico", helena, "Simulado — Bem Jurídico (rascunho)",
    "Rascunho ainda não publicado.", null, 30, 1, false, 3, null,
    Alt("O Direito Penal Econômico protege predominantemente:", 1,
        "Bens individuais", "Bens supraindividuais", "Apenas o patrimônio privado", "Apenas a fé pública"));

var hAnatomia = NovaProva(hof, "Anatomia Aplicada à Face", camila, "Avaliação — Anatomia Aplicada",
    "Prova objetiva sobre musculatura e vascularização da face.", Dia(-20, 23, 59), 30, 1, true, 130, null,
    Alt("Qual músculo é o principal responsável pelas rugas horizontais da fronte?", 1,
        "Prócero", "Frontal", "Corrugador do supercílio", "Orbicular do olho"),
    Alt("As linhas verticais entre as sobrancelhas resultam principalmente da ação do:", 0,
        "Corrugador do supercílio", "Masseter", "Zigomático maior", "Platisma"),
    Alt("A artéria facial, de grande importância nos preenchimentos, é ramo da:", 1,
        "Artéria carótida interna", "Artéria carótida externa", "Artéria vertebral", "Artéria subclávia"),
    Alt("O músculo orbicular da boca tem como função principal:", 1,
        "Elevar a pálpebra", "Fechar e protrair os lábios", "Elevar o ângulo da boca", "Deprimir a mandíbula"));

var hPlanejamento = NovaProva(hof, "Planejamento e Biossegurança", camila, "Avaliação — Planejamento e Biossegurança",
    "Três questões objetivas e uma dissertativa.", Dia(-5, 23, 59), 45, 1, true, 90, null,
    Alt("A esterilização de instrumentais críticos deve ser feita preferencialmente por:", 1,
        "Álcool 70%", "Autoclave (calor úmido sob pressão)", "Água fervente", "Luz ultravioleta ambiente"),
    Alt("Na avaliação inicial do paciente, a fotografia padronizada serve para:", 1,
        "Apenas divulgação em redes sociais", "Documentação, planejamento e comparação de resultados",
        "Substituir o termo de consentimento", "Dispensar a anamnese"),
    Alt("Materiais perfurocortantes devem ser descartados em:", 2,
        "Lixo comum", "Saco branco leitoso", "Recipiente rígido e resistente à perfuração", "Pia com água corrente"),
    Diss("Descreva os principais itens de um planejamento facial antes de um procedimento de harmonização.",
        "Anamnese e histórico, expectativas do paciente, análise facial (proporções, terços, simetria), avaliação da pele, fotografias padronizadas, escolha de produtos e sequência, TCLE e plano de retorno."));

var hToxina = NovaProva(hof, "Toxina Botulínica Estética e Funcional", marcelo, "Avaliação — Toxina Botulínica",
    "Prova objetiva. Tempo limite de 30 minutos.", Dia(18, 23, 59), 30, 1, true, 15, null,
    Alt("A toxina botulínica tipo A atua bloqueando a liberação de:", 1,
        "Dopamina", "Acetilcolina na junção neuromuscular", "Serotonina", "Histamina"),
    Alt("O efeito clínico da toxina botulínica dura, em média:", 1,
        "1 semana", "3 a 6 meses", "2 anos", "É permanente"),
    Alt("A ptose palpebral após aplicação na glabela está relacionada à difusão para o músculo:", 0,
        "Levantador da pálpebra superior", "Masseter", "Bucinador", "Temporal"),
    Alt("É contraindicação ao uso de toxina botulínica:", 0,
        "Miastenia gravis", "Pele oleosa", "Idade acima de 30 anos", "Uso de protetor solar"));

NovaProva(hof, "Preenchedores e Bioestimuladores", marcelo, "Avaliação — Preenchedores e Bioestimuladores",
    "Em elaboração.", null, 40, 1, false, 2, null,
    Alt("O ácido poli-L-láctico é classificado como:", 1,
        "Preenchedor permanente de efeito imediato", "Bioestimulador de colágeno", "Toxina", "Anestésico local"),
    Alt("Diante de suspeita de oclusão vascular por ácido hialurônico, a conduta imediata inclui:", 1,
        "Compressa de gelo e aguardar 48 h", "Interromper a aplicação e usar hialuronidase",
        "Aplicar mais produto no local", "Prescrever apenas analgésico"));

var eGestao = NovaProva(educacao, "Gestão Escolar Democrática", fernanda, "Avaliação — Gestão Escolar Democrática",
    "Prova objetiva sobre gestão democrática e PPP.", Dia(-30, 23, 59), 30, 1, true, 85, null,
    Alt("A gestão democrática do ensino público é princípio previsto:", 1,
        "Apenas no regimento escolar", "Na Constituição Federal (art. 206) e na LDB", "Somente na BNCC", "Em nenhuma norma"),
    Alt("O Conselho Escolar caracteriza-se por:", 1,
        "Ser composto apenas pelo diretor", "Reunir representantes da comunidade escolar e local",
        "Substituir a secretaria de educação", "Ter função exclusivamente disciplinar"),
    Alt("O Projeto Político-Pedagógico (PPP) deve ser:", 1,
        "Elaborado apenas pela direção", "Construído coletivamente pela comunidade escolar",
        "Copiado de outra escola", "Revisado a cada dez anos"),
    Alt("Para Libâneo, a organização e a gestão da escola são:", 1,
        "Atividades meramente burocráticas", "Meios para assegurar o funcionamento e os objetivos pedagógicos",
        "Responsabilidade exclusiva do Estado", "Independentes do trabalho docente"));

var eBncc = NovaProva(educacao, "Currículo e BNCC", fernanda, "Avaliação — Currículo e BNCC",
    "Três questões objetivas e uma dissertativa.", Dia(-3, 23, 59), 45, 1, true, 60, null,
    Alt("A BNCC define:", 1,
        "O currículo completo de cada escola", "As aprendizagens essenciais que todos os alunos devem desenvolver",
        "Os livros didáticos obrigatórios", "O calendário escolar nacional"),
    Alt("Quantas competências gerais a BNCC estabelece para a Educação Básica?", 2, "5", "8", "10", "12"),
    Alt("Na BNCC, a Educação Infantil é organizada em:", 1,
        "Disciplinas", "Campos de experiências", "Componentes curriculares", "Itinerários formativos"),
    Diss("Explique a diferença entre BNCC e currículo e o papel da escola nessa relação.",
        "A BNCC é a referência normativa obrigatória; o currículo é construído pelas redes e escolas a partir dela, considerando o contexto, o PPP e a parte diversificada."));

NovaProva(educacao, "Avaliação da Aprendizagem", fernanda, "Avaliação — Avaliação da Aprendizagem",
    "Prova objetiva.", Dia(21, 23, 59), 30, 1, true, 10, null,
    Alt("Para Luckesi, a avaliação da aprendizagem deve ser essencialmente:", 1,
        "Classificatória", "Diagnóstica, a serviço da aprendizagem", "Punitiva", "Opcional"),
    Alt("A avaliação formativa caracteriza-se por:", 1,
        "Ocorrer apenas no fim do ano", "Acompanhar o processo e orientar intervenções", "Gerar ranking de alunos", "Dispensar feedback"),
    Alt("Qual instrumento favorece a autoavaliação e o registro da trajetória do aluno?", 0,
        "Portfólio", "Lista de chamada", "Boletim bimestral", "Ata de reunião"),
    Alt("A avaliação somativa tem como objetivo principal:", 1,
        "Diagnosticar conhecimentos prévios", "Verificar resultados ao final de uma etapa", "Substituir o planejamento", "Eliminar a recuperação"));

NovaProva(educacao, "Tecnologias Digitais na Educação", fernanda, "Atividade — Tecnologias na Prática (Turma EDU 2026.1)",
    "Atividade da turma: duas questões objetivas e uma proposta de aula.", Dia(40, 23, 59), 0, 1, true, 5, turmaEdu2026,
    Alt("Um Ambiente Virtual de Aprendizagem (AVA) é:", 1,
        "Um laboratório físico de informática", "Uma plataforma online para organizar conteúdos, atividades e interação",
        "Um tipo de computador", "Uma rede social pessoal"),
    Alt("O ensino híbrido combina:", 1,
        "Apenas aulas gravadas", "Atividades presenciais e online de forma integrada",
        "Somente aulas expositivas", "Ensino domiciliar e escolar sem planejamento"),
    Diss("Proponha uma atividade que use uma tecnologia digital para promover aprendizagem ativa na sua área.",
        "Objetivo claro, tecnologia como meio, protagonismo do aluno e critérios de avaliação definidos."));

// ============================================================ MATRÍCULAS, PROGRESSO E ENTREGAS
// Gabarito das objetivas, na ordem da prova: C = acertou, E = errou, - = em branco.

// Ana — concluiu o mestrado e gerou o certificado
var mAna = Matricular(ana, direito, 610);
mAna.Status = StatusMatricula.Concluida;
mAna.DataConclusao = Dia(-40, 16);
Assistir(mAna, direito, 10);
Entrega(pBem, ana, "CCCC", Dia(-560, 19), true);
Entrega(pPessoa, ana, "CCC", Dia(-500, 20), true,
    ("Um programa de compliance efetivo demonstra que a empresa adotou mecanismos de prevenção, detecção e resposta a ilícitos (código de conduta, canal de denúncias, due diligence, treinamentos). Isso pode afastar ou atenuar a culpabilidade organizacional e ajuda a individualizar a responsabilidade dos gestores.", 9,
     "Ótima relação entre compliance e culpabilidade organizacional."));
Entrega(pLavagem, ana, "CCEC", Dia(-300, 18), true);
Entrega(pTributario, ana, "CCC", Dia(-200, 21), true,
    ("Elisão é o planejamento tributário lícito, feito antes do fato gerador. Evasão é a redução ilícita do tributo devido. A sonegação é a evasão dolosa mediante fraude ou omissão e é tipificada na Lei 8.137/90, por isso tem relevância penal.", 8,
     "Correto. Poderia citar a Súmula Vinculante 24."));
Entrega(pConsensual, ana, "CCCE", Dia(-60, 20), true);

// Bruno — quase no fim; a última entrega aguarda correção
var mBruno = Matricular(bruno, direito, 600);
Assistir(mBruno, direito, 9, 35);
Entrega(pBem, bruno, "CECC", Dia(-550, 22), true);
Entrega(pPessoa, bruno, "CCE", Dia(-490, 21), true,
    ("O compliance mostra que a empresa tentou evitar o crime, então pode diminuir a pena da empresa.", 7,
     "Correto, mas faltou desenvolver a relação com a culpabilidade e a responsabilidade individual."));
Entrega(pLavagem, bruno, "CCCC", Dia(-280, 19), true);
Entrega(pTributario, bruno, "CCE", Dia(-2, 22), false,
    ("Elisão é legal e feita antes do fato gerador. Evasão é ilegal. A sonegação é crime previsto na Lei 8.137/90, por exemplo omitir informação ou fraudar a fiscalização.", 0, ""));

// Carolina — começando o curso
var mCarolina = Matricular(carolina, direito, 120);
Assistir(mCarolina, direito, 3, 60);
Entrega(pBem, carolina, "CCCE", Dia(-70, 20), true);

// Diego — trancou a matrícula
var mDiego = Matricular(diego, direito, 115);
mDiego.Status = StatusMatricula.Trancada;
mDiego.TrancadaEm = Dia(-20, 11);
Assistir(mDiego, direito, 4);
Entrega(pBem, diego, "CCCC", Dia(-75, 21), true);

// Eduarda — metade do curso, provas corrigidas
var mEduarda = Matricular(eduarda, hof, 150);
Assistir(mEduarda, hof, 5, 50);
Entrega(hAnatomia, eduarda, "CCCC", Dia(-25, 19), true);
Entrega(hPlanejamento, eduarda, "CCC", Dia(-8, 20), true,
    ("Anamnese completa, expectativas do paciente, análise facial por terços e simetria, avaliação da pele, fotos padronizadas, escolha dos produtos, assinatura do termo de consentimento e agendamento de retorno.", 8.5,
     "Muito bom. Inclua também a avaliação de contraindicações."));

// Felipe — abaixo da média na primeira prova
var mFelipe = Matricular(felipe, hof, 140);
Assistir(mFelipe, hof, 2, 20);
Entrega(hAnatomia, felipe, "CEEC", Dia(-22, 23), true);

// Gabriela — dois cursos (HOF e Educação)
var mGabrielaHof = Matricular(gabriela, hof, 150);
Assistir(mGabrielaHof, hof, 4, 15);
Entrega(hAnatomia, gabriela, "CCEC", Dia(-24, 18), true);
Entrega(hPlanejamento, gabriela, "CEC", Dia(-6, 21), false,
    ("Fazer anamnese, tirar fotos, analisar a face do paciente e explicar o procedimento antes de começar.", 0, ""));
var mGabrielaEdu = Matricular(gabriela, educacao, 90);
Assistir(mGabrielaEdu, educacao, 3, 40);
Entrega(eGestao, gabriela, "CCCC", Dia(-35, 20), true);

// Henrique — entrega da BNCC aguardando correção
var mHenrique = Matricular(henrique, educacao, 100);
Assistir(mHenrique, educacao, 5, 70);
Entrega(eGestao, henrique, "CCCE", Dia(-40, 19), true);
Entrega(eBncc, henrique, "CCC", Dia(-4, 22), false,
    ("A BNCC é o documento normativo que define o que todos devem aprender. O currículo é feito por cada rede e escola a partir da BNCC, adaptando à realidade local e ao PPP.", 0, ""));

// Isabela — acabou de entrar
var mIsabela = Matricular(isabela, educacao, 9);
Assistir(mIsabela, educacao, 0, 25);

// João — abaixo da média em Gestão
var mJoao = Matricular(joao, educacao, 95);
Assistir(mJoao, educacao, 3);
Entrega(eGestao, joao, "CEEC", Dia(-32, 23), true);

// ============================================================ CERTIFICADO (Ana)

var notasAna = resultados.Where(r => r.AlunoId == ana.Id)
    .Select(r => (r.NotaNormalizada, Peso: provas.First(p => p.Id == r.ProvaId).Peso)).ToList();
var certificado = new Certificado
{
    Id = MongoContext.NovoId(),
    MatriculaId = mAna.Id,
    CursoId = direito.Id,
    AlunoId = ana.Id,
    Titulo = direito.Nome,
    AlunoNome = ana.Nome,
    CursoNome = direito.Nome,
    CargaHorariaHoras = direito.CargaHorariaHoras,
    MediaFinal = Math.Round(notasAna.Sum(n => n.NotaNormalizada * n.Peso) / notasAna.Sum(n => n.Peso), 1),
    DataInicio = mAna.DataMatricula,
    DataConclusao = mAna.DataConclusao,
    EmitidoEm = Dia(-38, 10)
};
certificado.CodigoVerificacao = $"EURO-{certificado.EmitidoEm:yyyy}-{certificado.Id[^8..].ToUpperInvariant()}";

// ============================================================ SOLICITAÇÕES PENDENTES

var solicitacoes = new List<SolicitacaoMatricula>
{
    new()
    {
        Id = MongoContext.NovoId(), Nome = "Larissa Gomes", Email = "larissa.gomes@european.com",
        SenhaHash = SenhaService.GerarHash(SenhaPadrao), CursoId = direito.Id,
        FormaPagamento = FormaPagamento.Pix, PagamentoEm = Dia(-2, 15, 20), CriadaEm = Dia(-2, 15, 5)
    },
    new()
    {
        Id = MongoContext.NovoId(), Nome = "Mateus Ribeiro", Email = "mateus.ribeiro@european.com",
        SenhaHash = SenhaService.GerarHash(SenhaPadrao), CursoId = educacao.Id, CriadaEm = Dia(-1, 9, 40)
    }
};

// ============================================================ NOTIFICAÇÕES

Notificar(kaique, "Solicitação de matrícula", $"Larissa Gomes fez o pagamento e aguarda a liberação em {direito.Nome}.", Dia(-2, 15, 21));
Notificar(kaique, "Matrícula trancada", $"{diego.Nome} trancou a matrícula em {direito.Nome}. Motivo: mudança de cidade.", Dia(-20, 11), true);
Notificar(kaique, "Certificado emitido", $"{ana.Nome} gerou o certificado de {direito.Nome} (código {certificado.CodigoVerificacao}).", certificado.EmitidoEm, true);
Notificar(ricardo, "Nova entrega", $"Uma entrega da prova \"{pTributario.Titulo}\" aguarda a sua correção.", Dia(-2, 22));
Notificar(camila, "Nova entrega", $"Uma entrega da prova \"{hPlanejamento.Titulo}\" aguarda a sua correção.", Dia(-6, 21));
Notificar(fernanda, "Nova entrega", $"Uma entrega da prova \"{eBncc.Titulo}\" aguarda a sua correção.", Dia(-4, 22));
Notificar(ana, "Curso concluído", $"Você concluiu {direito.Nome}. Gere o seu certificado na aba Certificado.", mAna.DataConclusao!.Value, true);
Notificar(bruno, "Nota liberada", $"A correção da prova \"{pLavagem.Titulo}\" foi concluída. Veja a nota em Provas › Realizadas.", Dia(-278, 10), true);
Notificar(carolina, "Nova prova", $"A prova \"{pCaso.Titulo}\" já está disponível para a sua turma.", Dia(-20, 9));
Notificar(eduarda, "Nota liberada", $"A correção da prova \"{hPlanejamento.Titulo}\" foi concluída. Veja a nota em Provas › Realizadas.", Dia(-6, 10));
Notificar(isabela, "Bem-vindo(a)!", "Sua matrícula foi liberada. As aulas já estão disponíveis.", Dia(-9, 14));

// ============================================================ GRAVAÇÃO

await MongoContext.Usuarios.InsertManyAsync(usuarios);
await MongoContext.Cursos.InsertManyAsync(cursos);
await MongoContext.Salas.InsertManyAsync(salas);
await MongoContext.Matriculas.InsertManyAsync(matriculas);
await MongoContext.Progressos.InsertManyAsync(progressos);
await MongoContext.Materiais.InsertManyAsync(materiais);
await MongoContext.Provas.InsertManyAsync(provas);
await MongoContext.Resultados.InsertManyAsync(resultados);
await MongoContext.Certificados.InsertOneAsync(certificado);
await MongoContext.Solicitacoes.InsertManyAsync(solicitacoes);
await MongoContext.Notificacoes.InsertManyAsync(notificacoes);
await MongoContext.InicializarAsync(); // índices (os cursos já existem, então não grava os iniciais)

Console.WriteLine($"""

    Pronto!
      {usuarios.Count(u => u.IsCoordenacao)} coordenador, {usuarios.Count(u => u.Roles == Roles.Professor)} professores, {usuarios.Count(u => u.Roles == Roles.Aluno)} alunos
      {cursos.Count} cursos, {cursos.Sum(c => c.Modulos.Count)} módulos, {cursos.Sum(c => c.TotalMaterias)} matérias, {cursos.Sum(c => c.TotalAulas)} aulas
      {salas.Count} turmas, {provas.Count} provas, {resultados.Count} entregas ({resultados.Count(r => !r.Corrigida)} aguardando correção)
      {matriculas.Count} matrículas, {solicitacoes.Count} solicitações pendentes, 1 certificado

    Login: kaique@european.com  /  {SenhaPadrao}  (todas as contas usam a mesma senha)
    """);

// ============================================================ AUXILIARES

static DateTime Dia(int dias, int hora = 10, int minuto = 0) => DateTime.Today.AddDays(dias).AddHours(hora).AddMinutes(minuto);

Usuario NovoUsuario(string nome, string email, Roles papel, DateTime nascimento, int criadoHaDias)
{
    var criado = Dia(-criadoHaDias, 9);
    var usuario = new Usuario
    {
        Id = MongoContext.NovoId(),
        Nome = nome,
        Email = email.ToLowerInvariant(),
        SenhaHash = SenhaService.GerarHash(SenhaPadrao),
        Roles = papel,
        StatusAcesso = StatusAcesso.Liberado,
        DataNascimento = nascimento,
        CreatedAt = criado,
        UpdatedAt = criado
    };
    usuarios.Add(usuario);
    return usuario;
}

Usuario NovoAluno(string nome, string email, DateTime nascimento, int criadoHaDias, FormaPagamento pagamento)
{
    var aluno = NovoUsuario(nome, email, Roles.Aluno, nascimento, criadoHaDias);
    aluno.FormaPagamento = pagamento;
    aluno.PagamentoEm = aluno.CreatedAt.AddHours(-20);
    return aluno;
}

static AulaDef Au(string titulo, string descricao, string videoId, string duracao) => new(titulo, descricao, videoId, duracao);
static MateriaDef Mat(string nome, string descricao, params AulaDef[] aulas) => new(nome, descricao, aulas);
static ModuloDef Mod(string titulo, string descricao, params MateriaDef[] materias) => new(titulo, descricao, materias);

static int Segundos(string duracao)
    => duracao.Split(':').Select(int.Parse).Aggregate(0, (total, parte) => total * 60 + parte);

Curso NovoCurso(string nome, string descricao, CategoriaEnum categoria, decimal preco, int criadoHaDias, Usuario[] professores, params ModuloDef[] grade)
{
    var criado = Dia(-criadoHaDias, 8);
    var curso = new Curso
    {
        Id = MongoContext.NovoId(),
        Nome = nome,
        Descricao = descricao,
        Categoria = categoria,
        Preco = preco,
        CargaHorariaHoras = 3000,
        IsPublicado = true,
        ProfessorIds = professores.Select(p => p.Id).ToList(),
        CreatedAt = criado,
        UpdatedAt = criado
    };

    for (var i = 0; i < grade.Length; i++)
    {
        var modulo = new Modulo { Id = MongoContext.NovoId(), Titulo = grade[i].Titulo, Descricao = grade[i].Descricao, Ordem = i + 1 };
        for (var j = 0; j < grade[i].Materias.Length; j++)
        {
            var def = grade[i].Materias[j];
            var materia = new Materia { Id = MongoContext.NovoId(), Nome = def.Nome, Descricao = def.Descricao, Ordem = j + 1 };
            for (var k = 0; k < def.Aulas.Length; k++)
            {
                var aula = def.Aulas[k];
                materia.Aulas.Add(new Aula
                {
                    Id = MongoContext.NovoId(),
                    Titulo = aula.Titulo,
                    Descricao = aula.Descricao,
                    VideoId = $"https://www.youtube.com/watch?v={aula.VideoId}",
                    DuracaoSegundos = Segundos(aula.Duracao),
                    Ordem = k + 1
                });
            }
            modulo.Materias.Add(materia);
        }
        curso.Modulos.Add(modulo);
    }

    return curso.Vincular();
}

void Material(Curso curso, string tituloAula, string titulo, string url, TipoMaterial tipo)
{
    var aula = curso.TodasAulas.First(a => a.Titulo == tituloAula);
    materiais.Add(new MaterialComplementar
    {
        Id = MongoContext.NovoId(),
        CursoId = curso.Id,
        AulaId = aula.Id,
        Titulo = titulo,
        Url = url,
        Tipo = tipo,
        Ordem = materiais.Count(m => m.AulaId == aula.Id) + 1
    });
}

Sala NovaSala(string nome, string descricao, Usuario professor, Curso curso, int criadaHaDias, params Usuario[] alunos)
{
    var criada = Dia(-criadaHaDias, 8);
    return new Sala
    {
        Id = MongoContext.NovoId(),
        Nome = nome,
        Descricao = descricao,
        ProfessorId = professor.Id,
        CursoId = curso.Id,
        AlunoIds = alunos.Select(a => a.Id).ToList(),
        CreatedAt = criada,
        UpdatedAt = criada
    };
}

static Questao Alt(string enunciado, int correta, params string[] alternativas)
    => new() { Tipo = TipoQuestao.Alternativas, Enunciado = enunciado, Alternativas = alternativas.ToList(), IndiceCorreta = correta };

static Questao Diss(string enunciado, string esperada, double peso = 1)
    => new() { Tipo = TipoQuestao.Dissertativa, Enunciado = enunciado, RespostaEsperada = esperada, Peso = peso };

Prova NovaProva(Curso curso, string materia, Usuario professor, string titulo, string instrucoes, DateTime? prazo,
    int duracaoMinutos, double peso, bool publicada, int criadaHaDias, Sala? sala, params Questao[] questoes)
{
    var criada = Dia(-criadaHaDias, 9);
    var prova = new Prova
    {
        Id = MongoContext.NovoId(),
        CursoId = curso.Id,
        MateriaId = curso.TodasMaterias.First(m => m.Nome == materia).Id,
        ProfessorId = professor.Id,
        SalaId = sala?.Id,
        Titulo = titulo,
        Instrucoes = instrucoes,
        NotaMaxima = 10,
        Peso = peso,
        NotaCorte = 6,
        Prazo = prazo,
        DuracaoMinutos = duracaoMinutos,
        IsPublicada = publicada,
        Questoes = questoes.ToList(),
        CreatedAt = criada,
        UpdatedAt = criada
    };
    provas.Add(prova);
    return prova;
}

Matricula Matricular(Usuario aluno, Curso curso, int haDias)
{
    var data = Dia(-haDias, 14);
    var matricula = new Matricula
    {
        Id = MongoContext.NovoId(),
        AlunoId = aluno.Id,
        CursoId = curso.Id,
        Status = StatusMatricula.Ativa,
        LiberadaPorId = kaique.Id,
        DataMatricula = data,
        CreatedAt = data,
        UpdatedAt = DateTime.Now
    };
    matriculas.Add(matricula);
    return matricula;
}

// Conclui as primeiras aulas na ordem do curso (a liberação é sequencial) e deixa a seguinte parcialmente assistida.
void Assistir(Matricula matricula, Curso curso, int concluidas, double percentualDaProxima = 0)
{
    var aulas = curso.TodasAulas.ToList();
    var inicio = matricula.DataMatricula;
    var fim = matricula.DataConclusao ?? matricula.TrancadaEm ?? DateTime.Now.AddHours(-3);

    for (var i = 0; i < concluidas; i++)
        progressos.Add(new ProgressoAula
        {
            Id = MongoContext.NovoId(),
            MatriculaId = matricula.Id,
            AulaId = aulas[i].Id,
            PercentualAssistido = 100,
            UltimaPosicaoSegundos = aulas[i].DuracaoSegundos,
            Concluida = true,
            AtualizadoEm = inicio + (fim - inicio) * (i + 1) / (concluidas + 1)
        });

    if (percentualDaProxima > 0 && concluidas < aulas.Count)
        progressos.Add(new ProgressoAula
        {
            Id = MongoContext.NovoId(),
            MatriculaId = matricula.Id,
            AulaId = aulas[concluidas].Id,
            PercentualAssistido = percentualDaProxima,
            UltimaPosicaoSegundos = Math.Round(aulas[concluidas].DuracaoSegundos * percentualDaProxima / 100),
            AtualizadoEm = fim
        });

    matricula.ProgressoPercentual = Math.Round(concluidas * 100.0 / aulas.Count);
}

// Mesma regra do ProvaService: objetivas valem 10 ou 0; dissertativas recebem a nota do professor ao corrigir.
void Entrega(Prova prova, Usuario aluno, string gabarito, DateTime quando, bool corrigida,
    params (string Texto, double Nota, string Comentario)[] dissertativas)
{
    if (gabarito.Length != prova.Questoes.Count(q => !q.IsDissertativa) || dissertativas.Length != prova.Questoes.Count(q => q.IsDissertativa))
        throw new InvalidOperationException($"Entrega de {aluno.Nome} não bate com as questões de \"{prova.Titulo}\".");

    var resultado = new ResultadoProva
    {
        Id = MongoContext.NovoId(),
        ProvaId = prova.Id,
        AlunoId = aluno.Id,
        CursoId = prova.CursoId,
        MateriaId = prova.MateriaId,
        TotalQuestoes = prova.Questoes.Count,
        NotaMaxima = prova.NotaMaxima,
        DataRealizacao = quando,
        Status = corrigida ? StatusProva.Corrigida : StatusProva.Realizada,
        CorrigidaEm = corrigida ? quando.AddDays(2) : null
    };

    int objetiva = 0, dissertativa = 0;
    foreach (var questao in prova.Questoes)
    {
        if (questao.IsDissertativa)
        {
            var d = dissertativas[dissertativa++];
            resultado.Respostas.Add(-1);
            resultado.RespostasTexto.Add(d.Texto);
            resultado.NotasQuestoes.Add(corrigida ? d.Nota : null);
            resultado.Comentarios.Add(corrigida ? d.Comentario : string.Empty);
            continue;
        }

        var marca = gabarito[objetiva++];
        var acertou = marca == 'C';
        resultado.Respostas.Add(acertou ? questao.IndiceCorreta : marca == '-' ? -1 : (questao.IndiceCorreta + 1) % questao.Alternativas.Count);
        resultado.RespostasTexto.Add(string.Empty);
        resultado.NotasQuestoes.Add(acertou ? 10 : 0);
        resultado.Comentarios.Add(string.Empty);
        if (acertou)
            resultado.Acertos++;
    }

    double pesoTotal = 0, pontos = 0;
    for (var i = 0; i < prova.Questoes.Count; i++)
    {
        var peso = prova.Questoes[i].Peso > 0 ? prova.Questoes[i].Peso : 1;
        pesoTotal += peso;
        pontos += peso * Math.Clamp(resultado.NotasQuestoes[i] ?? 0, 0, 10) / 10;
    }
    resultado.NotaObtida = Math.Round(pontos / pesoTotal * prova.NotaMaxima, 2);

    resultados.Add(resultado);
}

void Notificar(Usuario usuario, string titulo, string mensagem, DateTime quando, bool lida = false)
    => notificacoes.Add(new Notificacao
    {
        Id = MongoContext.NovoId(),
        UsuarioId = usuario.Id,
        Titulo = titulo,
        Mensagem = mensagem,
        CriadaEm = quando,
        Lida = lida
    });

record AulaDef(string Titulo, string Descricao, string VideoId, string Duracao);
record MateriaDef(string Nome, string Descricao, AulaDef[] Aulas);
record ModuloDef(string Titulo, string Descricao, MateriaDef[] Materias);
