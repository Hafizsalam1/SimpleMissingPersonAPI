using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MissingPersonApp.Models;


public class Kronologi{
    public int id {get; set;}
    public string dateAndTime {get; set;}
    public string activityName {get; set;}
    public string? additionalNote {get; set;}
    public int bioid { get; set; }

    [JsonIgnore]
    public Bio? bio {get; set;}

}