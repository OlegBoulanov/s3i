﻿using System;
using System.Diagnostics.Contracts;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace s3iLib
{
    public class Outcome<TR, TE>
    {
        #region Properties
        public TR Result { get; set; }
        public List<TE> Errors { get; private set; }
        public bool Failed { get { return null != Errors; } }
        public bool Succeeded { get { return !Failed; } }
        #endregion
        #region Constructors
        public Outcome(TR result = default)
        {
            Result = result;
        }
        #endregion
        #region Operations
        public Outcome<TR, TE> AddErrors(IEnumerable<TE> errors)
        {
            //(Errors ??= new List<E>()).AddRange(errors);
            if (null == Errors) Errors = new List<TE>(); Errors.AddRange(errors);
            return this;
        }
        public Outcome<TR, TE> AddErrors(params TE[] errors)
        {
            return AddErrors(errors.ToList());
        }
        public void ResetErrors()
        {
            Errors = null;
        }
        #endregion
        #region Composition
        public Outcome<TR, TE> Merge(Outcome<TR, TE> other, Func<TR, TR, TR> merge = null)
        {
            Contract.Requires(null != other);
            Result = null != merge ? merge(Result, other.Result) : other.Result;
            if (other.Failed) AddErrors(other.Errors);
            return this;
        }
        #endregion
    }
}
