using LinqToDB;
using LinqToDB.Mapping;
using VRPMS.DataAccess.Constants;

namespace VRPMS.DataAccess.Entities;

[Table("point_routes", Schema = DbConstants.Schema)]
internal class PointRoute
{
    [PrimaryKey, Identity]
    [Column("id", DataType = DataType.Int32, IsPrimaryKey = true, PrimaryKeyOrder = 0), NotNull]
    public int Id { get; set; }

    [Column("from_point_id", DataType = DataType.Int32), NotNull]
    public int FromPointId { get; set; }

    [Column("to_point_id", DataType = DataType.Int32), NotNull]
    public int ToPointId { get; set; }

    [Column("duration", DataType = DataType.Interval), NotNull]
    public TimeSpan Duration { get; set; }

    [Column("distance", DataType = DataType.Double), NotNull]
    public double Distance { get; set; }

    [Association(ThisKey = nameof(FromPointId), OtherKey = nameof(Entities.Point.Id), CanBeNull = false)]
    public Point FromPoint { get; set; } = null!;

    [Association(ThisKey = nameof(ToPointId), OtherKey = nameof(Entities.Point.Id), CanBeNull = false)]
    public Point ToPoint { get; set; } = null!;
}
