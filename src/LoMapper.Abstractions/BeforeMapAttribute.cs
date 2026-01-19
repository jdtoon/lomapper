using System;

namespace LoMapper;

/// <summary>
/// Specifies a method to call before performing the mapping operation.
/// The method must have the signature: void MethodName(TSource source) where TSource is the source type.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class BeforeMapAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the method to call before mapping.
    /// </summary>
    public string MethodName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BeforeMapAttribute"/> class.
    /// </summary>
    /// <param name="methodName">The name of the method to call before mapping.
    /// The method must accept the source object as a parameter: void MethodName(TSource source)</param>
    public BeforeMapAttribute(string methodName)
    {
        MethodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
    }
}
