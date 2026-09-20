using System.ComponentModel.DataAnnotations;
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

        public int AnoFabricacao { get; set; }

        public int Quilometragem { get; set; }

        [Required]
        [MaxLength(8)]
        public string? Placa { get; set; }

        [MaxLength(30)]
        public string? Cor { get; set; }

        public bool Disponivel { get; set; } = true;

        public int FabricanteId { get; set; }

        [ForeignKey("FabricanteId")]
        public Fabricante? Fabricante { get; set; }

        public int CategoriaId { get; set; }

        [ForeignKey("CategoriaId")]
        public Categoria? Categoria { get; set; }

        public ICollection<Aluguel> Alugueis { get; set; } = new List<Aluguel>();
    }
}
