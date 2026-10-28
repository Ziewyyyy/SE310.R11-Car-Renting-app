namespace CarRentalAPI.Models;

public class Report
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CreatedById { get; set; }
    public string ReportType { get; set; } = "";
    public string Content { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
