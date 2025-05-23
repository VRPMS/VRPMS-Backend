namespace VRPMS.BusinessLogic.Interfaces.Services;

public interface IDataService
{
    Task ImportData(Stream fileStream);
}
