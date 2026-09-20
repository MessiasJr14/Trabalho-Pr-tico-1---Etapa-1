# Modelagem do Banco de Dados — Locadora de Veículos

Trabalho Prático 1 · Etapa 1 · Messias Junio

Este documento detalha o caminho **modelo conceitual → modelo lógico → implementação física**
do banco `LocadoraVeiculos`, criado no SQL Server Express pelo Entity Framework Core (*Code First*).

## 1. Modelo conceitual

![Modelo conceitual](modelo-conceitual.png)

| Relacionamento | Cardinalidade | Leitura |
|---|---|---|
| FABRICANTE **fabrica** VEÍCULO | 1 : N | Todo veículo pertence a exatamente um fabricante; um fabricante pode ter vários veículos |
| VEÍCULO **pertence a** CATEGORIA | N : 1 | Todo veículo tem exatamente uma categoria; uma categoria classifica vários veículos |
| CLIENTE **realiza** ALUGUEL | 1 : N | Todo aluguel é de exatamente um cliente; um cliente pode realizar vários aluguéis |
| VEÍCULO **é locado em** ALUGUEL | 1 : N | Todo aluguel é de exatamente um veículo; um veículo é alugado várias vezes ao longo do tempo |
| ALUGUEL **recebe** PAGAMENTO | 1 : N | Todo pagamento pertence a exatamente um aluguel; um aluguel pode ter vários pagamentos (sinal + restante) |

As duas entidades além das pedidas no enunciado são **Categoria** (define o valor base da diária e
classifica a frota) e **Pagamento** (controla o que já foi recebido de cada locação).

## 2. Do conceitual para o relacional

| No modelo conceitual | No modelo relacional (EF Core) |
|---|---|
| Entidade | Classe em `Models/` + `DbSet<>` no `ApplicationContext` → tabela |
| Atributo identificador | Propriedade `Id` com `[Key]` → `PRIMARY KEY` + `IDENTITY(1,1)` |
| Relacionamento 1:N | Propriedade `XxxId` + navegação com `[ForeignKey]` no lado N → `FOREIGN KEY` |
| Atributo único | `HasIndex(...).IsUnique()` → `UNIQUE INDEX` |
| Atributo obrigatório / opcional | `[Required]` ou tipo não anulável → `NOT NULL`; tipo anulável (`string?`, `int?`) → `NULL` |
| Regra de domínio | `HasCheckConstraint(...)` → `CHECK` |

## 3. Dicionário de dados

Legenda: **PK** chave primária · **FK** chave estrangeira · **UK** índice único · **NN** `NOT NULL`

### 3.1 Fabricantes

| Coluna | Tipo | Restrições | Descrição |
|---|---|---|---|
| Id | `int` | PK, identity | Identificador |
| Nome | `nvarchar(80)` | NN, UK | Nome da marca (Fiat, Toyota...) |
| PaisOrigem | `nvarchar(60)` | — | País de origem |

### 3.2 Categorias

| Coluna | Tipo | Restrições | Descrição |
|---|---|---|---|
| Id | `int` | PK, identity | Identificador |
| Nome | `nvarchar(50)` | NN, UK | Econômico, Sedan, SUV, Luxo... |
| Descricao | `nvarchar(200)` | — | Descrição da categoria |
| ValorDiariaBase | `decimal(10,2)` | NN, CHECK `> 0` | Valor sugerido da diária |

### 3.3 Veiculos

| Coluna | Tipo | Restrições | Descrição |
|---|---|---|---|
| Id | `int` | PK, identity | Identificador |
| FabricanteId | `int` | NN, **FK → Fabricantes(Id)** | Fabricante do veículo |
| CategoriaId | `int` | NN, **FK → Categorias(Id)** | Categoria do veículo |
| Modelo | `nvarchar(80)` | NN | Modelo (Argo, Corolla...) |
| AnoFabricacao | `int` | NN, CHECK `BETWEEN 1950 AND 2100` | Ano de fabricação |
| Quilometragem | `int` | NN, CHECK `>= 0` | Quilometragem atual |
| Placa | `char(7)` | NN, UK, CHECK `LEN = 7` | Placa sem hífen (ABC1234 ou ABC1D23) |
| Cor | `nvarchar(30)` | — | Cor |
| Status | `nvarchar(20)` | NN, CHECK `IN ('Disponivel','Alugado','EmManutencao')` | Situação do veículo |

### 3.4 Clientes

| Coluna | Tipo | Restrições | Descrição |
|---|---|---|---|
| Id | `int` | PK, identity | Identificador |
| Nome | `nvarchar(120)` | NN | Nome completo |
| Cpf | `char(11)` | NN, UK, CHECK 11 dígitos numéricos | CPF sem pontuação |
| Email | `nvarchar(150)` | NN, UK | E-mail |
| Telefone | `nvarchar(20)` | — | Telefone |
| Cnh | `char(11)` | NN, UK, CHECK 11 dígitos numéricos | Registro da CNH |
| DataNascimento | `date` | NN | Data de nascimento |
| DataCadastro | `datetime2` | NN, DEFAULT `GETDATE()` | Preenchida pelo banco |

### 3.5 Alugueis

| Coluna | Tipo | Restrições | Descrição |
|---|---|---|---|
| Id | `int` | PK, identity | Identificador |
| ClienteId | `int` | NN, **FK → Clientes(Id)** | Cliente que alugou |
| VeiculoId | `int` | NN, **FK → Veiculos(Id)** | Veículo alugado |
| DataInicio | `datetime2` | NN | Início do período |
| DataFimPrevista | `datetime2` | NN, CHECK `> DataInicio` | Fim previsto do período |
| DataDevolucao | `datetime2` | CHECK `NULL` ou `>= DataInicio` | **Devolução** do veículo (nula enquanto em andamento) |
| QuilometragemInicial | `int` | NN, CHECK `>= 0` | Km na retirada |
| QuilometragemFinal | `int` | CHECK `NULL` ou `>= QuilometragemInicial` | Km na devolução |
| ValorDiaria | `decimal(10,2)` | NN, CHECK `> 0` | Valor da diária contratada |
| ValorTotal | `decimal(10,2)` | NN, CHECK `>= 0` | Valor total da locação |
| Status | `nvarchar(20)` | NN, CHECK `IN ('EmAndamento','Finalizado','Cancelado')` | Situação do aluguel |
| Observacoes | `nvarchar(500)` | — | Observações |

Índice único filtrado `UX_Alugueis_VeiculoId_EmAndamento` em `VeiculoId WHERE Status = 'EmAndamento'`:
**um veículo só pode ter um aluguel em andamento por vez** — a regra fica garantida pelo próprio banco.

### 3.6 Pagamentos

| Coluna | Tipo | Restrições | Descrição |
|---|---|---|---|
| Id | `int` | PK, identity | Identificador |
| AluguelId | `int` | NN, **FK → Alugueis(Id)** `ON DELETE CASCADE` | Aluguel pago |
| DataPagamento | `datetime2` | NN, DEFAULT `GETDATE()` | Data do pagamento |
| Valor | `decimal(10,2)` | NN, CHECK `> 0` | Valor pago |
| FormaPagamento | `nvarchar(20)` | NN, CHECK `IN ('Dinheiro','CartaoCredito','CartaoDebito','Pix')` | Forma de pagamento |

## 4. Chaves estrangeiras e regra de exclusão

| FK | Referência | Ao excluir o pai | Motivo |
|---|---|---|---|
| `FK_Veiculos_Fabricantes_FabricanteId` | Fabricantes(Id) | `NO ACTION` (Restrict) | Não se apaga um fabricante que ainda tem veículos |
| `FK_Veiculos_Categorias_CategoriaId` | Categorias(Id) | `NO ACTION` (Restrict) | Não se apaga uma categoria em uso |
| `FK_Alugueis_Clientes_ClienteId` | Clientes(Id) | `NO ACTION` (Restrict) | Preserva o histórico de locações do cliente |
| `FK_Alugueis_Veiculos_VeiculoId` | Veiculos(Id) | `NO ACTION` (Restrict) | Preserva o histórico de locações do veículo |
| `FK_Pagamentos_Alugueis_AluguelId` | Alugueis(Id) | `CASCADE` | O pagamento não existe sem o aluguel |

## 5. Decisões de projeto

- **Data Annotations + Fluent API.** O que descreve a própria classe (`[Key]`, `[ForeignKey]`, `[Required]`,
  `[StringLength]`, tipo da coluna) fica nas entidades; o que é regra do banco (índices únicos, CHECKs,
  DEFAULTs e comportamento de exclusão) fica centralizado no `ApplicationContext.OnModelCreating`.
- **Enums gravados como texto** (`HasConversion<string>()`): o dado fica legível em consultas SQL e cada
  coluna tem uma CHECK com os valores aceitos, montada a partir do próprio enum.
- **CPF, CNH e placa em `char` de tamanho fixo**, somente com os caracteres significativos, para que o índice
  único não seja burlado por diferenças de formatação.
- **Validação em duas camadas:** as mesmas regras das CHECK constraints existem como validação do modelo
  (`[Range]`, `[RegularExpression]`, `IValidatableObject` em `Aluguel`), para a API responder `400` antes de
  chegar ao banco; o banco continua sendo a última barreira.
- **`ValorTotal`** é gravado (e não calculado em consulta) porque o enunciado pede o registro do valor total da
  locação: é previsto na abertura e recalculado na devolução.

## 6. Testes de integridade executados

Script SQL executado direto no banco criado pela migration (SQL Server 2025 Express):

| # | Teste | Resultado |
|---|---|---|
| 1 | Inserir fabricante, categoria, veículo, cliente, aluguel e pagamento válidos | ✅ aceito; `DataCadastro` e `DataPagamento` preenchidas pelo DEFAULT |
| 2 | Cliente com CPF já cadastrado | ✅ bloqueado — `IX_Clientes_Cpf` |
| 3 | CPF com letra | ✅ bloqueado — `CK_Clientes_Cpf` |
| 4 | Veículo com fabricante inexistente | ✅ bloqueado — `FK_Veiculos_Fabricantes_FabricanteId` |
| 5 | Veículo com ano de fabricação 1800 | ✅ bloqueado — `CK_Veiculos_AnoFabricacao` |
| 6 | Veículo com status inválido | ✅ bloqueado — `CK_Veiculos_Status` |
| 7 | Placa duplicada | ✅ bloqueado — `IX_Veiculos_Placa` |
| 8 | Aluguel com fim previsto antes do início | ✅ bloqueado — `CK_Alugueis_Periodo` |
| 9 | Quilometragem final menor que a inicial | ✅ bloqueado — `CK_Alugueis_QuilometragemFinal` |
| 10 | Segundo aluguel em andamento para o mesmo veículo | ✅ bloqueado — `UX_Alugueis_VeiculoId_EmAndamento` |
| 11 | Registrar a devolução e alugar o mesmo veículo de novo | ✅ aceito |
| 12 | Excluir fabricante que possui veículos | ✅ bloqueado — FK com `NO ACTION` |
| 13 | Excluir aluguel com pagamentos | ✅ pagamentos removidos em cascata |
| 14 | Pagamento com valor zero | ✅ bloqueado — `CK_Pagamentos_Valor` |

> Observação para quem usar `sqlcmd`: tabelas com índice filtrado exigem `SET QUOTED_IDENTIFIER ON`
> (flag `-I`). No SSMS e no Entity Framework essa opção já vem ligada.

O DDL completo gerado pelo Entity Framework está em [`script-criacao-banco.sql`](script-criacao-banco.sql).
