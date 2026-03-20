using hms.Application.Mapping;
using Mapster;
using MapsterMapper;

namespace hms.Application.Tests.Common;

internal static class MapsterTestMapperFactory
{
    public static IMapper Create()
    {
        var config = new TypeAdapterConfig();
        config.Scan(typeof(MappingAssemblyMarker).Assembly);
        return new Mapper(config);
    }
}
