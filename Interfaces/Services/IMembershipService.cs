using Data;

public interface IMembershipService
{
    Task<string> CreateAsync(MembershipDto membershipDto);

    Task<List<Membership>> GetAllAsync();

    Task<Membership> GetByIdAsync(int id);

    Task<string> UpdateAsync(MembershipDto membershipDto, int id);

    Task<string> DeleteAsync(int id);
}