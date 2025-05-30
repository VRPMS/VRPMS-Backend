using LinqToDB;
using LinqToDB.Mapping;
using VRPMS.DataAccess.Constants;

namespace VRPMS.DataAccess.Entities;

[Table("solution_route_visits", Schema = DbConstants.Schema)]
internal class SolutionRouteVisit
{
    [PrimaryKey, Identity]
    [Column("id", DataType = DataType.Int32, IsPrimaryKey = true, PrimaryKeyOrder = 0), NotNull]
    public int Id { get; set; }

    [Column("solution_route_id", DataType = DataType.Int32), NotNull]
    public int SolutionRouteId { get; set; }

    [Column("sequence_number", DataType = DataType.Int32), NotNull]
    public int SequenceNumber { get; set; }

    [Column("point_id", DataType = DataType.Int32), NotNull]
    public int PointId { get; set; }

    [Column("arrival_time", DataType = DataType.Interval), NotNull]
    public TimeSpan ArrivalTime { get; set; }

    [Column("departure_time", DataType = DataType.Interval)]
    public TimeSpan? DepartureTime { get; set; }

    [Association(ThisKey = nameof(SolutionRouteId), OtherKey = nameof(Entities.SolutionRoute.Id), CanBeNull = false)]
    public SolutionRoute SolutionRoute { get; set; } = null!;

    [Association(ThisKey = nameof(PointId), OtherKey = nameof(Entities.Point.Id), CanBeNull = false)]
    public Point Point { get; set; } = null!;
}
