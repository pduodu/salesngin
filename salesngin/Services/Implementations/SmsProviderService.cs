using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace salesngin.Services.Implementations;

public class SmsProviderService(IOptions<SmsProviderConfig> smsProviderConfig, IConfiguration configuration, HttpClient httpClient) : ISmsProviderService
    {
        private readonly SmsProviderConfig _smsProviderConfig = smsProviderConfig.Value;
        private readonly IConfiguration _configuration = configuration;
        private readonly HttpClient _httpClient = httpClient;

        public async Task<string> SendSmsAsync(string[] phoneNumbers, string message)
        {
            string _apiUrl = _configuration.GetValue<string>("SmsProvider:ApiUrl");
            string _apiKey = _configuration.GetValue<string>("SmsProvider:ApiKey");
            string _apiSenderId = _configuration.GetValue<string>("SmsProvider:ApiSenderId");
            _httpClient.BaseAddress = new Uri(_apiUrl);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            var requestContent = new StringContent(JsonConvert.SerializeObject(new
            {
                recipient = phoneNumbers,
                message = message,
                sender = _apiSenderId
            }), Encoding.UTF8, "application/json");

            //var response = await _httpClient.PostAsync(new Uri(_apiUrl), requestContent);
            var response = await _httpClient.PostAsync($"sms/quick?key={_apiKey}", requestContent);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to send SMS: {response.StatusCode}");
            }

            return await response.Content.ReadAsStringAsync();


            // Assuming your API endpoint expects the phone number, message, and API key as query parameters
            //var requestUrl = $"{_apiUrl}?phoneNumber={phoneNumber}&message={message}&apiKey={_apiKey}";

            // Send HTTP request to fetch SMS configuration
            //var response = _httpClient.GetAsync(requestUrl).Result;

            //if (response.IsSuccessStatusCode)
            //{
            //    // Process the response if needed
            //    Console.WriteLine("SMS configuration fetched successfully");
            //}
            //else
            //{
            //    // Handle error
            //    Console.WriteLine($"Failed to fetch SMS configuration. Status code: {response.StatusCode}");
            //}

            // Send SMS using fetched configuration
            //Console.WriteLine($"Sending SMS to {phoneNumber}: {message}");
        }
    }

