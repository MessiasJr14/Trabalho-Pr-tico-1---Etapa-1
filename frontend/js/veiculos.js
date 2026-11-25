// Tela de veículos: pesquisa com filtros, cadastro, edição e exclusão.
// Fabricantes e categorias vêm da API para preencher os selects e mostrar os nomes.

const RECURSO = 'veiculos';
let veiculos = [];
let fabricantes = [];
let categorias = [];
let idEmEdicao = null;

const formFiltro = document.getElementById('formFiltro');
const formCadastro = document.getElementById('formCadastro');
const tabela = document.getElementById('tabela');
let modal;

async function carregarApoio() {
    [fabricantes, categorias] = await Promise.all([
        api.listar('fabricantes'),
        api.listar('categorias')
    ]);

    preencherSelect(document.getElementById('filtroFabricante'), fabricantes, f => f.nome, 'Todos');
    preencherSelect(document.getElementById('filtroCategoria'), categorias, c => c.nome, 'Todas');
    preencherSelect(document.getElementById('campoFabricante'), fabricantes, f => f.nome, 'Selecione...');
    preencherSelect(document.getElementById('campoCategoria'), categorias,
        c => `${c.nome} (${formatarMoeda(c.valorDiaria)}/dia)`, 'Selecione...');
}

const nomeFabricante = id => fabricantes.find(f => f.id === id)?.nome ?? '—';
const nomeCategoria = id => categorias.find(c => c.id === id)?.nome ?? '—';

async function carregar() {
    tabela.innerHTML = linhaInformativa(9, 'Carregando...');

    try {
        veiculos = await api.listar(RECURSO, lerFormulario(formFiltro));
        renderizar();
    } catch (erro) {
        tabela.innerHTML = linhaInformativa(9, 'Não foi possível carregar os veículos.');
        mostrarErro(erro);
    }
}

function renderizar() {
    document.getElementById('total').textContent = `${veiculos.length} veículo(s) encontrado(s)`;

    if (veiculos.length === 0) {
        tabela.innerHTML = linhaInformativa(9, 'Nenhum veículo encontrado.');
        return;
    }

    tabela.innerHTML = veiculos.map(v => `
        <tr>
            <td class="fw-semibold">${escapar(v.modelo)}</td>
            <td>${escapar(v.placa)}</td>
            <td>${v.anoFabricacao}</td>
            <td class="text-end">${formatarNumero(v.quilometragem)}</td>
            <td>${escapar(v.cor) || '—'}</td>
            <td>${escapar(nomeFabricante(v.fabricanteId))}</td>
            <td>${escapar(nomeCategoria(v.categoriaId))}</td>
            <td>${v.disponivel
                ? '<span class="badge text-bg-success">Disponível</span>'
                : '<span class="badge text-bg-warning">Alugado</span>'}</td>
            <td class="acoes">${botoesAcao(v.id)}</td>
        </tr>`).join('');
}

function abrirFormulario(veiculo) {
    idEmEdicao = veiculo ? veiculo.id : null;
    formCadastro.reset();
    document.getElementById('tituloModal').textContent = veiculo ? 'Editar veículo' : 'Novo veículo';

    if (veiculo) {
        preencherFormulario(formCadastro, veiculo);
    }

    modal.show();
}

async function salvar(evento) {
    evento.preventDefault();
    const campos = lerFormulario(formCadastro);

    const dados = {
        modelo: campos.modelo,
        placa: campos.placa.toUpperCase(),
        anoFabricacao: Number(campos.anoFabricacao),
        quilometragem: Number(campos.quilometragem),
        cor: campos.cor,
        disponivel: document.getElementById('campoDisponivel').checked,
        fabricanteId: Number(campos.fabricanteId),
        categoriaId: Number(campos.categoriaId)
    };

    try {
        if (idEmEdicao) {
            await api.atualizar(RECURSO, idEmEdicao, { id: idEmEdicao, ...dados });
            mostrarMensagem('Veículo atualizado com sucesso.');
        } else {
            await api.criar(RECURSO, dados);
            mostrarMensagem('Veículo cadastrado com sucesso.');
        }

        modal.hide();
        carregar();
    } catch (erro) {
        mostrarErro(erro);
    }
}

async function excluir(id) {
    const veiculo = veiculos.find(v => v.id === id);

    if (!await confirmar(`Deseja excluir o veículo ${veiculo.modelo} (${veiculo.placa})?`)) {
        return;
    }

    try {
        await api.excluir(RECURSO, id);
        mostrarMensagem('Veículo excluído com sucesso.');
        carregar();
    } catch (erro) {
        mostrarErro(erro);
    }
}

document.addEventListener('DOMContentLoaded', async () => {
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
            abrirFormulario(veiculos.find(v => v.id === id));
        } else {
            excluir(id);
        }
    });

    try {
        await carregarApoio();
    } catch (erro) {
        mostrarErro(erro);
    }

    carregar();
});
