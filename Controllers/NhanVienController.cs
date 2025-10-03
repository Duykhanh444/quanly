using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMApi.Data;
using HRMApi.Models;
using HRMApi.Models.Dto;
using System.Security.Claims;

namespace HRMApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NhanVienController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public NhanVienController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ========== LẤY DANH SÁCH NHÂN VIÊN ==========
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

            var list = await _context.NhanViens
                .Include(n => n.WorkDays)
                .Where(n => n.UserId == userId)
                .AsNoTracking()
                .ToListAsync();

            return Ok(list.Select(MapToDto));
        }

        // ========== LẤY 1 NHÂN VIÊN ==========
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

            var nv = await _context.NhanViens
                .Include(n => n.WorkDays)
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (nv == null) return NotFound();
            return Ok(MapToDto(nv));
        }

        // ========== THÊM NHÂN VIÊN ==========
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromForm] string hoTen,
            [FromForm] string soDienThoai,
            [FromForm] string chucVu,
            [FromForm] decimal luongTheoGio,
            [FromForm] IFormFile? anhDaiDien)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

            if (string.IsNullOrWhiteSpace(hoTen))
                return BadRequest("Họ tên không được để trống");
            if (string.IsNullOrWhiteSpace(chucVu))
                return BadRequest("Chức vụ không được để trống");

            var nv = new NhanVien
            {
                HoTen = hoTen,
                SoDienThoai = soDienThoai ?? "",
                ChucVu = chucVu,
                LuongTheoGio = luongTheoGio,
                WorkDays = new List<WorkDay>(),
                UserId = userId,
                CreatedBy = User.Identity?.Name ?? ""
            };

            // Upload ảnh
            if (anhDaiDien != null)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(anhDaiDien.FileName);
                var filePath = Path.Combine(_env.ContentRootPath, "wwwroot/uploads", fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                using var fs = new FileStream(filePath, FileMode.Create);
                await anhDaiDien.CopyToAsync(fs);
                nv.AnhDaiDien = fileName;
            }

            _context.NhanViens.Add(nv);
            await _context.SaveChangesAsync();

            nv = await _context.NhanViens.Include(x => x.WorkDays).FirstAsync(x => x.Id == nv.Id);
            return Ok(MapToDto(nv));
        }

        // ========== SỬA NHÂN VIÊN ==========
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            [FromForm] string hoTen,
            [FromForm] string soDienThoai,
            [FromForm] string chucVu,
            [FromForm] decimal luongTheoGio,
            [FromForm] IFormFile? anhDaiDien)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

            var nv = await _context.NhanViens.Include(n => n.WorkDays)
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
            if (nv == null) return NotFound();

            nv.HoTen = hoTen;
            nv.SoDienThoai = soDienThoai ?? "";
            nv.ChucVu = chucVu;
            nv.LuongTheoGio = luongTheoGio;

            if (anhDaiDien != null)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(anhDaiDien.FileName);
                var filePath = Path.Combine(_env.ContentRootPath, "wwwroot/uploads", fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                using var fs = new FileStream(filePath, FileMode.Create);
                await anhDaiDien.CopyToAsync(fs);
                nv.AnhDaiDien = fileName;
            }

            await _context.SaveChangesAsync();
            nv = await _context.NhanViens.Include(x => x.WorkDays).FirstAsync(x => x.Id == nv.Id);
            return Ok(MapToDto(nv));
        }

        // ========== XÓA NHÂN VIÊN ==========
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

            var nv = await _context.NhanViens.Include(n => n.WorkDays)
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
            if (nv == null) return NotFound();

            _context.NhanViens.Remove(nv);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ========== THÊM NGÀY CÔNG ==========
        [HttpPost("{id}/WorkDays")]
        public async Task<IActionResult> AddWorkDay(int id, [FromBody] WorkDayRequestDto dto)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

            var nv = await _context.NhanViens.Include(n => n.WorkDays)
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
            if (nv == null) return NotFound();

            var wd = new WorkDay
            {
                Ngay = dto.Ngay,
                SoGio = dto.SoGio,
                NhanVienId = nv.Id
            };

            _context.WorkDays.Add(wd);
            await _context.SaveChangesAsync();

            nv = await _context.NhanViens.Include(n => n.WorkDays).FirstAsync(n => n.Id == id);
            return Ok(MapToDto(nv));
        }

        // ========== SỬA NGÀY CÔNG ==========
        [HttpPut("{id}/WorkDays/{workDayId}")]
        public async Task<IActionResult> UpdateWorkDay(int id, int workDayId, [FromBody] WorkDayRequestDto dto)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

            var nvCheck = await _context.NhanViens.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
            if (nvCheck == null) return NotFound();

            var wd = await _context.WorkDays.FirstOrDefaultAsync(w => w.Id == workDayId && w.NhanVienId == id);
            if (wd == null) return NotFound();

            wd.Ngay = dto.Ngay;
            wd.SoGio = dto.SoGio;

            await _context.SaveChangesAsync();
            var nv = await _context.NhanViens.Include(n => n.WorkDays).FirstAsync(n => n.Id == id);
            return Ok(MapToDto(nv));
        }

        // ========== XÓA NGÀY CÔNG ==========
        [HttpDelete("{id}/WorkDays/{workDayId}")]
        public async Task<IActionResult> DeleteWorkDay(int id, int workDayId)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

            var nvCheck = await _context.NhanViens.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
            if (nvCheck == null) return NotFound();

            var wd = await _context.WorkDays.FirstOrDefaultAsync(w => w.Id == workDayId && w.NhanVienId == id);
            if (wd == null) return NotFound();

            _context.WorkDays.Remove(wd);
            await _context.SaveChangesAsync();

            var nv = await _context.NhanViens.Include(n => n.WorkDays).FirstAsync(n => n.Id == id);
            return Ok(MapToDto(nv));
        }

        // ========== MAPPER ==========
        private NhanVienDto MapToDto(NhanVien nv)
        {
            return new NhanVienDto
            {
                Id = nv.Id,
                HoTen = nv.HoTen ?? "",
                SoDienThoai = nv.SoDienThoai ?? "",
                ChucVu = nv.ChucVu ?? "",
                LuongTheoGio = (double)nv.LuongTheoGio,
                AnhDaiDien = nv.AnhDaiDien,
                WorkDays = nv.WorkDays.Select(w => new WorkDayDto
                {
                    Id = w.Id,
                    Ngay = w.Ngay,
                    SoGio = w.SoGio
                }).ToList()
            };
        }
    }
}
