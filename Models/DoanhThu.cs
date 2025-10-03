namespace HRMApi.Models
{
    public class DoanhThu
    {
        public int Id { get; set; }

        public decimal TongTien { get; set; }

        public DateTime Ngay { get; set; }

        // Liên kết với User
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
