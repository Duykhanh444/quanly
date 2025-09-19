using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMApi.Data;
using HRMApi.Models;
using HRMApi.Models.Dto;

namespace HRMApi.Controllers
{
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

        // ========================= Nhân viên =========================

        // GET: api/NhanVien
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _context.NhanViens
                .Include(n => n.WorkDays)
                .AsNoTracking()
                .ToListAsync();
            var dtoList = list.Select(MapToDto).ToList();
            return Ok(dtoList);
        }

        // GET: api/NhanVien/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var nv = await _context.NhanViens
                .Include(n => n.WorkDays)
                .FirstOrDefaultAsync(n => n.Id == id);
            if (nv == null) return NotFound();
            return Ok(MapToDto(nv));
        }

        // POST: api/NhanVien
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromForm] string hoTen,
            [FromForm] string soDienThoai,
            [FromForm] string chucVu,
            [FromForm] decimal luongTheoGio,
            [FromForm] IFormFile? anhDaiDien)
        {
            var nv = new NhanVien
            {
                HoTen = hoTen,
                SoDienThoai = soDienThoai,
                ChucVu = chucVu,
                LuongTheoGio = luongTheoGio,
                WorkDays = new List<WorkDay>()
            };

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

        // PUT: api/NhanVien/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            [FromForm] string hoTen,
            [FromForm] string soDienThoai,
            [FromForm] string chucVu,
            [FromForm] decimal luongTheoGio,
            [FromForm] IFormFile? anhDaiDien)
        {
            var nv = await _context.NhanViens.Include(n => n.WorkDays)
                .FirstOrDefaultAsync(n => n.Id == id);
            if (nv == null) return NotFound();

            nv.HoTen = hoTen;
            nv.SoDienThoai = soDienThoai;
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

        // DELETE: api/NhanVien/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var nv = await _context.NhanViens.Include(n => n.WorkDays).FirstOrDefaultAsync(n => n.Id == id);
            if (nv == null) return NotFound();

            _context.NhanViens.Remove(nv);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ========================= WorkDays =========================

        // POST: api/NhanVien/5/WorkDays
        [HttpPost("{id}/WorkDays")]
        public async Task<IActionResult> AddWorkDay(int id, [FromBody] WorkDayRequestDto dto)
        {
            var nv = await _context.NhanViens.Include(n => n.WorkDays).FirstOrDefaultAsync(n => n.Id == id);
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

        // PUT: api/NhanVien/5/WorkDays/10
        [HttpPut("{id}/WorkDays/{workDayId}")]
        public async Task<IActionResult> UpdateWorkDay(int id, int workDayId, [FromBody] WorkDayRequestDto dto)
        {
            var wd = await _context.WorkDays.FirstOrDefaultAsync(w => w.Id == workDayId && w.NhanVienId == id);
            if (wd == null) return NotFound();

            wd.Ngay = dto.Ngay;
            wd.SoGio = dto.SoGio;

            await _context.SaveChangesAsync();

            var nv = await _context.NhanViens.Include(n => n.WorkDays).FirstAsync(n => n.Id == id);
            return Ok(MapToDto(nv));
        }

        // DELETE: api/NhanVien/5/WorkDays/10
        [HttpDelete("{id}/WorkDays/{workDayId}")]
        public async Task<IActionResult> DeleteWorkDay(int id, int workDayId)
        {
            var wd = await _context.WorkDays.FirstOrDefaultAsync(w => w.Id == workDayId && w.NhanVienId == id);
            if (wd == null) return NotFound();

            _context.WorkDays.Remove(wd);
            await _context.SaveChangesAsync();

            var nv = await _context.NhanViens.Include(n => n.WorkDays).FirstAsync(n => n.Id == id);
            return Ok(MapToDto(nv));
        }

        // ========================= Mapping =========================
        private NhanVienDto MapToDto(NhanVien nv)
        {
            return new NhanVienDto
            {
                Id = nv.Id,
                HoTen = nv.HoTen,
                SoDienThoai = nv.SoDienThoai,
                ChucVu = nv.ChucVu,
                LuongTheoGio = (double)nv.LuongTheoGio,
                AnhDaiDien = nv.AnhDaiDien,
                WorkDays = nv.WorkDays.Select(w => new WorkDayResponseDto
                {
                    Id = w.Id,
                    Ngay = w.Ngay,
                    SoGio = w.SoGio
                }).ToList()
            };
        }
    }
}
