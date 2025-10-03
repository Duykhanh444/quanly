using System.ComponentModel.DataAnnotations.Schema;

namespace HRMApi.Models
{
    public class HoaDonItem
    {
        public int Id { get; set; }

        // 🔹 FK tới bảng HoaDon
        public int HoaDonId { get; set; }

        // tên hàng/sản phẩm
        public string TenHang { get; set; } = string.Empty;

        // số lượng mua
        public int SoLuong { get; set; }

        // đơn giá từng sản phẩm
        public int GiaTien { get; set; }

        // 🔹 Navigation tới hóa đơn
        public HoaDon? HoaDon { get; set; }
    }
}
