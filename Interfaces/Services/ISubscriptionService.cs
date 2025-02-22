using Data;

public interface ISubscriptionService
{
    Task<string> CreateAsync(SubscriptionDto subscriptionDto);

    Task<List<Subscription>> GetAllAsync();

    Task<Subscription> GetByIdAsync(int id);

    Task<string> UpdateAsync(int id, SubscriptionDto subscriptionInfo);

    Task<string> DeleteAsync(int id);
}