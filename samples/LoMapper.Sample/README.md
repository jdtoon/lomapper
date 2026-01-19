# LoMapper.Sample

This sample console app shows LoMapper usage end-to-end. Run `dotnet run --project samples/LoMapper.Sample/LoMapper.Sample.csproj` to see the generated mappings in action.

## Highlights
- Basic, flattened, and collection mapping via the mapper in [samples/LoMapper.Sample/Mappers/UserMapper.cs](samples/LoMapper.Sample/Mappers/UserMapper.cs).
- Lifecycle hooks demonstrated by `MapUserWithHooks`, which validates the source and appends an audit tag in `AfterMap`.
- Generated code artifacts available under `samples/LoMapper.Sample/obj/GeneratedFiles` after build.

## Circular reference detection (LOM010)
LoMapper reports circular mappings at build time. This sample snippet shows the pattern and resulting diagnostic:

```csharp
[Mapper]
public partial class CircularMapper
{
    // LOM010: Circular reference detected for SourceA => TargetA via SourceB/TargetB
    public partial TargetA Map(SourceA source);
    public partial TargetB Map(SourceB source);
}

public class SourceA { public SourceB? Child { get; set; } }
public class SourceB { public SourceA? Parent { get; set; } }

public class TargetA { public TargetB? Child { get; set; } }
public class TargetB { public TargetA? Parent { get; set; } }
```

Building code like this yields diagnostic `LOM010` describing the loop. Break the cycle by ignoring one side or introducing a DTO shape without back-references.
