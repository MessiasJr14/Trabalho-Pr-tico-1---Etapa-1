using LocadoraVeiculos.Data;
using LocadoraVeiculos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public ClientesController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cliente>>> GetClientes()
        {
            return await _context.Clientes.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Cliente>> GetCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
            {
                return NotFound("Cliente não encontrado.");
            }

            return cliente;
        }

        [HttpGet("sem-aluguel")]
        public async Task<IActionResult> GetClientesSemAluguel()
        {
            var clientes = await (from c in _context.Clientes
                                  join a in _context.Alugueis on c.Id equals a.ClienteId into alugueisDoCliente
                                  from a in alugueisDoCliente.DefaultIfEmpty()
                                  where a == null
                                  select new
                                  {
                                      c.Id,
                                      c.Nome,
                                      c.Cpf,
                                      c.Email
                                  }).ToListAsync();

            return Ok(clientes);
        }

        [HttpPost]
        public async Task<ActionResult<Cliente>> PostCliente(Cliente cliente)
        {
            if (!cliente.Cpf!.All(char.IsDigit))
            {
                return BadRequest("O CPF deve conter apenas números.");
            }

            if (await _context.Clientes.AnyAsync(c => c.Cpf == cliente.Cpf))
            {
                return Conflict("Já existe um cliente cadastrado com este CPF.");
            }

            if (await _context.Clientes.AnyAsync(c => c.Email == cliente.Email))
            {
                return Conflict("Já existe um cliente cadastrado com este e-mail.");
            }

            try
            {
                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Erro ao salvar o cliente no banco de dados.");
            }

            return CreatedAtAction(nameof(GetCliente), new { id = cliente.Id }, cliente);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCliente(int id, Cliente cliente)
        {
            var clienteExistente = await _context.Clientes.FindAsync(id);

            if (clienteExistente == null)
            {
                return NotFound("Cliente não encontrado.");
            }

            if (!cliente.Cpf!.All(char.IsDigit))
            {
                return BadRequest("O CPF deve conter apenas números.");
            }

            if (await _context.Clientes.AnyAsync(c => c.Cpf == cliente.Cpf && c.Id != id))
            {
                return Conflict("Já existe um cliente cadastrado com este CPF.");
            }

            if (await _context.Clientes.AnyAsync(c => c.Email == cliente.Email && c.Id != id))
            {
                return Conflict("Já existe um cliente cadastrado com este e-mail.");
            }

            clienteExistente.Nome = cliente.Nome;
            clienteExistente.Cpf = cliente.Cpf;
            clienteExistente.Email = cliente.Email;
            clienteExistente.Telefone = cliente.Telefone;
            clienteExistente.Cnh = cliente.Cnh;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Erro ao atualizar o cliente no banco de dados.");
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
            {
                return NotFound("Cliente não encontrado.");
            }

            if (await _context.Alugueis.AnyAsync(a => a.ClienteId == id))
            {
                return BadRequest("Não é possível excluir um cliente que possui aluguéis registrados.");
            }

            try
            {
                _context.Clientes.Remove(cliente);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Erro ao excluir o cliente no banco de dados.");
            }

            return NoContent();
        }
    }
}
