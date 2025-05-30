using LinqToDB;
using LinqToDB.Mapping;
using VRPMS.DataAccess.Constants;

namespace VRPMS.DataAccess.Entities;

[Table("cars", Schema = DbConstants.Schema)]
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

    [Column("route_start_point_id", DataType = DataType.Int32), NotNull]
    public int RouteStartPointId { get; set; }

    [Association(ThisKey = nameof(RouteStartPointId), OtherKey = nameof(Entities.Point.Id), CanBeNull = false)]
    public Point RouteStartPoint { get; set; } = null!;

    [Association(ThisKey = nameof(Id), OtherKey = nameof(CarCapacity.CarId))]
    public IEnumerable<CarCapacity> CarCapacities { get; set; } = null!;
}
