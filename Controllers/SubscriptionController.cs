using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;


[ApiController]
[Route("api/subscription")]
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }
    
    [Authorize(Roles = "Member")]
    [HttpPost("create/{membershipId}")]
    public async Task<IActionResult> CreateSubscription([FromBody] SubscriptionDto subscriptionDto, int membershipId)
    {
        subscriptionDto.MembershipId = membershipId;
        var result =  await _subscriptionService.CreateAsync(subscriptionDto);
        return Ok(result);
    }

    [Authorize(Roles = "Member")]
    [HttpGet("getallbymemberid")]
    public async Task<IActionResult> GetAllSubscriptionsByMemberId()
    {
        var subscriptions = await _subscriptionService.GetAllByMemberIdAsync();
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

    [Authorize(Roles = "Member")]
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteSubscription(int id)
    {
        var message = await _subscriptionService.DeleteAsync(id);
        return Ok(message);
    }

}
