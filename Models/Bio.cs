using System.ComponentModel.DataAnnotations;

namespace MissingPersonApp.Models;

public class Bio
{
    public int id { get; set; }
    public string name { get; set; }
    public DateTime dateofbirth { get; set; }
    public string address {get;set;}
    // public ICollection<Kontak> kontaks { get; set; }
    // public string additional_info {get; set;}
}