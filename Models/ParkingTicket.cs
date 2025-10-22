using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingSystem.Models;

[Table("ParkingTicket")]
public class ParkingTicket
{
    [Key]
    public int TicketID { get; set; }

    public int VehicleID { get; set; }
    public int ParkingSpaceID { get; set; }

    public DateTime EntryTime { get; set; }
    public DateTime? ExitTime { get; set; }
    public decimal? Fee { get; set; }

    public Vehicle? Vehicle { get; set; }
    public ParkingSpace? ParkingSpace { get; set; }
}
