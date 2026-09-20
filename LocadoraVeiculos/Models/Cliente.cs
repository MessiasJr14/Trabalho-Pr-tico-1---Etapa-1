using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LocadoraVeiculos.Models
{
    /// <summary>
    /// Cliente da locadora. CPF, e-mail e CNH são únicos.
    /// </summary>
    [Table("Clientes")]
    public class Cliente
    {
        // Chave primária
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do cliente é obrigatório.")]
        [StringLength(120, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 120 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        /// <summary>CPF somente com os 11 dígitos (sem pontos e traço).</summary>
        [Required(ErrorMessage = "O CPF é obrigatório.")]
        [Column(TypeName = "char(11)")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "O CPF deve conter exatamente 11 dígitos numéricos.")]
        public string Cpf { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        [StringLength(150, ErrorMessage = "O e-mail deve ter no máximo 150 caracteres.")]
        public string Email { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "O telefone deve ter no máximo 20 caracteres.")]
        public string? Telefone { get; set; }

        /// <summary>Número de registro da CNH (11 dígitos).</summary>
        [Required(ErrorMessage = "A CNH é obrigatória.")]
        [Column(TypeName = "char(11)")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "A CNH deve conter exatamente 11 dígitos numéricos.")]
        public string Cnh { get; set; } = string.Empty;

        public DateOnly DataNascimento { get; set; }

        /// <summary>Preenchida pelo banco (DEFAULT GETDATE()) quando não informada.</summary>
        public DateTime DataCadastro { get; set; }

        // Relacionamento 1:N -> um cliente realiza vários aluguéis
        [JsonIgnore]
        public ICollection<Aluguel> Alugueis { get; set; } = new List<Aluguel>();
    }
}
