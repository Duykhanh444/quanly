using System;
using System.ComponentModel.DataAnnotations;

namespace HRMApi.Models
{
    public class TongHopDanhSach
    {
        [Key]
        public int Id { get; set; }

        public string Loai { get; set; } // "NhanVien" hoặc "HoaDon"
        public string Ten { get; set; }
        public string ChucVu { get; set; } // chỉ cho nhân viên
        public string TrangThai { get; set; } // chỉ cho hóa đơn
        public decimal? TongTien { get; set; } // chỉ cho hóa đơn
        public DateTime NgayTao { get; set; } = DateTime.Now;
    }
}
