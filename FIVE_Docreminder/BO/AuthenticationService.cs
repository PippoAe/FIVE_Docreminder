using docreminder.InfoShareService;
using System;
using System.Security.Cryptography;
using System.Text;

namespace docreminder.BO
{
    /// <summary>
    /// Defines methods to log on to and log off from the info share WCF service.
    /// </summary>
    public class AuthenticationService
    {
        AuthenticationClient AuthenticationClient;

        public AuthenticationService()
        {
            this.AuthenticationClient = new AuthenticationClient();
        }


        public AuthenticationService(string sEndPointURL)
        {
            this.AuthenticationClient = new AuthenticationClient("BasicHttpBinding_Authentication", sEndPointURL);
        }

        /// <summary>
        /// A user logs on to the info share WCF service.
        /// 
        /// Calls the logon method on an instance of the InfoShareService class. 
        /// </summary>
        /// <param name="user">the user</param>
        /// <param name="password">the password</param>
        /// <returns>the connection id of the user who logs on</returns>
        public string Logon(string user, string password)
        {
            // last argument: the offset of the client time zone to UTC in minutes
            LogonResultContract logonResult = this.AuthenticationClient.Logon(null, user, password, null, 0);
            string connUserID = logonResult.ConnectionId;

            return connUserID;

        }

        internal string LogonWithSSO()
        {
            LogonResultContract logonResult = this.AuthenticationClient.LogonWithSingleSignOn(null, null, 0);
            return logonResult.ConnectionId;
        }

        /// <summary>
        /// A user logs off from the info share WCF service with the specified connection id.
        /// 
        /// Calls the logoff method on an instance of the InfoShareService class.
        /// </summary>
        /// <param name="client">the authentication client of the info share WCF service</param>
        /// <param name="connectionID">the connection id of the user who logs off</param>
        public void Logoff(string connectionID)
        {
            this.AuthenticationClient.Logoff(connectionID);
        }

        /// <summary>
        /// Encodes a password to base64 with secure hash algorithm SHA-512.
        /// </summary>
        /// <param name="password">the password</param>
        /// <returns>encoded password</returns>
        public string EncodeStringToBase64SHA512(string password)
        {
            if (password == null || password.Length == 0)
            {
                return password;
            }
            SHA512Managed hashTool = new SHA512Managed();
            Byte[] passwordAsByte = Encoding.UTF8.GetBytes(password);
            Byte[] encryptedBytes = hashTool.ComputeHash(passwordAsByte);
            hashTool.Clear();
            return System.Convert.ToBase64String(encryptedBytes);

        }

    }

}
