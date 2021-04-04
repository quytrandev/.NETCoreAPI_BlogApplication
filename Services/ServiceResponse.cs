

namespace Services
{
    public class ServiceResponse<T>
    {
        public T Data { get; set; }
        public int currentItemId { get; set; }
        public bool isSuccess { get; set; }
        public string Message { get; set; }
    }
}
