
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMApi.Data;
using HRMApi.Models;

namespace HRMApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HoaDonController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HoaDonController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int TinhTongTien(HoaDon hd)
        {
            return hd.Items.Sum(i => i.SoLuong * i.GiaTien);
        }

        // 🔹 Lấy tất cả hóa đơn của user đăng nhập
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HoaDon>>> GetAll()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var list = await _context.HoaDons
                .Include(h => h.Items)
                .Where(h => h.UserId == userId)
                .ToListAsync();

            return Ok(list);
        }

        // 🔹 Lấy 1 hóa đơn theo Id
        [HttpGet("{id}")]
        public async Task<ActionResult<HoaDon>> GetById(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var hd = await _context.HoaDons
                .Include(h => h.Items)
                .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

            if (hd == null) return NotFound();
            return Ok(hd);
        }

        // 🔹 Thêm hóa đơn mới
        [HttpPost]
        public async Task<ActionResult<HoaDon>> Create(HoaDon hd)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            hd.UserId = userId;
            hd.TongTien = TinhTongTien(hd);

            // ✅ Lưu phương thức thanh toán
            hd.PhuongThuc = hd.PhuongThuc ?? "Tiền mặt";

            _context.HoaDons.Add(hd);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = hd.Id }, hd);
        }

        // 🔹 Cập nhật hóa đơn
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, HoaDon hd)
        {
            if (id != hd.Id) return BadRequest();

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var existing = await _context.HoaDons
                .Include(h => h.Items)
                .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

            if (existing == null) return NotFound();

            // ✅ Cập nhật field chính
            existing.MaHoaDon = hd.MaHoaDon;
            existing.LoaiHoaDon = hd.LoaiHoaDon;
            existing.NgayLap = hd.NgayLap;
            existing.TrangThai = hd.TrangThai;
            existing.PhuongThuc = hd.PhuongThuc; // <== thêm vào
            existing.TongTien = TinhTongTien(hd);

            // ✅ Cập nhật Items
            var idsMoi = hd.Items.Select(i => i.Id).ToList();
            var itemsToDelete = existing.Items.Where(i => !idsMoi.Contains(i.Id)).ToList();
            _context.HoaDonItems.RemoveRange(itemsToDelete);

            foreach (var item in hd.Items)
            {
                var existingItem = existing.Items.FirstOrDefault(i => i.Id == item.Id);
                if (existingItem != null)
                {
                    existingItem.TenHang = item.TenHang;
                    existingItem.SoLuong = item.SoLuong;
                    existingItem.GiaTien = item.GiaTien;
                }
                else
                {
                    existing.Items.Add(new HoaDonItem
                    {
                        TenHang = item.TenHang,
                        SoLuong = item.SoLuong,
                        GiaTien = item.GiaTien
                    });
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // 🔹 Xóa hóa đơn
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var hd = await _context.HoaDons.Include(h => h.Items)
                .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

            if (hd == null) return NotFound();

            _context.HoaDonItems.RemoveRange(hd.Items);
            _context.HoaDons.Remove(hd);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpPost("tao-theo-ma/{maSanPham}")]
public async Task<IActionResult> TaoHoaDonTheoMa(string maSanPham)
{
    try
    {
        // ✅ 1️⃣ Tìm sản phẩm trong kho theo mã
        var sanPham = await _context.KhoHangs.FirstOrDefaultAsync(x => x.MaKho == maSanPham);
        if (sanPham == null)
        {
            return NotFound(new { message = $"Không tìm thấy sản phẩm có mã {maSanPham}" });
        }

        // ✅ 2️⃣ Tạo hóa đơn mới
        var hoaDon = new HoaDon
        {
            MaHoaDon = $"HD-{DateTime.Now:yyyyMMddHHmmss}",
            NgayLap = DateTime.Now,
            LoaiHoaDon = "Xuất kho",
            PhuongThuc = "Tiền mặt",
            TrangThai = "Chưa thanh toán",
            TongTien = sanPham.GiaTri, // dùng giá trị sản phẩm làm tổng tiền
            Items = new List<HoaDonChiTiet>()
            {
                new HoaDonChiTiet
                {
                    MaSanPham = sanPham.MaKho,
                    TenSanPham = sanPham.TenKho,
                    SoLuong = 1,
                    DonGia = sanPham.GiaTri,
                    ThanhTien = sanPham.GiaTri
                }
            }
        };

        // ✅ 3️⃣ Lưu hóa đơn
        _context.HoaDons.Add(hoaDon);
        await _context.SaveChangesAsync();

        return Ok(hoaDon);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { message = "Lỗi khi tạo hóa đơn từ mã sản phẩm", error = ex.Message });
    }
}

    }
}
