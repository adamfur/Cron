namespace Cron
{
    public interface ICronParser
    {
        ICronScheduler Parse(string cron);
    }
}
