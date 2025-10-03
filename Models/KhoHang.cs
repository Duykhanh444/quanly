using System;
using System.ComponentModel.DataAnnotations;

namespace HRMApi.Models
{
    public class KhoHang
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string TenKho { get; set; } = string.Empty;

        public string? GhiChu { get; set; }

        [Required]
        public DateTime NgayNhap { get; set; }

        public DateTime? NgayXuat { get; set; }

        [Required]
        public string TrangThai { get; set; } = "Hoạt động"; // "Hoạt động" hoặc "Đã xuất"

        // 🔹 Giá trị của kho (số tiền, trị giá hàng hóa)
        [Required]
        public decimal GiaTri { get; set; }

        // 🔹 Liên kết với user hiện đang đăng nhập
        public string UserId { get; set; } = string.Empty;
    }
}
