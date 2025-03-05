using Data;

public interface IMembershipService
{
    Task<ResponseBase> CreateAsync(MembershipDto membershipDto);

    Task<GetMembershipsResponse> GetAllAsync();

    Task<GetMembershipResponse> GetByIdAsync(int id);

    Task<ResponseBase> UpdateAsync(MembershipDto membershipDto, int id);

    Task<ResponseBase> DeleteAsync(int id);
}