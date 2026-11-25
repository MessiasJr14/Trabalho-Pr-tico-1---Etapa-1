// Partes comuns a todas as telas: menu, mensagens, confirmação e formatação.

const PAGINAS = [
    { arquivo: 'index.html', titulo: 'Início', icone: 'bi-speedometer2' },
    { arquivo: 'fabricantes.html', titulo: 'Fabricantes', icone: 'bi-building' },
    { arquivo: 'categorias.html', titulo: 'Categorias', icone: 'bi-tags' },
    { arquivo: 'veiculos.html', titulo: 'Veículos', icone: 'bi-car-front' },
    { arquivo: 'clientes.html', titulo: 'Clientes', icone: 'bi-people' },
    { arquivo: 'alugueis.html', titulo: 'Aluguéis', icone: 'bi-calendar-check' },
    { arquivo: 'consultas.html', titulo: 'Consultas', icone: 'bi-search' }
];

function montarMenu() {
    const atual = location.pathname.split('/').pop() || 'index.html';

    const links = PAGINAS.map(p => `
        <li class="nav-item">
            <a class="nav-link ${p.arquivo === atual ? 'active' : ''}" href="${p.arquivo}">
                <i class="bi ${p.icone} me-1"></i>${p.titulo}
            </a>
        </li>`).join('');

    document.getElementById('menu').innerHTML = `
        <nav class="navbar navbar-expand-lg navbar-dark bg-dark">
            <div class="container">
                <a class="navbar-brand fw-semibold" href="index.html">
                    <i class="bi bi-key-fill me-1"></i>Locadora de Veículos
                </a>
                <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#menuLinks">
                    <span class="navbar-toggler-icon"></span>
                </button>
                <div class="collapse navbar-collapse" id="menuLinks">
                    <ul class="navbar-nav ms-auto">${links}</ul>
                </div>
            </div>
        </nav>`;
}

// Área das mensagens (toasts) e modal de confirmação, criados uma vez por página
function montarComponentes() {
    document.body.insertAdjacentHTML('beforeend', `
        <div class="toast-container position-fixed bottom-0 end-0 p-3" id="mensagens"></div>

        <div class="modal fade" id="modalConfirmacao" tabindex="-1">
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">Confirmar exclusão</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body" id="textoConfirmacao"></div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-outline-secondary" data-bs-dismiss="modal">Cancelar</button>
                        <button type="button" class="btn btn-danger" id="btnConfirmar">Excluir</button>
                    </div>
                </div>
            </div>
        </div>`);
}

function mostrarMensagem(texto, tipo = 'success') {
    const id = `msg${Date.now()}`;

    document.getElementById('mensagens').insertAdjacentHTML('beforeend', `
        <div id="${id}" class="toast align-items-center text-bg-${tipo} border-0" role="alert">
            <div class="d-flex">
                <div class="toast-body">${escapar(texto)}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        </div>`);

    const elemento = document.getElementById(id);
    elemento.addEventListener('hidden.bs.toast', () => elemento.remove());
    new bootstrap.Toast(elemento, { delay: tipo === 'danger' ? 6000 : 3000 }).show();
}

function mostrarErro(erro) {
    mostrarMensagem(erro.message || String(erro), 'danger');
}

// Abre o modal de confirmação e resolve true se o usuário clicar em "Excluir"
function confirmar(mensagem) {
    return new Promise(resolve => {
        const elemento = document.getElementById('modalConfirmacao');
        const modal = bootstrap.Modal.getOrCreateInstance(elemento);
        const botao = document.getElementById('btnConfirmar');
        let confirmado = false;

        document.getElementById('textoConfirmacao').textContent = mensagem;

        botao.onclick = () => {
            confirmado = true;
            modal.hide();
        };

        elemento.addEventListener('hidden.bs.modal', () => resolve(confirmado), { once: true });
        modal.show();
    });
}

// Evita que textos vindos do banco sejam interpretados como HTML
function escapar(valor) {
    return String(valor ?? '')
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

function formatarData(valor) {
    if (!valor) {
        return '—';
    }
    return new Date(valor).toLocaleDateString('pt-BR');
}

// "2026-11-25T00:00:00" -> "2026-11-25" (formato do input type="date")
function paraInputData(valor) {
    return valor ? String(valor).substring(0, 10) : '';
}

function formatarMoeda(valor) {
    return Number(valor || 0).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}

function formatarCpf(cpf) {
    const numeros = String(cpf || '').replace(/\D/g, '');
    return numeros.length === 11
        ? numeros.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4')
        : escapar(cpf);
}

function formatarNumero(valor) {
    return Number(valor || 0).toLocaleString('pt-BR');
}

// Converte os campos de um formulário em objeto
function lerFormulario(form) {
    return Object.fromEntries(new FormData(form).entries());
}

// Preenche um formulário a partir de um objeto vindo da API (propriedades em camelCase)
function preencherFormulario(form, dados) {
    Array.from(form.elements).forEach(campo => {
        if (!campo.name || !(campo.name in dados)) {
            return;
        }

        if (campo.type === 'checkbox') {
            campo.checked = Boolean(dados[campo.name]);
        } else if (campo.type === 'date') {
            campo.value = paraInputData(dados[campo.name]);
        } else {
            campo.value = dados[campo.name] ?? '';
        }
    });
}

// Linha exibida na tabela enquanto carrega ou quando não há registros
function linhaInformativa(colunas, texto) {
    return `<tr><td colspan="${colunas}" class="text-center text-muted py-4">${texto}</td></tr>`;
}

function botoesAcao(id) {
    return `
        <div class="btn-group btn-group-sm">
            <button class="btn btn-outline-primary" data-acao="editar" data-id="${id}" title="Editar">
                <i class="bi bi-pencil"></i> Editar
            </button>
            <button class="btn btn-outline-danger" data-acao="excluir" data-id="${id}" title="Excluir">
                <i class="bi bi-trash"></i> Excluir
            </button>
        </div>`;
}

// Preenche um <select> com registros da API
function preencherSelect(select, itens, texto, primeiraOpcao) {
    const valorAtual = select.value;
    const opcoes = itens.map(item => `<option value="${item.id}">${escapar(texto(item))}</option>`).join('');

    select.innerHTML = (primeiraOpcao ? `<option value="">${primeiraOpcao}</option>` : '') + opcoes;
    select.value = valorAtual;
}

document.addEventListener('DOMContentLoaded', () => {
    montarMenu();
    montarComponentes();
});
