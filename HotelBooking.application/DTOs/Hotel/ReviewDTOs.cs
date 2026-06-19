using System.ComponentModel.DataAnnotations;

namespace HotelBooking.application.DTOs.Hotel;

public class ReviewRequestDTO
{
    [Required]
    public int BookingId { get; set; }
    
    [Required]
    public int HotelId { get; set; }
    
    [Range(1, 10)]
    public int Rating { get; set; }
    
    [MaxLength(1000)]
    public string? Comment { get; set; }
}

public class ReviewDetailDTO
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // New fields for Owner Response
    public string? OwnerResponse { get; set; }
    public DateTime? OwnerResponseAt { get; set; }
    public bool IsHidden { get; set; }
}

public class ReviewReplyRequestDTO
{
    [Required]
    public int ReviewId { get; set; }
    [Required]
    [MaxLength(1000)]
    public string ReplyText { get; set; } = string.Empty;
}

public class ReviewModerationRequestDTO
{
    [Required]
    public int ReviewId { get; set; }
    public string? Reason { get; set; }
}
