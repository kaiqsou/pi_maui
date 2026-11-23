# European — API

API em **Express + MongoDB** para o app MAUI **European** (repositório `pi_maui`).
Antes o app abria conexão direto com o MongoDB; agora ele busca e grava tudo por **HTTP** nesta API.

- Usa o mesmo banco (`european`) e as mesmas coleções de antes, então os dados do **Seed** continuam valendo.
- As regras de negócio continuam no app (ViewModels e Services). A API só guarda e devolve os dados:
  cada rota corresponde a um método dos repositórios em `European/Data`.
- Arquivos (documentos da matrícula e materiais das aulas) ficam no GridFS, bucket `arquivos`, como antes.

## Subir

### Com Docker
```powershell
docker compose up -d --build
```
MongoDB na porta 27017 e API em http://localhost:3000.

### Sem Docker (MongoDB instalado na máquina)
Na pasta `European-API`:
```powershell
.\rodar-sem-docker.ps1 -Seed
```
- `-Seed` apaga o banco e grava os dados de demonstração do `DADOS.txt` (todas as senhas: `Senha123`).
  Sem `-Seed`, só sobe o MongoDB e a API com os dados que já estão lá.
- Se o MongoDB já estiver rodando na porta 27017, o script usa ele. Senão, procura o `mongod` no PATH ou em
  `C:\Program Files\MongoDB\Server`, e guarda os dados em `European-API\dados-mongo`.
- Outro `mongod` ou outra pasta de dados: `.\rodar-sem-docker.ps1 -Mongod "C:\...\mongod.exe" -Dados "C:\...\dados"`.

Ou na mão: `npm install` e `npm start` (com o MongoDB rodando).

Variáveis: `MONGO_URL` (padrão `mongodb://localhost:27017`), `DB_NAME` (padrão `european`), `PORT` (padrão `3000`).

## App

O app chama `http://localhost:3000` (no emulador Android, `http://10.0.2.2:3000`).
Para outro endereço (ex.: celular físico apontando para o IP do PC), defina a variável de ambiente
`EUROPEAN_API` antes de abrir o app. No Android o HTTP sem TLS já está liberado no `AndroidManifest.xml`.

O que mudou no `pi_maui`:

- `European/Data/Api.cs` — cliente HTTP (JSON igual ao que era gravado no banco: nomes das propriedades,
  enums como texto, datas no horário local).
- `European/Data/ApiContext.cs` — substitui o `MongoContext` (inicialização e preenchimento de Id/datas).
- `European/Data/Ids.cs` — gera ids no formato ObjectId sem precisar do driver do MongoDB.
- `European/Data/*Repositorio.cs` — mesmos métodos de antes, agora chamando a API.
- O pacote `MongoDB.Driver` saiu do app. O `MongoContext.cs` e o `ColecaoExtensions.cs` foram para
  `Seed/Data`, porque o Seed continua gravando direto no banco.

## Rotas

| Recurso | Rotas |
|---|---|
| Inicialização | `POST /inicializar` (índices + cursos iniciais se o banco estiver vazio), `GET /manutencao/senhas-texto`, `PATCH /manutencao/senhas` |
| Usuários | `GET /usuarios?role=`, `GET /usuarios/existe`, `GET /usuarios/coordenacao`, `GET /usuarios/email/:email`, `GET /usuarios/:id`, `POST /usuarios/ids`, `POST /usuarios` (409 se e-mail repetido), `PUT /usuarios/:id`, `PATCH /usuarios/:id/senha` |
| Cursos | `GET /cursos`, `GET /cursos/professor/:id`, `GET /cursos/:id`, `POST /cursos/ids`, `PUT /cursos/:id` (cria ou substitui), `PATCH /cursos/:id/aulas/:aulaId/duracao`, `DELETE /cursos/:id` (apaga matrículas, progresso, provas, resultados e materiais do curso) |
| Matrículas | `GET /matriculas/aluno/:id`, `GET /matriculas/aluno/:alunoId/curso/:cursoId`, `POST /matriculas/alunos`, `GET /matriculas/curso/:id`, `GET /matriculas/ativas-por-curso`, `POST /matriculas`, `PUT /matriculas/:id`, `PATCH /matriculas/:id/agenda` |
| Progresso | `GET /progressos/matricula/:id`, `GET /progressos/matricula/:mid/aula/:aid`, `PUT /progressos/:id` |
| Materiais | `GET /materiais/aula/:id`, `POST /materiais`, `DELETE /materiais/:id`, `POST /materiais/excluir-aulas`, `DELETE /materiais/curso/:id` |
| Provas | `GET /provas/:id`, `POST /provas/ids`, `GET /provas/professor/:id`, `POST /provas/cursos`, `PUT /provas/:id`, `PUT /provas/ordem`, `DELETE /provas/:id`, `DELETE /provas/materia/:id` |
| Resultados | `GET /resultados/:id`, `GET /resultados/aluno/:id`, `GET /resultados/prova/:id`, `POST /resultados/provas`, `GET /resultados/existe?alunoId=&provaId=`, `POST /resultados`, `PUT /resultados/:id` |
| Certificados | `GET /certificados/aluno/:id`, `GET /certificados/matricula/:id`, `POST /certificados` |
| Solicitações | `GET /solicitacoes`, `GET /solicitacoes/:id`, `GET /solicitacoes/email/:email`, `POST /solicitacoes` (409 se o e-mail já tem conta ou pedido), `PUT /solicitacoes/:id`, `PATCH /solicitacoes/:id/senha`, `DELETE /solicitacoes/:id` |
| Notificações | `GET /notificacoes/usuario/:id`, `GET /notificacoes/usuario/:id/nao-lidas`, `POST /notificacoes/lote`, `POST /notificacoes/usuario/:id/chaves`, `POST /notificacoes/usuario/:id/marcar-lidas` |
| Arquivos | `POST /arquivos?nome=...` (corpo = bytes), `GET /arquivos/:id`, `POST /arquivos/excluir` |

No JSON, o `_id` do banco aparece como `Id` (inclusive dentro de módulos, matérias, aulas e questões).
Erros voltam como `{ "erro": "mensagem" }`.

## Limitação conhecida

A conferência da senha (Argon2id) continua no app, como era antes: a rota de usuário devolve o `SenhaHash`
para o app comparar. Para produção, o certo seria a API fazer o login e devolver um token, sem expor o hash.
