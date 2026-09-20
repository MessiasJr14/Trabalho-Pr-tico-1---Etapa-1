IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Categorias] (
    [Id] int NOT NULL IDENTITY,
    [Nome] nvarchar(50) NOT NULL,
    [Descricao] nvarchar(200) NULL,
    [ValorDiariaBase] decimal(10,2) NOT NULL,
    CONSTRAINT [PK_Categorias] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Categorias_ValorDiariaBase] CHECK ([ValorDiariaBase] > 0)
);

CREATE TABLE [Clientes] (
    [Id] int NOT NULL IDENTITY,
    [Nome] nvarchar(120) NOT NULL,
    [Cpf] char(11) NOT NULL,
    [Email] nvarchar(150) NOT NULL,
    [Telefone] nvarchar(20) NULL,
    [Cnh] char(11) NOT NULL,
    [DataNascimento] date NOT NULL,
    [DataCadastro] datetime2 NOT NULL DEFAULT (GETDATE()),
    CONSTRAINT [PK_Clientes] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Clientes_Cnh] CHECK (LEN([Cnh]) = 11 AND [Cnh] NOT LIKE '%[^0-9]%'),
    CONSTRAINT [CK_Clientes_Cpf] CHECK (LEN([Cpf]) = 11 AND [Cpf] NOT LIKE '%[^0-9]%')
);

CREATE TABLE [Fabricantes] (
    [Id] int NOT NULL IDENTITY,
    [Nome] nvarchar(80) NOT NULL,
    [PaisOrigem] nvarchar(60) NULL,
    CONSTRAINT [PK_Fabricantes] PRIMARY KEY ([Id])
);

CREATE TABLE [Veiculos] (
    [Id] int NOT NULL IDENTITY,
    [FabricanteId] int NOT NULL,
    [CategoriaId] int NOT NULL,
    [Modelo] nvarchar(80) NOT NULL,
    [AnoFabricacao] int NOT NULL,
    [Quilometragem] int NOT NULL,
    [Placa] char(7) NOT NULL,
    [Cor] nvarchar(30) NULL,
    [Status] nvarchar(20) NOT NULL,
    CONSTRAINT [PK_Veiculos] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Veiculos_AnoFabricacao] CHECK ([AnoFabricacao] BETWEEN 1950 AND 2100),
    CONSTRAINT [CK_Veiculos_Placa] CHECK (LEN([Placa]) = 7),
    CONSTRAINT [CK_Veiculos_Quilometragem] CHECK ([Quilometragem] >= 0),
    CONSTRAINT [CK_Veiculos_Status] CHECK ([Status] IN ('Disponivel', 'Alugado', 'EmManutencao')),
    CONSTRAINT [FK_Veiculos_Categorias_CategoriaId] FOREIGN KEY ([CategoriaId]) REFERENCES [Categorias] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Veiculos_Fabricantes_FabricanteId] FOREIGN KEY ([FabricanteId]) REFERENCES [Fabricantes] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Alugueis] (
    [Id] int NOT NULL IDENTITY,
    [ClienteId] int NOT NULL,
    [VeiculoId] int NOT NULL,
    [DataInicio] datetime2 NOT NULL,
    [DataFimPrevista] datetime2 NOT NULL,
    [DataDevolucao] datetime2 NULL,
    [QuilometragemInicial] int NOT NULL,
    [QuilometragemFinal] int NULL,
    [ValorDiaria] decimal(10,2) NOT NULL,
    [ValorTotal] decimal(10,2) NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [Observacoes] nvarchar(500) NULL,
    CONSTRAINT [PK_Alugueis] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Alugueis_DataDevolucao] CHECK ([DataDevolucao] IS NULL OR [DataDevolucao] >= [DataInicio]),
    CONSTRAINT [CK_Alugueis_Periodo] CHECK ([DataFimPrevista] > [DataInicio]),
    CONSTRAINT [CK_Alugueis_QuilometragemFinal] CHECK ([QuilometragemFinal] IS NULL OR [QuilometragemFinal] >= [QuilometragemInicial]),
    CONSTRAINT [CK_Alugueis_QuilometragemInicial] CHECK ([QuilometragemInicial] >= 0),
    CONSTRAINT [CK_Alugueis_Status] CHECK ([Status] IN ('EmAndamento', 'Finalizado', 'Cancelado')),
    CONSTRAINT [CK_Alugueis_ValorDiaria] CHECK ([ValorDiaria] > 0),
    CONSTRAINT [CK_Alugueis_ValorTotal] CHECK ([ValorTotal] >= 0),
    CONSTRAINT [FK_Alugueis_Clientes_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Clientes] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Alugueis_Veiculos_VeiculoId] FOREIGN KEY ([VeiculoId]) REFERENCES [Veiculos] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Pagamentos] (
    [Id] int NOT NULL IDENTITY,
    [AluguelId] int NOT NULL,
    [DataPagamento] datetime2 NOT NULL DEFAULT (GETDATE()),
    [Valor] decimal(10,2) NOT NULL,
    [FormaPagamento] nvarchar(20) NOT NULL,
    CONSTRAINT [PK_Pagamentos] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Pagamentos_FormaPagamento] CHECK ([FormaPagamento] IN ('Dinheiro', 'CartaoCredito', 'CartaoDebito', 'Pix')),
    CONSTRAINT [CK_Pagamentos_Valor] CHECK ([Valor] > 0),
    CONSTRAINT [FK_Pagamentos_Alugueis_AluguelId] FOREIGN KEY ([AluguelId]) REFERENCES [Alugueis] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_Alugueis_ClienteId] ON [Alugueis] ([ClienteId]);

CREATE INDEX [IX_Alugueis_VeiculoId] ON [Alugueis] ([VeiculoId]);

CREATE UNIQUE INDEX [UX_Alugueis_VeiculoId_EmAndamento] ON [Alugueis] ([VeiculoId]) WHERE [Status] = 'EmAndamento';

CREATE UNIQUE INDEX [IX_Categorias_Nome] ON [Categorias] ([Nome]);

CREATE UNIQUE INDEX [IX_Clientes_Cnh] ON [Clientes] ([Cnh]);

CREATE UNIQUE INDEX [IX_Clientes_Cpf] ON [Clientes] ([Cpf]);

CREATE UNIQUE INDEX [IX_Clientes_Email] ON [Clientes] ([Email]);

CREATE UNIQUE INDEX [IX_Fabricantes_Nome] ON [Fabricantes] ([Nome]);

CREATE INDEX [IX_Pagamentos_AluguelId] ON [Pagamentos] ([AluguelId]);

CREATE INDEX [IX_Veiculos_CategoriaId] ON [Veiculos] ([CategoriaId]);

CREATE INDEX [IX_Veiculos_FabricanteId] ON [Veiculos] ([FabricanteId]);

CREATE UNIQUE INDEX [IX_Veiculos_Placa] ON [Veiculos] ([Placa]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260920204926_CriacaoInicial', N'10.0.12');

COMMIT;
GO

