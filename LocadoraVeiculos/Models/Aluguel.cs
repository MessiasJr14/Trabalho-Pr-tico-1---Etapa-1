using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using LocadoraVeiculos.Models.Enums;

namespace LocadoraVeiculos.Models
{
    /// <summary>
    /// Aluguel (locação): liga um cliente a um veículo em um período de tempo.
    /// A devolução é registrada em DataDevolucao e QuilometragemFinal.
    /// </summary>
    [Table("Alugueis")]
    public class Aluguel : IValidatableObject
    {
        // Chave primária
        [Key]
        public int Id { get; set; }

        // Chave estrangeira -> Clientes(Id)
        [Range(1, int.MaxValue, ErrorMessage = "Informe um cliente válido.")]
        public int ClienteId { get; set; }

        [ForeignKey(nameof(ClienteId))]
        public Cliente? Cliente { get; set; }

        // Chave estrangeira -> Veiculos(Id)
        [Range(1, int.MaxValue, ErrorMessage = "Informe um veículo válido.")]
        public int VeiculoId { get; set; }

        [ForeignKey(nameof(VeiculoId))]
        public Veiculo? Veiculo { get; set; }

        // Período do aluguel
        public DateTime DataInicio { get; set; }

        public DateTime DataFimPrevista { get; set; }

        /// <summary>Data da devolução do veículo. Nula enquanto o aluguel está em andamento.</summary>
        public DateTime? DataDevolucao { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "A quilometragem inicial não pode ser negativa.")]
        public int QuilometragemInicial { get; set; }

        /// <summary>Quilometragem na devolução. Nula enquanto o aluguel está em andamento.</summary>
        [Range(0, int.MaxValue, ErrorMessage = "A quilometragem final não pode ser negativa.")]
        public int? QuilometragemFinal { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Range(0.01, 99999999.99, ErrorMessage = "O valor da diária deve ser maior que zero.")]
        public decimal ValorDiaria { get; set; }

        /// <summary>Valor total da locação: previsto na abertura e recalculado na devolução.</summary>
        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 99999999.99, ErrorMessage = "O valor total não pode ser negativo.")]
        public decimal ValorTotal { get; set; }

        public StatusAluguel Status { get; set; } = StatusAluguel.EmAndamento;

        [StringLength(500, ErrorMessage = "As observações devem ter no máximo 500 caracteres.")]
        public string? Observacoes { get; set; }

        // Relacionamento 1:N -> um aluguel pode receber vários pagamentos
        [JsonIgnore]
        public ICollection<Pagamento> Pagamentos { get; set; } = new List<Pagamento>();

        // Regras entre campos (as mesmas garantidas pelas CHECK constraints do banco)
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DataFimPrevista <= DataInicio)
            {
                yield return new ValidationResult(
                    "A data de fim prevista deve ser posterior à data de início.",
                    new[] { nameof(DataFimPrevista) });
            }

            if (DataDevolucao.HasValue && DataDevolucao.Value < DataInicio)
            {
                yield return new ValidationResult(
                    "A data de devolução não pode ser anterior à data de início.",
                    new[] { nameof(DataDevolucao) });
            }

            if (QuilometragemFinal.HasValue && QuilometragemFinal.Value < QuilometragemInicial)
            {
                yield return new ValidationResult(
                    "A quilometragem final não pode ser menor que a inicial.",
                    new[] { nameof(QuilometragemFinal) });
            }
        }
    }
}
