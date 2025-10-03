using System;
using System.Collections.Generic;
using System.Linq;

namespace HRMApi.Models.Dto
{
    // DTO chính cho nhân viên
    public class NhanVienDto
    {
        public int Id { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string ChucVu { get; set; } = string.Empty;
        public double LuongTheoGio { get; set; }
        public string? AnhDaiDien { get; set; }

        // Danh sách ngày làm việc
        public List<WorkDayDto> WorkDays { get; set; } = new List<WorkDayDto>();

        // Read-only properties tự tính
        public int TongSoGioDaChamCong => WorkDays.Sum(w => w.SoGio);
        public int TongSoBuoiLam => WorkDays.Count;
    }

    // DTO cho ngày làm việc khi trả về
    public class WorkDayDto
    {
        public int Id { get; set; }
        public DateTime Ngay { get; set; }
        public int SoGio { get; set; }
    }

    // DTO nhận từ client khi thêm/sửa ngày làm việc
    public class WorkDayRequestDto
    {
        public DateTime Ngay { get; set; }
        public int SoGio { get; set; }
    }
}
