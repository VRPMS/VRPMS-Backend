using LinqToDB;
using LinqToDB.Mapping;

namespace VRPMS.DataAccess.Entities;

[Table("demands", Schema = "vrpms")]
internal class Demand
{
    [PrimaryKey, Identity]
    [Column("id", DataType = DataType.Int32, IsPrimaryKey = true, PrimaryKeyOrder = 0), NotNull]
    public int Id { get; set; }

    [Column("name", DataType = DataType.VarChar), NotNull]
    public required string Name { get; set; } = null!;

    [Association(ThisKey = nameof(Id), OtherKey = nameof(PointDemand.DemandId))]
    public IEnumerable<PointDemand> PointDemands { get; set; } = null!;

    [Association(ThisKey = nameof(Id), OtherKey = nameof(CarCapacity.DemandId))]
    public IEnumerable<CarCapacity> CarCapacities { get; set; } = null!;
}
