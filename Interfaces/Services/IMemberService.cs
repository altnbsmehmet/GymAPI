using Data;

public interface IMemberService
{
    Task<GetMemberResponse> CreateAsync(MemberDomain memberDomain);

    Task<GetMembersResponse> GetAllAsync();

    Task<GetMemberResponse> GetByIdAsync(int id);
    Task<GetMemberResponse> GetByUserIdAsync (string id);

    Task<ResponseBase> UpdateAsync(UserDomain userDomain, string id);

    Task<ResponseBase> DeleteAsync(string id);
}