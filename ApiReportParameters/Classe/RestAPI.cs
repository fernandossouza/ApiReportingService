using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using System.IO;
using System.Configuration;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiReportParameters.Classe
{
    public class RestAPI
    {
        public Models.BigTable BigTableGet(int thingId,long dtIni, long dtFim)
        {
            var url = ConfigurationManager.AppSettings["UrlBigTable"];
            url = url + "?thingId=" + thingId;
            url = url + "&startDate=" + dtIni;
            url = url + "&endDate=" + dtFim;

            var bigTableValues = RequestOtherAPI("GET", url, "xpto", "application/json", null);

            var bigTable = JsonConvert.DeserializeObject<Models.BigTable>(bigTableValues);

            return bigTable;
        }

        public List<Models.Thing> ThingGet()
        {
            var url = ConfigurationManager.AppSettings["UrlThing"];

            var ThingsValues = RequestOtherAPI("GET", url, "xpto", "application/json", null);

            var things = JsonConvert.DeserializeObject<List<Models.Thing>>(ThingsValues);

            return things;
        }


        private string RequestOtherAPI(string method, string url, string authHeader, string contentType, string data = null)
        {

            string content = null;

            WebRequest webRequest = null;
            WebResponse webResponse = null;
            HttpWebResponse httpResponse = null;
            Stream dataStream = null;
            StreamReader streamReader = null;

            try
            {
                webRequest = WebRequest.Create(url);
                webRequest.Method = method;
                webRequest.ContentType = contentType;
                webRequest.Headers.Add(HttpRequestHeader.Authorization, authHeader);

                // If there is data to send,
                // do appropriate logic
                if (!string.IsNullOrEmpty(data))
                {
                    byte[] byteArray = Encoding.UTF8.GetBytes(data);
                    webRequest.ContentLength = byteArray.Length;
                    dataStream = webRequest.GetRequestStream();
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Close();
                }

                webResponse = webRequest.GetResponse();

                httpResponse = (HttpWebResponse)webResponse;

                dataStream = webResponse.GetResponseStream();

                streamReader = new StreamReader(dataStream);

                content = streamReader.ReadToEnd();
            }
            catch (Exception ex)
            {
                return "";
            }
            finally
            {
                if (streamReader != null) streamReader.Close();
                if (dataStream != null) dataStream.Close();
                if (httpResponse != null) httpResponse.Close();
                if (webResponse != null) webResponse.Close();
                if (webRequest != null) webRequest.Abort();
            }

            return content;
        }
    }
}