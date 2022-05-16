namespace Noctus.Application.PipelineComponents
{
    public class JobExecutionBlockOptions
    {
        public int Threads { get; set; }
        public int BlockTimeOut { get; set; }
        public bool EnableBlockTimeOut { get; set; }

        public static JobExecutionBlockOptions Default = new JobExecutionBlockOptions
        {
            BlockTimeOut = 60, 
            Threads = 1, 
            EnableBlockTimeOut = true
        };
    }
}