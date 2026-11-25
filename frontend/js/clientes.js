// Tela de clientes: pesquisa com filtros, cadastro, edição e exclusão

const RECURSO = 'clientes';
let clientes = [];
let idEmEdicao = null;

const formFiltro = document.getElementById('formFiltro');
const formCadastro = document.getElementById('formCadastro');
const tabela = document.getElementById('tabela');
let modal;

async function carregar() {
    tabela.innerHTML = linhaInformativa(6, 'Carregando...');

    const filtros = lerFormulario(formFiltro);
    filtros.cpf = filtros.cpf.replace(/\D/g, '');

    try {
        clientes = await api.listar(RECURSO, filtros);
        renderizar();
    } catch (erro) {
        tabela.innerHTML = linhaInformativa(6, 'Não foi possível carregar os clientes.');
        mostrarErro(erro);
    }
}

function renderizar() {
    document.getElementById('total').textContent = `${clientes.length} cliente(s) encontrado(s)`;

    if (clientes.length === 0) {
        tabela.innerHTML = linhaInformativa(6, 'Nenhum cliente encontrado.');
        return;
    }

    tabela.innerHTML = clientes.map(c => `
        <tr>
            <td class="fw-semibold">${escapar(c.nome)}</td>
            <td>${formatarCpf(c.cpf)}</td>
            <td>${escapar(c.email)}</td>
            <td>${escapar(c.telefone) || '—'}</td>
            <td>${escapar(c.cnh) || '—'}</td>
            <td class="acoes">${botoesAcao(c.id)}</td>
        </tr>`).join('');
}

function abrirFormulario(cliente) {
    idEmEdicao = cliente ? cliente.id : null;
    formCadastro.reset();
    document.getElementById('tituloModal').textContent = cliente ? 'Editar cliente' : 'Novo cliente';

    if (cliente) {
        preencherFormulario(formCadastro, cliente);
    }

    modal.show();
}

async function salvar(evento) {
    evento.preventDefault();
    const dados = lerFormulario(formCadastro);

    try {
        if (idEmEdicao) {
            await api.atualizar(RECURSO, idEmEdicao, { id: idEmEdicao, ...dados });
            mostrarMensagem('Cliente atualizado com sucesso.');
        } else {
            await api.criar(RECURSO, dados);
            mostrarMensagem('Cliente cadastrado com sucesso.');
        }

        modal.hide();
        carregar();
    } catch (erro) {
        mostrarErro(erro);
    }
}

async function excluir(id) {
    const cliente = clientes.find(c => c.id === id);

    if (!await confirmar(`Deseja excluir o cliente "${cliente.nome}"?`)) {
        return;
    }

    try {
        await api.excluir(RECURSO, id);
        mostrarMensagem('Cliente excluído com sucesso.');
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
            abrirFormulario(clientes.find(c => c.id === id));
        } else {
            excluir(id);
        }
    });

    carregar();
});
