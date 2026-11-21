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

var kaique = NovoUsuario("Kaique", "kaique@european.com", Roles.Coordenador, new DateTime(1990, 4, 12), 400);

var helena = NovoUsuario("Helena Duarte", "helena.duarte@european.com", Roles.Professor, new DateTime(1978, 9, 3), 380);
var camila = NovoUsuario("Camila Rezende", "camila.rezende@european.com", Roles.Professor, new DateTime(1985, 6, 14), 380);
var fernanda = NovoUsuario("Fernanda Lopes", "fernanda.lopes@european.com", Roles.Professor, new DateTime(1983, 1, 8), 380);

var ana = NovoAluno("Ana Beatriz Souza", "ana.souza@european.com", new DateTime(1994, 3, 17), 75, FormaPagamento.Pix);
var bruno = NovoAluno("Bruno Carvalho", "bruno.carvalho@european.com", new DateTime(1991, 7, 2), 40, FormaPagamento.CartaoCredito);
var carolina = NovoAluno("Carolina Mendes", "carolina.mendes@european.com", new DateTime(1997, 12, 5), 5, FormaPagamento.CartaoDebito);

// ============================================================ CURSOS (3 módulos cada)

var direito = NovoCurso(
    "Mestrado em Direito Penal Econômico",
    "Mestrado internacional (3.000 h · 120 ECTS) voltado à criminalidade econômica e empresarial: bem jurídico, responsabilidade da pessoa coletiva, lavagem de dinheiro, crimes tributários e justiça penal consensual.",
    CategoriaEnum.Direito, 28900m, 380, new[] { helena },
    Mod("Módulo 1 — Fundamentos", "Bases dogmáticas e político-criminais do Direito Penal Econômico.",
        Mat("Bem Jurídico Penal Econômico", "O bem jurídico como limite ao poder punitivo e a tutela de bens supraindividuais.",
            Au("O bem jurídico como chave do Direito Penal", "Conceito de bem jurídico e sua função crítica e limitadora.", "dzsgd1ybGXo", "6:54"),
            Au("Bem jurídico penal e bens supraindividuais", "Ordem econômica, tributária e financeira como objetos de tutela penal.", "rgnUZsUhBUA", "41:25")),
        Mat("Responsabilidade Penal da Pessoa Coletiva", "Fundamentos, modelos de imputação e consequências jurídicas para empresas.",
            Au("Responsabilidade penal da pessoa jurídica", "Previsão constitucional, Lei 9.605/98 e teoria da dupla imputação.", "CuuC6wz8YsI", "8:52"),
            Au("A responsabilidade da empresa na prática", "Visão institucional e casos concretos de responsabilização.", "gJGq5PGvFoM", "2:32"))),
    Mod("Módulo 2 — Crimes em Espécie", "Lavagem de dinheiro e crimes contra a ordem tributária.",
        Mat("Branqueamento de Capitais / Lavagem de Dinheiro", "Fases da lavagem, crimes antecedentes e novas tecnologias.",
            Au("Lei 9.613/98: estrutura do crime de lavagem", "Tipo penal, fases da lavagem e crimes antecedentes.", "of-ABr6MA3M", "13:24"),
            Au("Lavagem de dinheiro com criptoativos", "Riscos, tipologias e investigação envolvendo criptoativos.", "SmVBHmUxmgQ", "32:08")),
        Mat("Crimes Tributários / Infrações Fiscais", "Lei 8.137/90, Súmula Vinculante 24 e extinção da punibilidade.",
            Au("Crimes contra a ordem tributária", "Visão geral da Lei 8.137/90.", "zdYiGekNIrY", "16:33"),
            Au("Lei 8.137/90 comentada", "Crimes materiais e formais, apropriação indébita tributária.", "PyUX10RjiLc", "30:13"))),
    Mod("Módulo 3 — Processo Penal Econômico", "Instrumentos de justiça penal consensual.",
        Mat("Justiça Penal Consensual", "Acordo de não persecução penal e colaboração premiada.",
            Au("Acordo de não persecução penal (art. 28-A do CPP)", "Requisitos, condições e controle judicial do ANPP.", "7Pe1KFnLk3s", "29:10"),
            Au("Colaboração premiada na Lei 12.850/13", "Natureza jurídica, benefícios e limites probatórios.", "ZChJDajZ14Y", "11:12"))));

var hof = NovoCurso(
    "Mestrado em Harmonização Orofacial",
    "Mestrado internacional (3.000 h · 120 ECTS) em harmonização orofacial: anatomia aplicada, planejamento, biossegurança, toxina botulínica, preenchedores e bioestimuladores.",
    CategoriaEnum.Saude, 34500m, 380, new[] { camila },
    Mod("Módulo 1 — Anatomia e Planejamento", "Bases anatômicas da face e planejamento seguro dos procedimentos.",
        Mat("Anatomia Aplicada à Face", "Musculatura da expressão facial e estruturas de risco.",
            Au("Músculos da expressão facial e análise funcional", "Anatomia muscular aplicada à harmonização orofacial.", "Uq3veLUBinQ", "52:48"),
            Au("Músculos da face em 3D", "Revisão tridimensional da musculatura facial.", "JjzqY4TfQ5Y", "16:58")),
        Mat("Planejamento e Biossegurança", "Avaliação do paciente, documentação e controle de infecção.",
            Au("Planejamento completo em harmonização orofacial", "Da avaliação facial ao plano de tratamento.", "8ksEdN7sdfI", "8:22"),
            Au("Biossegurança no consultório", "Esterilização, EPIs e descarte de resíduos.", "v1V0Ubst6jM", "14:16"))),
    Mod("Módulo 2 — Toxina Botulínica", "Mecanismo de ação, indicações e técnica de aplicação.",
        Mat("Toxina Botulínica Estética e Funcional", "Mecanismo de ação, indicações e técnica de aplicação.",
            Au("Mecanismo de ação da toxina botulínica", "Junção neuromuscular e bloqueio da acetilcolina.", "abcELFM2u4c", "6:35"),
            Au("Aplicação de toxina botulínica na glabela", "Pontos, doses e cuidados na região glabelar.", "8yco8OsbX-I", "6:45"))),
    Mod("Módulo 3 — Preenchedores e Bioestimuladores", "Ácido hialurônico, bioestimuladores e manejo de intercorrências.",
        Mat("Preenchedores e Bioestimuladores", "Ácido hialurônico, bioestimuladores de colágeno e manejo de complicações.",
            Au("Bioestimuladores de colágeno: conceitos fundamentais", "Tipos de bioestimuladores e indicações.", "NSGKnXc6aVo", "20:10"),
            Au("Intercorrências com ácido hialurônico", "Reconhecimento e conduta nas complicações.", "RqgGWt0FM9w", "13:30"))));

var educacao = NovoCurso(
    "Mestrado em Ciências da Educação",
    "Mestrado internacional (3.000 h · 120 ECTS) para educadores e gestores: gestão democrática, currículo e BNCC, avaliação, tecnologias digitais e pesquisa em educação.",
    CategoriaEnum.Educacao, 19800m, 380, new[] { fernanda },
    Mod("Módulo 1 — Gestão e Currículo", "Organização da escola e referências curriculares nacionais.",
        Mat("Gestão Escolar Democrática", "Dimensões da gestão escolar e participação da comunidade.",
            Au("Gestão escolar: conceitos e dimensões", "Gestão pedagógica, administrativa e de pessoas.", "VUyHBRVYxAc", "22:57"),
            Au("Gestão democrática na escola", "Conselho escolar, PPP e participação.", "KPh0CsW9XIw", "1:00:08")),
        Mat("Currículo e BNCC", "A Base Nacional Comum Curricular e a construção do currículo.",
            Au("BNCC de forma resumida", "Estrutura geral da BNCC.", "8loI6H4PqMQ", "8:28"),
            Au("BNCC: o que todo educador precisa saber", "Competências gerais e organização por etapas.", "fiPp4JX7Tmw", "11:18"))),
    Mod("Módulo 2 — Avaliação e Tecnologia", "Práticas avaliativas e tecnologias digitais na aprendizagem.",
        Mat("Avaliação da Aprendizagem", "Avaliação diagnóstica, formativa e somativa.",
            Au("Avaliação da aprendizagem segundo Luckesi", "A avaliação a serviço da aprendizagem.", "nR3DmB6mKoM", "16:18")),
        Mat("Tecnologias Digitais na Educação", "AVA, ensino híbrido e tecnologias para a aprendizagem ativa.",
            Au("Tecnologias e educação: o essencial", "Panorama das tecnologias na prática docente.", "YlrM1qadXRg", "13:58"),
            Au("Como as tecnologias digitais ajudam a aprender", "Experiências e evidências de uso em sala de aula.", "IcCECS_IDQI", "24:38"))),
    Mod("Módulo 3 — Pesquisa em Educação", "Metodologia para o projeto de dissertação.",
        Mat("Metodologia da Pesquisa em Educação", "Fundamentos para o projeto de dissertação.",
            Au("O que é metodologia científica", "Método, tipos de pesquisa e delineamento.", "NRt5mnEh0nQ", "15:03"))));

var cursos = new List<Curso> { direito, hof, educacao };

// ============================================================ MATERIAIS COMPLEMENTARES

Material(direito, "Responsabilidade penal da pessoa jurídica", "Lei nº 9.605/1998 — Crimes ambientais", "https://www.planalto.gov.br/ccivil_03/leis/l9605.htm", TipoMaterial.Link);
Material(direito, "Lei 9.613/98: estrutura do crime de lavagem", "Lei nº 9.613/1998 — Lavagem de dinheiro", "https://www.planalto.gov.br/ccivil_03/leis/l9613.htm", TipoMaterial.Link);
Material(direito, "Crimes contra a ordem tributária", "Lei nº 8.137/1990 — Crimes contra a ordem tributária", "https://www.planalto.gov.br/ccivil_03/leis/l8137.htm", TipoMaterial.Link);
Material(direito, "Acordo de não persecução penal (art. 28-A do CPP)", "Código de Processo Penal", "https://www.planalto.gov.br/ccivil_03/decreto-lei/del3689.htm", TipoMaterial.Link);
Material(hof, "Biossegurança no consultório", "Biossegurança: conceitos básicos (vídeo)", "https://www.youtube.com/watch?v=3KuMdkvZ6rg", TipoMaterial.Video);
Material(hof, "Bioestimuladores de colágeno: conceitos fundamentais", "Mecanismo de ação dos bioestimuladores (vídeo)", "https://www.youtube.com/watch?v=g6iTkD26O-I", TipoMaterial.Video);
Material(educacao, "Gestão democrática na escola", "Lei nº 9.394/1996 — LDB", "https://www.planalto.gov.br/ccivil_03/leis/l9394.htm", TipoMaterial.Link);
Material(educacao, "BNCC de forma resumida", "Base Nacional Comum Curricular (MEC)", "http://basenacionalcomum.mec.gov.br/", TipoMaterial.Link);

// ============================================================ PROVAS (uma por módulo, liberadas nesta ordem)

var pPessoa = NovaProva(direito, "Responsabilidade Penal da Pessoa Coletiva", helena, "Avaliação 1 — Fundamentos",
    "Três questões objetivas e uma dissertativa. A dissertativa é corrigida pela professora.", 60, 1,
    Alt("No Brasil, a Constituição admite expressamente a responsabilidade penal da pessoa jurídica em crimes:", 1,
        "Contra a vida", "Ambientais (art. 225, §3º)", "Eleitorais", "Militares"),
    Alt("Os crimes de perigo abstrato, frequentes no Direito Penal Econômico, caracterizam-se por:", 1,
        "Exigir dano efetivo ao bem jurídico", "Dispensar a demonstração de perigo concreto no caso",
        "Serem sempre culposos", "Admitirem apenas pena de multa"),
    Alt("Qual destas NÃO é uma pena aplicável à pessoa jurídica na Lei 9.605/98?", 2,
        "Multa", "Prestação de serviços à comunidade", "Privativa de liberdade", "Suspensão parcial ou total de atividades"),
    Diss("Explique como um programa de compliance efetivo pode influenciar a responsabilização penal de uma empresa.",
        "Relacionar o compliance à prevenção e detecção de ilícitos, à demonstração de diligência da organização, à possível atenuação de sanções e à delimitação das responsabilidades individuais."));

var pTributario = NovaProva(direito, "Crimes Tributários / Infrações Fiscais", helena, "Avaliação 2 — Crimes em Espécie",
    "Três questões objetivas e uma dissertativa.", 60, 1,
    Alt("As três fases clássicas da lavagem de dinheiro são:", 0,
        "Colocação, ocultação (dissimulação) e integração", "Captação, investimento e resgate",
        "Fraude, sonegação e evasão", "Depósito, saque e transferência"),
    Alt("Segundo a Súmula Vinculante 24 do STF, o crime material contra a ordem tributária só se tipifica:", 1,
        "Com a instauração do inquérito", "Após o lançamento definitivo do tributo",
        "Com a simples declaração falsa", "Após o trânsito em julgado da execução fiscal"),
    Alt("O pagamento integral do débito tributário, nos crimes da Lei 8.137/90:", 1,
        "Não produz efeito penal", "Extingue a punibilidade", "Apenas reduz a pena pela metade", "Converte o crime em contravenção"),
    Diss("Diferencie elisão fiscal, evasão fiscal e sonegação, indicando qual delas pode ter relevância penal.",
        "Elisão: planejamento lícito, antes do fato gerador. Evasão: conduta ilícita para reduzir ou suprimir tributo. Sonegação: evasão dolosa por fraude ou omissão, tipificada na Lei 8.137/90 — é a que tem relevância penal."));

var pConsensual = NovaProva(direito, "Justiça Penal Consensual", helena, "Avaliação 3 — Justiça Penal Consensual",
    "Prova objetiva sobre ANPP e colaboração premiada.", 40, 1,
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

var hAnatomia = NovaProva(hof, "Anatomia Aplicada à Face", camila, "Avaliação 1 — Anatomia e Planejamento",
    "Prova objetiva sobre musculatura e vascularização da face.", 30, 1,
    Alt("Qual músculo é o principal responsável pelas rugas horizontais da fronte?", 1,
        "Prócero", "Frontal", "Corrugador do supercílio", "Orbicular do olho"),
    Alt("As linhas verticais entre as sobrancelhas resultam principalmente da ação do:", 0,
        "Corrugador do supercílio", "Masseter", "Zigomático maior", "Platisma"),
    Alt("A artéria facial, de grande importância nos preenchimentos, é ramo da:", 1,
        "Artéria carótida interna", "Artéria carótida externa", "Artéria vertebral", "Artéria subclávia"),
    Alt("A esterilização de instrumentais críticos deve ser feita preferencialmente por:", 1,
        "Álcool 70%", "Autoclave (calor úmido sob pressão)", "Água fervente", "Luz ultravioleta ambiente"));

var hToxina = NovaProva(hof, "Toxina Botulínica Estética e Funcional", camila, "Avaliação 2 — Toxina Botulínica",
    "Três questões objetivas e uma dissertativa.", 45, 1,
    Alt("A toxina botulínica tipo A atua bloqueando a liberação de:", 1,
        "Dopamina", "Acetilcolina na junção neuromuscular", "Serotonina", "Histamina"),
    Alt("O efeito clínico da toxina botulínica dura, em média:", 1,
        "1 semana", "3 a 6 meses", "2 anos", "É permanente"),
    Alt("É contraindicação ao uso de toxina botulínica:", 0,
        "Miastenia gravis", "Pele oleosa", "Idade acima de 30 anos", "Uso de protetor solar"),
    Diss("Descreva os cuidados para evitar a ptose palpebral na aplicação de toxina na glabela.",
        "Respeitar a distância da borda orbitária, usar doses e diluição adequadas, aplicar com a agulha superficial e orientar o paciente a não massagear a região."));

var hPreenchedores = NovaProva(hof, "Preenchedores e Bioestimuladores", camila, "Avaliação 3 — Preenchedores e Bioestimuladores",
    "Prova objetiva.", 30, 1,
    Alt("O ácido poli-L-láctico é classificado como:", 1,
        "Preenchedor permanente de efeito imediato", "Bioestimulador de colágeno", "Toxina", "Anestésico local"),
    Alt("Diante de suspeita de oclusão vascular por ácido hialurônico, a conduta imediata inclui:", 1,
        "Compressa de gelo e aguardar 48 h", "Interromper a aplicação e usar hialuronidase",
        "Aplicar mais produto no local", "Prescrever apenas analgésico"),
    Alt("A hialuronidase é usada para:", 0,
        "Degradar o ácido hialurônico", "Aumentar o volume do preenchimento", "Anestesiar a região", "Estimular colágeno"));

var eGestao = NovaProva(educacao, "Gestão Escolar Democrática", fernanda, "Avaliação 1 — Gestão e Currículo",
    "Três questões objetivas e uma dissertativa.", 45, 1,
    Alt("A gestão democrática do ensino público é princípio previsto:", 1,
        "Apenas no regimento escolar", "Na Constituição Federal (art. 206) e na LDB", "Somente na BNCC", "Em nenhuma norma"),
    Alt("O Projeto Político-Pedagógico (PPP) deve ser:", 1,
        "Elaborado apenas pela direção", "Construído coletivamente pela comunidade escolar",
        "Copiado de outra escola", "Revisado a cada dez anos"),
    Alt("A BNCC define:", 1,
        "O currículo completo de cada escola", "As aprendizagens essenciais que todos os alunos devem desenvolver",
        "Os livros didáticos obrigatórios", "O calendário escolar nacional"),
    Diss("Explique a diferença entre BNCC e currículo e o papel da escola nessa relação.",
        "A BNCC é a referência normativa obrigatória; o currículo é construído pelas redes e escolas a partir dela, considerando o contexto, o PPP e a parte diversificada."));

var eAvaliacao = NovaProva(educacao, "Avaliação da Aprendizagem", fernanda, "Avaliação 2 — Avaliação e Tecnologia",
    "Prova objetiva.", 30, 1,
    Alt("Para Luckesi, a avaliação da aprendizagem deve ser essencialmente:", 1,
        "Classificatória", "Diagnóstica, a serviço da aprendizagem", "Punitiva", "Opcional"),
    Alt("A avaliação formativa caracteriza-se por:", 1,
        "Ocorrer apenas no fim do ano", "Acompanhar o processo e orientar intervenções", "Gerar ranking de alunos", "Dispensar feedback"),
    Alt("O ensino híbrido combina:", 1,
        "Apenas aulas gravadas", "Atividades presenciais e online de forma integrada",
        "Somente aulas expositivas", "Ensino domiciliar e escolar sem planejamento"));

var ePesquisa = NovaProva(educacao, "Metodologia da Pesquisa em Educação", fernanda, "Avaliação 3 — Pesquisa em Educação",
    "Duas questões objetivas e uma dissertativa.", 0, 1,
    Alt("A pesquisa qualitativa em educação busca principalmente:", 1,
        "Medir variáveis com grandes amostras", "Compreender significados e processos no contexto",
        "Substituir a revisão de literatura", "Dispensar a coleta de dados"),
    Alt("O problema de pesquisa deve ser:", 0,
        "Claro, delimitado e possível de investigar", "O mais amplo possível", "Definido só no fim da pesquisa", "Igual ao título do trabalho"),
    Diss("Apresente, em poucas linhas, um problema de pesquisa para a sua futura dissertação.",
        "Problema claro e delimitado, com contexto, sujeitos e relevância para a área."));

// ============================================================ MATRÍCULAS, PROGRESSO E ENTREGAS
// Cada prova fica aberta 30 dias; a seguinte abre quando o prazo da anterior termina (calendário do aluno).
// Gabarito das objetivas, na ordem da prova: C = acertou, E = errou, - = em branco.

// Ana — Direito, há 75 dias: 1ª prova corrigida (com comentário), 2ª aguardando correção, 3ª aberta agora
var mAna = Matricular(ana, direito, 75);
Assistir(mAna, direito, 6, 40);
Entrega(pPessoa, ana, "CCC", Dia(-60, 19), true,
    ("Um programa de compliance efetivo demonstra que a empresa adotou mecanismos de prevenção, detecção e resposta a ilícitos (código de conduta, canal de denúncias, due diligence, treinamentos). Isso pode afastar ou atenuar a culpabilidade organizacional e ajuda a individualizar a responsabilidade dos gestores.", 9,
     "Ótima relação entre compliance e culpabilidade organizacional."));
Entrega(pTributario, ana, "CCE", Dia(-20, 21), false,
    ("Elisão é o planejamento tributário lícito, feito antes do fato gerador. Evasão é a redução ilícita do tributo devido. A sonegação é a evasão dolosa mediante fraude ou omissão e é tipificada na Lei 8.137/90, por isso tem relevância penal.", 0, ""));

// Bruno — HOF, há 40 dias: 1ª prova corrigida (com erros), 2ª aberta agora
var mBruno = Matricular(bruno, hof, 40);
Assistir(mBruno, hof, 3, 25);
Entrega(hAnatomia, bruno, "CEEC", Dia(-30, 20), true);

// Carolina — Educação, há 5 dias: acabou de entrar, 1ª prova aberta
var mCarolina = Matricular(carolina, educacao, 5);
Assistir(mCarolina, educacao, 1, 30);

foreach (var matricula in matriculas)
    Agendar(matricula);

// ============================================================ NOTIFICAÇÕES

Notificar(helena, "Nova entrega", $"Uma entrega da prova \"{pTributario.Titulo}\" aguarda correção.", Dia(-20, 21));
Notificar(ana, "Nota liberada", $"A correção da prova \"{pPessoa.Titulo}\" foi concluída. Veja a nota em Provas › Realizadas.", Dia(-58, 10), true);
Notificar(ana, "Nova prova liberada", $"A prova \"{pConsensual.Titulo}\" já está disponível na aba Provas.", Dia(-15, 14), chave: $"prova-liberada:{pConsensual.Id}");
Notificar(bruno, "Nota liberada", $"A correção da prova \"{hAnatomia.Titulo}\" foi concluída. Veja a nota em Provas › Realizadas.", Dia(-28, 10), true);
Notificar(bruno, "Nova prova liberada", $"A prova \"{hToxina.Titulo}\" já está disponível na aba Provas.", Dia(-10, 14), chave: $"prova-liberada:{hToxina.Id}");
Notificar(carolina, "Bem-vindo(a)!", "Sua matrícula foi liberada. As aulas já estão disponíveis e a primeira prova também: cada prova tem um prazo, e quando ele termina a próxima é liberada.", Dia(-5, 14));
Notificar(carolina, "Nova prova liberada", $"A prova \"{eGestao.Titulo}\" já está disponível na aba Provas.", Dia(-5, 14), chave: $"prova-liberada:{eGestao.Id}");
Notificar(kaique, "Solicitação de matrícula", $"{carolina.Nome} enviou os documentos e o pagamento e aguarda a análise.", Dia(-6, 16), true);

// ============================================================ GRAVAÇÃO

await MongoContext.Usuarios.InsertManyAsync(usuarios);
await MongoContext.Cursos.InsertManyAsync(cursos);
await MongoContext.Matriculas.InsertManyAsync(matriculas);
await MongoContext.Progressos.InsertManyAsync(progressos);
await MongoContext.Materiais.InsertManyAsync(materiais);
await MongoContext.Provas.InsertManyAsync(provas);
await MongoContext.Resultados.InsertManyAsync(resultados);
await MongoContext.Notificacoes.InsertManyAsync(notificacoes);
await MongoContext.InicializarAsync(); // índices (os cursos já existem, então não grava os iniciais)

Console.WriteLine($"""

    Pronto!
      {usuarios.Count(u => u.IsCoordenacao)} coordenador, {usuarios.Count(u => u.Roles == Roles.Professor)} professores, {usuarios.Count(u => u.Roles == Roles.Aluno)} alunos
      {cursos.Count} cursos, {cursos.Sum(c => c.Modulos.Count)} módulos, {cursos.Sum(c => c.TotalMaterias)} matérias, {cursos.Sum(c => c.TotalAulas)} aulas
      {provas.Count} provas, {resultados.Count} entregas ({resultados.Count(r => !r.Corrigida)} aguardando correção)
      {matriculas.Count} matrículas

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

// aluno já liberado: pagou na solicitação de matrícula (o comprovante fica na conta)
Usuario NovoAluno(string nome, string email, DateTime nascimento, int criadoHaDias, FormaPagamento pagamento)
{
    var aluno = NovoUsuario(nome, email, Roles.Aluno, nascimento, criadoHaDias);
    aluno.FormaPagamento = pagamento;
    aluno.PagamentoEm = aluno.CreatedAt.AddDays(-1);
    aluno.TransacaoId = $"PAG-{aluno.PagamentoEm:yyyyMMdd}-{aluno.Id[^6..].ToUpperInvariant()}";
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

static Questao Alt(string enunciado, int correta, params string[] alternativas)
    => new() { Tipo = TipoQuestao.Alternativas, Enunciado = enunciado, Alternativas = alternativas.ToList(), IndiceCorreta = correta };

static Questao Diss(string enunciado, string esperada, double peso = 1)
    => new() { Tipo = TipoQuestao.Dissertativa, Enunciado = enunciado, RespostaEsperada = esperada, Peso = peso };

// Prova publicada, com prazo padrão de 30 dias; a ordem de criação no curso é a ordem de liberação.
Prova NovaProva(Curso curso, string materia, Usuario professor, string titulo, string instrucoes,
    int duracaoMinutos, double peso, params Questao[] questoes)
{
    var criada = curso.CreatedAt.AddDays(1);
    var prova = new Prova
    {
        Id = MongoContext.NovoId(),
        CursoId = curso.Id,
        MateriaId = curso.TodasMaterias.First(m => m.Nome == materia).Id,
        ProfessorId = professor.Id,
        Titulo = titulo,
        Instrucoes = instrucoes,
        NotaMaxima = 10,
        Peso = peso,
        NotaCorte = 6,
        PrazoDias = Prova.PrazoDiasPadrao,
        Ordem = provas.Count(p => p.CursoId == curso.Id) + 1,
        DuracaoMinutos = duracaoMinutos,
        IsPublicada = true,
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

// Calendário de provas da matrícula (mesma regra do ProvaService): a 1ª abre na matrícula, cada uma fica aberta
// pelo seu prazo e a seguinte abre quando ele termina.
void Agendar(Matricula matricula)
{
    var inicio = matricula.DataMatricula;
    foreach (var prova in provas.Where(p => p.CursoId == matricula.CursoId).OrderBy(p => p.Ordem))
    {
        var fim = inicio.AddDays(prova.PrazoDias);
        matricula.AgendaProvas.Add(new ProvaAgendada { ProvaId = prova.Id, LiberadaEm = inicio, PrazoAte = fim });
        inicio = fim;
    }
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

void Notificar(Usuario usuario, string titulo, string mensagem, DateTime quando, bool lida = false, string? chave = null)
    => notificacoes.Add(new Notificacao
    {
        Id = MongoContext.NovoId(),
        UsuarioId = usuario.Id,
        Titulo = titulo,
        Mensagem = mensagem,
        CriadaEm = quando,
        Lida = lida,
        Chave = chave
    });

record AulaDef(string Titulo, string Descricao, string VideoId, string Duracao);
record MateriaDef(string Nome, string Descricao, AulaDef[] Aulas);
record ModuloDef(string Titulo, string Descricao, MateriaDef[] Materias);
