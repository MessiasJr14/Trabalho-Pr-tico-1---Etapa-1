using LocadoraVeiculos.Data;
using LocadoraVeiculos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public CategoriasController(ApplicationContext context)
        {
            _context = context;
        }

        // GET api/categorias?nome=suv&valorMin=100&valorMax=300
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Categoria>>> GetCategorias(string? nome, decimal? valorMin, decimal? valorMax)
        {
            var consulta = _context.Categorias.AsQueryable();

            if (!string.IsNullOrWhiteSpace(nome))
            {
                consulta = consulta.Where(c => c.Nome!.Contains(nome));
            }

            if (valorMin.HasValue)
            {
                consulta = consulta.Where(c => c.ValorDiaria >= valorMin.Value);
            }

            if (valorMax.HasValue)
            {
                consulta = consulta.Where(c => c.ValorDiaria <= valorMax.Value);
            }

            return await consulta.OrderBy(c => c.Nome).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Categoria>> GetCategoria(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);

            if (categoria == null)
            {
                return NotFound("Categoria não encontrada.");
            }

            return categoria;
        }

        [HttpPost]
        public async Task<ActionResult<Categoria>> PostCategoria(Categoria categoria)
        {
            try
            {
                _context.Categorias.Add(categoria);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Erro ao salvar a categoria no banco de dados.");
            }

            return CreatedAtAction(nameof(GetCategoria), new { id = categoria.Id }, categoria);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategoria(int id, Categoria categoria)
        {
            var categoriaExistente = await _context.Categorias.FindAsync(id);

            if (categoriaExistente == null)
            {
                return NotFound("Categoria não encontrada.");
            }

            categoriaExistente.Nome = categoria.Nome;
            categoriaExistente.Descricao = categoria.Descricao;
            categoriaExistente.ValorDiaria = categoria.ValorDiaria;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Erro ao atualizar a categoria no banco de dados.");
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategoria(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);

            if (categoria == null)
            {
                return NotFound("Categoria não encontrada.");
            }

            if (await _context.Veiculos.AnyAsync(v => v.CategoriaId == id))
            {
                return BadRequest("Não é possível excluir uma categoria que possui veículos cadastrados.");
            }

            try
            {
                _context.Categorias.Remove(categoria);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Erro ao excluir a categoria no banco de dados.");
            }

            return NoContent();
        }
    }
}
