using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocadoraVeiculos.Models
{
    public class Veiculo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(80)]
        public string? Modelo { get; set; }

        [Range(1950, 2100)]
        public int AnoFabricacao { get; set; }

        [Range(0, int.MaxValue)]
        public int Quilometragem { get; set; }

        [Required]
        [MaxLength(8)]
        public string? Placa { get; set; }

        [MaxLength(30)]
        public string? Cor { get; set; }

        public bool Disponivel { get; set; } = true;

        public int FabricanteId { get; set; }

        [JsonIgnore]
        [ForeignKey("FabricanteId")]
        public Fabricante? Fabricante { get; set; }

        public int CategoriaId { get; set; }

        [JsonIgnore]
        [ForeignKey("CategoriaId")]
        public Categoria? Categoria { get; set; }

        [JsonIgnore]
        public ICollection<Aluguel> Alugueis { get; set; } = new List<Aluguel>();
    }
}
