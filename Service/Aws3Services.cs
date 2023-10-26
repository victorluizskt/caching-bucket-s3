using Amazon;
using Amazon.S3;
using CachingS3.Dto;
using CachingS3.Interface;

namespace CachingS3.Service
{
    public class Aws3Services : IAws3Services
    {
        private readonly string _bucketName;
        private readonly IAmazonS3 _awsS3Client;

        public Aws3Services(
            string awsAccessKeyId, 
            string awsSecretAccessKey, 
            string region, 
            string bucketName
        )
        {
            _bucketName = bucketName;
            _awsS3Client = new AmazonS3Client(
                awsAccessKeyId, 
                awsSecretAccessKey, 
                RegionEndpoint.GetBySystemName(region)
            );
        }

        public async Task<UserDto> GetUserWithCache(BodyDto bodyDto)
        {
            var hash = Cache.GetHash(bodyDto, NameCache.FIDC);
            var cacheS3 = await Cache.GetCacheS3<UserDto>(hash, _bucketName, _awsS3Client);

            if(cacheS3 != null)
            {
                return cacheS3;
            }

            // Simulate operation in database
            await Task.Delay(3000);
            var dto = new UserDto
            {
                Cpf = "1234567890",
                Idade = 25,
                Nome = "Isaac",
                Sobrenome = "Newton"
            };

            await Cache.SaveCache(hash, dto, _bucketName, _awsS3Client);
            return dto;
        }
    }
}