﻿using NLog;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.X509;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace AlexaApiCoreLibs.Validators
{
    public static class AlexaRequestSignatureVerifierService
    {
        private static Func<string, string> _getCertCacheKey = (string url) => string.Format("{0}_{1}", AlexaConstants.SIGNATURE_CERT_URL_REQUEST_HEADER, url);

        private static CacheItemPolicy _policy = new CacheItemPolicy
        {
            Priority = CacheItemPriority.Default,
            AbsoluteExpiration = DateTimeOffset.UtcNow.AddHours(24)
        };


        /// <summary>
        /// Verifying the Signature Certificate URL per requirements documented at
        /// https://developer.amazon.com/public/solutions/alexa/alexa-skills-kit/docs/developing-an-alexa-skill-as-a-web-service
        /// </summary>
        public static bool VerifyCertificateUrl(string certChainUrl)
        {
            if (String.IsNullOrEmpty(certChainUrl))
            {
                return false;
            }

            Uri certChainUri;
            if (!Uri.TryCreate(certChainUrl, UriKind.Absolute, out certChainUri))
            {
                return false;
            }

            return
                certChainUri.Host.Equals(AlexaConstants.SIGNATURE_CERT_URL_HOST, StringComparison.OrdinalIgnoreCase) &&
                certChainUri.PathAndQuery.StartsWith(AlexaConstants.SIGNATURE_CERT_URL_PATH) &&
                certChainUri.Scheme == Uri.UriSchemeHttps &&
                certChainUri.Port == 443;
        }


        /// <summary>
        /// Verifies request signature and manages the caching of the signature certificate
        /// </summary>
        public static bool VerifyRequestSignature(
            byte[] serializedSpeechletRequest, string expectedSignature, string certChainUrl,Logger logger)
        {
           // logger.Debug("In Verify Sig");
            string certCacheKey = _getCertCacheKey(certChainUrl);
            X509Certificate cert = MemoryCache.Default.Get(certCacheKey) as X509Certificate;
            if (cert == null ||
                !CheckRequestSignature(serializedSpeechletRequest, expectedSignature, cert,logger))
            {
               // logger.Debug("Inside if getting cert");
                // download the cert 
                // if we don't have it in cache or
                // if we have it but it's stale because the current request was signed with a newer cert
                // (signaled by signature check fail with cached cert)
                cert = RetrieveAndVerifyCertificate(certChainUrl);
                if (cert == null)
                {
                    logger.Debug("no cert");
                    return false;
                }
                
                MemoryCache.Default.Set(certCacheKey, cert, _policy);
            }
          //  logger.Debug(CheckRequestSignature(serializedSpeechletRequest, expectedSignature, cert, logger));
            return CheckRequestSignature(serializedSpeechletRequest, expectedSignature, cert, logger);
        }


        /// <summary>
        /// Verifies request signature and manages the caching of the signature certificate
        /// </summary>
        public static async Task<bool> VerifyRequestSignatureAsync(
            byte[] serializedSpeechletRequest, string expectedSignature, string certChainUrl)
        {

            string certCacheKey = _getCertCacheKey(certChainUrl);
            X509Certificate cert = MemoryCache.Default.Get(certCacheKey) as X509Certificate;
            if (cert == null ||
                !CheckRequestSignature(serializedSpeechletRequest, expectedSignature, cert,null))
            {

                // download the cert 
                // if we don't have it in cache or 
                // if we have it but it's stale because the current request was signed with a newer cert
                // (signaled by signature check fail with cached cert)
                cert = await RetrieveAndVerifyCertificateAsync(certChainUrl);
                if (cert == null) return false;

                MemoryCache.Default.Set(certCacheKey, cert, _policy);
            }

            return CheckRequestSignature(serializedSpeechletRequest, expectedSignature, cert,null);
        }


        /// <summary>
        /// 
        /// </summary>
        public static X509Certificate RetrieveAndVerifyCertificate(string certChainUrl)
        {
            // making requests to externally-supplied URLs is an open invitation to DoS
            // so restrict host to an Alexa controlled subdomain/path
            if (!VerifyCertificateUrl(certChainUrl)) return null;

            var webClient = new WebClient();
            var content = webClient.DownloadString(certChainUrl);

            var pemReader = new Org.BouncyCastle.OpenSsl.PemReader(new StringReader(content));
            var cert = (X509Certificate)pemReader.ReadObject();
            try
            {
                cert.CheckValidity();
                if (!CheckCertSubjectNames(cert)) return null;
            }
            catch (CertificateExpiredException)
            {
                return null;
            }
            catch (CertificateNotYetValidException)
            {
                return null;
            }

            return cert;
        }


        /// <summary>
        /// 
        /// </summary>
        public static async Task<X509Certificate> RetrieveAndVerifyCertificateAsync(string certChainUrl)
        {
            // making requests to externally-supplied URLs is an open invitation to DoS
            // so restrict host to an Alexa controlled subdomain/path
            if (!VerifyCertificateUrl(certChainUrl)) return null;

            var httpClient = new HttpClient();
            var httpResponse = await httpClient.GetAsync(certChainUrl);
            var content = await httpResponse.Content.ReadAsStringAsync();
            if (String.IsNullOrEmpty(content)) return null;

            var pemReader = new Org.BouncyCastle.OpenSsl.PemReader(new StringReader(content));
            var cert = (X509Certificate)pemReader.ReadObject();
            try
            {
                cert.CheckValidity();
                if (!CheckCertSubjectNames(cert)) return null;
            }
            catch (CertificateExpiredException)
            {
                return null;
            }
            catch (CertificateNotYetValidException)
            {
                return null;
            }

            return cert;
        }


        /// <summary>
        /// 
        /// </summary>
        public static bool CheckRequestSignature(byte[] serializedSpeechletRequest, string expectedSignature, X509Certificate cert, Logger logger)
        {
           // logger.Debug("In CheckRequestSignature");
            byte[] expectedSig = null;
            try
            {
                expectedSig = Convert.FromBase64String(expectedSignature);
            }
            catch (FormatException)
            {
               // logger.Debug("format exception");
                return false;
            }


            var publicKey = (Org.BouncyCastle.Crypto.Parameters.RsaKeyParameters)cert.GetPublicKey();
            var signer = Org.BouncyCastle.Security.SignerUtilities.GetSigner(AlexaConstants.SIGNATURE_ALGORITHM);
            signer.Init(false, publicKey);
            signer.BlockUpdate(serializedSpeechletRequest, 0, serializedSpeechletRequest.Length);
           // logger.Debug("out CheckRequestSignature");
            return signer.VerifySignature(expectedSig);
        }


        /// <summary>
        /// 
        /// </summary>
        private static bool CheckCertSubjectNames(X509Certificate cert)
        {
            bool found = false;
            ArrayList subjectNamesList = (ArrayList)cert.GetSubjectAlternativeNames();
            for (int i = 0; i < subjectNamesList.Count; i++)
            {
                ArrayList subjectNames = (ArrayList)subjectNamesList[i];
                for (int j = 0; j < subjectNames.Count; j++)
                {
                    if (subjectNames[j] is String && subjectNames[j].Equals(AlexaConstants.ECHO_API_DOMAIN_NAME))
                    {
                        found = true;
                        break;
                    }
                }
            }

            return found;
        }
    }
}
