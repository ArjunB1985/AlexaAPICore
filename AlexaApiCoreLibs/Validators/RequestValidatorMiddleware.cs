using AlexaApiCoreLibs.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlexaApiCoreLibs.Validators
{
    public class RequestValidatorMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestValidatorMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                // Do something with context near the beginning of request processing.
                string buildMode = "Debug";// ConfigurationHelper.AppSettings["Mode"];

                if (!buildMode.Equals("Debug"))
                {
                    SpeechletRequestValidationResult validationResult = SpeechletRequestValidationResult.OK;

                    string chainUrl = null;
                    if (!context.Request.Headers.Keys.Contains(AlexaConstants.SIGNATURE_CERT_URL_REQUEST_HEADER) ||
                        String.IsNullOrEmpty(chainUrl = context.Request.Headers.First(p => p.Key == AlexaConstants.SIGNATURE_CERT_URL_REQUEST_HEADER).Value))
                    {
                        Console.WriteLine("No cert url header");
                        validationResult = validationResult | SpeechletRequestValidationResult.NoCertHeader;
                    }

                    string signature = null;
                    if (!context.Request.Headers.Keys.Contains(AlexaConstants.SIGNATURE_REQUEST_HEADER) ||
                        String.IsNullOrEmpty(signature = context.Request.Headers.First(p => p.Key == AlexaConstants.SIGNATURE_REQUEST_HEADER).Value))
                    {

                        Console.WriteLine("No signature request header");
                        validationResult = validationResult | SpeechletRequestValidationResult.NoSignatureHeader;
                    }

                    //MemoryStream ms = new MemoryStream();
                    //context.Request.Body.CopyTo(ms);
                    //// If you need it...
                    //byte[] alexaBytes = ms.ToArray();


                    // attempt to verify signature only if we were able to locate certificate and signature headers
                    //if (validationResult == SpeechletRequestValidationResult.OK)
                    //{
                    //    if (!AlexaRequestSignatureVerifierService.VerifyRequestSignature(alexaBytes, signature, chainUrl))
                    //    {

                    //        Console.WriteLine("Verify signature failed");
                    //        validationResult = validationResult | SpeechletRequestValidationResult.InvalidSignature;
                    //    }
                    //}

                    if (validationResult != SpeechletRequestValidationResult.OK)
                    {
                        throw new Exception("VALIDATION");
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.TraceError(e.StackTrace);
            }
            finally
            {
                await _next.Invoke(context);
            }
            // Clean up.
        }
    }

   
}
