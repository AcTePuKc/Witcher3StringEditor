using Microsoft.Data.Sqlite;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.SqliteVec;
using System.Net.Http;
using Witcher3StringEditor.Interfaces;
using Witcher3StringEditor.Models;

namespace Witcher3StringEditor.Services;

internal class KnowledgeService : IKnowledgeService,IDisposable
{
    private bool disposedValue;
    private readonly HttpClient httpClient;
    private readonly IEmbeddingGenerator<string, Embedding<float>> generator;
    private readonly VectorStoreCollection<long, IW3KItem> knowledge;

    public KnowledgeService(IEmbeddedModelSettings modelSettings)
    {
        httpClient = new HttpClient
        {
            BaseAddress = new Uri(modelSettings.EndPoint)
        };
#pragma warning disable SKEXP0010 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
        var kernel = Kernel.CreateBuilder().AddOpenAIEmbeddingGenerator(modelSettings.ModelId, modelSettings.ApiKey, httpClient: httpClient).Build();
#pragma warning restore SKEXP0010 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
        generator = kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
        var vectorStore = new SqliteVectorStore(new SqliteConnectionStringBuilder
        {
            DataSource = "knowledge.db",
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString(), new SqliteVectorStoreOptions { EmbeddingGenerator = generator });
        knowledge = vectorStore.GetCollection<long, IW3KItem>("knowledge", new VectorStoreCollectionDefinition
        {
            EmbeddingGenerator = generator,
            Properties =
            [
                new VectorStoreKeyProperty("Id", typeof(long)),
                new VectorStoreDataProperty("Text", typeof(string)),
                new VectorStoreVectorProperty("Embedding", typeof(float), modelSettings.Dimensions)
            ]
        });
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

    public async Task Delete(IEnumerable<long> items)
    {
        await knowledge.DeleteAsync(items);
    }

    public async Task Clear()
    {
        await knowledge.EnsureCollectionDeletedAsync();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                httpClient.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}