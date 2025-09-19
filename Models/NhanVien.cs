using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
 
namespace HRMApi.Models
{
    [Table("NhanViens")]
     public class NhanVien
{
    public int Id { get; set; }
    public string HoTen { get; set; } = "";
    public string SoDienThoai { get; set; } = "";
    public string ChucVu { get; set; } = "";
    public string? AnhDaiDien { get; set; } // giữ string
    public decimal LuongTheoGio { get; set; }
    public DateTime NgayChamCong { get; set; }

    public string? NgayLamTrongTuanJson { get; set; }

    [NotMapped]
    public List<string> NgayLamTrongTuan
    {
        get => string.IsNullOrEmpty(NgayLamTrongTuanJson)
            ? new List<string>()
            : JsonSerializer.Deserialize<List<string>>(NgayLamTrongTuanJson) ?? new List<string>();
        set => NgayLamTrongTuanJson = JsonSerializer.Serialize(value);
    }

    public List<WorkDay> WorkDays { get; set; } = new();
    
    [NotMapped]
    public int TongSoGioDaChamCong => WorkDays?.Sum(w => w.SoGio) ?? 0;
}


    [Table("WorkDays")]
    public class WorkDay
    {
        public int Id { get; set; }
        public DateTime Ngay { get; set; }
        public int SoGio { get; set; }

        // 🔹 Khóa ngoại
        public int NhanVienId { get; set; }

        [JsonIgnore]
        public NhanVien? NhanVien { get; set; }
    }
}
