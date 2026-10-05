namespace DVLD.Domain.Entities;

public class LicenseClass
{
    public int LicenseClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string ClassDescription { get; set; } = string.Empty;
    public int MinimumAllowedAge { get; set; }
    public int ValidityLength { get; set; }
    public decimal ClassFees { get; set; }
}
