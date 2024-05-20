using System.ComponentModel.DataAnnotations;

namespace NewsletterProvider.Models;

public class UnsubscribeRequest
{
    [Required]
    public string Email { get; set; } = null!;
}
