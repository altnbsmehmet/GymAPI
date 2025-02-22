using Data;

public interface IMemberService
{
    Task<string> CreateAsync(MemberDomain memberDomain);

    Task<List<Member>> GetAllAsync();

    Task<Member> GetByIdAsync(int id);

    Task<string> UpdateAsync(UserDomain userDomain, string id);

    Task<string> DeleteAsync(string id);
}