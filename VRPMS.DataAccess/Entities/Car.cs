using LinqToDB;
using LinqToDB.Mapping;

namespace VRPMS.DataAccess.Entities;

[Table("cars", Schema = "vrpms")]
internal class Car
{
    [PrimaryKey, Identity]
    [Column("id", DataType = DataType.Int32, IsPrimaryKey = true, PrimaryKeyOrder = 0), NotNull]
    public int Id { get; set; }

    [Column("car_type_id", DataType = DataType.Int32), NotNull]
    public int CarTypeId { get; set; }

    [Column("capacity_overload_penalty", DataType = DataType.Int32), NotNull]
    public int CapacityOverloadPenalty { get; set; }

    [Column("max_capacity_overload_penalty", DataType = DataType.Int32), NotNull]
    public int MaxCapacityOverloadPenalty { get; set; }

    [Column("overwork_penalty", DataType = DataType.Int32), NotNull]
    public int OverWorkPenalty { get; set; }

    [Column("route_template"), NotNull]
    public int?[] RouteTemplate { get; set; } = [];

    [Association(ThisKey = nameof(CarTypeId), OtherKey = nameof(Entities.CarType.Id))]
    public CarType CarType { get; set; } = null!;

    [Association(ThisKey = nameof(Id), OtherKey = nameof(CarCapacity.CarId))]
    public IEnumerable<CarCapacity> CarCapacities { get; set; } = null!;

    [Association(ThisKey = nameof(Id), OtherKey = nameof(CarTimeWindow.CarId))]
    public IEnumerable<CarTimeWindow> CarTimeWindows { get; set; } = null!;
}
