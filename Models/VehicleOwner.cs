using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingSystem.Models;

[Table("VehicleOwner")]
public class VehicleOwner
{
    [Key]
    public int OwnerID { get; set; }

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = null!;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = null!;

    [Required, MaxLength(20)]
    public string PhoneNumber { get; set; } = null!

;
    [MaxLength(200)]
    public string? Email { get; set; }

    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

    [NotMapped]
    public string FullName => $"{LastName} {FirstName}";
}
