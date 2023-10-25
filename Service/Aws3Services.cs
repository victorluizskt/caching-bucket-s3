using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using CachingS3.Dto;
using CachingS3.Interface;
using Newtonsoft.Json;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

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

        public async Task<ReturnDto> GetInfoUser(BodyDto bodyDto)
        {
            var hash = GetHash(bodyDto);
            var getInfoS3 = await GetDtoS3(hash);

            if(getInfoS3 != null)
            {
                return getInfoS3;
            }

            // caso contrário busque no banco

            // ~operação no banco~
            var dto = new ReturnDto
            {
                Cpf = "1234567890",
                Idade = 25,
                Nome = "Isaac",
                Sobrenome = "Newton"
            };

            // salva no cache
            await SaveCache(hash, dto);
            return dto;
        }

        private async Task<bool> SaveCache(string hash, ReturnDto dto)
        {
            // Converta o objeto JSON em uma string JSON
            string json = JsonConvert.SerializeObject(dto);
            // Converter a string JSON em um array de bytes
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
            var putRequest = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = hash,
                InputStream = new MemoryStream(jsonBytes)
            };
            // Fazer o upload do JSON diretamente para o Amazon S3
            try
            {
                PutObjectResponse response = await _awsS3Client.PutObjectAsync(putRequest);
                return response.HttpStatusCode == HttpStatusCode.OK;
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine($"Ocorreu um erro ao enviar o arquivo para o S3: {ex.Message}");
            }
            throw new NotImplementedException();
        }

        private async Task<ReturnDto> GetDtoS3(string hash)
        {
            try
            {
                GetObjectRequest getObjectRequest = new()
                {
                    BucketName = _bucketName,
                    Key = hash
                };

                using (var response = await _awsS3Client.GetObjectAsync(getObjectRequest))
                {
                    if (response.HttpStatusCode == HttpStatusCode.OK)
                    {
                        using var ms = new MemoryStream();
                        await response.ResponseStream.CopyToAsync(ms);
                        if (ms is null || ms.ToArray().Length < 1)
                            throw new FileNotFoundException(string.Format("The document '{0}' is not found", hash));
                        return DeserializeFromMemoryStream(ms);   
                    }
                }

                return new ReturnDto();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static ReturnDto DeserializeFromMemoryStream(MemoryStream memoryStream)
        {
            using StreamReader streamReader = new(memoryStream);
            using JsonReader jsonReader = new JsonTextReader(streamReader);
            var serializer = new Newtonsoft.Json.JsonSerializer();
            return serializer.Deserialize<ReturnDto>(jsonReader) ?? new ReturnDto();
        }

        private static string GetHash(BodyDto bodyDto)
        {
            var serializeObject = JsonConvert.SerializeObject(bodyDto);
            using SHA256 sha256 = SHA256.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(serializeObject);
            byte[] hashBytes = sha256.ComputeHash(inputBytes);
            // Converta os bytes do hash para uma representação em hexadecimal
            string hashValue = $"{BitConverter.ToString(hashBytes).Replace("-", "").ToLower()}.json";
            return hashValue;
        }
    }
}