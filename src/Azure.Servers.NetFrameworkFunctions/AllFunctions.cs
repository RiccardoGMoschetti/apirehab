using System.Linq;
using System;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.Runtime.Remoting.Messaging;

namespace Azure.Servers.NetFrameworkFunctions
{
    public class AllFunctions
    {
        private readonly ILogger _logger;
        private static Random random = new Random();
        private readonly Task<RedisConnection>? _redisConnectionFactory;
        private RedisConnection? _redisConnection;
        private IConfiguration _configuration;
        private object MakePerson(string? fromCache = null)
        {
            return new
            {
                id = Guid.NewGuid().ToString(),
                name = fromCache ?? new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ", 20).Select(s => s[random.Next(s.Length)]).ToArray())
            };
        }
        public AllFunctions(ILoggerFactory loggerFactory, Task<RedisConnection> redisConnectionFactory, IConfiguration configuration)
        {
            _logger = loggerFactory.CreateLogger<AllFunctions>();
            _redisConnectionFactory = redisConnectionFactory;
            _configuration = configuration;



        }
       
        [Function("GetFromCache")]
 
        public async Task<HttpResponseData> GetFromCache([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
       
            _logger.LogInformation("Entered the GetFromCache API");
            _redisConnection = await _redisConnectionFactory;
            //get the token from the header
            var headers = req.Headers;
            if (!headers.TryGetValues("Authorization", out var jwtToken))
            { 
                return req.CreateResponse(HttpStatusCode.Unauthorized);
            }
          


            var keyToRetrieve = "A" + (random.Next(100) + 1).ToString();
            string jsonToReturn = string.Empty;
            try
            {
                var valueFromCache = await _redisConnection.BasicRetryAsync(async (db) => await db.StringGetAsync(keyToRetrieve));
                jsonToReturn = JsonConvert.SerializeObject(MakePerson(valueFromCache));
            }

            catch (Exception ex)
            {
                _logger.LogError($"Error in reading from cache: {ex.Message}");
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            response.WriteString(jsonToReturn);
            _logger.LogInformation("Leaving the GetFromCache API");
            return response;
        }
    }
}
