using System;
using System.Threading;
using System.Threading.Tasks;
using Witcher3StringEditor.Common.Translation;
using Witcher3StringEditor.Common.TranslationMemory;

namespace Witcher3StringEditor.Services;

internal sealed class TranslationMemoryService : ITranslationMemoryService
{
    private readonly ITranslationMemorySettingsProvider settingsProvider;
    private readonly ITranslationMemoryDatabaseInitializer databaseInitializer;
    private readonly ITranslationMemoryStoreFactory storeFactory;
    private readonly SemaphoreSlim initializationLock = new(1, 1);

    private bool isInitialized;
    private ITranslationMemoryStore? store;

    public TranslationMemoryService(
        ITranslationMemorySettingsProvider settingsProvider,
        ITranslationMemoryDatabaseInitializer databaseInitializer,
        ITranslationMemoryStoreFactory storeFactory)
    {
        this.settingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));
        this.databaseInitializer = databaseInitializer ?? throw new ArgumentNullException(nameof(databaseInitializer));
        this.storeFactory = storeFactory ?? throw new ArgumentNullException(nameof(storeFactory));
    }

    public async Task<TranslationMemoryEntry?> LookupAsync(
        TranslationMemoryQuery query,
        TranslationPipelineContext? context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (!IsEnabled(context))
        {
            return null;
        }

        var activeStore = await GetOrInitializeStoreAsync(cancellationToken).ConfigureAwait(false);
        if (activeStore is null)
        {
            return null;
        }

        return await activeStore.FindAsync(query, cancellationToken).ConfigureAwait(false);
    }

    public async Task SaveAsync(
        TranslationMemoryEntry entry,
        TranslationPipelineContext? context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (!IsEnabled(context))
        {
            return;
        }

        var activeStore = await GetOrInitializeStoreAsync(cancellationToken).ConfigureAwait(false);
        if (activeStore is null)
        {
            return;
        }

        await activeStore.SaveAsync(entry, cancellationToken).ConfigureAwait(false);
    }

    private async Task<ITranslationMemoryStore?> GetOrInitializeStoreAsync(CancellationToken cancellationToken)
    {
        if (isInitialized)
        {
            return store;
        }

        await initializationLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (isInitialized)
            {
                return store;
            }

            var settings = settingsProvider.GetSettings();
            if (!settings.Enabled)
            {
                isInitialized = true;
                return null;
            }

            await databaseInitializer.InitializeAsync(settings, cancellationToken).ConfigureAwait(false);
            var createdStore = storeFactory.Create(settings);
            await createdStore.InitializeAsync(cancellationToken).ConfigureAwait(false);

            store = createdStore;
            isInitialized = true;
            return store;
        }
        finally
        {
            initializationLock.Release();
        }
    }

    private static bool IsEnabled(TranslationPipelineContext? context)
    {
        return context?.UseTranslationMemory == true;
    }
}
