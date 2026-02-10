using Witcher3StringEditor.Common.Settings;

namespace Witcher3StringEditor.Services;

/// <summary>
///     No-op placeholder for explicit settings loading orchestration.
///     TODO: Replace with a composed implementation once settings loading moves out of view models.
/// </summary>
public class NoopSettingsDeferredLoadService : ISettingsDeferredLoadService
{
    public Task<IReadOnlyList<string>> LoadCachedModelsAsync()
        => Task.FromResult<IReadOnlyList<string>>([]);

    public Task LoadProfilesAsync()
        => Task.CompletedTask;

    public Task LoadTerminologyAsync()
        => Task.CompletedTask;

    public Task LoadStyleGuideAsync()
        => Task.CompletedTask;
}
