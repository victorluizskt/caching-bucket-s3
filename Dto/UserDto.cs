using Newtonsoft.Json;

namespace CachingS3.Dto
{
    [Serializable]
    public class UserDto
    {
        [JsonProperty(nameof(Nome))]
        public string? Nome { get; set; }
        [JsonProperty(nameof(Idade))]
        public int Idade { get; set; }
        [JsonProperty(nameof(Sobrenome))]
        public string? Sobrenome { get; set; }
        [JsonProperty(nameof(Cpf))]
        public string? Cpf { get; set; }
        public UserDto() { }
    }
}
