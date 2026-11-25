// Comunicação com a API do Trabalho Prático 1 (ASP.NET Core + SQL Server).
// Todas as telas usam estas funções, que chamam as rotas com fetch.

const API_URL = 'http://localhost:5269/api';

// Monta a query string só com os filtros preenchidos.
// Ex.: { nome: 'fiat', paisOrigem: '' } -> "?nome=fiat"
function montarQuery(filtros) {
    const params = new URLSearchParams();

    Object.entries(filtros || {}).forEach(([chave, valor]) => {
        if (valor !== undefined && valor !== null && String(valor).trim() !== '') {
            params.append(chave, String(valor).trim());
        }
    });

    const texto = params.toString();
    return texto ? `?${texto}` : '';
}

// Lê a mensagem de erro devolvida pela API.
// Os controllers devolvem texto simples (ex.: "Cliente não encontrado.") e a
// validação automática do [ApiController] devolve JSON com a lista "errors".
async function lerErro(resposta) {
    const texto = await resposta.text();

    if (!texto) {
        return `Erro ${resposta.status} ao acessar a API.`;
    }

    try {
        const json = JSON.parse(texto);

        if (json.errors) {
            return Object.values(json.errors).flat().join(' ');
        }

        return json.title || json.message || texto;
    } catch {
        return texto;
    }
}

async function requisicao(caminho, opcoes = {}) {
    let resposta;

    try {
        resposta = await fetch(`${API_URL}${caminho}`, {
            headers: { 'Content-Type': 'application/json' },
            ...opcoes
        });
    } catch {
        throw new Error('Não foi possível conectar à API. Verifique se o back-end está rodando em http://localhost:5269.');
    }

    if (!resposta.ok) {
        throw new Error(await lerErro(resposta));
    }

    // 204 No Content (PUT e DELETE) não tem corpo
    if (resposta.status === 204) {
        return null;
    }

    const texto = await resposta.text();
    return texto ? JSON.parse(texto) : null;
}

const api = {
    // GET /api/{recurso}?filtros
    listar: (recurso, filtros) => requisicao(`/${recurso}${montarQuery(filtros)}`),

    // GET /api/{recurso}/{id}
    obter: (recurso, id) => requisicao(`/${recurso}/${id}`),

    // POST /api/{recurso}
    criar: (recurso, dados) => requisicao(`/${recurso}`, {
        method: 'POST',
        body: JSON.stringify(dados)
    }),

    // PUT /api/{recurso}/{id}
    atualizar: (recurso, id, dados) => requisicao(`/${recurso}/${id}`, {
        method: 'PUT',
        body: JSON.stringify(dados)
    }),

    // DELETE /api/{recurso}/{id}
    excluir: (recurso, id) => requisicao(`/${recurso}/${id}`, { method: 'DELETE' }),

    // Rotas específicas (consultas com join, devolução etc.)
    get: (caminho) => requisicao(caminho),
    put: (caminho) => requisicao(caminho, { method: 'PUT' })
};
