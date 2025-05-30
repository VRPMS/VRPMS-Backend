using LinqToDB;
using LinqToDB.Mapping;
using VRPMS.DataAccess.Constants;

namespace VRPMS.DataAccess.Entities;

[Table("solution_routes", Schema = DbConstants.Schema)]
internal class SolutionRoute
{
    [PrimaryKey, Identity]
    [Column("id", DataType = DataType.Int32, IsPrimaryKey = true, PrimaryKeyOrder = 0), NotNull]
    public int Id { get; set; }

    [Column("solution_id", DataType = DataType.Int32), NotNull]
    public int SolutionId { get; set; }

    [Column("car_id", DataType = DataType.Int32), NotNull]
    public int CarId { get; set; }

    [Association(ThisKey = nameof(SolutionId), OtherKey = nameof(Solution.Id), CanBeNull = false)]
    public Solution Solution { get; set; } = null!;

    [Association(ThisKey = nameof(CarId), OtherKey = nameof(Entities.Car.Id), CanBeNull = false)]
    public Car Car { get; set; } = null!;

    [Association(ThisKey = nameof(Id), OtherKey = nameof(SolutionRouteVisit.SolutionRouteId))]
    public IEnumerable<SolutionRouteVisit> SolutionRouteVisits { get; set; } = null!;
}
