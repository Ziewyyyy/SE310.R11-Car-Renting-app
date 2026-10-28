namespace CarRentalAPI.Models;

public class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ContractId { get; set; }
    public decimal Amount { get; set; }
    public DateTime? PaidAt { get; set; }
    public string Method { get; set; } = "VNPay";
    public string Status { get; set; } = "Pending";
    public string? TransactionCode { get; set; }
}
