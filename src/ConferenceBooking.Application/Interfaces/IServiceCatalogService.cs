using ConferenceBooking.Application.DTOs.Services;

namespace ConferenceBooking.Application.Interfaces;

public interface IServiceCatalogService
{
    Task<ServiceResponse> CreateAsync(ServiceRequest request);
    Task<ServiceResponse?> GetByIdAsync(int id);
    Task<List<ServiceResponse>> GetAllAsync();
    Task<ServiceResponse> UpdateAsync(int id, ServiceRequest request);
    Task DeleteAsync(int id);
}