namespace ChristmasPortal.mqtt;

public class SetMessage
{
    public const string Topic = "xmaspi/set";

    public byte? Brightness { get; set; } = 255;

    public Color? Color { get; set; }

    public string? Effect { get; set; }
    
    public string? State { get; set; }
}