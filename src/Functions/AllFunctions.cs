using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net;
    
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.Extensions.Configuration;

namespace APIRehab.Azure.Servers.Functions {
    public class AllFunctions {
        private readonly TelemetryClient telemetryClient;
        private readonly ILogger _logger;
        private readonly Task<RedisConnection> _redisConnectionFactory;
        private RedisConnection? _redisConnection;
        private TelemetryClient _telemetryClient;
        private IConfiguration _configuration;
        public AllFunctions(ILoggerFactory loggerFactory, Task<RedisConnection> redisConnectionFactory, IConfiguration configuration) {
            _logger = loggerFactory.CreateLogger<AllFunctions>();
            _redisConnectionFactory = redisConnectionFactory;
            _configuration = configuration;
    
        }

        private static Random random = new Random();
        private object MakePerson(string? fromCache=null) {
            return new {
                id = Guid.NewGuid().ToString(),
                name = fromCache?? new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ", 20).Select(s => s[random.Next(s.Length)]).ToArray())
            };
        }

        [Function ("GetFromCache")]
        public async Task<HttpResponseData> GetFromCache([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req) {
            _redisConnection = await _redisConnectionFactory;
            _logger.LogInformation("Entered the GetFromCache API");

            var keyToRetrieve = "A"+(random.Next(100)+1).ToString();
            string jsonToReturn = string.Empty;
            try {
                var valueFromCache = await _redisConnection.BasicRetryAsync(async (db) => await db.StringGetAsync(keyToRetrieve));
                jsonToReturn = JsonConvert.SerializeObject(MakePerson(valueFromCache)) ;
            }

            catch (Exception ex) {
                _logger.LogError($"Error in reading from cache: {ex.Message}");
            }
       
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            response.WriteString(jsonToReturn);
            _logger.LogInformation("Leaving the GetFromCache API");
            return response;
        }
      
        [Function("SimpleJson")]
        public  HttpResponseData SimpleJson([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req) {
            _logger.LogInformation("Entered the SimpleJson API");
            string jsonToReturn = JsonConvert.SerializeObject(MakePerson());
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            response.WriteString(jsonToReturn);
            _logger.LogInformation("Leaving the SimpleJson API");
            return response;
        }

        [Function("JustWait")]
        public async Task<HttpResponseData> JustWait([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req) {

            _logger.LogInformation("Entered the JustWait API");
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);

            HttpResponseData responseData;

            var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
          
            telemetryConfiguration.TelemetryInitializers.Add(new HttpDependenciesParsingTelemetryInitializer());
            var dependencyTrackingModule = new DependencyTrackingTelemetryModule();
            dependencyTrackingModule.Initialize(telemetryConfiguration);
          
            _telemetryClient = new TelemetryClient(telemetryConfiguration);
            _telemetryClient.InstrumentationKey = "ed8e1974-7f5d-4faf-b879-5a58d945b7e5";
            int waitMS;
            if (query.Count != 0 && query["waitMS"] != null && int.TryParse(query["waitMS"], out waitMS)) {
                _logger.LogInformation("Now waiting...");
                await Task.Delay(waitMS);
                _logger.LogInformation("Wait is over!");
                _telemetryClient.TrackDependency("Wait",$"Waiting {waitMS} milliseconds",DateTime.Now,TimeSpan.FromMicroseconds(waitMS),true);
                string jsonToReturn = JsonConvert.SerializeObject(MakePerson());

                responseData = req.CreateResponse(HttpStatusCode.OK);
                responseData.Headers.Add("Content-Type", "application/json; charset=utf-8");
                responseData.WriteString(jsonToReturn);

                _logger.LogInformation("Leaving the JustWait API");
                return responseData;
            }
            else {
                responseData = req.CreateResponse(HttpStatusCode.BadRequest);
                responseData.WriteString("waitMS query parameter is incorrect");
                return responseData;
            }
        }


        [Function("JustWaitWithoutAsync")]
        public HttpResponseData JustWaitWithoutAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req) {
            _logger.LogInformation("Entered the JustWaitWithoutAsync API");
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);

            HttpResponseData responseData;

            int waitMS = 0;
            if (query.Count != 0 && query["waitMS"] != null && int.TryParse(query["waitMS"], out waitMS)) {
                _logger.LogInformation("Now waiting...");
                Task.Delay(waitMS);
                _logger.LogInformation("Wait is over!");

                string jsonToReturn = JsonConvert.SerializeObject(MakePerson());

                responseData = req.CreateResponse(HttpStatusCode.OK);
                responseData.Headers.Add("Content-Type", "application/json; charset=utf-8");
                responseData.WriteString(jsonToReturn);

                _logger.LogInformation("Leaving the JustWaitWithoutAsync API");
                return responseData;
            }
            else {
                responseData = req.CreateResponse(HttpStatusCode.BadRequest);
                responseData.WriteString("waitMS query parameter is incorrect");
                return responseData;
            }
        }
    }
}
