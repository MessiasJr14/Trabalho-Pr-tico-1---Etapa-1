using LocadoraVeiculos.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace LocadoraVeiculos.Controllers
{
    /// <summary>
    /// Endpoints de apoio da Etapa 1: conferem a conexão com o SQL Server e o mapeamento
    /// feito pelo Entity Framework (tabelas, chaves primárias e chaves estrangeiras).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BancoController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public BancoController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: api/Banco/status
        /// <summary>Verifica a conexão com o banco e conta os registros de cada tabela.</summary>
        /// <response code="200">Banco conectado.</response>
        /// <response code="503">Não foi possível conectar ao SQL Server.</response>
        [HttpGet("status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> GetStatus()
        {
            if (!await _context.Database.CanConnectAsync())
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    conectado = false,
                    mensagem = "Não foi possível conectar ao SQL Server. Confira a connection string 'DefaultConnection' no appsettings.json."
                });
            }

            var conexao = _context.Database.GetDbConnection();

            return Ok(new
            {
                conectado = true,
                servidor = conexao.DataSource,
                banco = conexao.Database,
                migrationsAplicadas = await _context.Database.GetAppliedMigrationsAsync(),
                registros = new
                {
                    fabricantes = await _context.Fabricantes.CountAsync(),
                    categorias = await _context.Categorias.CountAsync(),
                    veiculos = await _context.Veiculos.CountAsync(),
                    clientes = await _context.Clientes.CountAsync(),
                    alugueis = await _context.Alugueis.CountAsync(),
                    pagamentos = await _context.Pagamentos.CountAsync()
                }
            });
        }

        // GET: api/Banco/modelo
        /// <summary>Lista as tabelas mapeadas pelo ApplicationContext com colunas, chaves primárias, chaves estrangeiras, índices únicos e CHECK constraints.</summary>
        /// <response code="200">Modelo relacional gerado pelo Entity Framework.</response>
        [HttpGet("modelo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetModelo()
        {
            // O modelo de design-time guarda também as CHECK constraints e os filtros de índice
            var modelo = _context.GetService<IDesignTimeModel>().Model;

            var tabelas = modelo.GetEntityTypes()
                .OrderBy(entidade => entidade.GetTableName())
                .Select(entidade => new
                {
                    entidade = entidade.ClrType.Name,
                    tabela = entidade.GetTableName(),
                    chavePrimaria = entidade.FindPrimaryKey()!.Properties.Select(p => p.GetColumnName()),
                    chavesEstrangeiras = entidade.GetForeignKeys().Select(fk => new
                    {
                        coluna = string.Join(", ", fk.Properties.Select(p => p.GetColumnName())),
                        referencia = $"{fk.PrincipalEntityType.GetTableName()}({string.Join(", ", fk.PrincipalKey.Properties.Select(p => p.GetColumnName()))})",
                        aoExcluir = fk.DeleteBehavior.ToString()
                    }),
                    indicesUnicos = entidade.GetIndexes()
                        .Where(indice => indice.IsUnique)
                        .Select(indice => new
                        {
                            colunas = string.Join(", ", indice.Properties.Select(p => p.GetColumnName())),
                            filtro = indice.GetFilter()
                        }),
                    restricoesCheck = entidade.GetCheckConstraints().Select(check => new
                    {
                        nome = check.Name,
                        regra = check.Sql
                    }),
                    colunas = entidade.GetProperties().Select(p => new
                    {
                        nome = p.GetColumnName(),
                        tipo = p.GetColumnType(),
                        obrigatoria = !p.IsNullable
                    })
                });

            return Ok(tabelas);
        }
    }
}
