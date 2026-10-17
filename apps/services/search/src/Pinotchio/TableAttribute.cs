namespace Pinotchio;

/// <summary>
/// Attribute to specify the table name for an entity.
/// Similar to Entity Framework's [Table] attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class TableAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the table.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TableAttribute"/> class.
    /// </summary>
    /// <param name="name">The name of the table.</param>
    public TableAttribute(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }
}