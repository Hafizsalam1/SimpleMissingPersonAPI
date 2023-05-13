using System.ComponentModel.DataAnnotations;

namespace MissingPersonApp.Models;

public class Relative
{
        public int id { get; set; }
        public string name { get; set; }
        public int bioid { get; set; }
        public Bio? bio { get; set; }
        public string relationToVictim { get; set; }
        public string phoneNumber { get; set; }



    // public string additional_info {get; set;}
}