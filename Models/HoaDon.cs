using System;
using System.Collections.Generic;
using System.Linq;

namespace HRMApi.Models
{
    public class HoaDon
    {
        public int Id { get; set; }

        public string MaHoaDon { get; set; } = string.Empty;
        public string? LoaiHoaDon { get; set; }
        public DateTime NgayLap { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public int TongTien { get; set; }

        // 🔹 Liên kết với user hiện đang đăng nhập
        public string UserId { get; set; } = string.Empty;

        // 🔹 Thêm phương thức thanh toán
        public string? PhuongThuc { get; set; }

        // 🔹 Navigation tới danh sách Items
        public ICollection<HoaDonItem> Items { get; set; } = new List<HoaDonItem>();

        // 🔹 Tính tổng tiền theo Items
        public int TinhTongTien()
        {
            return Items?.Sum(i => i.SoLuong * i.GiaTien) ?? 0;
        }
    }
}
