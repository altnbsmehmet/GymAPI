using Microsoft.EntityFrameworkCore;
using Data;
using AutoMapper;
using System.Diagnostics;


public class SubscriptionService : ISubscriptionService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    public SubscriptionService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
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

    public async Task<GetSubscriptionsResponse> GetAllAsync()
    {
        try {
            var subscriptions = await _context.Subscription.ToListAsync();
            return new GetSubscriptionsResponse { IsSuccess = true, Message = "Subscriptions read.", Subscriptions = subscriptions };
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
            return new ResponseBase { IsSuccess = false, Message = $"" };
        } catch (Exception e) {
            return new ResponseBase { IsSuccess = false, Message = $"Error --> {e.InnerException?.Message ?? e.Message}" };
        }
    }

}