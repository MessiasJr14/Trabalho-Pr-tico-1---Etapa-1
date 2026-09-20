using LocadoraVeiculos.Data;
using LocadoraVeiculos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlugueisController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public AlugueisController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Aluguel>>> GetAlugueis()
        {
            return await _context.Alugueis.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Aluguel>> GetAluguel(int id)
        {
            var aluguel = await _context.Alugueis.FindAsync(id);

            if (aluguel == null)
            {
                return NotFound("Aluguel não encontrado.");
            }

            return aluguel;
        }

        [HttpGet("cliente/{cpf}")]
        public async Task<IActionResult> GetAlugueisPorCliente(string cpf)
        {
            var alugueis = await (from a in _context.Alugueis
                                  join c in _context.Clientes on a.ClienteId equals c.Id
                                  join v in _context.Veiculos on a.VeiculoId equals v.Id
                                  where c.Cpf == cpf
                                  select new
                                  {
                                      a.Id,
                                      Cliente = c.Nome,
                                      c.Cpf,
                                      Veiculo = v.Modelo,
                                      v.Placa,
                                      a.DataInicio,
                                      a.DataFim,
                                      a.DataDevolucao,
                                      a.ValorTotal
                                  }).ToListAsync();

            return Ok(alugueis);
        }

        [HttpGet("abertos")]
        public async Task<IActionResult> GetAlugueisEmAberto()
        {
            var alugueis = await (from a in _context.Alugueis
                                  join c in _context.Clientes on a.ClienteId equals c.Id
                                  join v in _context.Veiculos on a.VeiculoId equals v.Id
                                  where a.DataDevolucao == null
                                  select new
                                  {
                                      a.Id,
                                      Cliente = c.Nome,
                                      c.Telefone,
                                      Veiculo = v.Modelo,
                                      v.Placa,
                                      a.DataInicio,
                                      a.DataFim,
                                      a.ValorDiaria
                                  }).ToListAsync();

            return Ok(alugueis);
        }

        [HttpPost]
        public async Task<ActionResult<Aluguel>> PostAluguel(Aluguel aluguel)
        {
            var cliente = await _context.Clientes.FindAsync(aluguel.ClienteId);

            if (cliente == null)
            {
                return BadRequest("Cliente informado não existe.");
            }

            var veiculo = await _context.Veiculos.FindAsync(aluguel.VeiculoId);

            if (veiculo == null)
            {
                return BadRequest("Veículo informado não existe.");
            }

            if (!veiculo.Disponivel)
            {
                return BadRequest("Veículo indisponível para aluguel.");
            }

            if (aluguel.DataFim <= aluguel.DataInicio)
            {
                return BadRequest("A data de fim deve ser maior que a data de início.");
            }

            var categoria = await _context.Categorias.FindAsync(veiculo.CategoriaId);

            int dias = (aluguel.DataFim.Date - aluguel.DataInicio.Date).Days;

            if (dias < 1)
            {
                dias = 1;
            }

            aluguel.QuilometragemInicial = veiculo.Quilometragem;
            aluguel.QuilometragemFinal = null;
            aluguel.DataDevolucao = null;
            aluguel.ValorDiaria = categoria!.ValorDiaria;
            aluguel.ValorTotal = dias * aluguel.ValorDiaria;

            veiculo.Disponivel = false;

            try
            {
                _context.Alugueis.Add(aluguel);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Erro ao salvar o aluguel no banco de dados.");
            }

            return CreatedAtAction(nameof(GetAluguel), new { id = aluguel.Id }, aluguel);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutAluguel(int id, Aluguel aluguel)
        {
            var aluguelExistente = await _context.Alugueis.FindAsync(id);

            if (aluguelExistente == null)
            {
                return NotFound("Aluguel não encontrado.");
            }

            if (aluguelExistente.DataDevolucao != null)
            {
                return BadRequest("Não é possível alterar um aluguel que já foi devolvido.");
            }

            if (aluguel.DataFim <= aluguel.DataInicio)
            {
                return BadRequest("A data de fim deve ser maior que a data de início.");
            }

            int dias = (aluguel.DataFim.Date - aluguel.DataInicio.Date).Days;

            if (dias < 1)
            {
                dias = 1;
            }

            aluguelExistente.DataInicio = aluguel.DataInicio;
            aluguelExistente.DataFim = aluguel.DataFim;
            aluguelExistente.ValorTotal = dias * aluguelExistente.ValorDiaria;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Erro ao atualizar o aluguel no banco de dados.");
            }

            return NoContent();
        }

        [HttpPut("{id}/devolucao")]
        public async Task<IActionResult> PutDevolucao(int id, int quilometragemFinal, DateTime? dataDevolucao)
        {
            var aluguel = await _context.Alugueis.FindAsync(id);

            if (aluguel == null)
            {
                return NotFound("Aluguel não encontrado.");
            }

            if (aluguel.DataDevolucao != null)
            {
                return BadRequest("Este aluguel já foi devolvido.");
            }

            if (quilometragemFinal < aluguel.QuilometragemInicial)
            {
                return BadRequest("A quilometragem final não pode ser menor que a quilometragem inicial.");
            }

            DateTime devolucao = dataDevolucao ?? DateTime.Now;

            if (devolucao < aluguel.DataInicio)
            {
                return BadRequest("A data de devolução não pode ser anterior à data de início.");
            }

            int dias = (devolucao.Date - aluguel.DataInicio.Date).Days;

            if (dias < 1)
            {
                dias = 1;
            }

            aluguel.DataDevolucao = devolucao;
            aluguel.QuilometragemFinal = quilometragemFinal;
            aluguel.ValorTotal = dias * aluguel.ValorDiaria;

            var veiculo = await _context.Veiculos.FindAsync(aluguel.VeiculoId);
            veiculo!.Quilometragem = quilometragemFinal;
            veiculo.Disponivel = true;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Erro ao registrar a devolução no banco de dados.");
            }

            return Ok(aluguel);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAluguel(int id)
        {
            var aluguel = await _context.Alugueis.FindAsync(id);

            if (aluguel == null)
            {
                return NotFound("Aluguel não encontrado.");
            }

            if (aluguel.DataDevolucao == null)
            {
                var veiculo = await _context.Veiculos.FindAsync(aluguel.VeiculoId);
                veiculo!.Disponivel = true;
            }

            try
            {
                _context.Alugueis.Remove(aluguel);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Erro ao excluir o aluguel no banco de dados.");
            }

            return NoContent();
        }
    }
}
