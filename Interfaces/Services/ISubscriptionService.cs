using Data;

public interface ISubscriptionService
{
    Task<ResponseBase> CreateAsync(SubscriptionDto subscriptionDto);

    Task<GetSubscriptionsResponse> GetAllAsync();

    Task<GetSubscriptionResponse> GetByIdAsync(int id);

    Task<ResponseBase> UpdateAsync(int id, SubscriptionDto subscriptionInfo);

    Task<ResponseBase> DeleteAsync(int id);
}