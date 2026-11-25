// Tela inicial: indicadores e resumo usando as rotas da API

function cardIndicador(titulo, valor, icone, link, detalhe) {
    return `
        <div class="col-6 col-md-4 col-xl">
            <a class="card card-indicador h-100 text-decoration-none text-reset" href="${link}">
                <div class="card-body d-flex justify-content-between align-items-center">
                    <div>
                        <div class="text-muted small">${titulo}</div>
                        <div class="valor">${valor}</div>
                        <div class="small text-muted">${detalhe || '&nbsp;'}</div>
                    </div>
                    <i class="bi ${icone} icone"></i>
                </div>
            </a>
        </div>`;
}

async function carregarPainel() {
    try {
        const [fabricantes, categorias, veiculos, clientes, abertos, disponiveis] = await Promise.all([
            api.listar('fabricantes'),
            api.listar('categorias'),
            api.listar('veiculos'),
            api.listar('clientes'),
            api.get('/alugueis/abertos'),
            api.get('/veiculos/disponiveis')
        ]);

        document.getElementById('indicadores').innerHTML = [
            cardIndicador('Fabricantes', fabricantes.length, 'bi-building', 'fabricantes.html'),
            cardIndicador('Categorias', categorias.length, 'bi-tags', 'categorias.html'),
            cardIndicador('Veículos', veiculos.length, 'bi-car-front', 'veiculos.html',
                `${disponiveis.length} disponível(is)`),
            cardIndicador('Clientes', clientes.length, 'bi-people', 'clientes.html'),
            cardIndicador('Aluguéis em aberto', abertos.length, 'bi-calendar-check', 'alugueis.html')
        ].join('');

        document.getElementById('tabelaAbertos').innerHTML = abertos.length === 0
            ? linhaInformativa(4, 'Nenhum aluguel em aberto.')
            : abertos.map(a => {
                const atrasado = new Date(a.dataFim) < new Date();
                return `
                    <tr>
                        <td>${escapar(a.cliente)}</td>
                        <td>${escapar(a.veiculo)} <span class="text-muted small">${escapar(a.placa)}</span></td>
                        <td>${formatarData(a.dataFim)}</td>
                        <td>${atrasado
                            ? '<span class="badge text-bg-danger">Atrasado</span>'
                            : '<span class="badge text-bg-primary">No prazo</span>'}</td>
                    </tr>`;
            }).join('');

        document.getElementById('tabelaDisponiveis').innerHTML = disponiveis.length === 0
            ? linhaInformativa(3, 'Nenhum veículo disponível.')
            : disponiveis.map(v => `
                <tr>
                    <td>${escapar(v.modelo)} <span class="text-muted small">${escapar(v.fabricante)}</span></td>
                    <td>${escapar(v.categoria)}</td>
                    <td class="text-end">${formatarMoeda(v.valorDiaria)}</td>
                </tr>`).join('');
    } catch (erro) {
        document.getElementById('indicadores').innerHTML =
            `<div class="col-12"><div class="alert alert-danger mb-0">${escapar(erro.message)}</div></div>`;
    }
}

document.addEventListener('DOMContentLoaded', carregarPainel);
