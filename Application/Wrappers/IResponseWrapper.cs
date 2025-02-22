namespace Application.Wrappers;

public interface IResponseWrapper
{
    public List<string> Messages { get; set; }
    public bool ISuccessful { get; set; }
}

public interface IResponseWrapper<out T> : IResponseWrapper
{
    public T Data { get; }
}
