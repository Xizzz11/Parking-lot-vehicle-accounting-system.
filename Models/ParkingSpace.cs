using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingSystem.Models;

[Table("ParkingSpace")]
public class ParkingSpace
{
    [Key]
    public int ParkingSpaceID { get; set; }

    [Required, MaxLength(10)]
    public string SpaceNumber { get; set; } = null!;

    [Required]
    public bool IsOccupied { get; set; }
}
