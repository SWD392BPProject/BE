using Microsoft.Data.SqlClient;
using System.Globalization;

namespace KidProjectServer.Util
{
    public class TextUtil
    {
        static public TimeSpan ConvertStringToTime(string timeString)
        {
            // Chuyển đổi chuỗi thành kiểu dữ liệu TimeSpan
            TimeSpan startTime = TimeSpan.ParseExact(timeString, "hh\\:mm", CultureInfo.InvariantCulture);
            return startTime;
        }
    }
}
