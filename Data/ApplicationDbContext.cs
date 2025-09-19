using Microsoft.EntityFrameworkCore;
using HRMApi.Models;
 

namespace HRMApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // =====================
        // 🔹 Các DbSet (bảng)
        // =====================
        public DbSet<NhanVien> NhanViens { get; set; }
        public DbSet<KhoHang> KhoHang { get; set; }
        public DbSet<WorkDay> WorkDays { get; set; }
        public DbSet<TongHopDanhSach> TongHopDanhSaches { get; set; }


        public DbSet<HoaDon> HoaDons { get; set; }
        public DbSet<HoaDonItem> HoaDonItems { get; set; }

        // =====================
        // 🔹 Cấu hình quan hệ
        // =====================
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1️⃣ Quan hệ 1-nhiều: NhanVien ↔ WorkDay
            modelBuilder.Entity<WorkDay>()
                .HasOne(w => w.NhanVien)
                .WithMany(n => n.WorkDays)
                .HasForeignKey(w => w.NhanVienId)
                .OnDelete(DeleteBehavior.Cascade);

            // 2️⃣ Quan hệ 1-nhiều: HoaDon ↔ HoaDonItem
            modelBuilder.Entity<HoaDonItem>()
                .HasOne(i => i.HoaDon)
                .WithMany(h => h.Items)
                .HasForeignKey(i => i.HoaDonId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
