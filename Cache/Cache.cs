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
            string hashValue = $"{enumCache}_{BitConverter.ToString(hashBytes).Replace("-", "").ToLower()}.json";
            return hashValue;
        }

        // Deserialize json to your object
        public static T? DeserializeFromMemoryStream<T>(GetObjectResponse response)
        {
            using Stream responseStream = response.ResponseStream;
            using StreamReader reader = new(responseStream);
            string jsonContent = reader.ReadToEnd();
            var jsonObject = JsonConvert.DeserializeObject<T>(jsonContent);
            return jsonObject;
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
                    return DeserializeFromMemoryStream<T>(response);
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
                var response = await awsS3Client.PutObjectAsync(putRequest);
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
