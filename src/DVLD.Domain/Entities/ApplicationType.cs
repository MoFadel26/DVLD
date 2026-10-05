namespace DVLD.Domain.Entities;

public class ApplicationType
{
    public int ApplicationTypeId { get; set; }
    public string ApplicationTypeTitle { get; set; } = string.Empty;
    public decimal ApplicationFees { get; set; }
}
