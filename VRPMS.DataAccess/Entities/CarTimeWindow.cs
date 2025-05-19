using LinqToDB;
using LinqToDB.Mapping;

namespace VRPMS.DataAccess.Entities;

[Table("car_time_windows", Schema = "vrpms")]
internal class CarTimeWindow
{
    [PrimaryKey, Identity]
    [Column("id", DataType = DataType.Int32, IsPrimaryKey = true, PrimaryKeyOrder = 0), NotNull]
    public int Id { get; set; }

    [Column("car_id", DataType = DataType.Int32), NotNull]
    public int CarId { get; set; }

    [Column("window_start", DataType = DataType.Time, SkipOnInsert = true), NotNull]
    public TimeSpan WindowStart { get; set; }

    [Column("window_end", DataType = DataType.Time, SkipOnInsert = true), NotNull]
    public TimeSpan WindowEnd { get; set; }

    [Association(ThisKey = nameof(CarId), OtherKey = nameof(Entities.Car.Id), CanBeNull = false)]
    public Car Car { get; set; } = null!;
}
