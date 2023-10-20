using System;
using System.Net.Http.Headers;
using System.Text;

namespace SocketLabs.NLog.Targets.OpenSearch.Models
{
    internal class BasicAuthenticationHeaderValue
    {
        private BasicAuthenticationHeaderValue() 
        {            
            // private ctor.
        }

        internal static AuthenticationHeaderValue Create(string username, string password)
        {
            var basicAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));

            return new AuthenticationHeaderValue("Basic", basicAuth);
        }
    }
}