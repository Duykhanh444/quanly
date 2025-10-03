using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HRMApi.Models
{
    [Table("NhanViens")]
    public class NhanVien
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Họ tên không được bỏ trống")]
        public string HoTen { get; set; } = "";

        [Required(ErrorMessage = "Số điện thoại không được bỏ trống")]
        public string SoDienThoai { get; set; } = "";

        [Required(ErrorMessage = "Chức vụ không được bỏ trống")]
        public string ChucVu { get; set; } = "";

        public string? AnhDaiDien { get; set; } // Đường dẫn ảnh

        [Range(0, double.MaxValue, ErrorMessage = "Lương theo giờ phải >= 0")]
        public decimal LuongTheoGio { get; set; }

        public DateTime NgayChamCong { get; set; }

        // Người tạo record
        public string CreatedBy { get; set; } = "";

        // ⚠️ UserId dạng string (GUID)
        public string UserId { get; set; } = ""; // khóa ngoại đến IdentityUser

        // Lưu JSON cho ngày làm việc
        public string? NgayLamTrongTuanJson { get; set; }

        [NotMapped]
        public List<string> NgayLamTrongTuan
        {
            get => string.IsNullOrEmpty(NgayLamTrongTuanJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(NgayLamTrongTuanJson) ?? new List<string>();
            set => NgayLamTrongTuanJson = JsonSerializer.Serialize(value);
        }

        // Quan hệ 1-n với WorkDays
        public List<WorkDay> WorkDays { get; set; } = new();

        // Tổng số giờ đã chấm công (không map DB)
        [NotMapped]
        public int TongSoGioDaChamCong => WorkDays?.Sum(w => w.SoGio) ?? 0;
    }

    [Table("WorkDays")]
    public class WorkDay
    {
        [Key]
        public int Id { get; set; }

        public DateTime Ngay { get; set; }

        public int SoGio { get; set; }

        public int NhanVienId { get; set; }

        [JsonIgnore]
        public NhanVien? NhanVien { get; set; }
    }
}
