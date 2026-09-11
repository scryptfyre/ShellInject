namespace Sample.Services;

public class SampleService : ISampleService
{
    public string GetMessage()
    {
        return "ISampleService resolved through constructor injection";
    }
}
