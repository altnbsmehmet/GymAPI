using Microsoft.EntityFrameworkCore;
using Data;
using AutoMapper;


public class SubscriptionService : ISubscriptionService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    public SubscriptionService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<string> CreateAsync(SubscriptionDto subscriptionDto)
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
            return $"Subscription with start date {subscription.StartDate} to end date {subscription.EndDate} created.";
        } catch (Exception e) {
            return $"Error --> {e.InnerException?.Message ?? e.Message}";
        }
    }

    public async Task<List<Subscription>> GetAllAsync()
    {
        var subscriptions = await _context.Subscription.ToListAsync();
        return subscriptions;
    }

    public async Task<Subscription> GetByIdAsync(int id)
    {
        var subscription = await _context.Subscription.FirstOrDefaultAsync(s => s.Id == id);
        return subscription;
    }

    public async Task<string> UpdateAsync(int id, SubscriptionDto subscriptionDto)
    {
        try {
            var subscriptionDomain = _mapper.Map<SubscriptionDto, SubscriptionDomain>(subscriptionDto);
            return "";
        } catch (Exception e) {
            return $"Error --> {e.Message}";
        }
    }

    public async Task<string> DeleteAsync(int id)
    {
        return "";
    }

}