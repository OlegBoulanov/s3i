using System;
using System.Collections.Generic;
using System.Text;

namespace s3iLib
{
    public class CommandProcessor<T>
    {
        public Outcome<TR, TE> Execute<TR, TE>(Func<Outcome<TR, TE>> function, Func<Exception, Outcome<TR, TE>> exception = null)
        {
            Outcome<TR, TE> outcome = null;
            try
            {
                outcome = function?.Invoke();
            }
            catch (Exception x)
            {
                outcome = exception?.Invoke(x) ?? Outcome<TR, TE>.Failure();
            }
            return outcome;
        }

    }


}
