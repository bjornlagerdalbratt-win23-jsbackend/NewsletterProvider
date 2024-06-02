using Data.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NewsletterProvider.Models;
using Newtonsoft.Json;
using System.Text;

namespace NewsletterProvider.Functions
{
    public class UpdateSubscriber
    {
        private readonly ILogger<UpdateSubscriber> _logger;
        private readonly DataContext _context;

        public UpdateSubscriber(ILogger<UpdateSubscriber> logger, DataContext context)
        {
            _logger = logger;
            _context = context;
        }

        [Function("UpdateSubscriber")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "put", Route = "subscriber/{email}")] HttpRequest req, string email)
        {
            string body = await new StreamReader(req.Body).ReadToEndAsync();

            if (string.IsNullOrEmpty(body))
            {
                return new BadRequestResult();
            }

            SubscribeRequest subscriberUpdate = null!;
            try
            {
                subscriberUpdate = JsonConvert.DeserializeObject<SubscribeRequest>(body)!;
            }
            catch (Exception ex)
            {
                _logger.LogError($"JsonConvert.DeserializeObject<SubscribeRequest> :: {ex.Message}");
                return new BadRequestResult();
            }

            if (subscriberUpdate == null || string.IsNullOrEmpty(subscriberUpdate.Email))
            {
                return new BadRequestResult();
            }

            var subscriber = await _context.Subscribers.FirstOrDefaultAsync(x => x.Email == email);
            if (subscriber == null)
            {
                return new NotFoundResult();
            }

            subscriber.Email = subscriberUpdate.Email;
            subscriber.DailyNewsletter = subscriberUpdate.DailyNewsletter;
            subscriber.AdvertisingUpdates = subscriberUpdate.AdvertisingUpdates;
            subscriber.WeekInReview = subscriberUpdate.WeekInReview;
            subscriber.EventUpdates = subscriberUpdate.EventUpdates;
            subscriber.StartupsWeekly = subscriberUpdate.StartupsWeekly;
            subscriber.Podcasts = subscriberUpdate.Podcasts;

            _context.Subscribers.Update(subscriber);
            await _context.SaveChangesAsync();
            return new OkObjectResult(subscriber);
        }
    }
}
