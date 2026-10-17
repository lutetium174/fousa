using System;

namespace Pinotchio;

/// <summary>
/// Attribute to specify the column name for an entity property.
/// Similar to Entity Framework's [Column] attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class ColumnAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the column.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColumnAttribute"/> class.
    /// </summary>
    /// <param name="name">The name of the column.</param>
    public ColumnAttribute(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }
}