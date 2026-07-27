namespace PocColor.Infrastructure.LegacyConfig;

public sealed class LegacyConfigDiagnostic
{
    public LegacyConfigDiagnostic(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public string Code { get; }

    public string Message { get; }
}
