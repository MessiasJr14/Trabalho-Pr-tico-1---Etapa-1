// Tela de fabricantes: pesquisa com filtros, cadastro, edição e exclusão

const RECURSO = 'fabricantes';
let fabricantes = [];
let idEmEdicao = null;

const formFiltro = document.getElementById('formFiltro');
const formCadastro = document.getElementById('formCadastro');
const tabela = document.getElementById('tabela');
let modal;

async function carregar() {
    tabela.innerHTML = linhaInformativa(4, 'Carregando...');

    try {
        fabricantes = await api.listar(RECURSO, lerFormulario(formFiltro));
        renderizar();
    } catch (erro) {
        tabela.innerHTML = linhaInformativa(4, 'Não foi possível carregar os fabricantes.');
        mostrarErro(erro);
    }
}

function renderizar() {
    document.getElementById('total').textContent = `${fabricantes.length} fabricante(s) encontrado(s)`;

    if (fabricantes.length === 0) {
        tabela.innerHTML = linhaInformativa(4, 'Nenhum fabricante encontrado.');
        return;
    }

    tabela.innerHTML = fabricantes.map(f => `
        <tr>
            <td>${f.id}</td>
            <td>${escapar(f.nome)}</td>
            <td>${escapar(f.paisOrigem) || '—'}</td>
            <td class="acoes">${botoesAcao(f.id)}</td>
        </tr>`).join('');
}

function abrirFormulario(fabricante) {
    idEmEdicao = fabricante ? fabricante.id : null;
    formCadastro.reset();
    document.getElementById('tituloModal').textContent = fabricante ? 'Editar fabricante' : 'Novo fabricante';

    if (fabricante) {
        preencherFormulario(formCadastro, fabricante);
    }

    modal.show();
}

async function salvar(evento) {
    evento.preventDefault();
    const dados = lerFormulario(formCadastro);

    try {
        if (idEmEdicao) {
            await api.atualizar(RECURSO, idEmEdicao, { id: idEmEdicao, ...dados });
            mostrarMensagem('Fabricante atualizado com sucesso.');
        } else {
            await api.criar(RECURSO, dados);
            mostrarMensagem('Fabricante cadastrado com sucesso.');
        }

        modal.hide();
        carregar();
    } catch (erro) {
        mostrarErro(erro);
    }
}

async function excluir(id) {
    const fabricante = fabricantes.find(f => f.id === id);

    if (!await confirmar(`Deseja excluir o fabricante "${fabricante.nome}"?`)) {
        return;
    }

    try {
        await api.excluir(RECURSO, id);
        mostrarMensagem('Fabricante excluído com sucesso.');
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
            abrirFormulario(fabricantes.find(f => f.id === id));
        } else {
            excluir(id);
        }
    });

    carregar();
});
