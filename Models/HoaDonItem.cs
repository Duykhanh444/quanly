using System;

namespace HRMApi.Models
{
    public class HoaDonItem
    {
        public int Id { get; set; }

        public int HoaDonId { get; set; }   // FK tới bảng HoaDon

        // tên hàng/sản phẩm
        public string TenHang { get; set; } = string.Empty;

        public int SoLuong { get; set; }    // số lượng mua

        public int GiaTien { get; set; }    // đơn giá từng sản phẩm

        // Navigation tới hóa đơn
        public HoaDon? HoaDon { get; set; }
    }
}
