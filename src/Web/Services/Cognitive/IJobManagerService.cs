using static AyBorg.Web.Services.Cognitive.JobManagerService;

namespace AyBorg.Web.Services.Cognitive;

public interface IJobManagerService {
    ValueTask<IEnumerable<JobMeta>> GetMetasAsync();
    ValueTask<Job> GetAsync(GetJobParameters parameters);
    ValueTask CreateAsync(CreateJobParameters parameters);
    ValueTask CancelAsync(CancelJobParameters parameters);
}
