using Data.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NewsletterProvider.Models;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NewsletterProvider.Functions;

public class UnSubscribe(ILogger<UnSubscribe> logger, DataContext context)
{
    private readonly ILogger<UnSubscribe> _logger = logger;
    private readonly DataContext _context = context;

    [Function("UnSubscribe")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "delete")] HttpRequest req)
    {
        string body;

        try
        {
            using var reader = new StreamReader(req.Body);
            body = await reader.ReadToEndAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"StreamReader :: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        if (!string.IsNullOrEmpty(body))
        {
            UnsubscribeRequest unsubscribeRequest;
            try
            {
                unsubscribeRequest = JsonConvert.DeserializeObject<UnsubscribeRequest>(body)!;
            }
            catch (Exception ex)
            {
                _logger.LogError($"JsonConvert.DeserializeObject<UnsubscribeRequest> :: {ex.Message}");
                return new BadRequestResult();
            }

            if (unsubscribeRequest != null && !string.IsNullOrEmpty(unsubscribeRequest.Email))
            {
                var subscriber = await _context.Subscribers.FirstOrDefaultAsync(x => x.Email == unsubscribeRequest.Email);
                if (subscriber != null)
                {
                    _context.Subscribers.Remove(subscriber);

                    try
                    {
                        await _context.SaveChangesAsync();

                        using var http = new HttpClient();
                        StringContent content = new StringContent(JsonConvert.SerializeObject(new { Email = subscriber.Email }), Encoding.UTF8, "application/json");
                        var response = await http.PostAsync("https://silicon-newsletterprovider.azurewebsites.net/api/unsubscribe", content);

                        if (!response.IsSuccessStatusCode)
                        {
                            _logger.LogError($"External unsubscribe API call failed with status code {response.StatusCode}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"http.PostAsync :: {ex.Message}");
                        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                    }

                    return new OkResult();
                }
                else
                {
                    return new NotFoundResult();
                }
            }
        }
        return new BadRequestResult();
    }
}
