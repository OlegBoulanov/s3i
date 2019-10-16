using System;
using System.Collections.Generic;
using System.Text;

namespace s3iLib
{
    public class CommandProcessor<T>
    {
        public Outcome<R, E> Execute<R, E>(Func<Outcome<R, E>> function, Func<Exception, Outcome<R, E>> exception = null)
        {
            Outcome<R, E> outcome = null;
            try
            {
                outcome = function();
            }
            catch (Exception x)
            {
                outcome = exception?.Invoke(x) ?? Outcome<R, E>.Failure();
            }
            return outcome;
        }

    }


}
