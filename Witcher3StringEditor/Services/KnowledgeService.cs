using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using MessagePack;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.SqliteVec;
using Serilog;
using Witcher3StringEditor.Interfaces;
using Witcher3StringEditor.Models;

namespace Witcher3StringEditor.Services;

internal sealed class KnowledgeService : IKnowledgeService, IDisposable
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> generator;
    private readonly HttpClient httpClient;
    private readonly VectorStoreCollection<string, W3KItem> knowledge;
    private readonly VectorStore vectorStore;
    private bool disposedValue;

    public KnowledgeService(IEmbeddedModelSettings modelSettings)
    {
        httpClient = new HttpClient
        {
            BaseAddress = new Uri(modelSettings.EndPoint)
        };
#pragma warning disable SKEXP0010 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
        var kernel = Kernel.CreateBuilder()
            .AddOpenAIEmbeddingGenerator(modelSettings.ModelId, Unprotect(modelSettings.ApiKey), httpClient: httpClient)
            .Build();
#pragma warning restore SKEXP0010 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
        generator = kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
        vectorStore = new SqliteVectorStore(new SqliteConnectionStringBuilder
        {
            DataSource = "knowledge.db",
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString(), new SqliteVectorStoreOptions { EmbeddingGenerator = generator });
        knowledge = vectorStore.GetCollection<string, W3KItem>("knowledge", new VectorStoreCollectionDefinition
        {
            EmbeddingGenerator = generator,
            Properties =
            [
                new VectorStoreKeyProperty("Id", typeof(string)),
                new VectorStoreDataProperty("Text", typeof(string)),
                new VectorStoreVectorProperty("Embedding", typeof(ReadOnlyMemory<float>), modelSettings.Dimensions)
            ]
        });
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async Task<IW3KItem> Learn(string text)
    {
        await knowledge.EnsureCollectionExistsAsync();
        var knowledgeItem = new W3KItem
        {
            Text = text,
            Embedding = await generator.GenerateVectorAsync(text)
        };
        await knowledge.UpsertAsync(knowledgeItem);
        return knowledgeItem;
    }

    public async IAsyncEnumerable<IW3KItem>? Search(string text, int count)
    {
        if (await knowledge.CollectionExistsAsync()) await knowledge.EnsureCollectionExistsAsync();
        await foreach (var item in knowledge.SearchAsync(text, count)) yield return item.Record;
    }

    public async IAsyncEnumerable<IW3KItem>? All()
    {
        if (await knowledge.CollectionExistsAsync()) await knowledge.EnsureCollectionExistsAsync();
        await foreach (var item in knowledge.GetAsync(_ => true, int.MaxValue)) yield return item;
    }

    public async Task<bool> Delete(IEnumerable<string> items)
    {
        try
        {
            await knowledge.DeleteAsync(items);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to delete knowledge items.");
            return false;
        }
    }

    public async Task Clear()
    {
        await knowledge.EnsureCollectionDeletedAsync();
    }

    public async Task Import(string path)
    {
        await using var stream = File.OpenRead(path);
        (await MessagePackSerializer.DeserializeAsync<List<W3KItem>>(stream)).ForEach(async void (item) =>
        {
            try
            {
                await knowledge.UpsertAsync(item);
            }
            catch (Exception e)
            {
                Log.Information(e, "Failed to import knowledge item.");
            }
        });
    }

    public async Task Export(string path,IEnumerable<IW3KItem> backup)
    {
        await using var fs = File.Create(path);
        await MessagePackSerializer.SerializeAsync(typeof(IW3KItem), fs, backup);
    }

    ~KnowledgeService()
    {
        Dispose(false);
    }

    private static string Unprotect(string encryptedKey)
    {
        var encryptedData = Convert.FromBase64String(encryptedKey);
        var data = ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(data);
    }

    private void Dispose(bool disposing)
    {
        if (disposedValue) return;
        if (disposing)
        {
            knowledge.Dispose();
            vectorStore.Dispose();
            httpClient.Dispose();
        }

        disposedValue = true;
    }
}