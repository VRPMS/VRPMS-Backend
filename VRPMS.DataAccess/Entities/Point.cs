using LinqToDB;
using LinqToDB.Mapping;

namespace VRPMS.DataAccess.Entities;

[Table("points", Schema = "vrpms")]
internal class Point
{
    [PrimaryKey, Identity]
    [Column("id", DataType = DataType.Int32, IsPrimaryKey = true, PrimaryKeyOrder = 0), NotNull]
    public int Id { get; set; }

    [Column("point_type_id"), NotNull]
    public int PointTypeId { get; set; }

    [Column("longitude", DataType = DataType.Double), NotNull]
    public double Longitude { get; set; }

    [Column("latitude", DataType = DataType.Double), NotNull]
    public double Latitude { get; set; }

    [Column("service_time", DataType = DataType.Interval), NotNull]
    public TimeSpan ServiceTime { get; set; }

    [Column("late_penalty", DataType = DataType.Int32), NotNull]
    public int LatePenalty { get; set; }

    [Column("wait_penalty", DataType = DataType.Int32), NotNull]
    public int WaitPenalty { get; set; }

    [Association(ThisKey = nameof(PointTypeId), OtherKey = nameof(PointType.Id), CanBeNull = false)]
    public PointType PointType { get; set; } = null!;

    [Association(ThisKey = nameof(Id), OtherKey = nameof(PointDemand.PointId))]
    public IEnumerable<PointDemand> PointDemands { get; set; } = null!;

    [Association(ThisKey = nameof(Id), OtherKey = nameof(PointTimeWindow.PointId))]
    public IEnumerable<PointTimeWindow> PointTimeWindows { get; set; } = null!;
}
