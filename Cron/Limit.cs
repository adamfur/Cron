namespace Cron
{
    public class Limit
    {
        public int Min { get; set; }
        public int Max { get; set; }

        public Limit(int min, int max)
        {
            Min = min;
            Max = max;
        }
    }
}
