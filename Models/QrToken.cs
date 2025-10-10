// Models/QrToken.cs
using System;
using System.ComponentModel.DataAnnotations;

public class QrToken
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Token { get; set; } = string.Empty; // Guid string

    public bool IsUsed { get; set; } = false;

    public DateTime CreatedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public string? Purpose { get; set; } // ví dụ "kho_nhap"
    public string? CreatedBy { get; set; } // user tạo token (nếu có)
}
