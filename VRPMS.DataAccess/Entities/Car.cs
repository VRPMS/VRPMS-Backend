using LinqToDB;
using LinqToDB.Mapping;

namespace VRPMS.DataAccess.Entities;

[Table("cars", Schema = "vrpms")]
internal class Car
{
    [PrimaryKey, Identity]
    [Column("id", DataType = DataType.Int32, IsPrimaryKey = true, PrimaryKeyOrder = 0), NotNull]
    public int Id { get; set; }

    [Column("capacity_overload_penalty", DataType = DataType.Int32), NotNull]
    public int CapacityOverloadPenalty { get; set; }

    [Column("max_capacity_overload_penalty", DataType = DataType.Int32), NotNull]
    public int MaxCapacityOverloadPenalty { get; set; }

    [Column("work_start", DataType = DataType.Time, SkipOnInsert = true), NotNull]
    public TimeSpan WorkStart { get; set; }

    [Column("work_end", DataType = DataType.Time, SkipOnInsert = true), NotNull]
    public TimeSpan WorkEnd { get; set; }

    [Column("overwork_penalty", DataType = DataType.Int32), NotNull]
    public int OverWorkPenalty { get; set; }

    [Column("route_template"), NotNull]
    public int?[] RouteTemplate { get; set; } = [];

    [Association(ThisKey = nameof(Id), OtherKey = nameof(CarCapacity.CarId))]
    public IEnumerable<CarCapacity> CarCapacities { get; set; } = null!;
}
