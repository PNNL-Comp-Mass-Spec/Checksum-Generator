using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Checksum_Generator
{
    internal class ChecksumGen : PRISM.EventNotifier
    {
        public string ErrorMessage
        {
            get;
            protected set;
        }

        public bool ThrowEvents
        {
            get;
            set;
        }

        private readonly MD5 mMD5Hasher;

        private readonly SHA1Managed mSHA1Hasher;

        /// <summary>
        /// Constructor
        /// </summary>
        public ChecksumGen()
        {
            ErrorMessage = string.Empty;
            ThrowEvents = false;

            mMD5Hasher = MD5.Create();
            mSHA1Hasher = new SHA1Managed();
        }

        public string GenerateMD5Hash(string filePath)
        {
            byte[] byteHash;

            // Verify input file exists
            var targetFile = new FileInfo(filePath);
            if (!targetFile.Exists)
            {
                var msg = "File not found in GenerateMD5HashFromFile: " + filePath;
                ReportError(msg);
                if (ThrowEvents)
                    throw new FileNotFoundException();

                return string.Empty;
            }

            using (var sourceFile = new FileStream(targetFile.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // Get the file's hash
                byteHash = mMD5Hasher.ComputeHash(sourceFile);
            }

            // Convert hash array to hex string
            var hashStrBld = new StringBuilder();
            foreach (var oneByte in byteHash)
            {
                hashStrBld.Append(oneByte.ToString("x2"));
            }

            return hashStrBld.ToString();
        }

        public string GenerateSha1Hash(string filePath)
        {
            var targetFile = new FileInfo(filePath);

            if (!targetFile.Exists)
            {
                var msg = "File not found in GenerateSha1Hash: " + filePath;
                ReportError(msg);
                if (ThrowEvents)
                    throw new FileNotFoundException();

                return string.Empty;
            }

            using var sourceFile = new FileStream(targetFile.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);

            var fileHash = mSHA1Hasher.ComputeHash(sourceFile);

            return ToHexString(fileHash);
        }

        /// <summary>
        /// Report an error.
        /// </summary>
        /// <param name="message"></param>
        protected void ReportError(string message)
        {
            ErrorMessage = message;

            OnErrorEvent(message);
        }

        public string ToHexString(byte[] buffer)
        {
            return BitConverter.ToString(buffer).Replace("-", string.Empty).ToLower();
        }
    }
}
