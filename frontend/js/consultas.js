// Tela de consultas: cada cartão chama uma rota com join do Trabalho Prático 1
// e mostra o resultado em uma tabela.

// Monta uma tabela a partir de uma lista e de uma definição de colunas
function tabelaResultado(itens, colunas) {
    if (itens.length === 0) {
        return '<div class="alert alert-light border mb-0">Nenhum registro encontrado.</div>';
    }

    const cabecalho = colunas.map(c => `<th class="${c.classe || ''}">${c.titulo}</th>`).join('');
    const linhas = itens.map(item => `
        <tr>${colunas.map(c => `<td class="${c.classe || ''}">${c.valor(item)}</td>`).join('')}</tr>`).join('');

    return `
        <table class="table table-sm table-hover mb-1">
            <thead><tr>${cabecalho}</tr></thead>
            <tbody>${linhas}</tbody>
        </table>
        <div class="small text-muted">${itens.length} registro(s)</div>`;
}

async function executar(destino, caminho, colunas) {
    const area = document.getElementById(destino);
    area.innerHTML = '<div class="text-muted small">Consultando...</div>';

    try {
        const itens = await api.get(caminho);
        area.innerHTML = tabelaResultado(itens, colunas);
    } catch (erro) {
        area.innerHTML = '';
        mostrarErro(erro);
    }
}

const colunasPorFabricante = [
    { titulo: 'Modelo', valor: v => escapar(v.modelo) },
    { titulo: 'Placa', valor: v => escapar(v.placa) },
    { titulo: 'Ano', valor: v => v.anoFabricacao },
    { titulo: 'Km', classe: 'text-end', valor: v => formatarNumero(v.quilometragem) },
    { titulo: 'Fabricante', valor: v => escapar(v.fabricante) }
];

const colunasDisponiveis = [
    { titulo: 'Modelo', valor: v => escapar(v.modelo) },
    { titulo: 'Placa', valor: v => escapar(v.placa) },
    { titulo: 'Fabricante', valor: v => escapar(v.fabricante) },
    { titulo: 'Categoria', valor: v => escapar(v.categoria) },
    { titulo: 'Diária', classe: 'text-end', valor: v => formatarMoeda(v.valorDiaria) }
];

const colunasPorCpf = [
    { titulo: 'Veículo', valor: a => `${escapar(a.veiculo)} (${escapar(a.placa)})` },
    { titulo: 'Início', valor: a => formatarData(a.dataInicio) },
    { titulo: 'Fim', valor: a => formatarData(a.dataFim) },
    { titulo: 'Devolução', valor: a => formatarData(a.dataDevolucao) },
    { titulo: 'Total', classe: 'text-end', valor: a => formatarMoeda(a.valorTotal) }
];

const colunasAbertos = [
    { titulo: 'Cliente', valor: a => escapar(a.cliente) },
    { titulo: 'Telefone', valor: a => escapar(a.telefone) || '—' },
    { titulo: 'Veículo', valor: a => `${escapar(a.veiculo)} (${escapar(a.placa)})` },
    { titulo: 'Fim previsto', valor: a => formatarData(a.dataFim) },
    { titulo: 'Diária', classe: 'text-end', valor: a => formatarMoeda(a.valorDiaria) }
];

const colunasSemAluguel = [
    { titulo: 'Id', valor: c => c.id },
    { titulo: 'Nome', valor: c => escapar(c.nome) },
    { titulo: 'CPF', valor: c => formatarCpf(c.cpf) },
    { titulo: 'E-mail', valor: c => escapar(c.email) }
];

document.addEventListener('DOMContentLoaded', () => {
    document.getElementById('formPorFabricante').addEventListener('submit', evento => {
        evento.preventDefault();
        const nome = evento.target.nome.value.trim();
        executar('resultadoPorFabricante', `/veiculos/fabricante/${encodeURIComponent(nome)}`, colunasPorFabricante);
    });

    document.getElementById('formDisponiveis').addEventListener('submit', evento => {
        evento.preventDefault();
        const query = montarQuery({ categoria: evento.target.categoria.value });
        executar('resultadoDisponiveis', `/veiculos/disponiveis${query}`, colunasDisponiveis);
    });

    document.getElementById('formPorCpf').addEventListener('submit', evento => {
        evento.preventDefault();
        const cpf = evento.target.cpf.value.replace(/\D/g, '');
        executar('resultadoPorCpf', `/alugueis/cliente/${cpf}`, colunasPorCpf);
    });

    const carregarAbertos = () => executar('resultadoAbertos', '/alugueis/abertos', colunasAbertos);
    const carregarSemAluguel = () => executar('resultadoSemAluguel', '/clientes/sem-aluguel', colunasSemAluguel);

    document.getElementById('btnAbertos').addEventListener('click', carregarAbertos);
    document.getElementById('btnSemAluguel').addEventListener('click', carregarSemAluguel);

    // Consultas sem parâmetro já carregam ao abrir a tela
    executar('resultadoDisponiveis', '/veiculos/disponiveis', colunasDisponiveis);
    carregarAbertos();
    carregarSemAluguel();
});
