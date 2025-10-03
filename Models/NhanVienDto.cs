using System.Collections.Generic;
using System.Linq;

namespace HRMApi.Models.Dto
{
    public class NhanVienDto
    {
        public int Id { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string ChucVu { get; set; } = string.Empty;
        public double LuongTheoGio { get; set; }
        public string? AnhDaiDien { get; set; }

        public List<WorkDayResponseDto> WorkDays { get; set; } = new List<WorkDayResponseDto>();

        // ✅ Read-only properties tính toán tự động
        public int TongSoGioDaChamCong => WorkDays.Sum(w => w.SoGio);
        public int TongSoBuoiLam => WorkDays.Count;
    }
}
