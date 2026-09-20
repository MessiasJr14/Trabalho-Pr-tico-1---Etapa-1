using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LocadoraVeiculos.Models.Enums;

namespace LocadoraVeiculos.Models
{
    /// <summary>
    /// Pagamento recebido referente a um aluguel. Um aluguel pode ter vários pagamentos
    /// (ex.: sinal na retirada e o restante na devolução).
    /// </summary>
    [Table("Pagamentos")]
    public class Pagamento
    {
        // Chave primária
        [Key]
        public int Id { get; set; }

        // Chave estrangeira -> Alugueis(Id)
        [Range(1, int.MaxValue, ErrorMessage = "Informe um aluguel válido.")]
        public int AluguelId { get; set; }

        [ForeignKey(nameof(AluguelId))]
        public Aluguel? Aluguel { get; set; }

        /// <summary>Preenchida pelo banco (DEFAULT GETDATE()) quando não informada.</summary>
        public DateTime DataPagamento { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Range(0.01, 99999999.99, ErrorMessage = "O valor do pagamento deve ser maior que zero.")]
        public decimal Valor { get; set; }

        public FormaPagamento FormaPagamento { get; set; } = FormaPagamento.Pix;
    }
}
