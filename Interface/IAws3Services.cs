using CachingS3.Dto;

namespace CachingS3.Interface
{
    public interface IAws3Services
    {
        Task<ReturnDto> GetUserWithCache(BodyDto bodyDto);
    }
}
