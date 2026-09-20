using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LocadoraVeiculos.Models
{
    public class Fabricante
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(80)]
        public string? Nome { get; set; }

        [MaxLength(60)]
        public string? PaisOrigem { get; set; }

        [JsonIgnore]
        public ICollection<Veiculo> Veiculos { get; set; } = new List<Veiculo>();
    }
}
