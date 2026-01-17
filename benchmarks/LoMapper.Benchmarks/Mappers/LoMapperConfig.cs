using LoMapper.Benchmarks.Models;

namespace LoMapper.Benchmarks.Mappers;

/// <summary>
/// LoMapper configuration - compile-time generated.
/// </summary>
[Mapper]
public partial class BenchmarkMapper
{
    public partial UserDto MapUser(UserEntity entity);
    public partial AddressDto MapAddress(AddressEntity entity);
    public partial SimpleDto MapSimple(SimpleEntity entity);
}
