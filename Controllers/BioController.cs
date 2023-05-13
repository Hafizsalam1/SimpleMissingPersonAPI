using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using MissingPersonApp.Models;
using System;
using System.Globalization;


namespace MissingPersonApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BioController : ControllerBase
    {
        private readonly string _connectionString;

        public BioController (IConfiguration configuration){
            _connectionString = configuration.GetConnectionString("MyDatabase");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Bio>>> GetBioAsync()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var bios = await connection.QueryAsync<Bio>("SELECT * FROM bios");

                foreach (var bio in bios)
                {
                    var relatives = await connection.QueryAsync<Relative>("SELECT * FROM relative WHERE bioid = @bioid", new { bioid = bio.id });
                    bio.relatives = relatives.ToList();
                }

                return bios.ToList();
            }
        }



        // [HttpGet("{id}")]
        // public async Task<ActionResult<Bio>> GetBioById(int id)
        // {
        //     var bio = await _context.bios.FindAsync(id);

        //     if (bio == null)
        //     {
        //         return NotFound();
        //     }

        //     return bio;
        // }


        [HttpPost]
        public async Task<ActionResult<Bio>> PostBioAsync(Bio bio)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            { 
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var dateOfBirth = DateTime.ParseExact(bio.dateofbirth,"yyyy-MM-dd", CultureInfo.InvariantCulture);

                        var result = await connection.QueryFirstOrDefaultAsync<int>("INSERT INTO bios (name, dateofbirth, address) VALUES (@name, @dateofbirth, @address) RETURNING Id", new { name = bio.name, dateofbirth = dateOfBirth, address = bio.address }, transaction);
                        bio.id = result;
                        Console.WriteLine(bio.id);

                        if (bio.relatives != null && bio.relatives.Count > 0)
                        {
                            foreach (var relative in bio.relatives)
                            {
                                await connection.ExecuteAsync("INSERT INTO relative (name, bioid, relationToVictim, phoneNumber) VALUES (@name, @bioid, @relationToVictim, @phoneNumber)", new { name = relative.name, bioid = bio.id, relationToVictim = relative.relationToVictim, phoneNumber = relative.phoneNumber}, transaction);
                            }
                        }

                        transaction.Commit();
                    }
                    catch (NpgsqlException)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }

                return CreatedAtAction(nameof(PostBioAsync), new { id = bio.id }, bio);
            }
        }





        
    }
}