using System.Numerics;

namespace KidProjectServer.Models
{
    public class ResponseArrayHandle<T>
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public T[] Data { get; set; }
        public int TotalPage { get; set; }

        static public ResponseArrayHandle<T> Success(T[] Data)
        {
            return new ResponseArrayHandle<T>
            {
                Code = Constants.STATUS_CODE_OK,
                Message = Constants.MESSAGE_SUCCESS,
                Data = Data,
                TotalPage = 1
            };
        }

        static public ResponseArrayHandle<T> Success(T[] Data, int total)
        {
            return new ResponseArrayHandle<T>
            {
                Code = Constants.STATUS_CODE_OK,
                Message = Constants.MESSAGE_SUCCESS,
                Data = Data,
                TotalPage = total
            };
        }

        static public ResponseArrayHandle<string> Error(string msg)
        {
            return new ResponseArrayHandle<string>
            {
                Code = Constants.STATUS_CODE_ERROR,
                Message = msg
            };
        }
    }
}
