using CachingS3.Interface;

namespace CachingS3.Configuration
{
    public class AppConfiguration : IAppConfiguration
    {
        public AppConfiguration()
        {
            // add configuration again
            BucketName = "";
            Region = "";
            AwsAccessKey = "";
            AwsSecretAccessKey = "";
        }

        public string BucketName { get; set; }
        public string Region { get; set; }
        public string AwsAccessKey { get; set; }
        public string AwsSecretAccessKey { get; set; }
    }
}
