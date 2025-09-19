using System;
using System.ComponentModel.DataAnnotations;

namespace HRMApi.Models
{
    public class KhoHang
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string TenKho { get; set; }

        public string? GhiChu { get; set; }

        [Required]
        public DateTime NgayNhap { get; set; }

        public DateTime? NgayXuat { get; set; }

        [Required]
        public string TrangThai { get; set; } // "Hoạt động" hoặc "Đã xuất"
    }
}
