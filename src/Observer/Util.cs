namespace Observer
{
    public static class Util
    {
        public static int ToInt(this string s, int d = 0)
        {
            int.TryParse(s, out int i);
            i = i == 0 ? d : i;
            return i;
        }
    }
}
