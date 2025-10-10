using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HRMApi.Models;
using Microsoft.EntityFrameworkCore.Design;

namespace HRMApi.Data
{
    // =====================
    // 🔹 DbContext chính (Identity + app tables)
    // =====================
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // =====================
        // 🔹 Các DbSet (bảng riêng của app)
        // =====================
        public DbSet<NhanVien> NhanViens { get; set; }
        public DbSet<KhoHang> KhoHang { get; set; }
        public DbSet<WorkDay> WorkDays { get; set; }
        public DbSet<TongHopDanhSach> TongHopDanhSaches { get; set; }
        public DbSet<QrToken> QrTokens { get; set; }

        public DbSet<HoaDon> HoaDons { get; set; }
        public DbSet<HoaDonItem> HoaDonItems { get; set; }

        // =====================
        // 🔹 Cấu hình quan hệ + decimal
        // =====================
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // ⚠️ cần gọi base để Identity hoạt động

            // 1️⃣ NhanVien ↔ WorkDay (1-nhiều)
            modelBuilder.Entity<WorkDay>()
                .HasOne(w => w.NhanVien)
                .WithMany(n => n.WorkDays)
                .HasForeignKey(w => w.NhanVienId)
                .OnDelete(DeleteBehavior.Cascade);

            // 2️⃣ HoaDon ↔ HoaDonItem (1-nhiều)
            modelBuilder.Entity<HoaDonItem>()
                .HasOne(i => i.HoaDon)
                .WithMany(h => h.Items)
                .HasForeignKey(i => i.HoaDonId)
                .OnDelete(DeleteBehavior.Cascade);

            // 3️⃣ Fix decimal warnings
            modelBuilder.Entity<NhanVien>()
                .Property(nv => nv.LuongTheoGio)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<TongHopDanhSach>()
                .Property(th => th.TongTien)
                .HasColumnType("decimal(18,2)");
        }
    }

    // =====================
    // 🔹 Factory để EF Core migration nhận DbContext
    // =====================
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            optionsBuilder.UseSqlServer(
                "Server=LAPTOP-UJ879HBC\\SQLSERVER;Database=HRMApi;Trusted_Connection=True;TrustServerCertificate=True;"
            );

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
