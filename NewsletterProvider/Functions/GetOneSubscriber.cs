using Data.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace NewsletterProvider.Functions;

public class GetOneSubscriber(ILogger<GetOneSubscriber> logger, DataContext context)
{
    private readonly ILogger<GetOneSubscriber> _logger = logger;
    private readonly DataContext _context = context;

    [Function("GetOneSubscriber")]
    public async Task <IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "subscriber/{id}")] HttpRequest req, string id)
    {
        try
        {
            var subscriber = await _context.Subscribers.FirstOrDefaultAsync(x => x.Email == id);
            if (subscriber != null)
            {
                return new OkObjectResult(subscriber);
            }
            return new NotFoundResult();
        }
        catch (Exception ex)
        {
            _logger.LogError($"GetOneSubscriber :: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
