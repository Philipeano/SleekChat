using System;
namespace SleekChat.Data.Helpers
{
    public class ResponseFormat
    {
        private string status;
        private string data;

        public ResponseFormat()
        {
        }

        public string Status { get => status; set => status = value; }

        public string Data { get => data; set => data = value; }
    }
}
