using LocadoraVeiculos.Models;
using LocadoraVeiculos.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Data
{
    /// <summary>
    /// Contexto do Entity Framework Core: mapeia as classes da camada Model
    /// para as tabelas do banco de dados SQL Server (SQL Express).
    /// </summary>
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
        }

        // Cada DbSet representa uma tabela do banco
        public DbSet<Fabricante> Fabricantes { get; set; } = null!;
        public DbSet<Categoria> Categorias { get; set; } = null!;
        public DbSet<Veiculo> Veiculos { get; set; } = null!;
        public DbSet<Cliente> Clientes { get; set; } = null!;
        public DbSet<Aluguel> Alugueis { get; set; } = null!;
        public DbSet<Pagamento> Pagamentos { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---------------------------------------------------------------
            // FABRICANTES
            // ---------------------------------------------------------------
            modelBuilder.Entity<Fabricante>(entity =>
            {
                entity.HasKey(f => f.Id);

                // Não permite dois fabricantes com o mesmo nome
                entity.HasIndex(f => f.Nome).IsUnique();
            });

            // ---------------------------------------------------------------
            // CATEGORIAS
            // ---------------------------------------------------------------
            modelBuilder.Entity<Categoria>(entity =>
            {
                entity.ToTable(t =>
                    t.HasCheckConstraint("CK_Categorias_ValorDiariaBase", "[ValorDiariaBase] > 0"));

                entity.HasKey(c => c.Id);

                entity.HasIndex(c => c.Nome).IsUnique();
            });

            // ---------------------------------------------------------------
            // VEICULOS
            // ---------------------------------------------------------------
            modelBuilder.Entity<Veiculo>(entity =>
            {
                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Veiculos_AnoFabricacao", "[AnoFabricacao] BETWEEN 1950 AND 2100");
                    t.HasCheckConstraint("CK_Veiculos_Quilometragem", "[Quilometragem] >= 0");
                    t.HasCheckConstraint("CK_Veiculos_Placa", "LEN([Placa]) = 7");
                    t.HasCheckConstraint("CK_Veiculos_Status", $"[Status] IN ({ValoresDoEnum<StatusVeiculo>()})");
                });

                entity.HasKey(v => v.Id);

                // A placa identifica o veículo no mundo real: não pode repetir
                entity.HasIndex(v => v.Placa).IsUnique();

                // Enum gravado como texto para o dado ficar legível no banco
                entity.Property(v => v.Status)
                      .HasConversion<string>()
                      .HasMaxLength(20);

                // FK: todo veículo pertence a um fabricante (N:1)
                entity.HasOne(v => v.Fabricante)
                      .WithMany(f => f.Veiculos)
                      .HasForeignKey(v => v.FabricanteId)
                      .OnDelete(DeleteBehavior.Restrict);

                // FK: todo veículo pertence a uma categoria (N:1)
                entity.HasOne(v => v.Categoria)
                      .WithMany(c => c.Veiculos)
                      .HasForeignKey(v => v.CategoriaId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------------------------------------------------------------
            // CLIENTES
            // ---------------------------------------------------------------
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Clientes_Cpf", "LEN([Cpf]) = 11 AND [Cpf] NOT LIKE '%[^0-9]%'");
                    t.HasCheckConstraint("CK_Clientes_Cnh", "LEN([Cnh]) = 11 AND [Cnh] NOT LIKE '%[^0-9]%'");
                });

                entity.HasKey(c => c.Id);

                // CPF, e-mail e CNH não podem se repetir entre clientes
                entity.HasIndex(c => c.Cpf).IsUnique();
                entity.HasIndex(c => c.Email).IsUnique();
                entity.HasIndex(c => c.Cnh).IsUnique();

                entity.Property(c => c.DataCadastro)
                      .HasDefaultValueSql("GETDATE()");
            });

            // ---------------------------------------------------------------
            // ALUGUEIS
            // ---------------------------------------------------------------
            modelBuilder.Entity<Aluguel>(entity =>
            {
                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Alugueis_Periodo", "[DataFimPrevista] > [DataInicio]");
                    t.HasCheckConstraint("CK_Alugueis_DataDevolucao", "[DataDevolucao] IS NULL OR [DataDevolucao] >= [DataInicio]");
                    t.HasCheckConstraint("CK_Alugueis_QuilometragemInicial", "[QuilometragemInicial] >= 0");
                    t.HasCheckConstraint("CK_Alugueis_QuilometragemFinal", "[QuilometragemFinal] IS NULL OR [QuilometragemFinal] >= [QuilometragemInicial]");
                    t.HasCheckConstraint("CK_Alugueis_ValorDiaria", "[ValorDiaria] > 0");
                    t.HasCheckConstraint("CK_Alugueis_ValorTotal", "[ValorTotal] >= 0");
                    t.HasCheckConstraint("CK_Alugueis_Status", $"[Status] IN ({ValoresDoEnum<StatusAluguel>()})");
                });

                entity.HasKey(a => a.Id);

                // Enum gravado como texto, igual ao status do veículo
                entity.Property(a => a.Status)
                      .HasConversion<string>()
                      .HasMaxLength(20);

                // FK: todo aluguel está atrelado a um cliente (N:1)
                entity.HasOne(a => a.Cliente)
                      .WithMany(c => c.Alugueis)
                      .HasForeignKey(a => a.ClienteId)
                      .OnDelete(DeleteBehavior.Restrict);

                // FK: todo aluguel está atrelado a um veículo (N:1)
                entity.HasOne(a => a.Veiculo)
                      .WithMany(v => v.Alugueis)
                      .HasForeignKey(a => a.VeiculoId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(a => a.VeiculoId, "IX_Alugueis_VeiculoId");

                // Um veículo só pode ter UM aluguel em andamento por vez
                entity.HasIndex(a => a.VeiculoId, "UX_Alugueis_VeiculoId_EmAndamento")
                      .IsUnique()
                      .HasFilter($"[Status] = '{nameof(StatusAluguel.EmAndamento)}'");
            });

            // ---------------------------------------------------------------
            // PAGAMENTOS
            // ---------------------------------------------------------------
            modelBuilder.Entity<Pagamento>(entity =>
            {
                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Pagamentos_Valor", "[Valor] > 0");
                    t.HasCheckConstraint("CK_Pagamentos_FormaPagamento", $"[FormaPagamento] IN ({ValoresDoEnum<FormaPagamento>()})");
                });

                entity.HasKey(p => p.Id);

                entity.Property(p => p.FormaPagamento)
                      .HasConversion<string>()
                      .HasMaxLength(20);

                entity.Property(p => p.DataPagamento)
                      .HasDefaultValueSql("GETDATE()");

                // FK: todo pagamento pertence a um aluguel (N:1).
                // Cascade: os pagamentos não existem sem o aluguel.
                entity.HasOne(p => p.Aluguel)
                      .WithMany(a => a.Pagamentos)
                      .HasForeignKey(p => p.AluguelId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        // Monta a lista 'A', 'B', 'C' com os nomes do enum para usar nas CHECK constraints
        private static string ValoresDoEnum<TEnum>() where TEnum : struct, Enum
        {
            return string.Join(", ", Enum.GetNames<TEnum>().Select(nome => $"'{nome}'"));
        }
    }
}
