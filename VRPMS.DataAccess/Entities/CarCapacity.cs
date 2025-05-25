using LinqToDB;
using LinqToDB.Mapping;
using VRPMS.DataAccess.Constants;

namespace VRPMS.DataAccess.Entities;

[Table("car_capacities", Schema = DbConstants.Schema)]
internal class CarCapacity
{
    [PrimaryKey, Identity]
    [Column("id", DataType = DataType.Int32, IsPrimaryKey = true, PrimaryKeyOrder = 0), NotNull]
    public int Id { get; set; }

    [Column("car_id", DataType = DataType.Int32), NotNull]
    public int CarId { get; set; }

    [Column("demand_id", DataType = DataType.Int32), NotNull]
    public int DemandId { get; set; }

    [Column("capacity", DataType = DataType.Double), NotNull]
    public double Capacity { get; set; }

    [Column("max_capacity", DataType = DataType.Double), NotNull]
    public double MaxCapacity { get; set; }

    [Association(ThisKey = nameof(CarId), OtherKey = nameof(Entities.Car.Id), CanBeNull = false)]
    public Car Car { get; set; } = null!;

    [Association(ThisKey = nameof(DemandId), OtherKey = nameof(Entities.Demand.Id), CanBeNull = false)]
    public Demand Demand { get; set; } = null!;
}
