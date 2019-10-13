using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace s3i_lib
{
    public class Outcome<R, E>
    {
        #region Properties
        public R Result { get; set; }
        public List<E> Errors { get; protected set; }
        public bool Failed { get { return null != Errors; } }
        public bool Succeeded { get { return !Failed; } }
        #endregion
        #region Constructors
        #endregion
        #region Operations
        public Outcome<R, E> AddErrors(IEnumerable<E> errors)
        {
            //(Errors ??= new List<E>()).AddRange(errors);
            if(null != Errors) Errors = new List<E>(); Errors.AddRange(errors);
            return this;
        }
        public Outcome<R, E> AddErrors(params E[] errors)
        {
            return AddErrors(errors.ToList());
        }
        #endregion
        #region Shortcuts
        public static implicit operator R(Outcome<R, E> outcome) { return outcome.Result; }
        public static implicit operator Outcome<R, E>(R result) { return new Outcome<R, E> { Result = result }; }
        public static Outcome<R, E> Success(R result) { return new Outcome<R, E> { Result = result }; }
        public static Outcome<R, E> Failure(params E[] errors) { return Failure(default, errors); }
        public static Outcome<R, E> Failure(IEnumerable<E> errors) { return Failure(default, errors); }
        public static Outcome<R, E> Failure(R result, params E[] errors) { return new Outcome<R, E> { Result = result }.AddErrors(errors); }
        public static Outcome<R, E> Failure(R result, IEnumerable<E> errors) { return new Outcome<R, E> { Result = result }.AddErrors(errors); }
        #endregion
        #region Composition
        public Outcome<R, E> Merge(Outcome<R, E> other, Func<R, R, R> merge = null)
        {
            Result = null != merge ? merge(Result, other.Result) : other.Result;
            if (other.Failed) AddErrors(other.Errors);
            return this;
        }
        #endregion
    }
}
