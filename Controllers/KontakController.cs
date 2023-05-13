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



    }






}
