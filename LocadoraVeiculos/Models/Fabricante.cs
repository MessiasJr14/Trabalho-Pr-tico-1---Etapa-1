using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LocadoraVeiculos.Models
{
    /// <summary>
    /// Fabricante (marca) dos veículos da locadora. Ex.: Fiat, Volkswagen, Toyota.
    /// </summary>
    [Table("Fabricantes")]
    public class Fabricante
    {
        // Chave primária
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do fabricante é obrigatório.")]
        [StringLength(80, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 80 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [StringLength(60, ErrorMessage = "O país de origem deve ter no máximo 60 caracteres.")]
        public string? PaisOrigem { get; set; }

        // Relacionamento 1:N -> um fabricante possui vários veículos
        [JsonIgnore]
        public ICollection<Veiculo> Veiculos { get; set; } = new List<Veiculo>();
    }
}
