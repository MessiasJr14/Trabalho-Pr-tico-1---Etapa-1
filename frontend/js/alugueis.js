// Tela de aluguéis: pesquisa com filtros, cadastro, edição, devolução e exclusão.
// A API calcula diária e valor total e muda a disponibilidade do veículo.

const RECURSO = 'alugueis';
let alugueis = [];
let clientes = [];
let veiculos = [];
let idEmEdicao = null;
let idEmDevolucao = null;

const formFiltro = document.getElementById('formFiltro');
const formCadastro = document.getElementById('formCadastro');
const formDevolucao = document.getElementById('formDevolucao');
const tabela = document.getElementById('tabela');
let modal;
let modalDevolucao;

// Data local no formato AAAA-MM-DD (toISOString usaria o fuso UTC)
const hoje = () => new Date().toLocaleDateString('sv-SE');
const nomeCliente = id => clientes.find(c => c.id === id)?.nome ?? '—';
const veiculoPorId = id => veiculos.find(v => v.id === id);
const descricaoVeiculo = v => v ? `${v.modelo} (${v.placa})` : '—';

async function carregarApoio() {
    [clientes, veiculos] = await Promise.all([
        api.listar('clientes'),
        api.listar('veiculos')
    ]);

    preencherSelect(document.getElementById('filtroCliente'), clientes, c => c.nome, 'Todos');
    preencherSelect(document.getElementById('filtroVeiculo'), veiculos, descricaoVeiculo, 'Todos');
    preencherSelect(document.getElementById('campoCliente'), clientes, c => `${c.nome} - ${formatarCpf(c.cpf)}`, 'Selecione...');
}

async function carregar() {
    tabela.innerHTML = linhaInformativa(9, 'Carregando...');

    try {
        alugueis = await api.listar(RECURSO, lerFormulario(formFiltro));
        renderizar();
    } catch (erro) {
        tabela.innerHTML = linhaInformativa(9, 'Não foi possível carregar os aluguéis.');
        mostrarErro(erro);
    }
}

function renderizar() {
    document.getElementById('total').textContent = `${alugueis.length} aluguel(is) encontrado(s)`;

    if (alugueis.length === 0) {
        tabela.innerHTML = linhaInformativa(9, 'Nenhum aluguel encontrado.');
        return;
    }

    tabela.innerHTML = alugueis.map(a => {
        const aberto = !a.dataDevolucao;
        const atrasado = aberto && new Date(a.dataFim) < new Date();

        let status = '<span class="badge text-bg-secondary">Devolvido</span>';
        if (atrasado) {
            status = '<span class="badge text-bg-danger">Atrasado</span>';
        } else if (aberto) {
            status = '<span class="badge text-bg-primary">Em aberto</span>';
        }

        const acoes = aberto
            ? `<button class="btn btn-outline-success" data-acao="devolver" data-id="${a.id}">
                   <i class="bi bi-box-arrow-in-left"></i> Devolver
               </button>
               <button class="btn btn-outline-primary" data-acao="editar" data-id="${a.id}">
                   <i class="bi bi-pencil"></i> Editar
               </button>`
            : '';

        return `
            <tr>
                <td class="fw-semibold">${escapar(nomeCliente(a.clienteId))}</td>
                <td>${escapar(descricaoVeiculo(veiculoPorId(a.veiculoId)))}</td>
                <td>${formatarData(a.dataInicio)}</td>
                <td>${formatarData(a.dataFim)}</td>
                <td>${formatarData(a.dataDevolucao)}</td>
                <td class="text-end">${formatarMoeda(a.valorDiaria)}</td>
                <td class="text-end fw-semibold">${formatarMoeda(a.valorTotal)}</td>
                <td>${status}</td>
                <td class="acoes">
                    <div class="btn-group btn-group-sm">
                        ${acoes}
                        <button class="btn btn-outline-danger" data-acao="excluir" data-id="${a.id}">
                            <i class="bi bi-trash"></i> Excluir
                        </button>
                    </div>
                </td>
            </tr>`;
    }).join('');
}

function abrirFormulario(aluguel) {
    idEmEdicao = aluguel ? aluguel.id : null;
    formCadastro.reset();

    const selectCliente = document.getElementById('campoCliente');
    const selectVeiculo = document.getElementById('campoVeiculo');

    document.getElementById('tituloModal').textContent = aluguel ? 'Editar aluguel' : 'Novo aluguel';

    if (aluguel) {
        // Na edição a API só altera as datas; cliente e veículo ficam travados
        preencherSelect(selectVeiculo, veiculos, descricaoVeiculo);
        preencherFormulario(formCadastro, aluguel);
        selectCliente.disabled = true;
        selectVeiculo.disabled = true;
        document.getElementById('ajudaVeiculo').textContent = 'Na edição só as datas podem ser alteradas.';
    } else {
        const disponiveis = veiculos.filter(v => v.disponivel);
        preencherSelect(selectVeiculo, disponiveis, descricaoVeiculo, 'Selecione...');
        selectVeiculo.value = '';
        selectCliente.disabled = false;
        selectVeiculo.disabled = false;
        document.getElementById('campoInicio').value = hoje();
        document.getElementById('ajudaVeiculo').textContent =
            `Somente veículos disponíveis (${disponiveis.length}).`;
    }

    modal.show();
}

async function salvar(evento) {
    evento.preventDefault();

    const dados = {
        clienteId: Number(document.getElementById('campoCliente').value),
        veiculoId: Number(document.getElementById('campoVeiculo').value),
        dataInicio: document.getElementById('campoInicio').value,
        dataFim: document.getElementById('campoFim').value
    };

    try {
        if (idEmEdicao) {
            await api.atualizar(RECURSO, idEmEdicao, { id: idEmEdicao, ...dados });
            mostrarMensagem('Aluguel atualizado com sucesso.');
        } else {
            await api.criar(RECURSO, dados);
            mostrarMensagem('Aluguel registrado com sucesso.');
        }

        modal.hide();
        await recarregarTudo();
    } catch (erro) {
        mostrarErro(erro);
    }
}

function abrirDevolucao(aluguel) {
    idEmDevolucao = aluguel.id;
    formDevolucao.reset();

    const veiculo = veiculoPorId(aluguel.veiculoId);
    const campoKm = document.getElementById('campoKmFinal');

    document.getElementById('resumoDevolucao').innerHTML =
        `<strong>${escapar(nomeCliente(aluguel.clienteId))}</strong> devolvendo
         <strong>${escapar(descricaoVeiculo(veiculo))}</strong>.<br>
         Km inicial: ${formatarNumero(aluguel.quilometragemInicial)} · Diária: ${formatarMoeda(aluguel.valorDiaria)}`;

    campoKm.min = aluguel.quilometragemInicial;
    campoKm.value = aluguel.quilometragemInicial;
    document.getElementById('campoDevolucao').value = hoje();
    document.getElementById('campoDevolucao').min = paraInputData(aluguel.dataInicio);

    modalDevolucao.show();
}

async function registrarDevolucao(evento) {
    evento.preventDefault();
    const query = montarQuery(lerFormulario(formDevolucao));

    try {
        const resultado = await api.put(`/${RECURSO}/${idEmDevolucao}/devolucao${query}`);
        mostrarMensagem(`Devolução registrada. Valor final: ${formatarMoeda(resultado.valorTotal)}.`);
        modalDevolucao.hide();
        await recarregarTudo();
    } catch (erro) {
        mostrarErro(erro);
    }
}

async function excluir(id) {
    const aluguel = alugueis.find(a => a.id === id);

    if (!await confirmar(`Deseja excluir o aluguel de ${nomeCliente(aluguel.clienteId)}? ` +
        'Se ainda estiver em aberto, o veículo volta a ficar disponível.')) {
        return;
    }

    try {
        await api.excluir(RECURSO, id);
        mostrarMensagem('Aluguel excluído com sucesso.');
        await recarregarTudo();
    } catch (erro) {
        mostrarErro(erro);
    }
}

// A disponibilidade dos veículos muda a cada aluguel ou devolução
async function recarregarTudo() {
    await carregarApoio();
    await carregar();
}

document.addEventListener('DOMContentLoaded', async () => {
    modal = new bootstrap.Modal(document.getElementById('modalFormulario'));
    modalDevolucao = new bootstrap.Modal(document.getElementById('modalDevolucao'));

    document.getElementById('btnNovo').addEventListener('click', () => abrirFormulario(null));
    formCadastro.addEventListener('submit', salvar);
    formDevolucao.addEventListener('submit', registrarDevolucao);

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
        const aluguel = alugueis.find(a => a.id === id);

        if (botao.dataset.acao === 'editar') {
            abrirFormulario(aluguel);
        } else if (botao.dataset.acao === 'devolver') {
            abrirDevolucao(aluguel);
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
