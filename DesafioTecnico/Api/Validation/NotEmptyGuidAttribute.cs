using System.ComponentModel.DataAnnotations;

namespace DesafioTecnico.Api.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public sealed class NotEmptyGuidAttribute : ValidationAttribute
    {
        public NotEmptyGuidAttribute() : base("O campo {0} não pode ser um Guid vazio.") { }

        public override bool IsValid(object? value)
            => value is Guid g && g != Guid.Empty;
    }
}
