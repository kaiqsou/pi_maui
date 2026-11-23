# DOCUMENTAÇÃO DO PROJETO - EUROPEAN

## 1. INTRODUÇÃO

O presente documento constitui a documentação do Projeto Ágil do Aplicativo European, uma plataforma AVA (Ambiente Virtual de Aprendizagem) de gerenciamento de estudos voltada ao público de mestrado. 

O aplicativo foi desenvolvido como parte do Projeto Integrador do 5º semestre do curso de Tecnologia em Análise e Desenvolvimento de Sistemas. 

## 2. CONTEXTO DE NEGÓCIO

A instituição envolvida é a European, entidade educacional para a qual o aplicativo será desenvolvido. O modelo de negócio está representado conforme descrito abaixo:

| Bloco | Descrição |
|---|---|
| Proposta de Valor | Plataforma AVA completa: compra e acesso a cursos, provas online, aulas gravadas em vídeo, emissão de certificados e comunicação direta entre professores e alunos. |
| Segmentos de Cliente | A instituição de ensino European e os seus alunos e professores (público de mestrado e demais cursos da instituição). |
| Canais | Aplicativo móvel (Android e iOS), acesso via lojas de aplicativos e portal web. |
| Relacionamento | Comunidade acadêmica, suporte interno da instituição e canal direto aluno-professor. |
| Fontes de Receita | Venda/comercialização de cursos, assinaturas e serviços de certificação. |
| Recursos-Chave | Conteúdos educacionais (vídeos), plataforma tecnológica, professores e equipe de desenvolvimento. |
| Atividades-Chave | Desenvolvimento e manutenção do aplicativo, gestão de conteúdos, matrícula e aplicação de provas. |
| Parcerias-Chave | Provedores de hospedagem, processamento de pagamentos e distribuição de vídeo. |
| Estrutura de Custos | Infraestrutura de nuvem, hospedagem de vídeo, licenças de software e equipe. |

## 3. CONTEXTO DO APLICATIVO

O aplicativo European não se trata de um aplicativo isolado. Ele está inserido em um ecossistema educacional da instituição, integrando-se a uma plataforma mais ampla de ensino a distância. Nesse contexto, o aplicativo móvel funciona como o principal ponto de contato entre alunos, professores e a instituição, complementando e sendo complementado pelo portal web e demais sistemas da infraestrutura acadêmica, como gestão de matrículas, banco de conteúdos e serviços de pagamento.

## 4. JUSTIFICATIVA E MOTIVAÇÃO

A criação do European atende a uma necessidade real da instituição: centralizar e organizar o processo de ensino-aprendizagem. Atualmente, professores e alunos carecem de uma ferramenta unificada que permita gerenciar cursos, assistir aulas gravadas, realizar provas, obter certificados e se comunicar diretamente. O aplicativo resolve esse problema ao reunir, em um único ambiente, as principais funcionalidades de um AVA, melhorando a organização e facilitando o acompanhamento do rendimento acadêmico.

## 5. VISÃO DO PRODUTO

O European é uma plataforma de gerenciamento de estudos para mestrado. Quando integralmente pronto e integrado, o aplicativo será capaz de fornecer vídeos dos cursos aos alunos, realizar provas online, emitir certificados e gerenciar o processo de matrícula. Além disso, os professores também terão acesso à plataforma, tanto para o gerenciamento de suas turmas quanto para auxiliar os alunos no esclarecimento de dúvidas, garantindo um ciclo completo de ensino-aprendizagem.

## 6. OBJETIVOS DO PROJETO

O objetivo do desenvolvimento do European é oferecer uma ferramenta prática que gere valor tanto para professores quanto para alunos. Por meio do aplicativo, é possível assistir aos vídeos dos cursos, realizar provas, obter acompanhamento de progresso e se comunicar diretamente com os professores. Trata-se de um AVA cuja finalidade é organizar e potencializar a experiência educacional da instituição.

## 7. EQUIPE DE DESENVOLVIMENTO

| Papel na Equipe | Nome |
|---|---|
| Product Owner (PO) | Gabriel Magina Coutinho |
| Scrum Master | Kaique Onencio de Souza |
| Desenvolvedora | Marina Frankin |

Repositório do projeto: [https://github.com/Gabriel-M-Coutinho/avaeuropean](https://github.com/Gabriel-M-Coutinho/avaeuropean)

## 8. ARQUITETURA DA SOLUÇÃO

A solução adota uma arquitetura em camadas, separando claramente o front-end, o back-end e a camada de dados, com integração a serviços externos via API REST. 

| Componente | Tecnologia | Descrição |
|---|---|---|
| Front-end (Mobile) | **.NET MAUI** | Interface do aplicativo, desenvolvida com .NET MAUI para Android e iOS, a partir de um único código-base. Responsável pela experiência do usuário (vídeos, provas, matrícula, comunicação e acompanhamento de progresso). |
| Back-end / API | **Node.js (Express)** | API REST que concentra o acesso ao banco de dados: usuários, cursos, matrículas, progresso, provas, resultados, certificados, solicitações de matrícula, notificações e arquivos. O aplicativo não acessa mais o banco diretamente; toda leitura e gravação passa pela API via HTTP. |
| Banco de Dados | **MongoDB** (com GridFS) | Banco principal para todas as coleções. Os arquivos enviados (documentos da matrícula e materiais das aulas) ficam no GridFS do próprio MongoDB. |
| Integrações Externas | PandaVideo, Cloudinary e Ommie | Distribuição e streaming de vídeo (PandaVideo), armazenamento de mídias (Cloudinary) e processamento de pagamentos (Ommie). |

### 8.1 Diagrama Conceitual da Arquitetura

O front-end, desenvolvido em **.NET MAUI** (multi-plataforma, com um único código-base para Android e iOS), comunica-se via HTTP/REST (JSON) com o back-end em **Node.js (Express)**. O back-end é o único componente com acesso à camada de dados (**MongoDB**, incluindo os arquivos no GridFS) e será o ponto de orquestração das integrações externas: PandaVideo e Cloudinary (vídeos e mídias), e Ommie (pagamentos).

```
[ App .NET MAUI ]  --HTTP/JSON-->  [ API Node.js + Express ]  --driver-->  [ MongoDB + GridFS ]
 (Android / iOS)                     (European-API)                         (banco "european")
```

### 8.2 Decisões Arquiteturais - Front-end e Back-end

A decisão de arquitetura para o front-end foi revista em relação ao plano inicial. **Será utilizado .NET MAUI** para o desenvolvimento da aplicação móvel. O back-end também foi revisto: em vez de Java (Spring Boot), **adotou-se Node.js com Express**, mantendo o MongoDB como banco de dados.

| Item | Decisão | Justificativa |
|---|---|---|
| Tipo de Aplicativo | Híbrido / Multi-plataforma (.NET MAUI) | Permite desenvolver para Android e iOS a partir de um único código-base, reduzindo custos, tempo de desenvolvimento e facilitando a manutenção. |
| Plataformas | Android e iOS (desde o início) | Abordagem multi-plataforma com .NET MAUI atende simultaneamente às duas principais plataformas móveis do público-alvo da European. |
| Back-end | Node.js (Express) | Leve e rápido de configurar; trabalha nativamente com JSON, o mesmo formato dos documentos do MongoDB; o driver oficial do MongoDB para Node.js dá acesso direto às coleções e ao GridFS. Sobe junto com o banco por Docker Compose. |
| Banco de Dados | MongoDB | Preferência da equipe, devido ao conhecimento prévio do banco. Modelo de documentos adequado à estrutura do curso (Curso > Módulo > Matéria > Aula em um único documento). | 

## 9. BACKLOG DO PRODUTO

O Backlog do Produto foi estruturado e atualizado, demonstrando o que já foi realizado, o que está em execução e o que está planejado. 

| ID | Épico/Funcionalidade | Status | Prioridade | Observações |
|---|---|---|---|---|
| 1 | Autenticação e gerenciamento de usuários (Alunos, Professores e Admin) | Realizado | Alta | Login, cadastro, perfis e controle de acesso via JWT. |
| 2 | Catálogo de cursos (Criação, edição, publicação e visualização) | Realizado | Alta | Estrutura Curso > Módulo > Matéria > Aula. |
| 3 | Matrículas de alunos em cursos | Realizado | Alta | Controle de matrículas, status e snapshots de curso/aluno. |
| 4 | Reprodução de aulas em vídeo (Player) | Em Execução | Alta | Integração com PandaVideo para streaming e controle de progresso de visualização. |
| 5 | Acompanhamento de progresso do aluno | Em Execução | Alta | Progresso por módulo, matéria, aula e percentual de conclusão. |
| 6 | Sistema de avaliações e provas | Planejado | Alta | Avaliações por matéria com componentes de nota, pesos, nota de corte e cálculo de média. |
| 7 | Emissão de certificados | Planejado | Alta | Emissão automática conforme conclusão e aprovação no curso. |
| 8 | Comunicação Aluno-Professor (Dúvidas/Tira-dúvidas) | Planejado | Média | Canal direto para esclarecimento de dúvidas. |
| 9 | Gerenciamento de pagamentos (Ommie) | Planejado | Alta | Integração para compra de cursos e assinaturas. |
| 10 | Armazenamento e gestão de materiais complementares | Planejado | Média | Upload e disponibilização de materiais via Cloudinary. |
| 11 | API REST do back-end (Node.js + Express) | Realizado | Alta | O aplicativo deixou de acessar o MongoDB diretamente e passou a consumir a API via HTTP. Arquivos no GridFS. |

## 10. JORNADA DO USUÁRIO (PRINCIPAL FUNCIONALIDADE)

A principal funcionalidade do aplicativo European é o acesso e acompanhamento de cursos pelo aluno, garantindo uma experiência de aprendizagem contínua.

### Jornada do Usuário - Aluno acessando um curso

| Etapa | Ação do Usuário | Resposta do Sistema | Objetivo |
|---|---|---|---|
| 1. Autenticação | Realiza login com suas credenciais. | Sistema valida e autentica o usuário, redirecionando para a tela inicial. | Garantir acesso seguro ao ambiente. |
| 2. Exploração do Catálogo | Navega pelos cursos disponíveis na plataforma. | Exibe catálogo com informações do curso, tipo, categoria e disponibilidade. | Encontrar curso de interesse. |
| 3. Matrícula | Seleciona um curso e realiza a matrícula (ou compra, conforme modelo comercial). | Sistema cria vínculo aluno-curso, gera snapshots e estrutura inicial de progresso. | Vincular aluno ao curso. |
| 4. Acesso ao Curso | Acessa "Meus Cursos" e seleciona o curso matriculado. | Exibe estrutura do curso (Módulos > Matérias > Aulas), com indicador de progresso. | Visualizar trilha de aprendizagem. |
| 5. Estudo das Aulas | Seleciona uma aula e assiste ao vídeo. | Player exibe vídeo via PandaVideo e registra percentual assistido (progresso). | Consumir conteúdo didático. |
| 6. Acompanhamento de Progresso | Visualiza progresso por matéria/módulo. | Sistema atualiza status de conclusão conforme aulas assistidas e critérios definidos. | Monitorar evolução no curso. |
| 7. Realização de Avaliações | Ao atingir requisitos (frequência/progresso), acessa avaliações da matéria. | Exibe componentes de avaliação, permite realizar prova/trabalho e registra notas. | Verificar aprendizado. |
| 8. Conclusão e Certificado | Conclui todas as matérias com aprovação (média >= nota de corte). | Sistema atualiza status do curso e disponibiliza certificado (quando aplicável). | Finalizar curso com comprovação. |

## 11. TRATAMENTO DE DADOS E LEGISLAÇÃO (LGPD/RGPD)

### 11.1 Considerações Legais

Conforme orientações do projeto, o aplicativo European é direcionado para fora do Brasil. Dessa forma, não se aplica exclusivamente a Lei Geral de Proteção de Dados (LGPD - Lei 13.709/2018), devendo ser considerada, principalmente, a **Regulamentação Geral sobre a Proteção de Dados (RGPD - General Data Protection Regulation - UE 2016/679)**, aplicável a dados de cidadãos da União Europeia.

Caso haja processamento de dados de usuários brasileiros, também serão observados os requisitos da LGPD. Portanto, o projeto adota uma abordagem de conformidade ampla (compliance), atendendo aos princípios fundamentais de proteção de dados.

### 11.2 Dados Coletados

Os dados coletados e processados pela plataforma são estritamente necessários para o funcionamento do AVA:

- Dados de identificação: Nome completo, e-mail, CPF (quando aplicável) e dados de perfil.
- Dados de autenticação: Credenciais criptografadas e tokens de acesso (JWT).
- Dados acadêmicos: Cursos matriculados, histórico de progresso, notas, frequência/assistência a vídeos, provas realizadas.
- Dados de pagamento: Informações necessárias para processamento via Ommie (não são armazenados dados sensíveis de cartão diretamente no sistema; utiliza-se gateway de pagamento).
- Dados de uso: Logs técnicos para segurança, auditoria e melhoria da plataforma.

### 11.3 Princípios de Proteção de Dados

O tratamento de dados seguirá os princípios da RGPD e LGPD:

- **Finalidade específica**: Dados coletados apenas para fins educacionais e operacionais da plataforma.
- **Necessidade e minimização**: Coleta apenas o estritamente necessário.
- **Transparência**: Usuários serão informados sobre coleta, uso e compartilhamento de seus dados.
- **Segurança e integridade**: Dados protegidos por criptografia, controle de acesso e boas práticas de segurança.
- **Limitação de armazenamento**: Dados mantidos pelo tempo necessário à finalidade ou conforme exigências legais.
- **Direitos dos titulares**: Garantidos direitos de acesso, correção, exclusão, portabilidade, oposição e revogação de consentimento (quando aplicável).
- **Responsabilização**: Registro de logs de auditoria (AuditMetadata) para rastreabilidade de operações sensíveis.

### 11.4 Medidas de Segurança e Conformidade

- **Autenticação e Autorização**: Implementadas com JWT, tokens de refresh, blacklist de tokens e controle por perfis (Roles).
- **Auditoria**: Uso de metadados de auditoria (`createdAt`, `updatedAt`, rastreamento de alterações) para garantir rastreabilidade.
- **Separação de dados**: Modelagem com snapshots (em matrículas) garante integridade histórica sem expor alterações indevidas.
- **Integrações seguras**: Comunicação com provedores externos (PandaVideo, Cloudinary, Ommie) via APIs autenticadas e com escopo restrito.
- **Criptografia e boas práticas**: Senhas armazenadas de forma segura, tráfego via HTTPS e princípios de menor privilégio aplicados.

## 12. ORGANIZAÇÃO E ESTRUTURA DO PROJETO

### 12.1 Back-end (European-API - Node.js/Express)

O back-end é uma API REST em **Node.js com Express**, na pasta `European-API` do repositório. Ela usa o driver oficial do **MongoDB** e o mesmo banco (`european`) e coleções já utilizados pelo projeto, de modo que os dados de demonstração gerados pelo projeto `Seed` continuam válidos.

**Estrutura principal:**

```
European-API/
├── src/server.js         → Rotas REST, agrupadas por domínio
├── src/db.js             → Conexão com o MongoDB, índices e GridFS (arquivos)
├── src/json.js           → Conversão entre os documentos do banco e o JSON da API
├── Dockerfile
├── docker-compose.yml    → Sobe o MongoDB e a API juntos
└── rodar-sem-docker.ps1  → Sobe o MongoDB local e a API (opção -Seed para dados de demonstração)
```

**Domínios (grupos de rotas):**

| Domínio | Rotas | Responsabilidade |
|---|---|---|
| Usuários | `/usuarios` | Cadastro, consulta por e-mail/papel, coordenação e troca de senha. |
| Cursos | `/cursos` | Catálogo e estrutura Curso > Módulo > Matéria > Aula; a exclusão de um curso remove em cascata matrículas, progresso, provas, resultados e materiais. |
| Matrículas e progresso | `/matriculas`, `/progressos` | Vínculo aluno-curso, calendário de provas e progresso por aula. |
| Avaliações | `/provas`, `/resultados` | Provas, ordem de liberação, entregas e notas. |
| Certificados | `/certificados` | Certificado final do curso, com código de verificação. |
| Solicitações | `/solicitacoes` | Pedidos de matrícula (documentos, pagamento, análise da coordenação). |
| Notificações | `/notificacoes` | Avisos por usuário, não lidas e marcação de leitura. |
| Arquivos | `/arquivos`, `/materiais` | Envio e download de arquivos no GridFS (documentos e materiais das aulas). |

A lista completa de rotas está no `European-API/README.md`. As regras de negócio (cálculo de notas, liberação de provas, emissão de certificados etc.) permanecem nos serviços do aplicativo; a API é responsável pela persistência e consulta dos dados.

**Execução:**

```powershell
cd European-API
docker compose up -d --build        # MongoDB + API em http://localhost:3000
# ou, sem Docker, com o MongoDB instalado:
.\rodar-sem-docker.ps1 -Seed        # -Seed grava os dados de demonstração (DADOS.txt)
```

### 12.2 Front-end (avaeuropean - .NET MAUI)

O front-end será desenvolvido em **.NET MAUI** (Multi-platform App UI), visando a criação de uma única base de código para Android e iOS. Essa mudança está prevista em relação à proposta inicial, mantendo-se alinhada com os objetivos de eficiência, manutenção e alcance multiplataforma.

A aplicação segue a estrutura de um projeto MAUI, organizada para separação de responsabilidades entre camadas de interface, serviços, modelos e navegação, garantindo consistência com os padrões de desenvolvimento modernos e com a arquitetura da API REST.

O acesso a dados fica na pasta `European/Data`: os repositórios (`UsuarioRepositorio`, `CursoRepositorio`, `MatriculaRepositorio` etc.) chamam a API por HTTP através do cliente `Api.cs`. O endereço padrão é `http://localhost:3000` (no emulador Android, `http://10.0.2.2:3000`) e pode ser alterado pela variável de ambiente `EUROPEAN_API`.

## 13. PLANEJAMENTO E GERENCIAMENTO ÁGIL

O projeto segue os princípios do **desenvolvimento ágil de software**, utilizando práticas inspiradas no framework **Scrum**.

- **Product Owner (PO)**: Responsável pelo Backlog do Produto, priorizações e alinhamento com os requisitos da instituição.
- **Scrum Master**: Responsável por facilitar o processo ágil, remover impedimentos e garantir a aderência às práticas.
- **Equipe de Desenvolvimento**: Responsável pela implementação, testes e entrega das funcionalidades.

O desenvolvimento está organizado em **Sprints**, com entregas incrementais e revisões periódicas, permitindo o refinamento contínuo do produto conforme o avanço do projeto.

## 14. CONSIDERAÇÕES FINAIS

O presente documento consolida a situação atual do Projeto European, contemplando todos os refinamentos realizados até o momento. Nele estão claramente definidos: o objetivo do aplicativo, as funcionalidades a serem desenvolvidas no contexto do Projeto Integrador do 5º semestre, a arquitetura da solução (com a definição de **.NET MAUI** para o front-end, **Node.js com Express** para o back-end e **MongoDB** como banco de dados), o Backlog atualizado com status (realizado/em execução/planejado), a Jornada do Usuário para a funcionalidade principal e o tratamento de dados com implicações legais (considerando LGPD e RGPD).

O projeto mantém-se alinhado à visão de oferecer uma plataforma AVA completa, prática e organizada, que agregue valor real aos alunos, professores e à instituição European.

---

**Versão:** 1.1 (back-end em Node.js + Express)  
**Data:** 23/11/2026  
**Autores:** Marina Frankin, Gabriel Magina Coutinho e Kaique Onencio de Souza
