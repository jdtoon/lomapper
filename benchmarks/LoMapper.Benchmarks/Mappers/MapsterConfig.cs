using Mapster;
using LoMapper.Benchmarks.Models;

namespace LoMapper.Benchmarks.Mappers;

/// <summary>
/// Mapster configuration.
/// </summary>
public static class MapsterConfig
{
    private static bool _configured;

    public static void Configure()
    {
        if (_configured) return;

        TypeAdapterConfig<UserEntity, UserDto>.NewConfig();
        TypeAdapterConfig<AddressEntity, AddressDto>.NewConfig();
        TypeAdapterConfig<SimpleEntity, SimpleDto>.NewConfig();

        _configured = true;
    }
}
