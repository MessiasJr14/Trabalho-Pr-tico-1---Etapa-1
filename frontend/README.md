# Front-end — Locadora de Veículos (Trabalho Prático 2)

Front-end em **HTML, CSS, JavaScript e Bootstrap 5** que consome, com `fetch`, a API ASP.NET Core do Trabalho Prático 1 (pasta `LocadoraVeiculos`).

## Telas

| Tela | Arquivo | Rotas usadas |
|---|---|---|
| Início (painel) | `index.html` | `GET /api/fabricantes`, `/categorias`, `/veiculos`, `/clientes`, `/alugueis/abertos`, `/veiculos/disponiveis` |
| Fabricantes | `fabricantes.html` | `GET /api/fabricantes?nome=&paisOrigem=`, `POST`, `PUT /{id}`, `DELETE /{id}` |
| Categorias | `categorias.html` | `GET /api/categorias?nome=&valorMin=&valorMax=`, `POST`, `PUT /{id}`, `DELETE /{id}` |
| Veículos | `veiculos.html` | `GET /api/veiculos?modelo=&placa=&fabricanteId=&categoriaId=&disponivel=`, `POST`, `PUT /{id}`, `DELETE /{id}` |
| Clientes | `clientes.html` | `GET /api/clientes?nome=&cpf=&email=`, `POST`, `PUT /{id}`, `DELETE /{id}` |
| Aluguéis | `alugueis.html` | `GET /api/alugueis?clienteId=&veiculoId=&status=&dataInicial=&dataFinal=`, `POST`, `PUT /{id}`, `PUT /{id}/devolucao`, `DELETE /{id}` |
| Consultas | `consultas.html` | `GET /api/veiculos/fabricante/{nome}`, `/veiculos/disponiveis?categoria=`, `/alugueis/cliente/{cpf}`, `/alugueis/abertos`, `/clientes/sem-aluguel` |

## Estrutura

```
frontend/
├── index.html, fabricantes.html, categorias.html, veiculos.html,
│   clientes.html, alugueis.html, consultas.html
├── css/estilo.css
└── js/
    ├── api.js        # URL da API e funções fetch (listar, obter, criar, atualizar, excluir)
    ├── layout.js     # menu, mensagens, modal de confirmação, formatação
    └── <tela>.js     # lógica de cada tela
```

## Como rodar

1. Suba a API (SQL Server Express com o banco `LocadoraVeiculos`):
   ```
   cd LocadoraVeiculos
   dotnet run --launch-profile http
   ```
   A API fica em `http://localhost:5269` (Swagger em `/swagger`).
2. Sirva a pasta `frontend` com qualquer servidor estático, por exemplo:
   ```
   cd frontend
   python -m http.server 5500
   ```
   ou a extensão **Live Server** do VS Code. Abra `http://localhost:5500`.

Se a API estiver em outra porta, altere `API_URL` em `js/api.js`.
