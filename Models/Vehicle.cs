using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingSystem.Models;

[Table("Vehicle")]
public class Vehicle
{
    [Key]
    public int VehicleID { get; set; }

    [Required, MaxLength(15)]
    public string LicensePlate { get; set; } = null!;

    [Required, MaxLength(100)]
    public string Make { get; set; } = null!;

    [Required, MaxLength(100)]
    public string Model { get; set; } = null!;

    [Required, MaxLength(50)]
    public string Color { get; set; } = null!;

    public int OwnerID { get; set; }
    public VehicleOwner? Owner { get; set; }
}
