﻿using System.Collections.Generic;
using System.Linq;

namespace Steepshot.Core.Models.Common
{
    public class OperationResult
    {
        public bool Success => !Errors.Any();
        public List<string> Errors { get; set; }


        public OperationResult()
        {
            Errors = new List<string>();
        }

        public OperationResult(List<string> errors)
        {
            Errors = errors;
        }

        public OperationResult(string error)
        {
            Errors = new List<string>
            {
                error
            };
        }
    }

    public class OperationResult<T> : OperationResult
    {
        public T Result { get; set; }


        public OperationResult(T result)
        {
            Result = result;
        }

        public OperationResult() { }

        public OperationResult(List<string> errors) : base(errors) { }

        public OperationResult(string error) : base(error) { }
    }
}