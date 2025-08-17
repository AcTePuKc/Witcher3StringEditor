using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.SqliteVec;
using Witcher3StringEditor.Interfaces;
using Witcher3StringEditor.Models;

namespace Witcher3StringEditor.Services;

internal class KnowledgeService : IKnowledgeService, IDisposable
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

    public async Task Learn(string text)
    {
        await knowledge.EnsureCollectionExistsAsync();
        var knowledgeItem = new W3KItem
        {
            Text = text,
            Embedding = await generator.GenerateVectorAsync(text)
        };
        await knowledge.UpsertAsync(knowledgeItem);
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

    public async Task Delete(IEnumerable<string> items)
    {
        await knowledge.DeleteAsync(items);
    }

    public async Task Clear()
    {
        await knowledge.EnsureCollectionDeletedAsync();
    }

    private static string Unprotect(string encryptedKey)
    {
        var encryptedData = Convert.FromBase64String(encryptedKey);
        var data = ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(data);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                knowledge.Dispose();
                vectorStore.Dispose();
                httpClient.Dispose();
            }

            disposedValue = true;
        }
    }
}