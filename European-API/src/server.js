// API do European (pi_maui). Cada rota corresponde a um método dos repositórios do app (European/Data),
// que antes falavam direto com o MongoDB. As regras de negócio continuam no app; aqui ficam só os dados.
const express = require('express');
const cors = require('cors');
const { ObjectId } = require('mongodb');
const { conectar, col, arquivos, criarIndices } = require('./db');
const { paraJson, deJson, ehTextoPuro } = require('./json');

const app = express();
app.use(cors());
app.use(express.json({ limit: '25mb' }));

const PORT = process.env.PORT || 3000;

// ---------- utilidades ----------
const json = (res, dados) => res.json(paraJson(dados));
const naoEncontrado = (res) => res.status(404).json({ erro: 'Não encontrado.' });
const ids = (req) => (Array.isArray(req.body?.ids) ? [...new Set(req.body.ids.map(String))] : []);
const emailNormalizado = (e) => String(e ?? '').trim().toLowerCase();
const ehDuplicado = (e) => e?.code === 11000;

async function porId(colecao, id) {
  return col(colecao).findOne({ _id: id });
}

async function porIds(colecao, lista) {
  return lista.length ? col(colecao).find({ _id: { $in: lista } }).toArray() : [];
}

// Substitui o documento inteiro. upsert=true cria se não existir (SalvarAsync do app).
async function substituir(colecao, id, corpo, upsert) {
  const doc = deJson(corpo);
  doc._id = id;
  return col(colecao).replaceOne({ _id: id }, doc, { upsert });
}

// Insere; responde 409 se violar um índice único (ex.: e-mail já cadastrado).
async function inserir(res, colecao, corpo) {
  const doc = deJson(corpo);
  if (!doc._id) doc._id = new ObjectId().toHexString();
  try {
    await col(colecao).insertOne(doc);
    res.status(201);
    return json(res, doc);
  } catch (e) {
    if (ehDuplicado(e)) return res.status(409).json({ erro: 'Registro duplicado.' });
    throw e;
  }
}

// Rotas comuns: buscar por id, por lista de ids e substituir.
function rotasComuns(prefixo, colecao, { upsert = false } = {}) {
  app.post(`/${prefixo}/ids`, async (req, res) => json(res, await porIds(colecao, ids(req))));
  app.get(`/${prefixo}/:id`, async (req, res) => {
    const doc = await porId(colecao, req.params.id);
    return doc ? json(res, doc) : naoEncontrado(res);
  });
  app.put(`/${prefixo}/:id`, async (req, res) => {
    await substituir(colecao, req.params.id, req.body, upsert);
    res.status(204).end();
  });
}

async function excluirArquivos(lista) {
  for (const id of lista) {
    if (!id || !ObjectId.isValid(id)) continue;
    try {
      await arquivos().delete(new ObjectId(id));
    } catch {
      // já removido
    }
  }
}

async function excluirMateriais(filtro) {
  const materiais = await col('materiais').find(filtro).toArray();
  await excluirArquivos(materiais.map((m) => m.ArquivoId));
  await col('materiais').deleteMany({ _id: { $in: materiais.map((m) => m._id) } });
}

// ======================================================= status / inicialização
app.get('/', (req, res) => res.json({ ok: true, api: 'European' }));

// Cria os índices e, se não houver nenhum curso, grava os cursos iniciais enviados pelo app.
app.post('/inicializar', async (req, res) => {
  await criarIndices();
  let cursosCriados = 0;
  const iniciais = req.body?.CursosIniciais;
  if (Array.isArray(iniciais) && iniciais.length && (await col('cursos').estimatedDocumentCount()) === 0) {
    await col('cursos').insertMany(iniciais.map(deJson));
    cursosCriados = iniciais.length;
  }
  res.json({ ok: true, cursosCriados });
});

// Senhas gravadas em texto por versões antigas: o app gera o hash (Argon2id) e devolve.
app.get('/manutencao/senhas-texto', async (req, res) => {
  const pendentes = [];
  for (const colecao of ['usuarios', 'solicitacoes']) {
    const docs = await col(colecao).find({}, { projection: { SenhaHash: 1 } }).toArray();
    for (const d of docs) if (ehTextoPuro(d.SenhaHash)) pendentes.push({ Colecao: colecao, Id: d._id, Senha: d.SenhaHash });
  }
  res.json(pendentes);
});

app.patch('/manutencao/senhas', async (req, res) => {
  const { Colecao, Id, SenhaHash } = req.body ?? {};
  if (!['usuarios', 'solicitacoes'].includes(Colecao) || !Id || !SenhaHash)
    return res.status(400).json({ erro: 'Dados inválidos.' });
  await col(Colecao).updateOne({ _id: Id }, { $set: { SenhaHash } });
  res.status(204).end();
});

// ======================================================= usuários
app.get('/usuarios/existe', async (req, res) => res.json({ existe: !!(await col('usuarios').findOne({}, { projection: { _id: 1 } })) }));

app.get('/usuarios/coordenacao', async (req, res) =>
  json(res, await col('usuarios').find({ Roles: { $in: ['Coordenador', 'Administrador'] } }).toArray()));

app.get('/usuarios/email/:email', async (req, res) => {
  const u = await col('usuarios').findOne({ Email: emailNormalizado(req.params.email) });
  return u ? json(res, u) : naoEncontrado(res);
});

// ?role=Aluno|Professor|Coordenador|Administrador
app.get('/usuarios', async (req, res) =>
  json(res, await col('usuarios').find({ Roles: String(req.query.role ?? '') }).sort({ Nome: 1 }).toArray()));

app.post('/usuarios', (req, res) => inserir(res, 'usuarios', req.body));

app.patch('/usuarios/:id/senha', async (req, res) => {
  const { SenhaHash, SenhaProvisoria } = req.body ?? {};
  await col('usuarios').updateOne(
    { _id: req.params.id },
    { $set: { SenhaHash, SenhaProvisoria: !!SenhaProvisoria, UpdatedAt: new Date() } });
  res.status(204).end();
});

rotasComuns('usuarios', 'usuarios');

// ======================================================= cursos
app.get('/cursos', async (req, res) => json(res, await col('cursos').find().toArray()));

app.get('/cursos/professor/:id', async (req, res) =>
  json(res, await col('cursos').find({ ProfessorIds: req.params.id }).toArray()));

// Duração real do vídeo detectada pelo player (não reescreve o curso inteiro).
app.patch('/cursos/:id/aulas/:aulaId/duracao', async (req, res) => {
  await col('cursos').updateOne(
    { _id: req.params.id },
    { $set: { 'Modulos.$[].Materias.$[].Aulas.$[a].DuracaoSegundos': Number(req.body?.segundos) || 0 } },
    { arrayFilters: [{ 'a._id': req.params.aulaId }] });
  res.status(204).end();
});

// Exclui o curso e tudo que depende dele (certificados já emitidos ficam).
app.delete('/cursos/:id', async (req, res) => {
  const cursoId = req.params.id;
  const matriculaIds = (await col('matriculas').find({ CursoId: cursoId }, { projection: { _id: 1 } }).toArray()).map((m) => m._id);
  const provaIds = (await col('provas').find({ CursoId: cursoId }, { projection: { _id: 1 } }).toArray()).map((p) => p._id);

  await col('progressos').deleteMany({ MatriculaId: { $in: matriculaIds } });
  await col('matriculas').deleteMany({ CursoId: cursoId });
  await col('resultados').deleteMany({ ProvaId: { $in: provaIds } });
  await col('provas').deleteMany({ CursoId: cursoId });
  await excluirMateriais({ CursoId: cursoId });
  await col('cursos').deleteOne({ _id: cursoId });
  res.status(204).end();
});

rotasComuns('cursos', 'cursos', { upsert: true });

// ======================================================= matrículas
app.get('/matriculas/aluno/:alunoId/curso/:cursoId', async (req, res) => {
  const m = await col('matriculas').findOne({ AlunoId: req.params.alunoId, CursoId: req.params.cursoId });
  return m ? json(res, m) : naoEncontrado(res);
});

app.get('/matriculas/aluno/:id', async (req, res) =>
  json(res, await col('matriculas').find({ AlunoId: req.params.id }).sort({ DataMatricula: -1 }).toArray()));

app.post('/matriculas/alunos', async (req, res) => {
  const lista = ids(req);
  json(res, lista.length ? await col('matriculas').find({ AlunoId: { $in: lista } }).toArray() : []);
});

app.get('/matriculas/curso/:id', async (req, res) =>
  json(res, await col('matriculas').find({ CursoId: req.params.id }).toArray()));

// { cursoId: quantidade de matrículas ativas }
app.get('/matriculas/ativas-por-curso', async (req, res) => {
  const grupos = await col('matriculas').aggregate([
    { $match: { Status: 'Ativa' } },
    { $group: { _id: '$CursoId', total: { $sum: 1 } } },
  ]).toArray();
  res.json(Object.fromEntries(grupos.map((g) => [g._id, g.total])));
});

app.post('/matriculas', (req, res) => inserir(res, 'matriculas', req.body));

// Só o calendário de provas (não sobrescreve status/progresso).
app.patch('/matriculas/:id/agenda', async (req, res) => {
  await col('matriculas').updateOne({ _id: req.params.id }, { $set: { AgendaProvas: deJson(req.body?.AgendaProvas ?? []) } });
  res.status(204).end();
});

rotasComuns('matriculas', 'matriculas');

// ======================================================= progresso das aulas
app.get('/progressos/matricula/:mid/aula/:aid', async (req, res) => {
  const p = await col('progressos').findOne({ MatriculaId: req.params.mid, AulaId: req.params.aid });
  return p ? json(res, p) : naoEncontrado(res);
});

app.get('/progressos/matricula/:id', async (req, res) =>
  json(res, await col('progressos').find({ MatriculaId: req.params.id }).toArray()));

rotasComuns('progressos', 'progressos', { upsert: true });

// ======================================================= materiais complementares
app.get('/materiais/aula/:id', async (req, res) =>
  json(res, await col('materiais').find({ AulaId: req.params.id }).sort({ Ordem: 1 }).toArray()));

// Insere no fim da lista da aula (Ordem = quantidade + 1). O arquivo, se houver, já foi enviado em POST /arquivos.
app.post('/materiais', async (req, res) => {
  const doc = deJson(req.body);
  doc._id = new ObjectId().toHexString();
  doc.Ordem = (await col('materiais').countDocuments({ AulaId: doc.AulaId })) + 1;
  await col('materiais').insertOne(doc);
  res.status(201);
  json(res, doc);
});

app.post('/materiais/excluir-aulas', async (req, res) => {
  const lista = ids(req);
  if (lista.length) await excluirMateriais({ AulaId: { $in: lista } });
  res.status(204).end();
});

app.delete('/materiais/curso/:id', async (req, res) => {
  await excluirMateriais({ CursoId: req.params.id });
  res.status(204).end();
});

app.delete('/materiais/:id', async (req, res) => {
  await excluirMateriais({ _id: req.params.id });
  res.status(204).end();
});

// ======================================================= provas
app.get('/provas/professor/:id', async (req, res) =>
  json(res, await col('provas').find({ ProfessorId: req.params.id }).sort({ UpdatedAt: -1 }).toArray()));

app.post('/provas/cursos', async (req, res) => {
  const lista = ids(req);
  json(res, lista.length ? await col('provas').find({ CursoId: { $in: lista } }).toArray() : []);
});

// Nova ordem das provas de um curso: { ids: [na ordem] } -> Ordem 1, 2, 3...
app.put('/provas/ordem', async (req, res) => {
  const lista = ids(req);
  if (lista.length)
    await col('provas').bulkWrite(lista.map((id, i) => ({ updateOne: { filter: { _id: id }, update: { $set: { Ordem: i + 1 } } } })));
  res.status(204).end();
});

app.delete('/provas/materia/:id', async (req, res) => {
  const provaIds = (await col('provas').find({ MateriaId: req.params.id }, { projection: { _id: 1 } }).toArray()).map((p) => p._id);
  await col('resultados').deleteMany({ ProvaId: { $in: provaIds } });
  await col('provas').deleteMany({ MateriaId: req.params.id });
  res.status(204).end();
});

app.delete('/provas/:id', async (req, res) => {
  await col('resultados').deleteMany({ ProvaId: req.params.id });
  await col('provas').deleteOne({ _id: req.params.id });
  res.status(204).end();
});

rotasComuns('provas', 'provas', { upsert: true });

// ======================================================= resultados das provas
app.get('/resultados/existe', async (req, res) =>
  res.json({ existe: !!(await col('resultados').findOne({ AlunoId: String(req.query.alunoId), ProvaId: String(req.query.provaId) })) }));

app.get('/resultados/aluno/:id', async (req, res) =>
  json(res, await col('resultados').find({ AlunoId: req.params.id }).sort({ DataRealizacao: -1 }).toArray()));

app.get('/resultados/prova/:id', async (req, res) =>
  json(res, await col('resultados').find({ ProvaId: req.params.id }).sort({ NotaObtida: -1 }).toArray()));

app.post('/resultados/provas', async (req, res) => {
  const lista = ids(req);
  json(res, lista.length ? await col('resultados').find({ ProvaId: { $in: lista } }).toArray() : []);
});

app.post('/resultados', (req, res) => inserir(res, 'resultados', req.body));

rotasComuns('resultados', 'resultados');

// ======================================================= certificados (só o certificado final do curso)
app.get('/certificados/aluno/:id', async (req, res) =>
  json(res, await col('certificados').find({ AlunoId: req.params.id, MateriaId: null }).sort({ EmitidoEm: -1 }).toArray()));

app.get('/certificados/matricula/:id', async (req, res) => {
  const c = await col('certificados').findOne({ MatriculaId: req.params.id, MateriaId: null });
  return c ? json(res, c) : naoEncontrado(res);
});

app.post('/certificados', (req, res) => inserir(res, 'certificados', req.body));

// ======================================================= solicitações de matrícula
app.get('/solicitacoes', async (req, res) =>
  json(res, await col('solicitacoes').find({ Status: { $ne: 'Cancelada' } }).sort({ CriadaEm: 1 }).toArray()));

app.get('/solicitacoes/email/:email', async (req, res) => {
  const s = await col('solicitacoes').findOne({ Email: emailNormalizado(req.params.email) });
  return s ? json(res, s) : naoEncontrado(res);
});

// 409 se o e-mail já tem conta ou outro pedido em andamento. Um pedido cancelado do mesmo e-mail é apagado.
app.post('/solicitacoes', async (req, res) => {
  const email = emailNormalizado(req.body?.Email);
  if (await col('usuarios').findOne({ Email: email }, { projection: { _id: 1 } }))
    return res.status(409).json({ erro: 'E-mail já cadastrado.' });

  await col('solicitacoes').deleteMany({ Email: email, Status: 'Cancelada' });
  return inserir(res, 'solicitacoes', { ...req.body, Email: email });
});

app.patch('/solicitacoes/:id/senha', async (req, res) => {
  await col('solicitacoes').updateOne({ _id: req.params.id }, { $set: { SenhaHash: req.body?.SenhaHash } });
  res.status(204).end();
});

// Apaga a solicitação e os arquivos dos documentos.
app.delete('/solicitacoes/:id', async (req, res) => {
  const s = await porId('solicitacoes', req.params.id);
  if (s) await excluirArquivos((s.Documentos ?? []).flatMap((d) => (d.Arquivos ?? []).map((a) => a.ArquivoId)));
  await col('solicitacoes').deleteOne({ _id: req.params.id });
  res.status(204).end();
});

rotasComuns('solicitacoes', 'solicitacoes');

// ======================================================= notificações
app.get('/notificacoes/usuario/:id/nao-lidas', async (req, res) =>
  res.json({ total: await col('notificacoes').countDocuments({ UsuarioId: req.params.id, Lida: false }) }));

app.get('/notificacoes/usuario/:id', async (req, res) =>
  json(res, await col('notificacoes').find({ UsuarioId: req.params.id }).sort({ CriadaEm: -1 }).limit(50).toArray()));

app.post('/notificacoes/lote', async (req, res) => {
  const lista = Array.isArray(req.body) ? req.body.map(deJson) : [];
  for (const n of lista) if (!n._id) n._id = new ObjectId().toHexString();
  if (lista.length) await col('notificacoes').insertMany(lista);
  res.status(204).end();
});

// Das chaves informadas, as que o usuário já recebeu.
app.post('/notificacoes/usuario/:id/chaves', async (req, res) => {
  const chaves = Array.isArray(req.body?.chaves) ? req.body.chaves : [];
  const docs = chaves.length
    ? await col('notificacoes').find({ UsuarioId: req.params.id, Chave: { $in: chaves } }, { projection: { Chave: 1 } }).toArray()
    : [];
  res.json([...new Set(docs.map((d) => d.Chave).filter(Boolean))]);
});

app.post('/notificacoes/usuario/:id/marcar-lidas', async (req, res) => {
  await col('notificacoes').updateMany({ UsuarioId: req.params.id, Lida: false }, { $set: { Lida: true } });
  res.status(204).end();
});

// ======================================================= arquivos (GridFS, bucket "arquivos")
// Envio: corpo = bytes do arquivo; ?nome=foto.jpg e o resto da query vira metadado (aulaId, cursoId, solicitacaoId, documento).
app.post('/arquivos', express.raw({ type: () => true, limit: '15mb' }), async (req, res) => {
  const { nome, ...metadata } = req.query;
  if (!nome) return res.status(400).json({ erro: 'Informe ?nome= do arquivo.' });
  const envio = arquivos().openUploadStream(String(nome), { metadata });
  await new Promise((ok, falha) => envio.on('finish', ok).on('error', falha).end(req.body));
  res.status(201).json({ id: envio.id.toHexString() });
});

app.get('/arquivos/:id', async (req, res) => {
  if (!ObjectId.isValid(req.params.id)) return naoEncontrado(res);
  const id = new ObjectId(req.params.id);
  const info = await arquivos().find({ _id: id }).next();
  if (!info) return naoEncontrado(res);
  res.setHeader('Content-Type', 'application/octet-stream');
  res.setHeader('Content-Length', info.length);
  arquivos().openDownloadStream(id).pipe(res);
});

app.post('/arquivos/excluir', async (req, res) => {
  await excluirArquivos(ids(req));
  res.status(204).end();
});

// ======================================================= erros
app.use((req, res) => res.status(404).json({ erro: `Rota não encontrada: ${req.method} ${req.path}` }));

app.use((err, req, res, next) => {
  if (err.type === 'entity.parse.failed') return res.status(400).json({ erro: 'JSON inválido no corpo da requisição.' });
  if (err.type === 'entity.too.large') return res.status(413).json({ erro: 'Arquivo ou corpo grande demais.' });
  console.error(err);
  res.status(500).json({ erro: 'Erro interno no servidor.' });
});

conectar()
  .then(criarIndices)
  .then(() => app.listen(PORT, '0.0.0.0', () => console.log(`API European rodando em http://localhost:${PORT}`)))
  .catch((e) => {
    console.error('Não foi possível conectar no MongoDB:', e.message);
    process.exit(1);
  });
