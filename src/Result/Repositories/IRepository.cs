namespace AyBorg.Result;

public interface IRepository
{
    ValueTask AddAsync(WorkflowResult result);

    ValueTask AddImageAsync(ImageResult result);
}
