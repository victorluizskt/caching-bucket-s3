namespace CachingS3.Interface
{
    public interface IAppConfiguration
    {
        public string AwsAccessKey { get; set; }
        public string AwsSecretAccessKey { get; set; }
        public string AwsSessionToken { get; set; }
        public string BucketName { get; set; }
        public string Region { get; set; }
    }
}
