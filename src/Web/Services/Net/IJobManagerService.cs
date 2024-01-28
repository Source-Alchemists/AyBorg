using static AyBorg.Web.Services.Net.JobManagerService;

namespace AyBorg.Web.Services.Net;

public interface IJobManagerService {
    ValueTask<IEnumerable<JobMeta>> GetMetasAsync();
    ValueTask<Job> GetAsync(GetJobParameters parameters);
    ValueTask CreateAsync(CreateJobParameters parameters);
    ValueTask CancelAsync(CancelJobParameters parameters);
}
