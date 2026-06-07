using Microsoft.AspNetCore.Authentication;

namespace Tests.Authentication
{
    // ISystemClock is required by the legacy auth stack; bridges it to the modern TimeProvider.
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
