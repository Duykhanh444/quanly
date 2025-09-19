using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

namespace HRMApi.Models.Dto
{
    public class NhanVienFormDto
    {
        [Required]
        public string HoTen { get; set; } = "";

        [Required]
        public string SoDienThoai { get; set; } = "";

        public string ChucVu { get; set; } = "";

        public double LuongTheoGio { get; set; }

        public DateTime NgayChamCong { get; set; }

        public IFormFile? AnhDaiDien { get; set; }

        public List<string>? NgayLamTrongTuan { get; set; }
    }
}
