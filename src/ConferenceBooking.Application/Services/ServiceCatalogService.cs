using ConferenceBooking.Application.DTOs.Services;
using ConferenceBooking.Application.Interfaces;
using ConferenceBooking.Domain.Entities;

namespace ConferenceBooking.Application.Services;

public class ServiceCatalogService : IServiceCatalogService
{
    private readonly IServiceRepository _serviceRepository;

    public ServiceCatalogService(IServiceRepository serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public async Task<ServiceResponse> CreateAsync(ServiceRequest request)
    {
        var service = new Service
        {
            Name = request.Name,
            Price = request.Price,
            TotalQuantity = request.TotalQuantity
        };

        await _serviceRepository.AddAsync(service);

        return ToResponse(service);
    }

    public async Task<ServiceResponse?> GetByIdAsync(int id)
    {
        var service = await _serviceRepository.GetByIdAsync(id);
        return service is null ? null : ToResponse(service);
    }

    public async Task<List<ServiceResponse>> GetAllAsync()
    {
        var services = await _serviceRepository.GetAllAsync();
        return services.Select(ToResponse).ToList();
    }

    public async Task<ServiceResponse> UpdateAsync(int id, ServiceRequest request)
    {
        var service = await _serviceRepository.GetByIdAsync(id);
        if (service is null)
            throw new KeyNotFoundException("Послугу не знайдено.");

        service.Name = request.Name;
        service.Price = request.Price;
        service.TotalQuantity = request.TotalQuantity;

        await _serviceRepository.UpdateAsync(service);

        return ToResponse(service);
    }

    public async Task DeleteAsync(int id)
    {
        var service = await _serviceRepository.GetByIdAsync(id);
        if (service is null)
            throw new KeyNotFoundException("Послугу не знайдено.");

        await _serviceRepository.DeleteAsync(id);
    }

    private static ServiceResponse ToResponse(Service service) =>
        new(service.Id, service.Name, service.Price, service.TotalQuantity);
}