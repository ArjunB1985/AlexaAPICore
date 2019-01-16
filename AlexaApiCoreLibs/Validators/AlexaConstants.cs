using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AlexaApiCoreLibs.Validators
{
    public class AlexaConstants
    {
        public const string VERSION = "1.0";
        public const string CHARACTER_ENCODING = "UTF-8";
        public const string ECHO_API_DOMAIN_NAME = "echo-api.amazon.com";
        public const string SIGNATURE_CERT_URL_REQUEST_HEADER = "SignatureCertChainUrl";
        public const string SIGNATURE_CERT_URL_HOST = "s3.amazonaws.com";
        public const string SIGNATURE_CERT_URL_PATH = "/echo.api/";
        public const string SIGNATURE_CERT_TYPE = "X.509";
        public const string SIGNATURE_REQUEST_HEADER = "Signature";
        public const string SIGNATURE_ALGORITHM = "SHA1withRSA";
        public const string SIGNATURE_KEY_TYPE = "RSA";
        public const int TIMESTAMP_TOLERANCE_SEC = 150;
        public const string BuildMode = "PROD";
        public const string Key = "hfDQHLVANjyCEjnqKK";
        public const string ApplicationID1 = "amzn1.ask.skill.17cce22d-34c5-4f59-b0f1-4f04c58f0d8b";
        public const string ApplicationID2 = "amzn1.ask.skill.ca6f52e9-a1ce-4bd8-abc9-babb9d6eb063";
        public const string ApplicationID3 = "amzn1.ask.skill.770117b4-75d2-4a9e-890c-e44b1bae28a2";
        public static JsonSerializerSettings DeserializationSettings = new JsonSerializerSettings
        {
            MissingMemberHandling = MissingMemberHandling.Ignore
        };
    }
    [Flags]
    public enum SpeechletRequestValidationResult
    {
        OK = 0,
        NoSignatureHeader = 1,
        NoCertHeader = 2,
        InvalidSignature = 4,
        InvalidTimestamp = 8,
        InvalidJson = 16,
        InvalidAppId = 32
    }
}
