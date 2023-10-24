using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using CachingS3.Interface;
using System.Net;

namespace CachingS3.Service
{
    public class Aws3Services : IAws3Services
    {
        private readonly string _bucketName;
        private readonly IAmazonS3 _awsS3Client;

        public Aws3Services(string awsAccessKeyId, string awsSecretAccessKey, string region, string bucketName)
        {
            _bucketName = bucketName;
            _awsS3Client = new AmazonS3Client(awsAccessKeyId, awsSecretAccessKey, RegionEndpoint.GetBySystemName(region));
        }

        public async Task<IEnumerable<string>> GetAllFileNames()
        {
            try
            {
                ListObjectsV2Request request = new()
                {
                    BucketName = _bucketName
                };

                var listFileNames = new List<string>();
                var response = await _awsS3Client.ListObjectsV2Async(request);
                foreach (var obj in response.S3Objects)
                {
                    listFileNames.Add(obj.Key);
                }

                return listFileNames;
            }
            catch (AmazonS3Exception ex)
            {
                throw new Exception("Erro ao listar os nomes dos arquivos no Amazon S3: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro desconhecido: " + ex.Message, ex);
            }
        }

        public async Task<byte[]> DownloadFileAsync(string file)
        {
            MemoryStream? ms = null;

            try
            {
                GetObjectRequest getObjectRequest = new()
                {
                    BucketName = _bucketName,
                    Key = file
                };

                using (var response = await _awsS3Client.GetObjectAsync(getObjectRequest))
                {
                    if (response.HttpStatusCode == HttpStatusCode.OK)
                    {
                        using (ms = new MemoryStream())
                        {
                            await response.ResponseStream.CopyToAsync(ms);
                        }
                    }
                }

                if (ms is null || ms.ToArray().Length < 1)
                    throw new FileNotFoundException(string.Format("The document '{0}' is not found", file));

                return ms.ToArray();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> UploadFileAsync(IFormFile file)
        {
            try
            {
                using var newMemoryStream = new MemoryStream();
                file.CopyTo(newMemoryStream);

                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = newMemoryStream,
                    Key = file.FileName,
                    BucketName = _bucketName,
                    ContentType = file.ContentType
                };

                var fileTransferUtility = new TransferUtility(_awsS3Client);

                await fileTransferUtility.UploadAsync(uploadRequest);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
