using LinqToDB;
using LinqToDB.Mapping;
using VRPMS.DataAccess.Constants;

namespace VRPMS.DataAccess.Entities;

[Table("point_demands", Schema = DbConstants.Schema)]
internal class PointDemand
{
    [PrimaryKey, Identity]
    [Column("id", DataType = DataType.Int32, IsPrimaryKey = true, PrimaryKeyOrder = 0), NotNull]
    public int Id { get; set; }

    [Column("point_id", DataType = DataType.Int32), NotNull]
    public int PointId { get; set; }

    [Column("demand_id", DataType = DataType.Int32), NotNull]
    public int DemandId { get; set; }

    [Column("demand", DataType = DataType.Double), NotNull]
    public double DemandValue { get; set; }

    [Association(ThisKey = nameof(PointId), OtherKey = nameof(Entities.Point.Id), CanBeNull = false)]
    public Point Point { get; set; } = null!;

    [Association(ThisKey = nameof(DemandId), OtherKey = nameof(Entities.Demand.Id), CanBeNull = false)]
    public Demand Demand { get; set; } = null!;
}
