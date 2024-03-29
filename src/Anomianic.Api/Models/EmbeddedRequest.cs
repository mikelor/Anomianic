﻿using System.Text.Json.Serialization;

namespace Anomianic.Api.Models
{
    /// <summary>
    /// This represents the request entity to take embedded image data.
    /// </summary>
    public class EmbeddedRequest
    {
        /// <summary>
        /// Gets or sets the person group.
        /// </summary>
        [JsonPropertyName("personGroup")]
        public virtual string PersonGroup { get; set; }

        /// <summary>
        /// Gets or sets the embedded image data.
        /// </summary>
        [JsonPropertyName("image")]
        public virtual string Image { get; set; }
    }
}