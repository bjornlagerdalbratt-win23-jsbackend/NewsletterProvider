using Data.Contexts;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NewsletterProvider.Models;
using Newtonsoft.Json;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace NewsletterProvider.Functions;

public class GetAllSubscribers(ILogger<GetAllSubscribers> logger, DataContext context)
{
    private readonly ILogger<GetAllSubscribers> _logger = logger;
    private readonly DataContext _context = context;

    [Function("GetAllSubscribers")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
    {
        try
        {
            var subscribers = await _context.Subscribers.ToListAsync();
            if (subscribers.Count != 0)
            {
                return new OkObjectResult(subscribers);
            }

            return new NotFoundResult();
        }
        catch (Exception ex)
        {
            _logger.LogError($"GetAllSubscribers :: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
