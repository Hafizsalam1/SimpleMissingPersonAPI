using System.ComponentModel.DataAnnotations;

namespace MissingPersonApp.Models;

public class Bio
{
    public int id { get; set; }
    public string name { get; set; }
    public string dateofbirth { get; set; }
    public string address {get;set;}
    public ICollection<Relative> relatives { get; set; }


    // public string additional_info {get; set;}
}