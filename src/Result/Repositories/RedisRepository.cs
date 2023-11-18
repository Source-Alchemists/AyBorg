using AyBorg.SDK.Common.Ports;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;

namespace AyBorg.Result;

public sealed class RedisRepository : IRepository
{
    private const string SERVICE_INDEX = "service_index";
    private const string WORKFLOWRESULT_INDEX = "workflowResult_index";
    private const string PORTRESULT_INDEX = "portResult_index";
    private const string IMAGERESULT_INDEX = "imageResult_index";
    private const string WORKFLOW_ELAPSED_TIME_KEY = "ts:elapsed_ms";
    private const string PORTT_TIMESERIES_KEY = "ts";
    private readonly IDatabase _database;
    private readonly int _maxCacheWorkflowResults;
    private readonly long _maxStatisticsRetentionTime;

    public RedisRepository(IDatabase database, IConfiguration configuration)
    {
        _database = database;
        _maxCacheWorkflowResults = configuration.GetValue("AyBorg:Cache:MaxWorkflowResults", 1000);
        _maxStatisticsRetentionTime = (long)TimeSpan.FromDays(configuration.GetValue("AyBorg:Cache:MaxStatisticsDays", 365)).TotalMilliseconds;
    }

    public async ValueTask AddAsync(WorkflowResult result)
    {
        string workflowResultKey = $"{result.ServiceUniqueName}:workflowResult:{result.IterationId}";
        long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long workflowResultTimestamp = ((DateTimeOffset)result.StopTime).ToUnixTimeMilliseconds();

        var workFlowResultHash = new HashEntry[]
        {
            new("Service", result.ServiceUniqueName),
            new("startTime", result.StartTime.ToString()),
            new("stopTime", result.StopTime.ToString()),
            new("elapsedMs", result.ElapsedMs),
            new("success", result.Success)
        };

        NRedisStack.TimeSeriesCommands timeSeriesCommand = _database.TS();
        await timeSeriesCommand.AddAsync($"{result.ServiceUniqueName}:{WORKFLOW_ELAPSED_TIME_KEY}", new NRedisStack.DataTypes.TimeStamp(workflowResultTimestamp), result.ElapsedMs, _maxStatisticsRetentionTime);

        ITransaction transaction = _database.CreateTransaction();
        _ = transaction.SortedSetAddAsync($"{result.ServiceUniqueName}:{WORKFLOWRESULT_INDEX}", new RedisValue(workflowResultKey), workflowResultTimestamp);
        _ = transaction.SortedSetAddAsync(SERVICE_INDEX, new RedisValue(result.ServiceUniqueName), currentTimestamp);
        _ = transaction.HashSetAsync(workflowResultKey, workFlowResultHash);


        foreach (SDK.Common.Models.Port port in result.Ports)
        {
            string portResultKey = $"{result.ServiceUniqueName}:portResult:{result.IterationId}:{port.Name}";
            string stringValue = (string)port.Value!;
            bool isNumeric = double.TryParse(stringValue, out double doubleValue);
            var portResultHash = new HashEntry[]
            {
                new("ID", port.Id.ToString()),
                new("Name", port.Name),
                new("Value", stringValue),
                new("Brand", isNumeric ? (int)PortBrand.Numeric : (int)port.Brand),
                new("Service", result.ServiceUniqueName)
            };
            _ = transaction.SortedSetAddAsync($"{result.ServiceUniqueName}:{PORTRESULT_INDEX}", new RedisValue(portResultKey), workflowResultTimestamp);
            _ = transaction.HashSetAsync(portResultKey, portResultHash);

            if (isNumeric)
            {
                await timeSeriesCommand.AddAsync($"{result.ServiceUniqueName}:{PORTT_TIMESERIES_KEY}:{port.Name}", new NRedisStack.DataTypes.TimeStamp(workflowResultTimestamp), doubleValue, _maxStatisticsRetentionTime);
            }
        }

        await transaction.ExecuteAsync();
        await Clean(result.ServiceUniqueName, currentTimestamp);
    }

    public async ValueTask AddImageAsync(ImageResult result)
    {
        string imageResultKey = $"{result.ServiceUniqueName}:imageResult:{result.IterationId}:{result.PortId}";
        long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        ITransaction transaction = _database.CreateTransaction();
        _ = transaction.SortedSetAddAsync($"{result.ServiceUniqueName}:{IMAGERESULT_INDEX}", new RedisValue(imageResultKey), currentTimestamp);

        var imageResultHash = new HashEntry[]
        {
            new("Data", result.Data.Memory.ToArray()),
            new("Width", result.Width),
            new("Height", result.Height),
            new("ScaledWidth", result.ScaledWidth),
            new("ScaledHeight", result.ScaledHeight)
        };

        _ = transaction.HashSetAsync(imageResultKey, imageResultHash);
        await transaction.ExecuteAsync();
    }

    private async Task Clean(string serviceUniqueName, long currentTimestamp)
    {
        string serviceWorkflowsIndexKey = $"{serviceUniqueName}:{WORKFLOWRESULT_INDEX}";
        long wfEntryCount = await _database.SortedSetLengthAsync(serviceWorkflowsIndexKey);
        if (wfEntryCount < _maxCacheWorkflowResults)
        {
            return;
        }

        var tmpImageKeys = new List<string>();
        var tmpPortKeys = new List<string>();

        RedisValue[] workflowEntries = await _database.SortedSetRangeByRankAsync(serviceWorkflowsIndexKey, 0, wfEntryCount - _maxCacheWorkflowResults);
        ITransaction transaction = _database.CreateTransaction();
        foreach (RedisValue wfEntry in workflowEntries)
        {
            string wfKey = wfEntry.ToString();
            tmpImageKeys.Add(wfKey.Replace(":workflowResult:", ":imageResult:"));
            tmpPortKeys.Add(wfKey.Replace(":workflowResult:", ":portResult:"));

            _ = transaction.SortedSetRemoveAsync(serviceWorkflowsIndexKey, wfEntry);
            _ = transaction.KeyDeleteAsync(wfKey);
        }

        await PrepareDeleteEntries($"{serviceUniqueName}:{PORTRESULT_INDEX}", transaction, tmpPortKeys, ":portResult:");
        await PrepareDeleteEntries($"{serviceUniqueName}:{IMAGERESULT_INDEX}", transaction, tmpImageKeys, ":imageResult:");

        _ = transaction.SortedSetRemoveRangeByScoreAsync(SERVICE_INDEX, 0, currentTimestamp - _maxStatisticsRetentionTime);

        await transaction.ExecuteAsync();
    }

    private async ValueTask PrepareDeleteEntries(string indexKey, ITransaction transaction, List<string> tmpKeys, string typeName)
    {
        foreach (RedisValue entry in await _database.SortedSetRangeByRankAsync(indexKey))
        {
            string key = entry.ToString();
            int firstSpacerIndex = key.IndexOf(typeName) + typeName.Length + 1;
            string matchKey = key.Remove(key.IndexOf(":", firstSpacerIndex));
            if (tmpKeys.Contains(matchKey))
            {
                _ = transaction.SortedSetRemoveAsync(indexKey, new RedisValue(key));
                _ = transaction.KeyDeleteAsync(key);
            }
        }
    }
}
