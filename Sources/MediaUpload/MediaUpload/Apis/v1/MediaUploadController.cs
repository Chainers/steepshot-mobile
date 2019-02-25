using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using MediaUpload.Attributes;
using MediaUpload.Helpers;
using MediaUpload.Models;
using MediaUpload.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace MediaUpload.Apis.v1
{
    [Authorize]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class MediaUploadController : Controller
    {
        private static readonly HashSet<string> SupportedMimeTypes = new HashSet<string>()
        {
            "image/jpeg"
        };

        private readonly ILogger<MediaUploadController> _logger;
        private readonly MediaProcessingService _mediaProcessingService;
        private static readonly FormOptions DefaultFormOptions = new FormOptions();


        public MediaUploadController(ILogger<MediaUploadController> logger, MediaProcessingService mediaProcessingService)
        {
            _logger = logger;
            _mediaProcessingService = mediaProcessingService;
        }

        [HttpPost]
        [DisableFormValueModelBinding]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Post()
        {
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                return BadRequest($"Expected a multipart request, but got {Request.ContentType}");
            }

            var login = HttpContext.User.Claims.First(c => c.Type == ClaimsIdentity.DefaultNameClaimType).Value;
            var role = HttpContext.User.Claims.First(c => c.Type == ClaimsIdentity.DefaultRoleClaimType).Value;

            var model = await TryLoadMediaModelAsync();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var processingModel = _mediaProcessingService.Enqueue(model);

            if (processingModel == null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            return Ok(processingModel);
        }

        private async Task<MediaModel> TryLoadMediaModelAsync()
        {
            var model = new MediaModel();
            try
            {
                var formAccumulator = new KeyValueAccumulator();
                var boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType), DefaultFormOptions.MultipartBoundaryLengthLimit);
                var reader = new MultipartReader(boundary.Value, HttpContext.Request.Body);

                var section = await reader.ReadNextSectionAsync();
                while (section != null)
                {
                    var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out ContentDispositionHeaderValue contentDisposition);

                    if (hasContentDispositionHeader)
                    {
                        if (contentDisposition == null || !contentDisposition.DispositionType.Equals("form-data"))
                            continue;

                        if (contentDisposition.FileName.HasValue || contentDisposition.FileNameStar.HasValue)
                        {
                            if (!SupportedMimeTypes.Contains(section.ContentType))
                                throw new NotSupportedException($"{section.ContentType}");

                            var targetFilePath = Path.GetTempFileName();
                            using (var targetStream = System.IO.File.Create(targetFilePath))
                            {
                                await section.Body.CopyToAsync(targetStream);
                                _logger.LogInformation($"Copied the uploaded file '{targetFilePath}'");
                            }
                            model.FilePath = targetFilePath;
                            model.FileName = contentDisposition.FileName.Value;
                        }
                        else
                        {
                            var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name);
                            var encoding = GetEncoding(section);
                            using (var streamReader = new StreamReader(section.Body, encoding, true, 1024, true))
                            {
                                var value = await streamReader.ReadToEndAsync();
                                if (string.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
                                {
                                    value = string.Empty;
                                }
                                formAccumulator.Append(key.Value, value.ToLower());

                                if (formAccumulator.ValueCount > DefaultFormOptions.ValueCountLimit)
                                {
                                    throw new InvalidDataException($"Form key count limit {DefaultFormOptions.ValueCountLimit} exceeded.");
                                }
                            }
                        }
                    }
                    section = await reader.ReadNextSectionAsync();
                }
                var formValueProvider = new FormValueProvider(BindingSource.Form, new FormCollection(formAccumulator.GetResults()), CultureInfo.CurrentCulture);
                var bindingSuccessful = await TryUpdateModelAsync(model, string.Empty, formValueProvider);
                if (bindingSuccessful)
                    return model;
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                if (!string.IsNullOrEmpty(model.FileName) && System.IO.File.Exists(model.FileName))
                {
                    System.IO.File.Delete(model.FileName);
                }

                ModelState.AddModelError(string.Empty, e.Message);
            }
            return null;
        }

        private static Encoding GetEncoding(MultipartSection section)
        {
            var hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out MediaTypeHeaderValue mediaType);
            if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding))
                return Encoding.UTF8;
            return mediaType.Encoding;
        }
    }
}