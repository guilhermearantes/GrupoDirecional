using System.ComponentModel.DataAnnotations;

namespace DesafioTecnico.Api.Validation
{
    /// <summary>
    /// Valida que a data de nascimento é anterior a hoje e posterior a 1900-01-01.
    /// </summary>
    public sealed class DataNascimentoValidaAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not DateTime data) return ValidationResult.Success;

            if (data >= DateTime.UtcNow.Date)
                return new ValidationResult("Data de nascimento não pode ser no futuro.");

            if (data < new DateTime(1900, 1, 1))
                return new ValidationResult("Data de nascimento inválida.");

            return ValidationResult.Success;
        }
    }
}
