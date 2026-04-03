using DeliveryAggregator.DTOs;
using DeliveryAggregator.Entities;
using DeliveryAggregator.Repositories.Interfaces;
using DeliveryAggregator.Services.Interfaces;

namespace DeliveryAggregator.Services;

public class ApplicationService : IApplicationService
{
    private readonly IApplicationRepository _applications;

    public ApplicationService(IApplicationRepository applications)
    {
        _applications = applications;
    }

    public async Task<ApplicationResponse> CreateAsync(CreateApplicationRequest request)
    {
        var application = new RegistrationApplication
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            DisplayName = request.DisplayName,
            Role = request.Role
        };

        await _applications.AddAsync(application);
        return Map(application);
    }

    public async Task<ApplicationResponse?> GetByIdAsync(Guid id)
    {
        var app = await _applications.GetByIdAsync(id);
        return app == null ? null : Map(app);
    }

    private static ApplicationResponse Map(RegistrationApplication a) => new(
        a.Id, a.Email, a.DisplayName,
        a.Role.ToString(), a.Status.ToString(),
        a.ModeratorNote, a.CreatedAt, a.ReviewedAt
    );
}
