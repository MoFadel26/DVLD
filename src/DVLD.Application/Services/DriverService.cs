using DVLD.Application.Common.Interfaces;
using DVLD.Application.DTOs;
using DVLD.Domain.Entities;
using DVLD.Domain.Exceptions;

namespace DVLD.Application.Services;

public class DriverService : IDriverService
{
    private readonly IUnitOfWork _unitOfWork;

    public DriverService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<DriverDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var drivers = await _unitOfWork.Drivers.GetAllWithDetailsAsync(cancellationToken);
        return drivers.Select(ToDto).ToList();
    }

    public async Task<DriverDto> GetByIdAsync(int driverId, CancellationToken cancellationToken = default)
    {
        var driver = await _unitOfWork.Drivers.GetDetailsByIdAsync(driverId, cancellationToken)
            ?? throw new EntityNotFoundException("Driver", driverId);
        return ToDto(driver);
    }

    private static DriverDto ToDto(Driver driver)
    {
        return new DriverDto(
            driver.DriverId,
            driver.PersonId,
            driver.Person.FullName,
            driver.Person.NationalNo,
            driver.CreatedDate,
            driver.Licenses.Count,
            driver.Licenses.Count(l => l.IsActive)
        );
    }
}
