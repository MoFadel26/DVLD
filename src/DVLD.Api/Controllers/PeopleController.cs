using DVLD.Application.DTOs;
using DVLD.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace DVLD.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PeopleController : ControllerBase
{
    private readonly IPersonService _personService;
    private readonly ILicenseService _licenseService;

    public PeopleController(IPersonService personService, ILicenseService licenseService)
    {
        _personService = personService;
        _licenseService = licenseService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PersonResponseDto>>> GetAll(CancellationToken cancellationToken)
    {
        var people = await _personService.GetAllAsync(cancellationToken);
        return Ok(people);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PersonResponseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var person = await _personService.GetByIdAsync(id, cancellationToken);
        return Ok(person);
    }

    [HttpGet("{id:int}/licenses")]
    public async Task<ActionResult<IReadOnlyList<LicenseResponseDto>>> GetLicenses(int id, CancellationToken cancellationToken)
    {
        var licenses = await _licenseService.GetLicensesByPersonIdAsync(id, cancellationToken);
        return Ok(licenses);
    }

    [HttpGet("by-national-no/{nationalNo}")]
    public async Task<ActionResult<PersonResponseDto>> GetByNationalNo(string nationalNo, CancellationToken cancellationToken)
    {
        var person = await _personService.GetByNationalNoAsync(nationalNo, cancellationToken);
        return Ok(person);
    }

    [HttpPost]
    public async Task<ActionResult<PersonResponseDto>> Create([FromBody] CreatePersonDto dto, CancellationToken cancellationToken)
    {
        var created = await _personService.CreatePersonAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.PersonId }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PersonResponseDto>> Update(int id, [FromBody] UpdatePersonDto dto, CancellationToken cancellationToken)
    {
        var updated = await _personService.UpdatePersonAsync(id, dto, cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _personService.DeletePersonAsync(id, cancellationToken);
        return NoContent();
    }
}
