using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MissingPersonApp.Models;

public class Relative
{
        public int id { get; set; }
        public string name { get; set; }
        public int bioid { get; set; }

        [JsonIgnore]
        public Bio? bio { get; set; }
        public string relationToVictim { get; set; }
        public string phoneNumber { get; set; }



    // public string additional_info {get; set;}
}