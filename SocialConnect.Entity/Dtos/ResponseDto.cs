namespace SocialConnect.Entity.Dtos
{
    public class ResponseDto<T>
    {
        public bool IsSucceeded => Errors == null;
        public T Content { get; set; } = default(T)!;
        public List<string>? Errors { get; set; } = null;
    }
}
