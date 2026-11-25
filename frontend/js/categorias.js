// Tela de categorias: pesquisa com filtros, cadastro, edição e exclusão

const RECURSO = 'categorias';
let categorias = [];
let idEmEdicao = null;

const formFiltro = document.getElementById('formFiltro');
const formCadastro = document.getElementById('formCadastro');
const tabela = document.getElementById('tabela');
let modal;

async function carregar() {
    tabela.innerHTML = linhaInformativa(5, 'Carregando...');

    try {
        categorias = await api.listar(RECURSO, lerFormulario(formFiltro));
        renderizar();
    } catch (erro) {
        tabela.innerHTML = linhaInformativa(5, 'Não foi possível carregar as categorias.');
        mostrarErro(erro);
    }
}

function renderizar() {
    document.getElementById('total').textContent = `${categorias.length} categoria(s) encontrada(s)`;

    if (categorias.length === 0) {
        tabela.innerHTML = linhaInformativa(5, 'Nenhuma categoria encontrada.');
        return;
    }

    tabela.innerHTML = categorias.map(c => `
        <tr>
            <td>${c.id}</td>
            <td>${escapar(c.nome)}</td>
            <td>${escapar(c.descricao) || '—'}</td>
            <td class="text-end">${formatarMoeda(c.valorDiaria)}</td>
            <td class="acoes">${botoesAcao(c.id)}</td>
        </tr>`).join('');
}

function abrirFormulario(categoria) {
    idEmEdicao = categoria ? categoria.id : null;
    formCadastro.reset();
    document.getElementById('tituloModal').textContent = categoria ? 'Editar categoria' : 'Nova categoria';

    if (categoria) {
        preencherFormulario(formCadastro, categoria);
    }

    modal.show();
}

async function salvar(evento) {
    evento.preventDefault();
    const campos = lerFormulario(formCadastro);
    const dados = { ...campos, valorDiaria: Number(campos.valorDiaria) };

    try {
        if (idEmEdicao) {
            await api.atualizar(RECURSO, idEmEdicao, { id: idEmEdicao, ...dados });
            mostrarMensagem('Categoria atualizada com sucesso.');
        } else {
            await api.criar(RECURSO, dados);
            mostrarMensagem('Categoria cadastrada com sucesso.');
        }

        modal.hide();
        carregar();
    } catch (erro) {
        mostrarErro(erro);
    }
}

async function excluir(id) {
    const categoria = categorias.find(c => c.id === id);

    if (!await confirmar(`Deseja excluir a categoria "${categoria.nome}"?`)) {
        return;
    }

    try {
        await api.excluir(RECURSO, id);
        mostrarMensagem('Categoria excluída com sucesso.');
        carregar();
    } catch (erro) {
        mostrarErro(erro);
    }
}

document.addEventListener('DOMContentLoaded', () => {
    modal = new bootstrap.Modal(document.getElementById('modalFormulario'));

    document.getElementById('btnNovo').addEventListener('click', () => abrirFormulario(null));
    formCadastro.addEventListener('submit', salvar);

    formFiltro.addEventListener('submit', evento => {
        evento.preventDefault();
        carregar();
    });
    formFiltro.addEventListener('reset', () => setTimeout(carregar));

    tabela.addEventListener('click', evento => {
        const botao = evento.target.closest('button[data-acao]');
        if (!botao) {
            return;
        }

        const id = Number(botao.dataset.id);
        if (botao.dataset.acao === 'editar') {
            abrirFormulario(categorias.find(c => c.id === id));
        } else {
            excluir(id);
        }
    });

    carregar();
});
