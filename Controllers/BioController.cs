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
                    var chronology = await connection.QueryAsync<Kronologi>("SELECT * FROM kronologi WHERE bioid = @bioid", new { bioid = bio.id });

                    bio.relatives = relatives.ToList();
                    bio.chronology = chronology.ToList();
                }

                return bios.ToList();
            }
        }


            [HttpDelete("{id}")]
            public async Task<IActionResult> DeleteBioByIdAsync(int id){
                using (var connection = new NpgsqlConnection(_connectionString)) {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        await connection.ExecuteAsync("DELETE FROM relative WHERE bioid = @bioid", new { bioid = id }, transaction);
                        await connection.ExecuteAsync("DELETE FROM kronologi WHERE bioid = @bioid", new { bioid = id }, transaction);
                        await connection.ExecuteAsync("DELETE FROM bios WHERE Id = @Id", new { Id = id }, transaction);

                        transaction.Commit();
                    }
                    catch (NpgsqlException)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            return NoContent();
            }

        [HttpGet("{id}")]
        public async Task<ActionResult<Bio>> GetBioById(int id){
            using (var connection = new NpgsqlConnection(_connectionString)){
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction()){
                    try{
                        Bio bio = await connection.QueryFirstOrDefaultAsync<Bio>("SELECT * FROM bios WHERE id = @Id", new { id = id }, transaction);
                        var relatives = await connection.QueryAsync<Relative>("SELECT * FROM relative WHERE bioid = @bioid", new { bioid = id }, transaction);
                        var chronology = await connection.QueryAsync<Kronologi>("SELECT * FROM kronologi WHERE bioid = @bioid", new { bioid = id }, transaction);
                        bio.relatives = (ICollection<Relative>?)relatives;
                        bio.chronology = (ICollection<Kronologi>?)chronology;
                        transaction.Commit();
                        return bio;

                    }
                    catch(NpgsqlException){
                        transaction.Rollback();


                    }
                    
                }
                return NoContent();



            }
            

        }


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
                        DateTime dateOfBirth = DateTime.ParseExact(bio.dateofbirth,"yyyy-MM-dd", CultureInfo.InvariantCulture);
                        DateTime lastSeenTimes = DateTime.ParseExact(bio.lastSeenTime,"yyyy/MM/dd hh:mm tt", CultureInfo.InvariantCulture);



                        var result = await connection.QueryFirstOrDefaultAsync<int>("INSERT INTO bios (name, dateofbirth, address, lastSeenTime, lastSeenPlace, additionalNote) VALUES (@name, @dateofbirth, @address, @lastSeenTime, @lastSeenPlace, @additionalNote) RETURNING Id", 
                        new { name = bio.name, dateofbirth = dateOfBirth, address = bio.address, lastSeenTime = lastSeenTimes, lastSeenPlace = bio.lastSeenPlace, additionalNote = bio.additionalNote}, transaction);
                        bio.id = result;

                        if (bio.relatives != null && bio.relatives.Count > 0 && bio.chronology != null && bio.chronology.Count > 0)
                        {
                            foreach (var relative in bio.relatives)
                            {
                                await connection.ExecuteAsync("INSERT INTO relative (name, bioid, relationToVictim, phoneNumber) VALUES (@name, @bioid, @relationToVictim, @phoneNumber)", 
                                new { name = relative.name, bioid = bio.id, relationToVictim = relative.relationToVictim, phoneNumber = relative.phoneNumber}, transaction);
                            }
                            foreach (var chronology in bio.chronology)
                            {
                                DateTime chroTime = DateTime.ParseExact(chronology.dateAndTime,"yyyy/MM/dd hh:mm tt", CultureInfo.InvariantCulture);


                                await connection.ExecuteAsync("INSERT INTO kronologi (activityName, bioid, dateAndTime, additionalNote) VALUES (@activityName, @bioid, @dateAndTime, @additionalNote)",
                                 new{activityName = chronology.activityName, bioid = bio.id, dateAndTime = chroTime, additionalNote = chronology.additionalNote});
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

                return CreatedAtAction(nameof(PostBioAsync), "Bio",new { id = bio.id }, bio);
            }
        }


        [HttpPut("{id}")]
        public async Task<ActionResult<Bio>> PutBioAsync(Bio bio, int id){
            using (var connection = new NpgsqlConnection(_connectionString)){
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction()){
                    try{
                        if (GetBioById(id)==null){
                            throw new Exception("There is no missiong person with that ID");
                        }
                        else{
                        DateTime dateOfBirth = DateTime.ParseExact(bio.dateofbirth,"yyyy-MM-dd", CultureInfo.InvariantCulture);
                        DateTime lastSeenTimes = DateTime.ParseExact(bio.lastSeenTime,"yyyy/MM/dd hh:mm tt", CultureInfo.InvariantCulture);
                            
                        var result = await connection.QueryFirstOrDefaultAsync<int>("UPDATE bios SET name = :name, dateofbirth = :dateofbirth, address = :address, lastSeenTime = :lastSeenTime, lastSeenPlace = :lastSeenPlace, additionalNote = :additionalNote WHERE id = :Id", 
                        new { name = bio.name, dateofbirth = dateOfBirth, address = bio.address, lastSeenTime = lastSeenTimes, lastSeenPlace = bio.lastSeenPlace, additionalNote = bio.additionalNote}, transaction);

                        if (bio.relatives != null && bio.relatives.Count > 0 && bio.chronology != null && bio.chronology.Count > 0){
                            foreach (var relative in bio.relatives)
                            {
                                await connection.ExecuteAsync("UPDATE relative SET name = :name, bioid = :bioid, relationToVictim = :relationToVictim, phoneNumber = :phoneNumber WHERE id = :Id", 
                                new { name = relative.name, bioid = bio.id, relationToVictim = relative.relationToVictim, phoneNumber = relative.phoneNumber}, transaction);
                                
                            }
                            foreach (var chronology in bio.chronology){
                                DateTime chroTime = DateTime.ParseExact(chronology.dateAndTime,"yyyy/MM/dd hh:mm tt", CultureInfo.InvariantCulture);
                                await connection.ExecuteAsync("UPDATE kronologi SET activityName = :activityName, bioid = :bioid, dateAndTime = :dateAndTime, additionalNote = :additionalNote WHERE id = :Id",
                                new{activityName = chronology.activityName, bioid = bio.id, dateAndTime = chroTime, additionalNote = chronology.additionalNote});
                            }


                        }
                        transaction.Commit();


                        }



                    }catch(NpgsqlException){
                        transaction.Rollback();
                        throw;
                        
                    }
                }

                return CreatedAtAction(nameof(PutBioAsync), "Bio",new { id = bio.id }, bio);

            }

        }







        
    }
}