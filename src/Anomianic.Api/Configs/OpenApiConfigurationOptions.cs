using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.OpenApi.Models;

namespace Anomianic.Api.Configs
{
    /// <summary>
    /// This represents the options entity for Open API configuration.
    /// </summary>
    public class OpenApiConfigurationOptions : IOpenApiConfigurationOptions
    {
        /// <inheritdoc />
        public OpenApiInfo Info { get; set; } = new OpenApiInfo()
        {
            Version = "0.1.0",
            Title = "Anomianic API Services",
            Description = "A set of services utilized by Anomianic clients.",
            TermsOfService = new Uri("https://github.com/mikelor/anomianic"),
            Contact = new OpenApiContact()
            {
                Name = "mikelor",
                Email = "mikelor@gmail.com",
                Url = new Uri("https://github.com/mikelor/anomianic/issues"),
            },
            License = new OpenApiLicense()
            {
                Name = "Apache 2.0",
                Url = new Uri("https://github.com/mikelor/Anomianic/blob/main/LICENSE"),
            }
        };

        public List<OpenApiServer> Servers { get; set; } = new List<OpenApiServer>();
    }
}
