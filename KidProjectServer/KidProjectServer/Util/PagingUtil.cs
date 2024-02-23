namespace KidProjectServer.Util
{
    public class PagingUtil
    {
        static public void GetPageSize(ref int page, ref int size, ref int offset)
        {
            if(page < 1)
            {
                page = 1;
            }
            if (size < 1)
            {
                size = 10;
            }
            offset = (page - 1) * size;
        }
    }
}
