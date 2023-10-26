using CachingS3.Dto;
using CachingS3.Interface;
using CachingS3.Service;
using Microsoft.AspNetCore.Mvc;

namespace CachingS3.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        public readonly IAppConfiguration _appConfiguration;
        public readonly IAws3Services _aws3Services;

        public UserController(IAppConfiguration appConfiguration)
        {
            _appConfiguration = appConfiguration;
            _aws3Services = new UserService(
                _appConfiguration.AwsAccessKey, 
                _appConfiguration.AwsSecretAccessKey,
                _appConfiguration.Region, 
                _appConfiguration.BucketName
            );
        }

        [HttpPost("getUser")]
        public async Task<IActionResult> GetInfoUser(
            [FromBody] BodyDto bodyDto    
        )
        {
            try
            {
                var result = await _aws3Services.GetUserWithCache(bodyDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    };
}
