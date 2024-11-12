using Amazon.Lambda.Core;
using Newtonsoft.Json;
using System.Text;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EsepWebhook;

public class Function
{
    
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public string FunctionHandler(string input, ILambdaContext context)
    {
        context.Logger.LogInformation($"FunctionHandler received: {input}");

        try
        {
            dynamic json = JsonConvert.DeserializeObject<dynamic>(input);
            
            if (json == null || json.issue == null || json.issue.html_url == null)
            {
                return "Invalid input format. Please provide a valid JSON with 'issue' and 'html_url'.";
            }

            // Construct the payload for Slack
            string payload = $"{{\"text\": \"Issue Created: {json.issue.html_url}\"}}";

            using (var client = new HttpClient())
            {
                var webRequest = new HttpRequestMessage(HttpMethod.Post, Environment.GetEnvironmentVariable("SLACK_URL"))
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                };

                var response = client.Send(webRequest);
                using (var reader = new StreamReader(response.Content.ReadAsStream()))
                {
                    return reader.ReadToEnd();
                }
            }
        }
        catch (JsonReaderException ex)
        {
            context.Logger.LogError($"JsonReaderException: {ex.Message}");
            return $"Error parsing input JSON: {ex.Message}";
        }
    }
}
