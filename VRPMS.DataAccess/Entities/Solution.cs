using LinqToDB;
using LinqToDB.Mapping;
using VRPMS.DataAccess.Constants;

namespace VRPMS.DataAccess.Entities;

[Table("solutions", Schema = DbConstants.Schema)]
internal class Solution
{
    [PrimaryKey, Identity]
    [Column("id", DataType = DataType.Int32, IsPrimaryKey = true, PrimaryKeyOrder = 0), NotNull]
    public int Id { get; set; }

    [Column("created_at", DataType = DataType.Timestamp, SkipOnInsert = true), NotNull]
    public DateTime CreatedAt { get; set; }

    [Column("total_score", DataType = DataType.Decimal), NotNull]
    public decimal TotalScore { get; set; }

    [Association(ThisKey = nameof(Id), OtherKey = nameof(SolutionRoute.SolutionId))]
    public IEnumerable<SolutionRoute> SolutionRoutes { get; set; } = null!;
}
