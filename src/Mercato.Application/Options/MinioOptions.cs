namespace Mercato.Application.Options;

public class MinioOptions
{
    public string Endpoint { get; set; } = null!;
    public string AccessKey { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
    public string BucketName { get; set; } = null!;
    public bool UseSSL { get; set; }
}