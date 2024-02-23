namespace KidProjectServer.Models
{
    public class ResponseHandle<T>
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        static public ResponseHandle<T> Success(T Data)
        {
            return new ResponseHandle<T>
            {
                Code = Constants.STATUS_CODE_OK,
                Message = Constants.MESSAGE_SUCCESS,
                Data = Data
            };
        }

        static public ResponseHandle<string> Error(string msg)
        {
            return new ResponseHandle<string>
            {
                Code = Constants.STATUS_CODE_ERROR,
                Message = msg
            };
        }
    }
}
