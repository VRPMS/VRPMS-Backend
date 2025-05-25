namespace VRPMS.DataAccess.Interfaces.Dtos;

public class DemandTypeDto
{
    public int Id { get; set; } = default;

    public required string Name { get; set; } = null!;
}
