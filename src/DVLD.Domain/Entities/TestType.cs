namespace DVLD.Domain.Entities;

public class TestType
{
    public int TestTypeId { get; set; }
    public string TestTypeTitle { get; set; } = string.Empty;
    public string TestTypeDescription { get; set; } = string.Empty;
    public decimal TestTypeFees { get; set; }
}
