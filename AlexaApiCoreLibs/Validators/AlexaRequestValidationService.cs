using Alexa.NET.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace AlexaApiCoreLibs.Validators
{
    public class AlexaRequestValidationService 
    {

        public SpeechletRequestValidationResult ValidateAlexaRequest(SkillRequest alexaRequest)
        {
            SpeechletRequestValidationResult validationResult = SpeechletRequestValidationResult.OK;

            if (AlexaConstants.BuildMode=="PROD")
            {
                // check timestamp
                if (!VerifyRequestTimestamp(alexaRequest, DateTime.UtcNow))
                {
                    validationResult = SpeechletRequestValidationResult.InvalidTimestamp;
                    throw new Exception(validationResult.ToString());
                }

                // check app id
                if (!VerifyApplicationIdHeader(alexaRequest))
                {
                    validationResult = SpeechletRequestValidationResult.InvalidAppId;
                    throw new Exception(validationResult.ToString());
                }

            }

            return validationResult;

        }


        private bool VerifyRequestTimestamp(SkillRequest alexaRequest, DateTime referenceTimeUtc)
        {
            // verify timestamp is within tolerance
            var diff = referenceTimeUtc - alexaRequest.Request.Timestamp;
            return (Math.Abs((decimal)diff.TotalSeconds) <= AlexaConstants.TIMESTAMP_TOLERANCE_SEC);
        }

        private bool VerifyApplicationIdHeader(SkillRequest alexaRequest)
        {
            return alexaRequest.Session.Application.ApplicationId.Equals(AlexaConstants.ApplicationID1) ||
                alexaRequest.Session.Application.ApplicationId.Equals(AlexaConstants.ApplicationID2);
        }
    }
}
