using System;
using System.Net;

namespace MiguModelViewer
{
    public class Network
    {
        public static bool CheckStatus()
        {
            try
            {
                string url = "http://www.gstatic.com/generate_204";

                var request = (HttpWebRequest)WebRequest.Create(url);
                request.KeepAlive = false;
                // Wait 3 seconds to connect; Should be fast enough in any internet connection since
                // gstatic pages have no content and will therefore load instantly
                request.Timeout = 3000;
                using (var response = (HttpWebResponse)request.GetResponse())
                    return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
