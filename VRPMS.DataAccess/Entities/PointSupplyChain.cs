using LinqToDB;
using LinqToDB.Mapping;

namespace VRPMS.DataAccess.Entities;

[Table("point_supply_chain", Schema = "vrpms")]
internal class PointSupplyChain
{
    [PrimaryKey, Identity]
    [Column("id", DataType = DataType.Int32, IsPrimaryKey = true, PrimaryKeyOrder = 0), NotNull]
    public int Id { get; set; }

    [Column("client_id", DataType = DataType.Int32), NotNull]
    public int ClientId { get; set; }

    [Column("warehouse_id", DataType = DataType.Int32)]
    public int? WarehouseId { get; set; }

    [Column("cross_dock_id", DataType = DataType.Int32)]
    public int? CrossDockId { get; set; }

    [Association(ThisKey = nameof(ClientId), OtherKey = nameof(Entities.Point.Id), CanBeNull = false)]
    public Point Client { get; set; } = null!;

    [Association(ThisKey = nameof(WarehouseId), OtherKey = nameof(Entities.Point.Id), CanBeNull = true)]
    public Point? Warehouse { get; set; }

    [Association(ThisKey = nameof(CrossDockId), OtherKey = nameof(Entities.Point.Id), CanBeNull = true)]
    public Point? CrossDock { get; set; }
}
