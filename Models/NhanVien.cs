using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HRMApi.Models
{
    [Table("NhanViens")]
    public class NhanVien
    {
        [Key] public int Id { get; set; }

        [Required] public string HoTen { get; set; } = "";
        [Required] public string SoDienThoai { get; set; } = "";
        [Required] public string ChucVu { get; set; } = "";
        public string? AnhDaiDien { get; set; }
        [Range(0, double.MaxValue)] public decimal LuongTheoGio { get; set; }
        public DateTime NgayChamCong { get; set; }
        public string CreatedBy { get; set; } = "";

        // 🔹 FK string
        public string UserId { get; set; } = "";

        public string? NgayLamTrongTuanJson { get; set; }

        [NotMapped]
        public List<string> NgayLamTrongTuan
        {
            get => string.IsNullOrEmpty(NgayLamTrongTuanJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(NgayLamTrongTuanJson) ?? new List<string>();
            set => NgayLamTrongTuanJson = JsonSerializer.Serialize(value);
        }

        // Quan hệ WorkDays
        public List<WorkDay> WorkDays { get; set; } = new();

        [NotMapped]
        public int TongSoGioDaChamCong => WorkDays?.Sum(w => w.SoGio) ?? 0;
    }

    [Table("WorkDays")]
    public class WorkDay
    {
        [Key] public int Id { get; set; }
        public DateTime Ngay { get; set; }
        public int SoGio { get; set; }
        public int NhanVienId { get; set; }

        [JsonIgnore] public NhanVien? NhanVien { get; set; }
    }
}
