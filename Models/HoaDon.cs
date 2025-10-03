using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization; // cần cho JsonIgnore nếu muốn dùng ở navigation property

namespace HRMApi.Models
{
    public class HoaDon
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string MaHoaDon { get; set; } = string.Empty;

        public string? LoaiHoaDon { get; set; }

        public DateTime NgayLap { get; set; }

        [Required]
        public string TrangThai { get; set; } = string.Empty;

        public int TongTien { get; set; }

        // 🔹 FK string
        [Required]
        public string UserId { get; set; } = string.Empty;

        public string? PhuongThuc { get; set; }

        // 🔹 Navigation tới danh sách Items
        public ICollection<HoaDonItem> Items { get; set; } = new List<HoaDonItem>();

        // 🔹 Tính tổng tiền theo Items
        public int TinhTongTien() => Items?.Sum(i => i.SoLuong * i.GiaTien) ?? 0;
    }
}
