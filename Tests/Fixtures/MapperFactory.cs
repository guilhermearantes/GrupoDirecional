using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;

namespace Tests.Fixtures
{
    internal static class MapperFactory
    {
        public static IMapper Create() =>
            new MapperConfiguration(
                cfg => cfg.AddProfile<DesafioTecnico.Api.Mapping.AutoMapperProfile>(),
                NullLoggerFactory.Instance
            ).CreateMapper();
    }
}
