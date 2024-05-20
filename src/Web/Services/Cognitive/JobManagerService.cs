using System.ComponentModel;
using Ayborg.Gateway.Cognitive.V1;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Web.Services.Cognitive;

public sealed class JobManagerService : IJobManagerService
{
    private readonly JobManager.JobManagerClient _jobManagerClient;

    public JobManagerService(JobManager.JobManagerClient client)
    {
        _jobManagerClient = client;
    }

    public async ValueTask<IEnumerable<JobMeta>> GetMetasAsync()
    {
        var metas = new List<JobMeta>();
        AsyncServerStreamingCall<Ayborg.Gateway.Cognitive.V1.JobMeta> response = _jobManagerClient.GetMetas(new Empty());
        await foreach (Ayborg.Gateway.Cognitive.V1.JobMeta metaDto in response.ResponseStream.ReadAllAsync())
        {
            metas.Add(ToModel(metaDto));
        }

        return metas;

    }

    public async ValueTask<Job> GetAsync(GetJobParameters parameters)
    {

        Ayborg.Gateway.Cognitive.V1.Job response = await _jobManagerClient.GetAsync(new GetJobRequest
        {
            Id = parameters.Id
        });

        return ToModel(response);
    }

    public async ValueTask CreateAsync(CreateJobParameters parameters)
    {

        await _jobManagerClient.CreateAsync(new CreateJobRequest
        {
            ProjectId = parameters.ProjectId,
            DatasetId = parameters.DatasetId,
            TrainingParameters = new TrainParameters
            {
                ModelName = parameters.ModelName,
                Iterations = parameters.Iterations
            }
        });
    }

    public async ValueTask CancelAsync(CancelJobParameters parameters)
    {

        await _jobManagerClient.CancelAsync(new CancelJobRequest
        {
            Id = parameters.Id
        });

    }

    private static JobMeta ToModel(Ayborg.Gateway.Cognitive.V1.JobMeta metaDto)
    {
        DateTime finishedDate = DateTime.MinValue;
        if (metaDto.FinishedDate != null)
        {
            finishedDate = metaDto.FinishedDate.ToDateTime();
        }

        return new JobMeta(Id: metaDto.Id,
            ProjectId: metaDto.ProjectId,
            ProjectName: metaDto.ProjectName,
            DatasetId: metaDto.DatasetId,
            DatasetName: metaDto.DatasetName,
            ModelId: metaDto.ModelId,
            AgentName: metaDto.AgentName,
            AgentUniqueName: metaDto.AgentUniqueName,
            QueueDate: metaDto.QueueDate.ToDateTime(),
            FinishedDate: finishedDate,
            Status: (JobStatus)metaDto.Status);
    }

    private static Job ToModel(Ayborg.Gateway.Cognitive.V1.Job jobDto)
    {
        return new Job(ToModel(jobDto.Meta));
    }

    public sealed record GetJobParameters(string Id);
    public sealed record CreateJobParameters(string ProjectId, string DatasetId, string ModelName, int Iterations);
    public sealed record CancelJobParameters(string Id);
    public sealed record JobMeta(string Id, string ProjectId, string ProjectName, string DatasetId, string DatasetName, string ModelId, string AgentName, string AgentUniqueName, DateTime QueueDate, DateTime FinishedDate, JobStatus Status);
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
