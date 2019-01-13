using AlexaApiCoreLibs.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Internal;

namespace AlexaApiCoreLibs.Validators
{
    public class RequestValidatorMiddleware
    {
        private readonly RequestDelegate _next;
        LoggingConfiguration config = new LoggingConfiguration();
        Logger logger = null;
        public RequestValidatorMiddleware(RequestDelegate next)
        {
            _next = next;
            var fileTarget = new FileTarget("target2")
            {
                FileName = "${basedir}/file.txt",
                Layout = "${longdate} ${level} ${message}  ${exception}"
            };
            config.AddTarget(fileTarget);
            config.AddRuleForOneLevel(LogLevel.Debug, fileTarget); // only errors to file
            LogManager.Configuration = config;
            logger = LogManager.GetLogger("AlexaAPILog");
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                // Do something with context near the beginning of request processing.
                

                if (AlexaConstants.BuildMode == "PROD")
                {
                    SpeechletRequestValidationResult validationResult = SpeechletRequestValidationResult.OK;

                    string chainUrl = null;
                    if (!context.Request.Headers.Keys.Contains(AlexaConstants.SIGNATURE_CERT_URL_REQUEST_HEADER) ||
                        String.IsNullOrEmpty(chainUrl = context.Request.Headers.First(p => p.Key == AlexaConstants.SIGNATURE_CERT_URL_REQUEST_HEADER).Value))
                    {
                        logger.Debug("No cert url header");
                        validationResult = validationResult | SpeechletRequestValidationResult.NoCertHeader;
                    }

                    string signature = null;
                    if (!context.Request.Headers.Keys.Contains(AlexaConstants.SIGNATURE_REQUEST_HEADER) ||
                        String.IsNullOrEmpty(signature = context.Request.Headers.First(p => p.Key == AlexaConstants.SIGNATURE_REQUEST_HEADER).Value))
                    {

                        logger.Debug("No signature request header");
                        validationResult = validationResult | SpeechletRequestValidationResult.NoSignatureHeader;
                    }

                    
                    MemoryStream ms = new MemoryStream();
                    context.Request.EnableRewind();
                    context.Request.Body.CopyTo(ms);
                               

                    //// If you need it...
                    byte[] alexaBytes = ms.ToArray();
                    context.Request.Body.Seek(0, SeekOrigin.Begin);
                    //HttpRequestStream

                    // attempt to verify signature only if we were able to locate certificate and signature headers
                    if (validationResult == SpeechletRequestValidationResult.OK)
                    {
                        if (!AlexaRequestSignatureVerifierService.VerifyRequestSignature(alexaBytes, signature, chainUrl, logger))
                        {

                            logger.Debug("Verify signature failed");
                            validationResult = validationResult | SpeechletRequestValidationResult.InvalidSignature;
                        }
                    }

                    if (validationResult != SpeechletRequestValidationResult.OK)
                    {
                        logger.Debug(validationResult.ToString());
                        throw new Exception("VALIDATION");
                    }
                    
                }
            }
            catch (Exception e)
            {
                logger.Debug(e.StackTrace);
                throw (e);
            }
            await _next.Invoke(context);
            // Clean up.
        }
    }

   
}
