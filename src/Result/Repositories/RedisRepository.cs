using StackExchange.Redis;

namespace AyBorg.Result;

public sealed class RedisRepository : IRepository
{
    private const string WORKFLOWRESULT_INDEX = "workflowResult_index";
    private const string PORTRESULT_INDEX = "portResult_index";
    private const string IMAGERESULT_INDEX = "imageResult_index";
    private readonly IDatabase _database;
    private readonly int _maxCacheWorkflowResults;

    public RedisRepository(IDatabase database, IConfiguration configuration)
    {
        _database = database;
        _maxCacheWorkflowResults = configuration.GetValue("AyBorg:Cache:MaxWorkflowResults", 1000);
    }

    public async ValueTask AddAsync(WorkflowResult result)
    {
        string workflowResultKey = $"workflowResult:{result.IterationId}";

        long workflowResultTimestamp = ((DateTimeOffset)result.StopTime).ToUnixTimeMilliseconds();

        var workFlowResultHash = new HashEntry[]
        {
            new HashEntry("Service", result.ServiceUniqueName),
            new HashEntry("startTime", result.StartTime.ToString()),
            new HashEntry("stopTime", result.StopTime.ToString()),
            new HashEntry("elapsedMs", result.ElapsedMs),
            new HashEntry("success", result.Success)
        };

        ITransaction transaction = _database.CreateTransaction();
        _ = transaction.SortedSetAddAsync(WORKFLOWRESULT_INDEX, new RedisValue(workflowResultKey), workflowResultTimestamp);
        _ = transaction.HashSetAsync(workflowResultKey, workFlowResultHash);

        foreach (SDK.Common.Models.Port port in result.Ports)
        {
            string portResultKey = $"portResult:{result.IterationId}:{port.Name}";
            var portResultHash = new HashEntry[]
            {
                new HashEntry("ID", port.Id.ToString()),
                new HashEntry("Name", port.Name),
                new HashEntry("Value", (string)port.Value!),
                new HashEntry("Brand", (int)port.Brand),
                new HashEntry("Service", result.ServiceUniqueName)
            };
            _ = transaction.SortedSetAddAsync(PORTRESULT_INDEX, new RedisValue(portResultKey), workflowResultTimestamp);
            _ = transaction.HashSetAsync(portResultKey, portResultHash);
        }

        await transaction.ExecuteAsync();
        await CleanByCount();
    }

    public async ValueTask AddImageAsync(ImageResult result)
    {
        string imageResultKey = $"imageResult:{result.IterationId}:{result.PortId}";
        long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        ITransaction transaction = _database.CreateTransaction();
        _ = transaction.SortedSetAddAsync(IMAGERESULT_INDEX, new RedisValue(imageResultKey), currentTimestamp);

        var imageResultHash = new HashEntry[]
        {
            new HashEntry("Data", result.Data.Memory.ToArray()),
            new HashEntry("Width", result.Width),
            new HashEntry("Height", result.Height),
            new HashEntry("ScaledWidth", result.ScaledWidth),
            new HashEntry("ScaledHeight", result.ScaledHeight)
        };

        _ = transaction.HashSetAsync(imageResultKey, imageResultHash);
        await transaction.ExecuteAsync();
    }

    private async Task CleanByCount()
    {
        long wfEntryCount = await _database.SortedSetLengthAsync(WORKFLOWRESULT_INDEX);
        if (wfEntryCount < _maxCacheWorkflowResults)
        {
            return;
        }

        RedisValue[] workflowEntries = await _database.SortedSetRangeByRankAsync(WORKFLOWRESULT_INDEX, 0, wfEntryCount - _maxCacheWorkflowResults);
        ITransaction transaction = _database.CreateTransaction();

        var tmpImageKeys = new List<string>();
        var tmpPortKeys = new List<string>();
        foreach (RedisValue wfEntry in workflowEntries)
        {
            string wfKey = wfEntry.ToString();
            wfKey = wfKey.Replace("workflowResult:", string.Empty);
            tmpImageKeys.Add($"imageResult:{wfKey}");
            tmpPortKeys.Add($"portResult:{wfKey}");

            _ = transaction.SortedSetRemoveAsync(WORKFLOWRESULT_INDEX, wfEntry);
            _ = transaction.KeyDeleteAsync(wfKey);
        }

        await PrepareDeleteEntries(PORTRESULT_INDEX, transaction, tmpPortKeys);
        await PrepareDeleteEntries(IMAGERESULT_INDEX, transaction, tmpImageKeys);
        await transaction.ExecuteAsync();
    }

    private async Task PrepareDeleteEntries(string indexKey, ITransaction transaction, List<string> tmpKeys)
    {
        var rmKeys = new List<RedisKey>();
        foreach (RedisValue entry in await _database.SortedSetRangeByRankAsync(indexKey))
        {
            string key = entry.ToString();
            int firstSpacerIndex = key.IndexOf(":")+1;
            string matchKey = key.Remove(key.IndexOf(":", firstSpacerIndex));
            if (tmpKeys.Contains(matchKey))
            {
                rmKeys.Add(key);
            }
        }
        foreach (RedisKey key in rmKeys)
        {
            _ = transaction.SortedSetRemoveAsync(indexKey, new RedisValue(key!));
        }

        _ = transaction.KeyDeleteAsync(rmKeys.ToArray());
    }
}
