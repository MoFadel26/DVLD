namespace DVLD.Domain.Entities;

/// <summary>
/// A signed-out token (by its JWT id). Kept until the token would have expired anyway.
/// </summary>
public class RevokedToken
{
    public int RevokedTokenId { get; set; }
    public string TokenId { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime RevokedAt { get; set; } = DateTime.UtcNow;
}
