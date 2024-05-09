using Data.Contexts;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NewsletterProvider.Models;
using Newtonsoft.Json;
using System.Text;
using System.Text.Json.Serialization;

namespace NewsletterProvider.Functions
{
    public class Subscribe(ILogger<Subscribe> logger, DataContext context)
    {
        private readonly ILogger<Subscribe> _logger = logger;
        private readonly DataContext _context = context;

        [Function("Subscribe")]
        public async Task <IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {

            string body = null!;

            try
            {
                body = await new StreamReader(req.Body).ReadToEndAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"StreamReader :: {ex.Message}");
            }

            if (body != null)
            {

                SubscribeRequest subscriber = null!;

                try
                {
                    subscriber = JsonConvert.DeserializeObject<SubscribeRequest>(body)!;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"JsonConvert.DeserializeObject<UserRegistrationRequest> :: {ex.Message}");
                }

                if (subscriber != null && !string.IsNullOrEmpty(subscriber.Email))
                {
                    if (!await _context.Subscribers.AnyAsync(x => x.Email == subscriber.Email)) 
                    {
                        var subscriberEntity = new SubscriberEntity
                        {
                            Email = subscriber.Email,
                            DailyNewsletter = subscriber.DailyNewsletter,
                            AdvertisingUpdates = subscriber.AdvertisingUpdates,
                            WeekInReview = subscriber.WeekInReview,
                            EventUpdates = subscriber.EventUpdates,
                            StartupsWeekly = subscriber.StartupsWeekly,
                            Podcasts = subscriber.Podcasts
                        };

                        try
                        {
                            using var http = new HttpClient();
                            StringContent content = new StringContent(JsonConvert.SerializeObject(new { Email = subscriberEntity.Email }), Encoding.UTF8, "application/json");
                            var response = await http.PostAsync("https://silicon-newsletterprovider.azurewebsites.net/api/subscribe", content);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"http.PostAsync :: {ex.Message}");
                        }

                        return new OkResult();
                    }
                    else
                    {
                        return new ConflictResult();
                    }
                }
            }

            return new BadRequestResult();

        }
    }
}
