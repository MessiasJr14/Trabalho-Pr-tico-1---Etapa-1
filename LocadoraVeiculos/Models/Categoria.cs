using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LocadoraVeiculos.Models
{
    /// <summary>
    /// Categoria do veículo (Econômico, Sedan, SUV, Luxo...). Define o valor base da diária.
    /// </summary>
    [Table("Categorias")]
    public class Categoria
    {
        // Chave primária
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome da categoria é obrigatório.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 50 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "A descrição deve ter no máximo 200 caracteres.")]
        public string? Descricao { get; set; }

        /// <summary>Valor sugerido da diária para os veículos desta categoria.</summary>
        [Column(TypeName = "decimal(10,2)")]
        [Range(0.01, 99999999.99, ErrorMessage = "O valor base da diária deve ser maior que zero.")]
        public decimal ValorDiariaBase { get; set; }

        // Relacionamento 1:N -> uma categoria classifica vários veículos
        [JsonIgnore]
        public ICollection<Veiculo> Veiculos { get; set; } = new List<Veiculo>();
    }
}
