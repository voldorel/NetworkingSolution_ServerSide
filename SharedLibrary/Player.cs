using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedLibrary;

public class Player
{
    
    public int Id { get; set; }
    public int Level{ get; set; }
    public float Health{ get; set; }
}