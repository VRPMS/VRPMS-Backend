using LinqToDB;
using LinqToDB.Mapping;
using VRPMS.DataAccess.Constants;

namespace VRPMS.DataAccess.Entities;

[Table("point_time_windows", Schema = DbConstants.Schema)]
internal class PointTimeWindow
{
    [PrimaryKey, Identity]
    [Column("id", DataType = DataType.Int32, IsPrimaryKey = true, PrimaryKeyOrder = 0), NotNull]
    public int Id { get; set; }

    [Column("point_id", DataType = DataType.Int32), NotNull]
    public int PointId { get; set; }

    [Column("window_start", DataType = DataType.Time, SkipOnInsert = true), NotNull]
    public TimeSpan WindowStart { get; set; }
    
    [Column("window_end", DataType = DataType.Time, SkipOnInsert = true), NotNull]
    public TimeSpan WindowEnd { get; set; }

    [Association(ThisKey = nameof(PointId), OtherKey = nameof(Entities.Point.Id), CanBeNull = false)]
    public Point Point { get; set; } = null!;
}
