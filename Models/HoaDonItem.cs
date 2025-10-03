using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMApi.Models
{
    [Table("HoaDonItems")]
    public class HoaDonItem
    {
        [Key]
        public int Id { get; set; }

        // 🔹 Foreign key tới bảng HoaDon
        [Required]
        public int HoaDonId { get; set; }

        // Tên hàng/sản phẩm
        [Required]
        public string TenHang { get; set; } = string.Empty;

        // Số lượng mua
        [Required]
        public int SoLuong { get; set; }

        // Đơn giá từng sản phẩm
        [Required]
        public int GiaTien { get; set; }

        // Navigation tới hóa đơn
        [ForeignKey("HoaDonId")]
        public HoaDon? HoaDon { get; set; }
    }
}
