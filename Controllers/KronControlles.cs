using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using MissingPersonApp.Models;
using System.Globalization;


namespace MissingPersonApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KronController :  ControllerBase{

        private readonly string _connectionString;

        public KronController(IConfiguration configuration){
            _connectionString = configuration.GetConnectionString("MyDatabase");
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<Kronologi>>> GetKontakAsync()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var chronologies = await connection.QueryAsync<Kronologi>("SELECT * FROM kronologi");

                return chronologies.ToList();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteKronByIdAsync(int id){
                using (var connection = new NpgsqlConnection(_connectionString)) {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        await connection.ExecuteAsync("DELETE FROM kronologi WHERE Id = @Id", new { Id = id }, transaction);

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
        public async Task<ActionResult<Kronologi>> GetKronologiById(int id){
            using (var connection = new NpgsqlConnection(_connectionString)){
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction()){
                    try{
                        Kronologi chronologies = await connection.QueryFirstOrDefaultAsync<Kronologi>("SELECT * FROM kronologi WHERE id = @Id", new { id = id }, transaction);
                        transaction.Commit();
                        return chronologies;

                    }
                    catch(NpgsqlException){
                        transaction.Rollback();


                    }
                    
                }
                return NoContent();



            }
            

        }

        [HttpPost]
        public async Task<ActionResult<Kronologi>> PostKronAsync(Kronologi chronology){
            using (var connection = new NpgsqlConnection(_connectionString)){
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction()){
                    try{
                        DateTime chroTime = DateTime.ParseExact(chronology.dateAndTime,"yyyy/MM/dd hh:mm tt", CultureInfo.InvariantCulture);
                        await connection.ExecuteAsync("INSERT INTO kronologi (activityName, bioid, dateAndTime, additionalNote) VALUES (@activityName, @bioid, @dateAndTime, @additionalNote) RETURNING Id", 
                        new { activityName = chronology.activityName, bioid = chronology.bioid, dateAndTime = chroTime, additionalNote = chronology.additionalNote}, transaction);
                        var chronologies = await connection.QueryAsync<Kronologi>("SELECT * FROM kronologi WHERE bioid = @bioid", new { bioid = chronology.bioid }, transaction);
                        chronologies.Append(chronology);
                        Bio bio = await connection.QueryFirstOrDefaultAsync<Bio>("SELECT * FROM bios WHERE id = @Id", new { id = chronology.bioid }, transaction);
                        bio.chronology = (ICollection<Kronologi>?)chronologies;
                        transaction.Commit();
                    }catch(NpgsqlException){
                        transaction.Rollback();
                        throw;
                    }
                }

                return CreatedAtAction(nameof(PostKronAsync), "Kron",new { id = chronology.id }, chronology);
            }

        }            







    }


    
}