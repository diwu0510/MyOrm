namespace WebApplication1.Models
{
    public class ErrorPageViewModel
    {
        public int Code { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        public ErrorPageViewModel()
        {
        }

        public ErrorPageViewModel(int code, string title, string message)
        {
            Code = code;
            Title = title;
            Message = message;
        }

        public ErrorPageViewModel(string title, string message)
        {
            Title = title;
            Message = message;
        }
    }
}
