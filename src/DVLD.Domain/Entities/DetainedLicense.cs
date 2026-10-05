namespace DVLD.Domain.Entities;

public class DetainedLicense
{
    public int DetainId { get; set; }
    public int LicenseId { get; set; }
    public DateTime DetainDate { get; set; } = DateTime.UtcNow;
    public decimal FineFees { get; set; }
    public int CreatedByUserId { get; set; }
    public bool IsReleased { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public int? ReleasedByUserId { get; set; }
    public int? ReleaseApplicationId { get; set; }

    // Navigations
    public License License { get; set; } = null!;
    public Application? ReleaseApplication { get; set; }

    public void Release(int releaseApplicationId, int releasedByUserId, DateTime releaseDate)
    {
        IsReleased = true;
        ReleaseApplicationId = releaseApplicationId;
        ReleasedByUserId = releasedByUserId;
        ReleaseDate = releaseDate;
    }
}
