using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using MissingPersonApp.Models;


namespace MissingPersonApp.Controllers{

    [ApiController]
    [Route("api/[controller]")]
    public class KontakController : ControllerBase{
        private readonly string _connectionString;
        // private readonly BioController _biocontroller;

        public KontakController(IConfiguration configuration){
            _connectionString = configuration.GetConnectionString("MyDatabase");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Relative>>> GetKontakAsync()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var relatives = await connection.QueryAsync<Relative>("SELECT * FROM relative");

                return relatives.ToList();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteKontakByIdAsync(int id){
                            using (var connection = new NpgsqlConnection(_connectionString)) {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        await connection.ExecuteAsync("DELETE FROM relative WHERE Id = @Id", new { Id = id }, transaction);

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
        public async Task<ActionResult<Relative>> GetRelativeById(int id){
            using (var connection = new NpgsqlConnection(_connectionString)){
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction()){
                    try{
                        Relative relatives = await connection.QueryFirstOrDefaultAsync<Relative>("SELECT * FROM relative WHERE id = @Id", new { Id = id }, transaction);
                        transaction.Commit();
                        return relatives;

                    }
                    catch(NpgsqlException){
                        transaction.Rollback();


                    }
                    
                }
                return NoContent();



            }
            

        }

        [HttpPost]
        public async Task<ActionResult<Relative>> PostRelativeAsync(Relative relative){
            using (var connection = new NpgsqlConnection(_connectionString)){
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction()){
                    try{
                        await connection.ExecuteAsync("INSERT INTO relative (name, bioid, relationToVictim, phoneNumber) VALUES (@name, @bioid, @relationToVictim, @phoneNumber) RETURNING Id", 
                        new { name = relative.name, relationToVictim = relative.relationToVictim, phoneNumber = relative.phoneNumber, bioid = relative.bioid}, transaction);
                        var relatives = await connection.QueryAsync<Relative>("SELECT * FROM relative WHERE bioid = @bioid", new { bioid = relative.bioid }, transaction);
                        relatives.Append(relative);
                        Bio bio = await connection.QueryFirstOrDefaultAsync<Bio>("SELECT * FROM bios WHERE id = @Id", new { id = relative.bioid }, transaction);
                        bio.relatives = (ICollection<Relative>?)relatives;
                        transaction.Commit();
                    }catch(NpgsqlException){
                        transaction.Rollback();
                        throw;
                    }
                }

                return CreatedAtAction(nameof(PostRelativeAsync), "Relative",new { id = relative.id }, relative);
            }

        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Relative>> PutRelativeAsync (int id, Relative relative){
            using(var connection = new NpgsqlConnection(_connectionString)){
                await connection.OpenAsync();
                using(var transaction = connection.BeginTransaction()){
                    try{
                        Relative relativeLama = await connection.QueryFirstOrDefaultAsync<Relative>("SELECT * FROM relative WHERE id = @id", new { id = id }, transaction);
                        await connection.ExecuteAsync("DELETE FROM relative WHERE bioid = @bioid", new { bioid = relativeLama.bioid }, transaction);

                        // await connection.ExecuteAsync("UPDATE relative SET name = @name, bioid = @bioid, relationToVictim = @relationToVictim, phoneNumber = @phoneNumber WHERE Id = @id", new { name =relative.name, bioid = relative.bioid, relationToVictim = relative.relationToVictim, phoneNumber = relative.phoneNumber, id = id}, transaction);
                        await connection.ExecuteAsync("INSERT INTO relative (name, bioid, relationToVictim, phoneNumber, id) VALUES (@name, @bioid, @relationToVictim, @phoneNumber, @id)", 
                        new { name = relative.name, bioid = relative.bioid, relationToVictim = relative.relationToVictim, phoneNumber = relative.phoneNumber, id = id}, transaction);
                        Bio bio = await connection.QueryFirstOrDefaultAsync<Bio>("SELECT * FROM bios WHERE id = @Id", new { id = relative.bioid }, transaction);

                        List<Relative> relativeBaru = new List<Relative>();
                        relativeBaru.Append(relative);
                        bio.relatives = relativeBaru;
                        transaction.Commit();
                    }catch(NpgsqlException){
                        transaction.Rollback();
                        throw;
                    }
                }
                return CreatedAtAction(nameof(PutRelativeAsync), "Relative",new { id = relative.id }, relative);

            }

        }





        



    }






}
