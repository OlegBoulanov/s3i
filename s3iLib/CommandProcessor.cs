using System;
using System.Collections.Generic;
using System.Text;

namespace s3iLib
{
    public class CommandProcessor<T>
    {
#pragma warning disable CA1031// warning CA1031: Modify '***' to catch a more specific exception type, or rethrow the exception.
        public Outcome<TR, TE> Execute<TR, TE>(Func<Outcome<TR, TE>> function, Func<Exception, Outcome<TR, TE>> exception = null)
        {
            Outcome<TR, TE> outcome = null;
            try
            {
                outcome = function?.Invoke();
            }
            catch (Exception x)
            {
                outcome = exception?.Invoke(x) ?? new Outcome<TR, TE>();
            }
            return outcome;
        }
#pragma warning restore CA1301
    }
}
