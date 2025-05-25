using VRPMS.DataContracts.Responses;

namespace VRPMS.BusinessLogic.Interfaces.Services;

public interface ILovsService
{
    Task<IEnumerable<BaseTypeResponse>> GetLocationTypesLov();
   
    Task<IEnumerable<BaseTypeResponse>> GetDemandTypesLov();
}
