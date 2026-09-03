namespace GovUK.Dfe.Lsrp.FileValidator.Models;

public class LocalAuthority
{
    public string? Name { get; set; }
    public string? Code { get; set; }

    override public string ToString() => $"{Code} {Name}";
}
