using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ASPCRUDV3.Data;
using ASPCRUDV3.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ASPCRUDV3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataItemsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DataItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var dataItems = await _context.DataItems.ToListAsync();
            return Ok(dataItems);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var item = await _context.DataItems.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] DataItem item)
        {
            if (item == null)
            {
                return BadRequest("Item cannot be null.");
            }

            await _context.DataItems.AddAsync(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] DataItem item)
        {
            if (item == null)
            {
                return BadRequest("Item cannot be null.");
            }

            if (id != item.Id)
            {
                return BadRequest("ID mismatch.");
            }

            var existingItem = await _context.DataItems.FindAsync(id);
            if (existingItem == null)
            {
                return NotFound();
            }

            existingItem.ChargisRange = item.ChargisRange;
            existingItem.EntryTime = item.EntryTime;
            existingItem.ExitTime = item.ExitTime;
            existingItem.Note = item.Note;
            existingItem.Line = item.Line;
            existingItem.Shift = item.Shift;

            _context.DataItems.Update(existingItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("shift-counts")]
        public async Task<IActionResult> GetShiftCounts()
        {
            var dataItems = await _context.DataItems.ToListAsync();

            int shift1Count = 0, shift2Count = 0, shift3Count = 0;

            foreach (var item in dataItems)
            {
                int count = CalculateChargisCount(item.ChargisRange);
                int shift = DetermineShift(item.EntryTime, item.ExitTime);

                switch (shift)
                {
                    case 1:
                        shift1Count += count;
                        break;
                    case 2:
                        shift2Count += count;
                        break;
                    case 3:
                        shift3Count += count;
                        break;
                }
            }

            var totalCount = shift1Count + shift2Count + shift3Count;

            return Ok(new
            {
                Shift1Count = shift1Count,
                Shift2Count = shift2Count,
                Shift3Count = shift3Count,
                TotalCount = totalCount
            });
        }

        private int CalculateChargisCount(string range)
        {
            var parts = range.Split('-');
            if (parts.Length == 2 && int.TryParse(parts[0].Trim(), out int start) && int.TryParse(parts[1].Trim(), out int end))
            {
                return end - start + 1;  // Calculate the number of units in the range
            }
            return 0;
        }

        private int DetermineShift(string entryTime, string exitTime)
        {
            TimeSpan shift1Start = TimeSpan.Parse("07:00");
            TimeSpan shift1End = TimeSpan.Parse("15:00");
            TimeSpan shift2Start = TimeSpan.Parse("15:01");
            TimeSpan shift2End = TimeSpan.Parse("23:00");
            TimeSpan shift3Start = TimeSpan.Parse("23:01");
            TimeSpan shift3End = TimeSpan.Parse("06:59");  // 06:59 is just before the start of shift 1 on the next day

            TimeSpan entryTimeOfDay = TimeSpan.Parse(entryTime);
            TimeSpan exitTimeOfDay = TimeSpan.Parse(exitTime);

            if ((entryTimeOfDay >= shift1Start && entryTimeOfDay <= shift1End) ||
                (exitTimeOfDay > shift1Start && exitTimeOfDay <= shift1End))
            {
                return 1;  // Shift 1
            }
            else if ((entryTimeOfDay >= shift2Start && entryTimeOfDay <= shift2End) ||
                     (exitTimeOfDay > shift2Start && exitTimeOfDay <= shift2End))
            {
                return 2;  // Shift 2
            }
            else if ((entryTimeOfDay >= shift3Start || entryTimeOfDay <= shift3End) ||
                     (exitTimeOfDay > shift3Start || exitTimeOfDay <= shift3End))
            {
                return 3;  // Shift 3
            }

            return 0;  // Undefined shift or error
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.DataItems.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            _context.DataItems.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}