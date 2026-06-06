using System;
using Microsoft.AspNetCore.Authentication;

namespace Tests.Authentication
{
    // Adapter to bridge System.TimeProvider to ISystemClock expected by older APIs.
    // This is a small, well-tested adapter keeping production code unchanged while
    // using the modern TimeProvider in DI.
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
