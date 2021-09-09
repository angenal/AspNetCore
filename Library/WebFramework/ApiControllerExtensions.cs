using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Collections.Generic;

namespace WebFramework
{
    /// <summary>
    /// Json format for specifying errors in HTTP API responses
    /// </summary>
    public class ErrorJsonBadRequestResultObject
    {
        /// <summary>
        /// The HTTP status code generated by the origin server for
        /// this occurrence of the problem.
        /// </summary>
        [ActionResultObjectValue]
        public int Status { get; set; } = 400;
        /// <summary>
        /// A short, human-readable summary of the problem.
        /// </summary>
        [ActionResultObjectValue]
        public string Title { get; set; }
        /// <summary>
        /// Errors for Status 400 Bad Request
        /// </summary>
        [ActionResultObjectValue]
        public IEnumerable<ErrorJsonBadRequestField> Errors { get; set; }
    }
    /// <summary></summary>
    public class ErrorJsonBadRequestField
    {
        /// <summary>
        /// The form field name.
        /// </summary>
        [ActionResultObjectValue]
        public string Code { get; set; }
        /// <summary>
        /// A short, human-readable summary of the error.
        /// </summary>
        [ActionResultObjectValue]
        public string[] Message { get; set; }
    }
    /// <summary>
    /// Json format for specifying errors in HTTP API responses
    /// </summary>
    public class ErrorJsonResultObject
    {
        /// <summary>
        /// The HTTP status code generated by the origin server for
        /// this occurrence of the problem.
        /// </summary>
        [ActionResultObjectValue]
        public int Status { get; set; }
        /// <summary>
        /// A short, human-readable summary of the problem.
        /// </summary>
        [ActionResultObjectValue]
        public string Title { get; set; }
        /// <summary>
        /// A human-readable explanation specific to this occurrence of the problem.
        /// </summary>
        [ActionResultObjectValue]
        public string Detail { get; set; }
    }
}
