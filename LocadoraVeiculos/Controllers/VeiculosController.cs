using LocadoraVeiculos.Data;
using LocadoraVeiculos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VeiculosController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public VeiculosController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Veiculo>>> GetVeiculos()
        {
            return await _context.Veiculos.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Veiculo>> GetVeiculo(int id)
        {
            var veiculo = await _context.Veiculos.FindAsync(id);

            if (veiculo == null)
            {
                return NotFound("Veículo não encontrado.");
            }

            return veiculo;
        }

        [HttpGet("fabricante/{nome}")]
        public async Task<IActionResult> GetVeiculosPorFabricante(string nome)
        {
            var veiculos = await (from v in _context.Veiculos
                                  join f in _context.Fabricantes on v.FabricanteId equals f.Id
                                  where f.Nome!.Contains(nome)
                                  select new
                                  {
                                      v.Id,
                                      v.Modelo,
                                      v.AnoFabricacao,
                                      v.Placa,
                                      v.Quilometragem,
                                      Fabricante = f.Nome
                                  }).ToListAsync();

            return Ok(veiculos);
        }

        [HttpGet("disponiveis")]
        public async Task<IActionResult> GetVeiculosDisponiveis(string? categoria)
        {
            var consulta = from v in _context.Veiculos
                           join c in _context.Categorias on v.CategoriaId equals c.Id
                           join f in _context.Fabricantes on v.FabricanteId equals f.Id
                           where v.Disponivel
                           select new
                           {
                               v.Id,
                               v.Modelo,
                               v.Placa,
                               Fabricante = f.Nome,
                               Categoria = c.Nome,
                               c.ValorDiaria
                           };

            if (!string.IsNullOrEmpty(categoria))
            {
                consulta = consulta.Where(x => x.Categoria!.Contains(categoria));
            }

            return Ok(await consulta.ToListAsync());
        }

        [HttpPost]
        public async Task<ActionResult<Veiculo>> PostVeiculo(Veiculo veiculo)
        {
            if (!await _context.Fabricantes.AnyAsync(f => f.Id == veiculo.FabricanteId))
            {
                return BadRequest("Fabricante informado não existe.");
            }

            if (!await _context.Categorias.AnyAsync(c => c.Id == veiculo.CategoriaId))
            {
                return BadRequest("Categoria informada não existe.");
            }

            if (await _context.Veiculos.AnyAsync(v => v.Placa == veiculo.Placa))
            {
                return Conflict("Já existe um veículo cadastrado com esta placa.");
            }

            try
            {
                _context.Veiculos.Add(veiculo);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Erro ao salvar o veículo no banco de dados.");
            }

            return CreatedAtAction(nameof(GetVeiculo), new { id = veiculo.Id }, veiculo);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutVeiculo(int id, Veiculo veiculo)
        {
            var veiculoExistente = await _context.Veiculos.FindAsync(id);

            if (veiculoExistente == null)
            {
                return NotFound("Veículo não encontrado.");
            }

            if (!await _context.Fabricantes.AnyAsync(f => f.Id == veiculo.FabricanteId))
            {
                return BadRequest("Fabricante informado não existe.");
            }

            if (!await _context.Categorias.AnyAsync(c => c.Id == veiculo.CategoriaId))
            {
                return BadRequest("Categoria informada não existe.");
            }

            if (await _context.Veiculos.AnyAsync(v => v.Placa == veiculo.Placa && v.Id != id))
            {
                return Conflict("Já existe um veículo cadastrado com esta placa.");
            }

            veiculoExistente.Modelo = veiculo.Modelo;
            veiculoExistente.AnoFabricacao = veiculo.AnoFabricacao;
            veiculoExistente.Quilometragem = veiculo.Quilometragem;
            veiculoExistente.Placa = veiculo.Placa;
            veiculoExistente.Cor = veiculo.Cor;
            veiculoExistente.Disponivel = veiculo.Disponivel;
            veiculoExistente.FabricanteId = veiculo.FabricanteId;
            veiculoExistente.CategoriaId = veiculo.CategoriaId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Erro ao atualizar o veículo no banco de dados.");
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVeiculo(int id)
        {
            var veiculo = await _context.Veiculos.FindAsync(id);

            if (veiculo == null)
            {
                return NotFound("Veículo não encontrado.");
            }

            if (await _context.Alugueis.AnyAsync(a => a.VeiculoId == id))
            {
                return BadRequest("Não é possível excluir um veículo que possui aluguéis registrados.");
            }

            try
            {
                _context.Veiculos.Remove(veiculo);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Erro ao excluir o veículo no banco de dados.");
            }

            return NoContent();
        }
    }
}
