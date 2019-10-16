using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace s3iLib
{
    public class ScheduledTask<T>
    {

        public async Task<T> RunAsScheduled(Func<T> func, CancellationToken cancellationToken)
        {
            return await Task.Run(() => {
                return func();
            }, cancellationToken);
        }

    }
}
