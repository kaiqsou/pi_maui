// Conexão com o MongoDB. Usa o mesmo banco ("european") e as mesmas coleções que o app usava direto,
// então os dados do Seed (dotnet run --project Seed) continuam valendo.
const { MongoClient, GridFSBucket } = require('mongodb');

const MONGO_URL = process.env.MONGO_URL || 'mongodb://localhost:27017';
const DB_NAME = process.env.DB_NAME || 'european';

const client = new MongoClient(MONGO_URL, { serverSelectionTimeoutMS: 5000 });
let db;

async function conectar() {
  await client.connect();
  db = client.db(DB_NAME);
  console.log(`MongoDB conectado em ${MONGO_URL}/${DB_NAME}`);
  return db;
}

const col = (nome) => db.collection(nome);
const arquivos = () => new GridFSBucket(db, { bucketName: 'arquivos' });

// Mesmos índices que o MongoContext.InicializarAsync do app criava.
async function criarIndices() {
  await col('usuarios').createIndex({ Email: 1 }, { unique: true });
  await col('solicitacoes').createIndex({ Email: 1 }, { unique: true });
  await col('matriculas').createIndex({ AlunoId: 1, CursoId: 1 });
  await col('progressos').createIndex({ MatriculaId: 1, AulaId: 1 }, { unique: true });
  await col('resultados').createIndex({ AlunoId: 1, ProvaId: 1 });
  await col('materiais').createIndex({ AulaId: 1 });
  await col('notificacoes').createIndex({ UsuarioId: 1, CriadaEm: -1 });
}

module.exports = { conectar, col, arquivos, criarIndices, client };
