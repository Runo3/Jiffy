﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zeiterfassungssoftware.Client.Data.Filter;
using Zeiterfassungssoftware.Data;
using Zeiterfassungssoftware.Data.Jiffy.Models;
using Zeiterfassungssoftware.SharedData.Time;

namespace Zeiterfassungssoftware.Services
{

    [Route("api/v1/[controller]")]
	[ApiController]
    [Authorize]
    public class EntriesController : ControllerBase
	{
        private ApplicationDbContext _context;

        public EntriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
		public IActionResult GetAllEntries()
		{
            string UserId = User.Claims.FirstOrDefault()?.Value ?? string.Empty;

            if (string.IsNullOrEmpty(UserId))
                return Unauthorized();

            string Username = User.IsInRole("Administrator") ? User.Identity.Name.Split("@")[0] : string.Empty;

            var Entries = _context.Entries.Where(e => e.UserId == UserId || User.IsInRole("Administrator"))
                .OrderByDescending(e => e.Start)
                .Select(e => e.ToTimeEntry(Username))
                .ToList();

            return Ok(Entries);
		}

		[HttpGet("{Id}")]
		public IActionResult GetEntryById(Guid Id)
		{
            string UserId = User.Claims.FirstOrDefault()?.Value ?? string.Empty;

            if (string.IsNullOrEmpty(UserId))
                return Unauthorized();

            Entry? DbEntry = _context.Entries.FirstOrDefault(e => e.Id == Id && ((e.UserId == UserId) || (User.IsInRole("Administrator"))));
                
            if (DbEntry is null)
                return NotFound();

            string Username = User.IsInRole("Administrator") ? User.Identity.Name.Split("@")[0] : string.Empty;

            return Ok(DbEntry.ToTimeEntry(Username));            
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] TimeEntry Entry)
        {
            if (!IsValid(Entry))
                return BadRequest("Invalid data");

            string UserId = User.Claims.FirstOrDefault()?.Value ?? string.Empty;

            if (string.IsNullOrEmpty(UserId))
                return Unauthorized();

            Entry DbEntry = new()
            {
                Id = Guid.NewGuid(),
                Start = Entry.Start,
                End = Entry.End,
                Title = Entry.Title,
                Description = Entry.Description,
                UserId = UserId,
            };

            _context.Entries.Add(DbEntry);
            _context.SaveChanges();

            string Username = User.IsInRole("Administrator") ? User.Identity.Name.Split("@")[0] : string.Empty;

            return Ok(DbEntry.ToTimeEntry(Username));
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteEntry(Guid Id)
        {
            string UserId = User.Claims.FirstOrDefault()?.Value ?? string.Empty;

            if (string.IsNullOrEmpty(UserId))
                return Unauthorized();

            Entry? Entry = _context.Entries.FirstOrDefault(e => (e.Id == Id) && ((e.UserId == UserId) || (User.IsInRole("Administrator"))));

            if (Entry is null)
                return NotFound();

            _context.Entries.Remove(Entry);
            _context.SaveChanges();

            return Ok();
        }

		[HttpPatch]
        public IActionResult PatchEntry([FromBody] TimeEntry Entry)
        {
            if (!IsValid(Entry))
                return BadRequest("Invalid data");

            string UserId = User.Claims.FirstOrDefault()?.Value ?? string.Empty;

            if (string.IsNullOrEmpty(UserId))
                return Unauthorized();

            Entry? DbEntry = _context.Entries.FirstOrDefault(e => (e.Id == Entry.Id) && ((e.UserId == UserId) || (User.IsInRole("Administrator"))));

            if (DbEntry is null)
                return NotFound();

            DbEntry.Start = Entry.Start;
            DbEntry.End = Entry.End;
            DbEntry.Title = Entry.Title;
            DbEntry.Description = Entry.Description;

            _context.SaveChanges();

            string Username = User.IsInRole("Administrator") ? User.Identity.Name.Split("@")[0] : string.Empty;

            return Ok(DbEntry.ToTimeEntry(Username));
        }

        public bool IsValid(TimeEntry Entry)
        {
            if(string.IsNullOrWhiteSpace(Entry.Title) || string.IsNullOrWhiteSpace(Entry.Description)) 
                return false;

            if ((Entry.End is not null) && Entry.Start >= Entry.End) 
                return false;

            return true;
        }

    }
}
