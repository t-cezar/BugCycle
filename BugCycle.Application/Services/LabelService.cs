using BugCycle.Application.DTOs.Labels;
using BugCycle.Application.Interfaces;
using BugCycle.Application.Interfaces.Repositories;
using BugCycle.Application.Interfaces.Services;
using BugCycle.Application.Mapping;
using BugCycle.Domain.Entities;
using BugCycle.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace BugCycle.Application.Services;

public class LabelService : ILabelService
{
    private readonly ILabelRepository _labels;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LabelService> _logger;

    public LabelService(
        ILabelRepository labels,
        IUnitOfWork unitOfWork,
        ILogger<LabelService> logger)
    {
        _labels = labels;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<LabelDto>> ListAsync(
        bool includeUsageCounts = false,
        CancellationToken ct = default)
    {
        var labels = await _labels.ListAllAsync(ct);

        if (!includeUsageCounts)
        {
            return labels.ToDtos();
        }

        var usages = await _labels.CountUsagesAsync(ct);
        return labels.ToDtos(usages);
    }

    public async Task<LabelDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var label = await _labels.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Eticheta", id);

        return label.ToDto();
    }

    public async Task<LabelDto> CreateAsync(SaveLabelDto dto, CancellationToken ct = default)
    {
        var name = dto.Name.Trim();

        if (await _labels.NameExistsAsync(name, null, ct))
        {
            throw new DomainException($"Exista deja o eticheta numita '{name}'.");
        }

        var label = new Label
        {
            Name = name,
            ColorHex = dto.ColorHex.Trim().ToLowerInvariant()
        };

        await _labels.AddAsync(label, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Eticheta '{LabelName}' (id {LabelId}) a fost creata.", label.Name, label.Id);

        return label.ToDto();
    }

    public async Task<LabelDto> UpdateAsync(int id, SaveLabelDto dto, CancellationToken ct = default)
    {
        var label = await _labels.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Eticheta", id);

        var name = dto.Name.Trim();

        if (await _labels.NameExistsAsync(name, id, ct))
        {
            throw new DomainException($"Exista deja o eticheta numita '{name}'.");
        }

        label.Name = name;
        label.ColorHex = dto.ColorHex.Trim().ToLowerInvariant();

        _labels.Update(label);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Eticheta {LabelId} a fost actualizata.", id);

        return label.ToDto();
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var label = await _labels.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Eticheta", id);

        _labels.Remove(label);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogWarning(
            "Eticheta '{LabelName}' (id {LabelId}) a fost stearsa de pe toate issue-urile.",
            label.Name, id);
    }
}
