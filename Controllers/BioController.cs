using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MissingPersonApp.Data;
using MissingPersonApp.Models;
using System.Globalization;

namespace MissingPersonApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BioController : ControllerBase
    {
        private readonly MyDbContext _context;

        public BioController (MyDbContext context){
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Bio>>> GetBios()
        {
            return await _context.bios.ToListAsync();
        }



        [HttpGet("{id}")]
        public async Task<ActionResult<Bio>> GetBioById(int id)
        {
            var bio = await _context.bios.FindAsync(id);

            if (bio == null)
            {
                return NotFound();
            }

            return bio;
        }


        [HttpPost]
        public async Task<ActionResult<Bio>> CreateTodo(Bio bio)
        {
            // DateTime date = DateTime.ParseExact(bio.dateofbirth, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            _context.bios.Add(bio);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBioById), new { id = bio.id }, bio);
        }





        
    }
}