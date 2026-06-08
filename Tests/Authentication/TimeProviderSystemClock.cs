using Microsoft.AspNetCore.Authentication;

namespace Tests.Authentication
{
    // ISystemClock é exigido pelo stack de autenticação legado; faz ponte com o TimeProvider moderno.
    internal sealed class TimeProviderSystemClock : ISystemClock
    {
        private readonly TimeProvider _timeProvider;

        public TimeProviderSystemClock(TimeProvider timeProvider)
        {
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        }

        public DateTimeOffset UtcNow => _timeProvider.GetUtcNow();
    }
}
