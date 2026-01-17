using AutoMapper;
using LoMapper.Benchmarks.Models;

namespace LoMapper.Benchmarks.Mappers;

/// <summary>
/// AutoMapper configuration.
/// </summary>
public static class AutoMapperConfig
{
    private static IMapper? _mapper;

    public static IMapper Mapper => _mapper ??= CreateMapper();

    private static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<UserEntity, UserDto>();
            cfg.CreateMap<AddressEntity, AddressDto>();
            cfg.CreateMap<SimpleEntity, SimpleDto>();
        });

        return config.CreateMapper();
    }
}
