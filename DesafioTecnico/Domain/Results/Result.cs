namespace DesafioTecnico.Domain.Results
{
    /// <summary>
    /// Representa o resultado de uma operação que pode falhar por regra de negócio,
    /// sem lançar exceção. Use <see cref="Ok()"/> para sucesso e <see cref="Fail"/> para falha.
    /// </summary>
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string Error { get; }

        protected Result(bool isSuccess, string error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Ok() => new(true, string.Empty);
        public static Result Fail(string error) => new(false, error);
        public static Result<T> Ok<T>(T value) => new(value, true, string.Empty);
        public static Result<T> Fail<T>(string error) => new(default!, false, error);
    }

    /// <summary>
    /// Variante tipada de <see cref="Result"/> que carrega um valor em caso de sucesso.
    /// </summary>
    public sealed class Result<T> : Result
    {
        private readonly T _value;

        internal Result(T value, bool isSuccess, string error) : base(isSuccess, error)
            => _value = value;

        /// <summary>O valor produzido pela operação. Só pode ser acessado quando <see cref="Result.IsSuccess"/> é <c>true</c>.</summary>
        public T Value => IsSuccess
            ? _value
            : throw new InvalidOperationException("Não é possível acessar Value de um Result com falha.");
    }
}
