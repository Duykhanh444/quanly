using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HRMApi.Models
{
    public class User
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString(); // ✅ ID kiểu string (GUID)

        [Required, MaxLength(100)]
        public string UserName { get; set; } = string.Empty;   // Có thể là Email nếu login bằng Google

        public string? PasswordHash { get; set; }              // null nếu đăng nhập Google

        [Required, EmailAddress, MaxLength(200)]
        public string Email { get; set; } = string.Empty;      // Luôn có email cho cả Google login

        public string? AvatarUrl { get; set; }                 // Ảnh đại diện (từ Google hoặc upload)
        
        public string Role { get; set; } = "User";             // Mặc định User, có thể Admin
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // 🔹 Navigation - mỗi User sẽ có dữ liệu RIÊNG
        public List<NhanVien> NhanViens { get; set; } = new();
        public List<HoaDon> HoaDons { get; set; } = new();
        public List<KhoHang> KhoHang { get; set; } = new();
        public List<DoanhThu> DoanhThus { get; set; } = new();
    }
}
