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

        // ✅ ĐÃ SỬA TỪ "int" THÀNH "long"
        public long TongTien { get; set; }

        // 🔹 Liên kết với user hiện đang đăng nhập
        public string UserId { get; set; } = string.Empty;

        // 🔹 Thêm phương thức thanh toán
        public string? PhuongThuc { get; set; }

        // 🔹 Navigation tới danh sách Items
        public ICollection<HoaDonItem> Items { get; set; } = new List<HoaDonItem>();

        // 🔹 Tính tổng tiền theo Items
        // ✅ ĐÃ SỬA KIỂU TRẢ VỀ VÀ PHÉP TÍNH CHO AN TOÀN
        public long TinhTongTien()
        {
            // (long)i.SoLuong đảm bảo phép nhân giữa hai số lớn không bị tràn
            return Items?.Sum(i => (long)i.SoLuong * i.GiaTien) ?? 0;
        }
    }
}