using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocadoraVeiculos.Models
{
    public class Categoria
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Nome { get; set; }

        [MaxLength(200)]
        public string? Descricao { get; set; }

        [Range(0.01, 100000)]
        [Column(TypeName = "decimal(10,2)")]
        public decimal ValorDiaria { get; set; }

        [JsonIgnore]
        public ICollection<Veiculo> Veiculos { get; set; } = new List<Veiculo>();
    }
}
