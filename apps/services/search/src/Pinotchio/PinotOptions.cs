namespace Pinotchio;

public record PinotOptions
{
    public const string Pinot = "Pinot";
    public string ControllerUri { get; set; } = string.Empty;
}