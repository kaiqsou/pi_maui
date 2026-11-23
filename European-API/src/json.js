// Conversão entre o formato do banco (como o driver C# grava) e o JSON que o app lê.
//
// Banco  -> JSON: "_id" vira "Id" (em qualquer nível: Módulos, Matérias, Aulas, Questões...),
//                 Decimal128 vira número e datas viram texto ISO.
// JSON -> banco:  "Id" volta a ser "_id" e textos de data ISO voltam a ser datas,
//                 para as ordenações e filtros por data funcionarem.
const { ObjectId, Decimal128, Long, Double, Int32 } = require('mongodb');

const DATA_ISO = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(\.\d+)?(Z|[+-]\d{2}:\d{2})$/;

function paraJson(valor) {
  if (valor === null || valor === undefined) return valor;
  if (valor instanceof Date) return valor.toISOString();
  if (valor instanceof ObjectId) return valor.toHexString();
  if (valor instanceof Decimal128) return parseFloat(valor.toString());
  if (valor instanceof Long) return valor.toNumber();
  if (valor instanceof Double || valor instanceof Int32) return valor.valueOf();
  if (Array.isArray(valor)) return valor.map(paraJson);
  if (typeof valor === 'object') {
    const saida = {};
    for (const [k, v] of Object.entries(valor)) saida[k === '_id' ? 'Id' : k] = paraJson(v);
    return saida;
  }
  return valor;
}

function deJson(valor) {
  if (valor === null || valor === undefined) return valor;
  if (typeof valor === 'string') return DATA_ISO.test(valor) ? new Date(valor) : valor;
  if (Array.isArray(valor)) return valor.map(deJson);
  if (typeof valor === 'object') {
    const saida = {};
    for (const [k, v] of Object.entries(valor)) saida[k === 'Id' ? '_id' : k] = deJson(v);
    return saida;
  }
  return valor;
}

// Senha gravada em texto por versões antigas do app (mesma regra do SenhaService.EhTextoPuro).
function ehTextoPuro(senha) {
  if (!senha || typeof senha !== 'string') return false;
  if (senha.startsWith('$argon2id$v=19$')) return false;
  if (/^[0-9a-fA-F]{64}$/.test(senha)) return false;
  const partes = senha.split('.');
  if (partes.length === 3 && parseInt(partes[0], 10) > 0) {
    const b64 = /^[A-Za-z0-9+/]+={0,2}$/;
    if (b64.test(partes[1]) && b64.test(partes[2])) return false;
  }
  return true;
}

module.exports = { paraJson, deJson, ehTextoPuro };
