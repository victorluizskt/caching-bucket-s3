namespace CachingS3.Dto
{
    [Serializable]
    public class ReturnDto
    {
        public string Nome { get; set; }
        public int Idade { get; set; }
        public string Sobrenome { get; set; }
        public string Cpf { get; set; }

        public ReturnDto() { }
    }
}
