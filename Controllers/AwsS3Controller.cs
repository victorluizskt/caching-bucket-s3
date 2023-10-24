using CachingS3.Interface;
using CachingS3.Service;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CachingS3.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AwsS3Controller : ControllerBase
    {
        public readonly IAppConfiguration _appConfiguration;
        public readonly IAws3Services _aws3Services;

        public AwsS3Controller(IAppConfiguration appConfiguration)
        {
            _appConfiguration = appConfiguration;
            _aws3Services = new Aws3Services(
                _appConfiguration.AwsAccessKey, 
                _appConfiguration.AwsSecretAccessKey,
                _appConfiguration.Region, 
                _appConfiguration.BucketName
            );
        }

        [HttpGet()]
        public async Task<IActionResult> GetDocumentFromS3()
        {
            try
            {
                var document = await _aws3Services.GetAllFileNames();
                return Ok(document);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult UploadDocumentToS3(IFormFile file)
        {
            try
            {
                if (file is null || file.Length <= 0)
                    return BadRequest("file is required to upload");
                var result = _aws3Services.UploadFileAsync(file);
                return Ok(HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    };
}
