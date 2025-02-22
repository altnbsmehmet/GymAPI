using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("/api/subscription")]
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }
    
    [HttpPost("create")]
    public async Task<IActionResult> CreateSubscription([FromBody] SubscriptionDto subscriptionInfo)
    {
        var message =  await _subscriptionService.CreateAsync(subscriptionInfo);
        return Ok(message);
    }

    [HttpGet("getall")]
    public async Task<IActionResult> GetAllSubscriptions()
    {
        var subscriptions = await _subscriptionService.GetAllAsync();
        return Ok(subscriptions);
    }

    [HttpGet("getbyid/{id}")]
    public async Task<IActionResult> GetSubscriptionById(int id)
    {
        var subscription =  await _subscriptionService.GetByIdAsync(id);
        return Ok(subscription);
    }

    [HttpPatch("update/{id}")]
    public async Task<IActionResult> UpdateSubscription([FromBody] SubscriptionDto subscriptionInfo, int id)
    {
        var message = await _subscriptionService.UpdateAsync(id, subscriptionInfo);
        return Ok(message);
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteSubscription(int id)
    {
        var message = await _subscriptionService.DeleteAsync(id);
        return Ok(message);
    }

}
