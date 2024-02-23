namespace KidProjectServer.Models
{
    public class RequestParams
    {
        static public string GetForKey(Dictionary<string, string> dic, string key)
        {
            if (dic.ContainsKey(key))
            {
                return dic[key];
            }
            return "";
        }
    }
}
