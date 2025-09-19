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

        public ICollection<HoaDonItem> Items { get; set; } = new List<HoaDonItem>();

        public int TinhTongTien()
        {
            if (Items == null || Items.Count == 0)
                return 0;

            return Items.Sum(i => i.SoLuong * i.GiaTien);
        }
    }
}
