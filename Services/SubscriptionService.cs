using Microsoft.EntityFrameworkCore;
using Data;
using AutoMapper;
using System.Diagnostics;


public class SubscriptionService : ISubscriptionService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    public SubscriptionService(AppDbContext context, IMapper mapper, IUserService userService)
    {
        _context = context;
        _mapper = mapper;
        _userService = userService;
    }

    public async Task<ResponseBase> CreateAsync(SubscriptionDto subscriptionDto)
    {
        try {
            var subscriptionDomain = _mapper.Map<SubscriptionDto, SubscriptionDomain>(subscriptionDto);

            var subscription = new Subscription {
                MemberId = subscriptionDomain.MemberId,
                MembershipId = subscriptionDomain.MembershipId,
                StartDate = DateTime.SpecifyKind(subscriptionDomain.StartDate, DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(subscriptionDomain.EndDate, DateTimeKind.Utc),
                Status = subscriptionDomain.Status
            };
            await _context.Subscription.AddAsync(subscription);
            await _context.SaveChangesAsync();
            return new ResponseBase { IsSuccess = true, Message = $"Subscription with start date {subscription.StartDate} to end date {subscription.EndDate} created." };
        } catch (Exception e) {
            return new ResponseBase { IsSuccess = false, Message = $"Error --> {e.InnerException?.Message ?? e.Message}" };
        }
    }

    public async Task<GetSubscriptionsResponse> GetAllByMemberIdAsync()
    {
        try {
            var user = await _userService.GetCurrentUserAsync();

            if (user.User.Role != "Member") return new GetSubscriptionsResponse { IsSuccess = false, Message = $"User is not a member, thus can't have any subscription." };

            var subscriptions = await _context.Subscription
            .Where(s => s.MemberId == user.Member.Id)
            .Include(s => s.Membership)
            .ToListAsync();
            if (subscriptions == null || subscriptions.Count == 0) return new GetSubscriptionsResponse { IsSuccess = false, Message = "No subscriptions found with given MemberId." };
            
            return new GetSubscriptionsResponse { IsSuccess = true, Message = "Subscriptions read associated with given MemberId.", Subscriptions = subscriptions };
        } catch (Exception e) {
            return new GetSubscriptionsResponse { IsSuccess = false, Message = $"Error --> {e.InnerException?.Message ?? e.Message}" };
        }
    }

    public async Task<GetSubscriptionResponse> GetByIdAsync(int id)
    {
        try {
            var subscription = await _context.Subscription.FirstOrDefaultAsync(s => s.Id == id);
            return new GetSubscriptionResponse { IsSuccess = true, Message = "", Subscription = subscription };
        } catch (Exception e) {
            return new GetSubscriptionResponse { IsSuccess = false, Message = $"Error --> {e.InnerException?.Message ?? e.Message}" };
        }
    }

    public async Task<ResponseBase> UpdateAsync(int id, SubscriptionDto subscriptionDto)
    {
        try {
            var subscriptionDomain = _mapper.Map<SubscriptionDto, SubscriptionDomain>(subscriptionDto);
            return new ResponseBase { IsSuccess = false, Message = "" };
        } catch (Exception e) {
            return new ResponseBase { IsSuccess = false, Message = $"Error --> {e.InnerException?.Message ?? e.Message}" };
        }
    }

    public async Task<ResponseBase> DeleteAsync(int id)
    {
        try {
            var subscription = await _context.Subscription.FirstOrDefaultAsync(s => s.Id == id);
            if (subscription == null) return new ResponseBase { IsSuccess = false, Message = "Subscription not found." };
            _context.Remove(subscription);
            await _context.SaveChangesAsync();
            int affectedRows =  await _context.SaveChangesAsync();
            if (affectedRows == 0) return new ResponseBase { IsSuccess = false, Message = "Failed to cancel subscription." };
            return new ResponseBase { IsSuccess = false, Message = $"" };
        } catch (Exception e) {
            return new ResponseBase { IsSuccess = false, Message = $"Error --> {e.InnerException?.Message ?? e.Message}" };
        }
    }

}