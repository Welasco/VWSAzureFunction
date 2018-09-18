using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web.Script.Serialization;
using System.Web;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace VWSAzureFunction
{
    public static class Weather
    {
        [FunctionName("Weather")]
        //public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "Weather/cityname/{cityname}")]HttpRequestMessage req, string cityname, TraceWriter log)
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // parse query parameter
            string cityname = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "cityname", true) == 0)
                .Value;

            // Get request body
            //dynamic data = await req.Content.ReadAsAsync<object>();
            dynamic data = req.Content.ReadAsAsync<object>();

            // Set name to query string or body data
            //name = name ?? data?.name;

            cityname = cityname ?? data?.cityname;
            InvokeWebRequest weatherreq = new InvokeWebRequest();
            WeatherDetails weatherdetails = new WeatherDetails();
            weatherdetails = weatherreq.InvokeWeatherWebRequest(cityname);
            try
            {
                weatherdetails = weatherreq.InvokeWeatherWebRequest(cityname);
            }
            catch (System.Exception)
            {

                cityname = "Invalid City: " + cityname;
                return req.CreateResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(cityname));
            }
            
            var sWeatherdetails = JsonConvert.SerializeObject(weatherdetails, Formatting.Indented);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(sWeatherdetails, Encoding.UTF8, "application/json")
            };

            //return cityname == null
            //    ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body")
            //    //: req.CreateResponse(HttpStatusCode.OK, "You asked for: " + cityname);
            //    : req.CreateResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(weatherdetails));


            //return cityname == null
            //    ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body")
            //    //: req.CreateResponse(HttpStatusCode.OK, "You asked for: " + cityname);
            //    : req.CreateResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(weatherdetails));

            // Fetching the name from the path parameter in the request URL
            //return req.CreateResponse(HttpStatusCode.OK, "Hello " + name);
        }
    }

    public class InvokeWebRequest
    {
        private string cityname = string.Empty;
        public string Cityname {
            get
            {
                return this.cityname;
            }
            set
            {
                this.cityname = value;
            }
        }
        

        public WeatherDetails InvokeWeatherWebRequest(string pcityname)
        {
            string uri = string.Empty;
            if (!string.IsNullOrEmpty(this.cityname) || !string.IsNullOrEmpty(pcityname))
            {
                if (!string.IsNullOrEmpty(pcityname))
                {
                    uri = "https://api.openweathermap.org/data/2.5/weather?q=" + pcityname + "&units=imperial&APPID=6917cf0abc84d703021d97d482f30b30";
                }
                else
                {
                    uri = "https://api.openweathermap.org/data/2.5/weather?q=" + this.cityname + "&units=imperial&APPID=6917cf0abc84d703021d97d482f30b30";
                }
            }
            return InvokeWeatherAPI(uri);
        }

        private WeatherDetails InvokeWeatherAPI(string uri)
        {
            WebRequest request = WebRequest.Create(uri);
            request.ContentType = "application/json; charset=utf-8";
            WebResponse response = request.GetResponse();


            // Display the status.  
            // Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            
            // Get the stream containing content returned by the server.  
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.  
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.  
            string responseFromServer = reader.ReadToEnd();
            // Display the content.  
            //Console.WriteLine(responseFromServer);
            // Clean up the streams and the response.  

            dynamic responseJSON = JsonConvert.DeserializeObject(responseFromServer);
            var weather = responseJSON.main;

            WeatherDetails weatherrequest = JsonConvert.DeserializeObject<WeatherDetails>(System.Convert.ToString(weather));

            reader.Close();
            response.Close();

            return weatherrequest;

        }


        //string uri = "https://api.openweathermap.org/data/2.5/weather?q=frisco,us&units=imperial&APPID=6917cf0abc84d703021d97d482f30b30";
        //WebRequest request = WebRequest.Create("https://api.openweathermap.org/data/2.5/weather?q=frisco,us&units=imperial&APPID=6917cf0abc84d703021d97d482f30b30");
    }

    public class WeatherDetails
    {
        public string temp { get; set; }
        public string humidity { get; set; }
        public string pressure { get; set; }
        public string temp_min { get; set; }
        public string temp_max { get; set; }
    }
}
