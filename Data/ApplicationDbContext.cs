using HRMApi.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<NhanVien> NhanViens { get; set; }
        public DbSet<HoaDon> HoaDons { get; set; }
        public DbSet<HoaDonItem> HoaDonItems { get; set; }
        public DbSet<KhoHang> KhoHang { get; set; }
        public DbSet<DoanhThu> DoanhThus { get; set; }
        public DbSet<WorkDay> WorkDays { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User ↔ NhanVien
            modelBuilder.Entity<NhanVien>()
                .HasOne<User>()
                .WithMany(u => u.NhanViens)
                .HasForeignKey(nv => nv.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User ↔ HoaDon
            modelBuilder.Entity<HoaDon>()
                .HasOne<User>()
                .WithMany(u => u.HoaDons)
                .HasForeignKey(hd => hd.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User ↔ KhoHang
            modelBuilder.Entity<KhoHang>()
                .HasOne<User>()
                .WithMany(u => u.KhoHang)
                .HasForeignKey(kh => kh.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User ↔ DoanhThu
            modelBuilder.Entity<DoanhThu>()
                .HasOne<User>()
                .WithMany(u => u.DoanhThus)
                .HasForeignKey(dt => dt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // NhanVien ↔ WorkDay
            modelBuilder.Entity<WorkDay>()
                .HasOne(w => w.NhanVien)
                .WithMany(nv => nv.WorkDays)
                .HasForeignKey(w => w.NhanVienId)
                .OnDelete(DeleteBehavior.Cascade);

            // HoaDon ↔ HoaDonItem
            modelBuilder.Entity<HoaDonItem>()
                .HasOne(i => i.HoaDon)
                .WithMany(h => h.Items)
                .HasForeignKey(i => i.HoaDonId)
                .OnDelete(DeleteBehavior.Cascade);

            // Fix decimal warnings
            modelBuilder.Entity<NhanVien>()
                .Property(nv => nv.LuongTheoGio)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<DoanhThu>()
                .Property(dt => dt.TongTien)
                .HasColumnType("decimal(18,2)");
        }
    }
}
