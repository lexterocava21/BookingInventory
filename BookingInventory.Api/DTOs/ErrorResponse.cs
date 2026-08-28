namespace BookingInventory.Api.DTOs;

public class ErrorResponse
{
    public string ErrorCode { get; set; } = null!;
    public string Message { get; set; } = null!;
}
