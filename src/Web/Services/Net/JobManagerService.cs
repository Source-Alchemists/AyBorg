using System.ComponentModel;
using Ayborg.Gateway.Net.V1;
using AyBorg.SDK.Common;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Web.Services.Net;

public sealed class JobManagerService : IJobManagerService
{
    private readonly ILogger<JobManagerService> _logger;
    private readonly JobManager.JobManagerClient _jobManagerClient;

    public JobManagerService(ILogger<JobManagerService> logger, JobManager.JobManagerClient client)
    {
        _logger = logger;
        _jobManagerClient = client;
    }

    public async ValueTask<IEnumerable<JobMeta>> GetMetasAsync()
    {
        try
        {
            var metas = new List<JobMeta>();
            AsyncServerStreamingCall<Ayborg.Gateway.Net.V1.JobMeta> response = _jobManagerClient.GetMetas(new Empty());
            await foreach (Ayborg.Gateway.Net.V1.JobMeta metaDto in response.ResponseStream.ReadAllAsync())
            {
                metas.Add(ToModel(metaDto));
            }

            return metas;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.Net), ex, "Failed to get job metas!");
            throw;
        }
    }

    public async ValueTask<Job> GetAsync(GetJobParameters parameters)
    {
        try
        {
            Ayborg.Gateway.Net.V1.Job response = await _jobManagerClient.GetAsync(new GetJobRequest
            {
                Id = parameters.Id
            });

            return ToModel(response);
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.Net), ex, "Failed to get job [{JobId}]!", parameters.Id);
            throw;
        }
    }

    public async ValueTask CreateAsync(CreateJobParameters parameters)
    {
        try
        {
            await _jobManagerClient.CreateAsync(new CreateJobRequest
            {
                ProjectId = parameters.ProjectId,
                DatasetId = parameters.DatasetId
            });
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.Net), ex, "Failed to create job for project [{ProjectId}], dataset [{DatasetId}]!", parameters.ProjectId, parameters.DatasetId);
            throw;
        }
    }

    public async ValueTask CancelAsync(CancelJobParameters parameters)
    {
        try
        {
            await _jobManagerClient.CancelAsync(new CancelJobRequest
            {
                Id = parameters.Id
            });
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.Net), ex, "Failed to cancel job [{JobId}]!", parameters.Id);
            throw;
        }
    }

    private static JobMeta ToModel(Ayborg.Gateway.Net.V1.JobMeta metaDto)
    {
        return new JobMeta(Id: metaDto.Id,
            ProjectId: metaDto.ProjectId,
            ProjectName: metaDto.ProjectName,
            DatasetId: metaDto.DatasetId,
            DatasetName: metaDto.DatasetName,
            ModelId: metaDto.ModelId,
            RunnerName: metaDto.RunnerName,
            RunnerUniqueName: metaDto.RunnerUniqueName,
            QueueDate: metaDto.QueueDate.ToDateTime(),
            FinishedDate: metaDto.FinishedDate.ToDateTime(),
            Status: (JobStatus)metaDto.Status);
    }

    private static Job ToModel(Ayborg.Gateway.Net.V1.Job jobDto)
    {
        return new Job(ToModel(jobDto.Meta));
    }

    public sealed record GetJobParameters(string Id);
    public sealed record CreateJobParameters(string ProjectId, string DatasetId);
    public sealed record CancelJobParameters(string Id);
    public sealed record JobMeta(string Id, string ProjectId, string ProjectName, string DatasetId, string DatasetName, string ModelId, string RunnerName, string RunnerUniqueName, DateTime QueueDate, DateTime FinishedDate, JobStatus Status);
    public sealed record Job(JobMeta meta);
    public enum JobStatus
    {
        [Description("Queued")]
        Queued = 0,
        [Description("Running")]
        Running = 1,
        [Description("Finished")]
        Finished = 2,
        [Description("Canceled")]
        Canceled = 3
    }
}
