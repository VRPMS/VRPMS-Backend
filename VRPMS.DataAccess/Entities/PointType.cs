using LinqToDB;
using LinqToDB.Mapping;

namespace VRPMS.DataAccess.Entities;

[Table("point_types", Schema = "vrpms")]
internal class PointType
{
    [PrimaryKey, Identity]
    [Column("id", DataType = DataType.Int32, IsPrimaryKey = true, PrimaryKeyOrder = 0), NotNull]
    public int Id { get; set; }

    [Column("name", DataType = DataType.VarChar), NotNull]
    public required string Name { get; set; } = null!;

    [Association(ThisKey = nameof(Id), OtherKey = nameof(Point.PointTypeId))]
    public IEnumerable<Point> Points { get; set; } = null!;
}
