namespace HRMApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        // Có thể thêm Email, Role...
        public List<NhanVien> NhanViens { get; set; } = new();
    }
}
