using LocadoraVeiculos.Data;
using LocadoraVeiculos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FabricantesController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public FabricantesController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Fabricante>>> GetFabricantes()
        {
            return await _context.Fabricantes.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Fabricante>> GetFabricante(int id)
        {
            var fabricante = await _context.Fabricantes.FindAsync(id);

            if (fabricante == null)
            {
                return NotFound("Fabricante não encontrado.");
            }

            return fabricante;
        }

        [HttpPost]
        public async Task<ActionResult<Fabricante>> PostFabricante(Fabricante fabricante)
        {
            try
            {
                _context.Fabricantes.Add(fabricante);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Erro ao salvar o fabricante no banco de dados.");
            }

            return CreatedAtAction(nameof(GetFabricante), new { id = fabricante.Id }, fabricante);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutFabricante(int id, Fabricante fabricante)
        {
            var fabricanteExistente = await _context.Fabricantes.FindAsync(id);

            if (fabricanteExistente == null)
            {
                return NotFound("Fabricante não encontrado.");
            }

            fabricanteExistente.Nome = fabricante.Nome;
            fabricanteExistente.PaisOrigem = fabricante.PaisOrigem;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Erro ao atualizar o fabricante no banco de dados.");
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFabricante(int id)
        {
            var fabricante = await _context.Fabricantes.FindAsync(id);

            if (fabricante == null)
            {
                return NotFound("Fabricante não encontrado.");
            }

            if (await _context.Veiculos.AnyAsync(v => v.FabricanteId == id))
            {
                return BadRequest("Não é possível excluir um fabricante que possui veículos cadastrados.");
            }

            try
            {
                _context.Fabricantes.Remove(fabricante);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Erro ao excluir o fabricante no banco de dados.");
            }

            return NoContent();
        }
    }
}
