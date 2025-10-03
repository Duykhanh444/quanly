using System;
using System.ComponentModel.DataAnnotations;

namespace HRMApi.Models
{
    public class DoanhThu
    {
        [Key] public int Id { get; set; }

        [Required] public decimal TongTien { get; set; }
        public DateTime Ngay { get; set; }

        // 🔹 FK string
        [Required] public string UserId { get; set; } = "";
        public User? User { get; set; }
    }
}
