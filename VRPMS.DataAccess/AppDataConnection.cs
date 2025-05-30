using LinqToDB;
using LinqToDB.Data;
using VRPMS.DataAccess.Entities;

namespace VRPMS.DataAccess;

public class AppDataConnection(DataOptions<AppDataConnection> options) : DataConnection(options.Options)
{
    internal ITable<PointType> PointTypes => this.GetTable<PointType>();

    internal ITable<Demand> Demands => this.GetTable<Demand>();

    internal ITable<Point> Points => this.GetTable<Point>();

    internal ITable<PointTimeWindow> PointTimeWindows => this.GetTable<PointTimeWindow>();

    internal ITable<PointDemand> PointDemands => this.GetTable<PointDemand>();

    internal ITable<PointRoute> PointRoutes => this.GetTable<PointRoute>();

    internal ITable<PointSupplyChain> PointSupplyChains => this.GetTable<PointSupplyChain>();

    internal ITable<Car> Cars => this.GetTable<Car>();

    internal ITable<CarCapacity> CarCapacities => this.GetTable<CarCapacity>();

    internal ITable<Solution> Solutions => this.GetTable<Solution>();

    internal ITable<SolutionRoute> SolutionRoutes => this.GetTable<SolutionRoute>();

    internal ITable<SolutionRouteVisit> SolutionRouteVisits => this.GetTable<SolutionRouteVisit>();
}
