using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;


using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.WindowsAzure.Storage.Blob;

using Anomianic.Api.Configs;
using Anomianic.Api.Extensions;
using Anomianic.Api.Handlers;
using Anomianic.Api.Models;
using Anomianic.Api.Services;


namespace Anomianic.Api
{
    public  class IdentifyFaceHttpTrigger
    {
        private readonly AppSettings _settings;
        private readonly IBlobService _blob;
        private readonly IFaceService _face;
        private readonly IEmbeddedRequestHandler _handler;
        private readonly ILogger<IdentifyFaceHttpTrigger> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentifyFaceHttpTrigger"/> class.
        /// </summary>
        /// <param name="settings"><see cref="AppSettings"/> instance.</param>
        /// <param name="blob"><see cref="IBlobService"/> instance.</param>
        /// <param name="face"><see cref="IFaceService"/> instance.</param>
        /// <param name="handler"><see cref="IEmbeddedRequestHandler"/> instance.</param>
        /// <param name="logger"><see cref="ILogger{IdentifyFaceHttpTrigger}"/> instance.</param>
        public IdentifyFaceHttpTrigger(AppSettings settings, IBlobService blob, IFaceService face, IEmbeddedRequestHandler handler, ILogger<IdentifyFaceHttpTrigger> logger)
        {
            this._settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this._blob = blob ?? throw new ArgumentNullException(nameof(blob));
            this._face = face ?? throw new ArgumentNullException(nameof(face));
            this._handler = handler ?? throw new ArgumentNullException(nameof(handler));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        [FunctionName("IdentifyFace")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var request = await this._handler
                .ProcessAsync(req.Body)
                .ConfigureAwait(false);

            var blobs = await this._blob
                .GetFacesAsync(this._handler.PersonGroup, this._settings.Blob.NumberOfPhotos)
                .ConfigureAwait(false);

            var uploaded = await this._blob
                .UploadAsync(this._handler.Body, this._handler.Filename, this._handler.ContentType)
                .ConfigureAwait(false);

            var faces = await this._face
                .DetectFacesAsync(uploaded)
                .ConfigureAwait(false);

            if (!this.HasOneFaceDetected(faces))
            {
                await this._blob
                    .DeleteAsync(this._handler.Filename)
                    .ConfigureAwait(false);

                return new OkObjectResult(new ResultResponse(HttpStatusCode.BadRequest, "Too many faces or no face detected"));
            }

            if (!this.HasEnoughPhotos(blobs))
            {
                return new OkObjectResult(new ResultResponse(HttpStatusCode.Created, $"Need {this._settings.Blob.NumberOfPhotos - blobs.Count} more photo(s)."));
            }

            var identified = await this._face
                .WithPersonGroup(this._handler.PersonGroup)
                .WithPerson(blobs)
                .TrainFacesAsync()
                .IdentifyFaceAsync(uploaded)
                .UpdateFaceIdentificationAsync()
                .ConfigureAwait(false);

            if (this.IsLessConfident(identified))
            {
                await this._blob
                    .DeleteAsync(this._handler.Filename)
                    .ConfigureAwait(false);

                return new OkObjectResult(new ResultResponse(HttpStatusCode.BadRequest, $"Face not identified: {identified.Confidence:0.00}"));
            }

            return new OkObjectResult(new ResultResponse(HttpStatusCode.OK, $"Face identified: {identified.Confidence:0.00}"));

            /*
            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonSerializer.Deserialize<string>(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
            */
        }

        private bool HasOneFaceDetected(List<DetectedFace> faces)
        {
            return faces.Count == 1;
        }

        private bool HasEnoughPhotos(List<CloudBlockBlob> blobs)
        {
            return blobs.Count >= this._settings.Blob.NumberOfPhotos;
        }

        private bool IsLessConfident(FaceEntity identified)
        {
            return identified.Confidence < this._settings.Face.Confidence;
        }
    }
}

