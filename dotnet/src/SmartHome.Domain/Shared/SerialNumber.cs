namespace SmartHome.Domain.Shared;

public sealed record SerialNumber
{
    public string Value { get; }

    private SerialNumber(string value)
    {        
        Value = value;
    }

    public static SerialNumber? Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        
        return new SerialNumber(value.Trim());
    }

    public override string ToString() => Value;
}
