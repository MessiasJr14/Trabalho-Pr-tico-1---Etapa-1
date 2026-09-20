using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LocadoraVeiculos.Models
{
    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(120)]
        public string? Nome { get; set; }

        [Required]
        [StringLength(11, MinimumLength = 11)]
        public string? Cpf { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? Telefone { get; set; }

        [MaxLength(11)]
        public string? Cnh { get; set; }

        [JsonIgnore]
        public ICollection<Aluguel> Alugueis { get; set; } = new List<Aluguel>();
    }
}
