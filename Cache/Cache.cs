using Amazon.S3.Model;
using Amazon.S3;
using Newtonsoft.Json;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace CachingS3
{
    public class Cache
    {

        // Generate a unique key to save/retrieve items on S3.
        public static string GetHash<T>(T bodyDto, Enum enumCache)
        {
            var serializeObject = JsonConvert.SerializeObject(bodyDto);
            using SHA256 sha256 = SHA256.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(serializeObject);
            byte[] hashBytes = sha256.ComputeHash(inputBytes);
            string hashValue = $"{enumCache}{BitConverter.ToString(hashBytes).Replace("-", "").ToLower()}.json";
            return hashValue;
        }

        // Deserialize json to your object
        public static T? DeserializeFromMemoryStream<T>(MemoryStream memoryStream)
        {
            using StreamReader streamReader = new(memoryStream);
            using JsonReader jsonReader = new JsonTextReader(streamReader);
            var serializer = new JsonSerializer();
            return serializer.Deserialize<T?>(jsonReader);
        }

        // Retrieve S3 cache information; if it exists, deserialize the object; otherwise, return an error.
        public static async Task<T?> GetCacheS3<T>(
            string hash, 
            string bucketName,
            IAmazonS3 awsS3Client
        )
        {
            try
            {
                var getObjectRequest = new GetObjectRequest()
                {
                    BucketName = bucketName,
                    Key = hash
                };

                var response = await awsS3Client.GetObjectAsync(getObjectRequest);
                if (response.HttpStatusCode == HttpStatusCode.OK)
                {
                    using var ms = new MemoryStream();
                    await response.ResponseStream.CopyToAsync(ms);
                    if (ms is null || ms.ToArray().Length < 1)
                        throw new FileNotFoundException(string.Format("The document '{0}' is not found", hash));
                    return DeserializeFromMemoryStream<T>(ms);
                }

                return default;
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.ErrorCode == "NoSuchKey")
                {
                    return default;
                }

                return default;
            }
        }

        // Save cache in s3
        public static async Task<bool> SaveCache<T>(
            string hash, 
            T dto, 
            string bucketName,
            IAmazonS3 awsS3Client
        )
        {
            string json = JsonConvert.SerializeObject(dto);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
            var putRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = hash,
                InputStream = new MemoryStream(jsonBytes),
            };

            try
            {
                PutObjectResponse response = await awsS3Client.PutObjectAsync(putRequest);
                return response.HttpStatusCode == HttpStatusCode.OK;
            }
            catch (AmazonS3Exception ex)
            {
                if(ex.ErrorCode.Equals(500))
                    return false;

                return false;
            }
        }
    }
}
