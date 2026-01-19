using System;

namespace LoMapper;

/// <summary>
/// Specifies a method to call after performing the mapping operation.
/// The method must have the signature: void MethodName(TTarget target) where TTarget is the target type.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class AfterMapAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the method to call after mapping.
    /// </summary>
    public string MethodName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AfterMapAttribute"/> class.
    /// </summary>
    /// <param name="methodName">The name of the method to call after mapping.
    /// The method must accept the target object as a parameter: void MethodName(TTarget target)</param>
    public AfterMapAttribute(string methodName)
    {
        MethodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
    }
}
