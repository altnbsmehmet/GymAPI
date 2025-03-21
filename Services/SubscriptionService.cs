using Microsoft.EntityFrameworkCore;
using Data;
using AutoMapper;


public class SubscriptionService : ISubscriptionService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IServiceProvider _serviceProvider;
    public SubscriptionService(AppDbContext context, IMapper mapper, IServiceProvider serviceProvider)
    {
        _context = context;
        _mapper = mapper;
        _serviceProvider = serviceProvider;
    }
    public IUserService GetUserService()
    {
        return _serviceProvider.GetRequiredService<IUserService>();
    }

    public async Task<ResponseBase> CreateAsync(SubscriptionDto subscriptionDto)
    {
        var _userService = GetUserService();
        var userResponse = await _userService.GetCurrentUserAsync();

        var subscriptionDomain = _mapper.Map<SubscriptionDto, SubscriptionDomain>(subscriptionDto);

        var subscription = new Subscription {
            MemberId = (int)userResponse.Member.Id,
            MembershipId = subscriptionDomain.MembershipId,
            StartDate = DateTime.SpecifyKind(subscriptionDomain.StartDate, DateTimeKind.Utc),
            EndDate = DateTime.SpecifyKind(subscriptionDomain.EndDate, DateTimeKind.Utc),
            Status = subscriptionDomain.Status
        };
        await _context.Subscription.AddAsync(subscription);
        await _context.SaveChangesAsync();
        return new ResponseBase { IsSuccess = true, Message = $"Subscription with start date {subscription.StartDate} to end date {subscription.EndDate} created." };
    }

    public async Task<GetSubscriptionsResponse> GetAllByMemberIdAsync()
    {
        var _userService = GetUserService();
        var user = await _userService.GetCurrentUserAsync();

        if (user.User.Role != "Member") return new GetSubscriptionsResponse { IsSuccess = false, Message = $"User is not a member, thus can't have any subscription." };

        var subscriptions = await _context.Subscription
            .Where(s => s.MemberId == user.Member.Id)
            .Include(s => s.Membership)
            .ToListAsync();
        if (subscriptions == null || subscriptions.Count == 0) return new GetSubscriptionsResponse { IsSuccess = false, Message = "No subscriptions found with given MemberId." };

        var subscriptionsDto = _mapper.Map<List<Subscription>, List<SubscriptionDto>>(subscriptions);
        return new GetSubscriptionsResponse { IsSuccess = true, Message = "Subscriptions read associated with given MemberId.", Subscriptions = subscriptionsDto };
    }

    public async Task<GetSubscriptionResponse> GetByIdAsync(int id)
    {
        var subscription = await _context.Subscription.FirstOrDefaultAsync(s => s.Id == id);
        var subscriptionDto = _mapper.Map<Subscription, SubscriptionDto>(subscription);
        return new GetSubscriptionResponse { IsSuccess = true, Message = "", Subscription = subscriptionDto };
    }

    public async Task<GetSubscriptionsResponse> GetAllByMembershipIdAsync(int membershipId)
    {
        var subscriptions = await _context.Subscription
            .Where(s => s.MembershipId == membershipId)
            .ToListAsync();
        var subscriptionsDto = _mapper.Map<List<Subscription>, List<SubscriptionDto>>(subscriptions);
        return new GetSubscriptionsResponse { IsSuccess = true, Message = "", Subscriptions = subscriptionsDto };
    }

    public async Task<ResponseBase> UpdateAsync(int id, SubscriptionDto subscriptionDto)
    {
        var subscriptionDomain = _mapper.Map<SubscriptionDto, SubscriptionDomain>(subscriptionDto);
        return new ResponseBase { IsSuccess = false, Message = "" };
    }

    public async Task<ResponseBase> DeleteAsync(int id)
    {
        var subscription = await _context.Subscription.FirstOrDefaultAsync(s => s.Id == id);
        if (subscription == null) return new ResponseBase { IsSuccess = false, Message = "Subscription not found." };
        _context.Remove(subscription);
        await _context.SaveChangesAsync();
        int affectedRows =  await _context.SaveChangesAsync();
        if (affectedRows == 0) return new ResponseBase { IsSuccess = false, Message = "Failed to cancel subscription." };
        return new ResponseBase { IsSuccess = false, Message = $"" };
    }

}