using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Checksum_Generator
{
    class clsChecksum : PRISM.EventNotifier
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

        private SHA1Managed mSHA1Hasher;
        private MD5 mMD5Hasher;

        /// <summary>
        /// Constructor
        /// </summary>
        public clsChecksum()
        {
            ErrorMessage = string.Empty;
            ThrowEvents = false;
        }

        public string GenerateMD5Hash(string filePath)
        {
            byte[] byteHash;

            //Verify input file exists
            var fi = new FileInfo(filePath);
            if (!fi.Exists)
            {
                var msg = "File not found in GenerateMD5HashFromFile: " + filePath;
                ReportError(msg);
                if (ThrowEvents)
                    throw new FileNotFoundException();

                return string.Empty;
            }

            if (mMD5Hasher == null)
                mMD5Hasher = MD5.Create();

            using (var sourceFile = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
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
            byte[] fileHash;
            var fi = new FileInfo(filePath);

            if (!fi.Exists)
            {
                var msg = "File not found in GenerateSha1Hash: " + filePath;
                ReportError(msg);
                if (ThrowEvents)
                    throw new FileNotFoundException();

                return string.Empty;
            }

            if (mSHA1Hasher == null)
            {
                mSHA1Hasher = new SHA1Managed();
            }

            using (var sourceFile = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                fileHash = mSHA1Hasher.ComputeHash(sourceFile);
            }

            var hashString = ToHexString(fileHash);

            return hashString;
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
