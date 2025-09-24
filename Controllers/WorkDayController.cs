using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMApi.Data;
using HRMApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace HRMApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkDayController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WorkDayController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ======================== DTO ========================
        public class WorkDayResponseDto
        {
            public int Id { get; set; }
            public DateTime Ngay { get; set; }
            public int SoGio { get; set; }
            public int NhanVienId { get; set; }
            public string? NhanVienTen { get; set; }
            public int TongSoGioDaChamCong { get; set; }
        }

        // ======================== GET all ========================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkDayResponseDto>>> GetWorkDays()
        {
            try
            {
                var workDays = await _context.WorkDays.Include(w => w.NhanVien).ToListAsync();
                var result = workDays.Select(w => new WorkDayResponseDto
                {
                    Id = w.Id,
                    Ngay = w.Ngay,
                    SoGio = w.SoGio,
                    NhanVienId = w.NhanVienId,
                    NhanVienTen = w.NhanVien?.HoTen,
                    TongSoGioDaChamCong = w.NhanVien?.WorkDays.Sum(x => x.SoGio) ?? 0
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetWorkDays Exception: {ex}");
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        // ======================== GET by id ========================
        [HttpGet("{id}")]
        public async Task<ActionResult<WorkDayResponseDto>> GetWorkDay(int id)
        {
            try
            {
                var workDay = await _context.WorkDays
                    .Include(w => w.NhanVien)
                    .ThenInclude(n => n.WorkDays)
                    .FirstOrDefaultAsync(w => w.Id == id);

                if (workDay == null) return NotFound();

                var result = new WorkDayResponseDto
                {
                    Id = workDay.Id,
                    Ngay = workDay.Ngay,
                    SoGio = workDay.SoGio,
                    NhanVienId = workDay.NhanVienId,
                    NhanVienTen = workDay.NhanVien?.HoTen,
                    TongSoGioDaChamCong = workDay.NhanVien?.WorkDays.Sum(x => x.SoGio) ?? 0
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetWorkDay Exception: {ex}");
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        // ======================== POST ========================
        [HttpPost]
        public async Task<ActionResult<WorkDayResponseDto>> PostWorkDay([FromBody] WorkDay workDay)
        {
            try
            {
                _context.WorkDays.Add(workDay);
                await _context.SaveChangesAsync();

                var nv = await _context.NhanViens.Include(n => n.WorkDays)
                    .FirstOrDefaultAsync(n => n.Id == workDay.NhanVienId);

                var result = new WorkDayResponseDto
                {
                    Id = workDay.Id,
                    Ngay = workDay.Ngay,
                    SoGio = workDay.SoGio,
                    NhanVienId = workDay.NhanVienId,
                    NhanVienTen = nv?.HoTen,
                    TongSoGioDaChamCong = nv?.WorkDays.Sum(x => x.SoGio) ?? 0
                };

                return CreatedAtAction(nameof(GetWorkDay), new { id = workDay.Id }, result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PostWorkDay Exception: {ex}");
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        // ======================== PUT ========================
        [HttpPut("{id}")]
        public async Task<ActionResult<WorkDayResponseDto>> PutWorkDay(int id, [FromBody] WorkDay workDay)
        {
            if (id != workDay.Id) return BadRequest("ID không khớp.");

            try
            {
                _context.Entry(workDay).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                var nv = await _context.NhanViens.Include(n => n.WorkDays)
                    .FirstOrDefaultAsync(n => n.Id == workDay.NhanVienId);

                var result = new WorkDayResponseDto
                {
                    Id = workDay.Id,
                    Ngay = workDay.Ngay,
                    SoGio = workDay.SoGio,
                    NhanVienId = workDay.NhanVienId,
                    NhanVienTen = nv?.HoTen,
                    TongSoGioDaChamCong = nv?.WorkDays.Sum(x => x.SoGio) ?? 0
                };

                return Ok(result);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.WorkDays.Any(w => w.Id == id)) return NotFound();
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PutWorkDay Exception: {ex}");
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        // ======================== DELETE ========================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorkDay(int id)
        {
            try
            {
                var workDay = await _context.WorkDays.FindAsync(id);
                if (workDay == null) return NotFound();

                _context.WorkDays.Remove(workDay);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeleteWorkDay Exception: {ex}");
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }
    }
}
