using System.Collections.Generic;
using System.Linq;
using AndcultureCode.CSharp.Core.Models;

namespace BlazorCMS.Shared.Dtos
{
    /// <summary>
    /// This class is needed because the JSON deserializer doesn't support deserializing interface types
    /// </summary>
    public class ResultDto<T>
    {
        public virtual int ErrorCount
        {
            get
            {
                if (this.Errors == null)
                    return 0;
                return this.Errors.Count;
            }
        }

        public virtual List<Error> Errors { get; set; }

        public virtual bool HasErrors
        {
            get
            {
                if (this.Errors != null)
                    return this.Errors.Any<Error>();
                return false;
            }
        }

        public virtual T ResultObject { get; set; }

        public ResultDto()
        {
        }

        public ResultDto(string errorMessage)
        {
            this.AddError(errorMessage);
        }

        public ResultDto(string errorKey, string errorMessage)
        {
            this.AddError(errorKey, errorMessage);
        }

        public ResultDto(T resultObject)
        {
            this.ResultObject = resultObject;
        }

        public void AddError(string errorKey, string errorMessage)
        {
            if (Errors == null)
            {
                Errors = new List<Error>();
            }

            Errors.Add(new Error
            {
                Key = errorKey,
                Message = errorMessage
            });
        }

        public void AddError(string message)
        {
            if (Errors == null)
            {
                Errors = new List<Error>();
            }

            Errors.Add(new Error
            {
                Message = message
            });
        }

        public bool HasErrorsOrResultIsNull() => HasErrors || ResultObject == null;
    }
}
