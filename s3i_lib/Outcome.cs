using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace s3i_lib
{
    public class Outcome<R, E>
    {
        #region Properties
        public R Result { get; protected set; }
        public List<E> Errors { get; protected set; }
        public bool Failed { get; protected set; }
        public bool Succeeded { get { return !Failed; } }
        #endregion
        #region Constructors
        public Outcome(R result, params E[] errors) : this(result, errors.ToList()) { }
        public Outcome(R result, IEnumerable<E> errors)
        {
            Result = result;
            Failed = 0 < errors.Count();
            Errors = Failed ? new List<E>(errors) : null;
        }
        #endregion
        #region Operations
        public Outcome<R, E> AddErrors(IEnumerable<E> errors)
        {
            Failed = true;
            if (null == Errors) Errors = new List<E>();
            Errors.AddRange(errors);
            return this;
        }
        public Outcome<R, E> AddErrors(params E[] errors)
        {
            return AddErrors(errors.ToList());
        }
        #endregion
        #region Shortcuts
        public static implicit operator R(Outcome<R, E> outcome) { return outcome.Result; }
        public static implicit operator Outcome<R, E>(R result) { return new Outcome<R, E>(result); }
        public static Outcome<R, E> Success(R result = default(R)) { return new Outcome<R, E>(result); }
        public static Outcome<R, E> Failure(params E[] errors) { return new Outcome<R, E>(default(R), errors); }
        public static Outcome<R, E> Failure(IEnumerable<E> errors) { return new Outcome<R, E>(default(R), errors); }
        public static Outcome<R, E> Failure(R result, params E[] errors) { return new Outcome<R, E>(result, errors); }
        public static Outcome<R, E> Failure(R result, IEnumerable<E> errors) { return new Outcome<R, E>(result, errors); }
        #endregion
    }
}
