using System.Net;
using System.Text.Json.Serialization;

namespace Anomianic.Api.Models
{
    /// <summary>
    /// This represents the response entity for the face identification result.
    /// </summary>
    public class ResultResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResultResponse"/> class.
        /// </summary>
        /// <param name="statusCode">HTTP status code.</param>
        /// <param name="message">Return message.</param>
        public ResultResponse(HttpStatusCode statusCode, string message)
        {
            this.StatusCode = (int)statusCode;
            this.Message = message;
        }

        /// <summary>
        /// Gets or sets the status code.
        /// </summary>
        [JsonPropertyName("statusCode")]
        public virtual int StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        [JsonPropertyName("message")]
        public virtual string Message { get; set; }
    }
}