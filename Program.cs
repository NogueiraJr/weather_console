using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace weather_console
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            // Não possuo um código para acesso às APIs do Google.
            // Tentei gerar um código na minha conta, porém, sem sucesso.
            //var apiKey = "< my_code >";
            ////GetZipInfo("123 something st, somewhere", apiKey);
            //await GetZipInfoAsync("123 something st, somewhere", apiKey);

            // Obtendo a temperatura com sucesso informando um zipcode válido.
            var zipCode = "94040,us";
            // var zipCode = "12236000,br";
            var ret = await GetWeather(zipCode);
            Console.WriteLine(ret);
            //
        }
        private static void GetZipInfo(String address, String apiKey)
        {
            string requestUri = string.Format("https://maps.googleapis.com/maps/api/geocode/xml?key={1}&address={0}&sensor=false", Uri.EscapeDataString(address), apiKey);

            WebRequest request = WebRequest.Create(requestUri);
            WebResponse response = request.GetResponse();
            XDocument xdoc = XDocument.Load(response.GetResponseStream());

            XElement result = xdoc.Element("GeocodeResponse").Element("result");
            XElement locationElement = result.Element("geometry").Element("location");
            XElement lat = locationElement.Element("lat");
            XElement lng = locationElement.Element("lng");
        }

        private static async Task GetZipInfoAsync(String address, String apiKey)
        {
            // var requestUri = string.Format("https://maps.googleapis.com/maps/api/geocode/xml?address={0}&sensor=false&key={1}", Uri.EscapeDataString(address), apiKey);
            var requestUri = string.Format("https://maps.googleapis.com/maps/api/geocode/xml?address={0}&sensor=true", Uri.EscapeDataString(address));

            using (var client = new HttpClient())
            {
                var request = await client.GetAsync(requestUri);
                var content = await request.Content.ReadAsStringAsync();
                //"<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<GeocodeResponse>\n <status>REQUEST_DENIED</status>\n <error_message>This API project is not authorized to use this API.</error_message>\n</GeocodeResponse>\n"
                //"<html><head><meta http-equiv=\"content-type\" content=\"text/html; charset=utf-8\"/><title>Sorry...</title><style> body { font-family: verdana, arial, sans-serif; background-color: #fff; color: #000; }</style></head><body><div><table><tr><td><b><font face=sans-serif size=10><font color=#4285f4>G</font><font color=#ea4335>o</font><font color=#fbbc05>o</font><font color=#4285f4>g</font><font color=#34a853>l</font><font color=#ea4335>e</font></font></b></td><td style=\"text-align: left; vertical-align: bottom; padding-bottom: 15px; width: 50%\"><div style=\"border-bottom: 1px solid #dfdfdf;\">Sorry...</div></td></tr></table></div><div style=\"margin-left: 4em;\"><h1>We're sorry...</h1><p>... but your computer or network may be sending automated queries. To protect our users, we can't process your request right now.</p></div><div style=\"margin-left: 4em;\">See <a href=\"https://support.google.com/websearch/answer/86640\">Google Help</a> for more information.<br/><br/></div><div style=\"text-align: center; border-top: 1px solid #dfdfdf;\"><a href=\"https://www.google.com\">Google Home</a></div></body></html>"
                var xmlDocument = XDocument.Parse(content);

            }
        }

        private static async Task<string> GetWeather(String zipInfo)
        {
            try
            {
                JsonDocument jD = await StartApi(zipInfo);
                var retCod = GetCod(jD);
                if (retCod.cod != 200)
                {
                    return retCod.message;
                }
                return GetTemperatureNow(jD);
            }
            catch (System.Exception ex)
            {
                return ex.Message;
            }
        }

        private static async Task<JsonDocument> StartApi(string zipInfo)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://api.openweathermap.org");
            var ret = await client.GetAsync($"/data/2.5/weather?zip={zipInfo}&appid=c44d8aa0c5e588db11ac6191c0bc6a60&units=metric");
            var strRet = await ret.Content.ReadAsStringAsync();
            var jDoc = JsonDocument.Parse(strRet);
            return jDoc;
        }

        private static string GetTemperatureNow(JsonDocument jD)
        {
            jD.RootElement.GetProperty("main").GetProperty("temp").TryGetDouble(out var val);
            var now = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            var ret = $"Temperatura em {now} é {val}°C";
            return ret;
        }

        private static RetCod GetCod(JsonDocument jD)
        {
            // "{\"cod\":\"404\",\"message\":\"city not found\"}"
            // "{\"coord\":{\"lon\":-122.09,\"lat\":37.39},\"weather\":[{\"id\":803,\"main\":\"Clouds\",\"description\":\"broken clouds\",\"icon\":\"04d\"}],\"base\":\"stations\",\"main\":{\"temp\":11.82,\"feels_like\":8.95,\"temp_min\":10,\"temp_max\":13.33,\"pressure\":1017,\"humidity\":57},\"visibility\":10000,\"wind\":{\"speed\":2.1,\"deg\":170},\"clouds\":{\"all\":75},\"dt\":1607621117,\"sys\":{\"type\":1,\"id\":5845,\"country\":\"US\",\"sunrise\":1607613147,\"sunset\":1607647844},\"timezone\":-28800,\"id\":0,\"name\":\"Mountain View\",\"cod\":200}"
            try
            {
                try
                {
                    return new RetCod()
                    {
                        cod = jD.RootElement.GetProperty("cod").GetInt16(),
                        message = ""
                    };
                }
                catch { }
                return new RetCod()
                {
                    cod = Int16.Parse(jD.RootElement.GetProperty("cod").GetString()),
                    message = jD.RootElement.GetProperty("message").GetString()
                };
            }
            catch (System.Exception ex)
            {
                return new RetCod()
                {
                    cod = 0,
                    message = ex.Message
                };
            }
        }
    }

    class RetCod
    {
        public int cod { get; set; }
        public String message { get; set; }
    }
}
