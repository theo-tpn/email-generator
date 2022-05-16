using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Noctus.GenWave.Desktop.App.Models;
using Noctus.Infrastructure;
using Stl.Fusion;

namespace Noctus.GenWave.Desktop.App.Managers
{
    [ComputeService]    
    public class PerformanceRecorder
    {
        private readonly FixedSizedQueue<PerformanceRecord> _queue = new(10);

        [ComputeMethod(AutoInvalidateTime = 1, KeepAliveTime = 0.9)]
        public virtual async Task<IEnumerable<PerformanceRecord>> Get()
        {
            return await Task.FromResult(_queue.Get());
        }

        public void PushData(float cpuPercent, float ramPercent)
        {
            _queue.Enqueue(new PerformanceRecord(cpuPercent, ramPercent, DateTime.Now));
        }
    }
}