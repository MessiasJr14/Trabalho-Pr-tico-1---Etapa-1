using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using LocadoraVeiculos.Models.Enums;

namespace LocadoraVeiculos.Models
{
    /// <summary>
    /// Veículo da frota. Todo veículo pertence a um fabricante e a uma categoria.
    /// </summary>
    [Table("Veiculos")]
    public class Veiculo
    {
        // Chave primária
        [Key]
        public int Id { get; set; }

        // Chave estrangeira -> Fabricantes(Id)
        [Range(1, int.MaxValue, ErrorMessage = "Informe um fabricante válido.")]
        public int FabricanteId { get; set; }

        [ForeignKey(nameof(FabricanteId))]
        public Fabricante? Fabricante { get; set; }

        // Chave estrangeira -> Categorias(Id)
        [Range(1, int.MaxValue, ErrorMessage = "Informe uma categoria válida.")]
        public int CategoriaId { get; set; }

        [ForeignKey(nameof(CategoriaId))]
        public Categoria? Categoria { get; set; }

        [Required(ErrorMessage = "O modelo do veículo é obrigatório.")]
        [StringLength(80, ErrorMessage = "O modelo deve ter no máximo 80 caracteres.")]
        public string Modelo { get; set; } = string.Empty;

        [Range(1950, 2100, ErrorMessage = "O ano de fabricação deve estar entre 1950 e 2100.")]
        public int AnoFabricacao { get; set; }

        /// <summary>Quilometragem atual do veículo (atualizada a cada devolução).</summary>
        [Range(0, int.MaxValue, ErrorMessage = "A quilometragem não pode ser negativa.")]
        public int Quilometragem { get; set; }

        /// <summary>Placa sem hífen, no padrão antigo (ABC1234) ou Mercosul (ABC1D23).</summary>
        [Required(ErrorMessage = "A placa é obrigatória.")]
        [Column(TypeName = "char(7)")]
        [RegularExpression(@"^[A-Z]{3}[0-9][A-Z0-9][0-9]{2}$", ErrorMessage = "Placa inválida. Use o formato ABC1234 ou ABC1D23.")]
        public string Placa { get; set; } = string.Empty;

        [StringLength(30, ErrorMessage = "A cor deve ter no máximo 30 caracteres.")]
        public string? Cor { get; set; }

        public StatusVeiculo Status { get; set; } = StatusVeiculo.Disponivel;

        // Relacionamento 1:N -> um veículo participa de vários aluguéis ao longo do tempo
        [JsonIgnore]
        public ICollection<Aluguel> Alugueis { get; set; } = new List<Aluguel>();
    }
}
