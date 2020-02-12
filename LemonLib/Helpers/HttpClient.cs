using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace LemonLib
{
    public class HttpClient : WebClient
    {
        public void Stop() {
            wr.Abort();
        }
        private WebRequest wr;
        protected override WebRequest GetWebRequest(Uri address)
        {
            wr = base.GetWebRequest(address);
            return wr;
        }
    }
}
