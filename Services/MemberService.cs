using Microsoft.EntityFrameworkCore;
using Data;
using AutoMapper;


public class MemberService : IMemberService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly Lazy<ISubscriptionService> subscriptionService;
    private readonly IServiceProvider _serviceProvider;
    public MemberService(AppDbContext context, IMapper mapper, IServiceProvider serviceProvider)
    {
        _context = context;
        _mapper = mapper;
        _serviceProvider = serviceProvider;
    }
    public ISubscriptionService GetSubscriptionService()
    {
        return _serviceProvider.GetRequiredService<ISubscriptionService>();
    }

    public async Task<GetMemberResponse> CreateAsync(MemberDomain memberDomain)
    {
        if (memberDomain == null) return new GetMemberResponse { IsSuccess = false, Message = "Member data is null." };
        try {
            var member = new Member { 
                UserId = memberDomain.UserId
            };
            _context.Member.Add(member);
            await _context.SaveChangesAsync();
            var memberDto = _mapper.Map<Member, MemberDto>(member);
            return new GetMemberResponse { IsSuccess = true, Message = "Member signed up.", Member = memberDto };
        } catch (Exception e) {
            return new GetMemberResponse { IsSuccess = false, Message = $"Error --> {e.Message}" };
        }
    }

    public async Task<GetMembersResponse> GetAllAsync()
    {
        try {
            var members = await _context.Member
                .Include(m => m.User)
                .ToListAsync();
            var membersDto = _mapper.Map<List<Member>, List<MemberDto>>(members);
            return new GetMembersResponse { IsSuccess = true, Message = "Members read.", Members = membersDto };
        } catch (Exception e) {
            return new GetMembersResponse { IsSuccess = false, Message = $"Error --> {e.Message}" };
        }
    }

    public async Task<GetMemberResponse> GetByIdAsync(int id)
    {
        try {
            var member = await _context.Member
                .Include(m => m.User)
                .FirstOrDefaultAsync(member => member.Id == id);
            if (member == null) return new GetMemberResponse { IsSuccess = false, Message = "No member associated with given userId." };
            var memberDto = _mapper.Map<Member, MemberDto>(member);
            return new GetMemberResponse { IsSuccess = true, Message = "Member read.", Member = memberDto };
        } catch (Exception e) {
            return new GetMemberResponse { IsSuccess = false, Message = $"Error --> {e.Message}" };
        }
    }

    public async Task<GetMemberResponse> GetByUserIdAsync(string id)
    {
        try {
            var member = await _context.Member
                .Include(e => e.User)
                .FirstOrDefaultAsync(member => member.UserId == id);
            if (member == null) return new GetMemberResponse { IsSuccess = false, Message = "No member associated with given UserId." };
            var memberDto = _mapper.Map<Member, MemberDto>(member);
            return new GetMemberResponse { IsSuccess = true, Message = "member read.", Member = memberDto};
        } catch (Exception e) {
            return new GetMemberResponse { IsSuccess = false, Message = $"Exception --> {e.Message}" };
        }
    }

    public async Task<GetMembersResponse> GetAllByMembershipIdAsync(int membershipId)
    {
        var _subscriptionService = GetSubscriptionService();
        try {
            var subscriptionsResponse = await _subscriptionService.GetAllByMembershipId(membershipId);
            var membersDto = new List<MemberDto>();
            foreach (var subscription in subscriptionsResponse.Subscriptions) {
                var memberResponse = await GetByIdAsync((int)subscription.MemberId);
                membersDto.Add(memberResponse.Member);
            }
            return new GetMembersResponse { IsSuccess = true, Message = $"All members read that are subscribers to given membershipId.", Members = membersDto };
        } catch (Exception e) {
            return new GetMembersResponse { IsSuccess = false, Message = $"Exception --> {e.Message}" };
        }
    }

    public async Task<ResponseBase> UpdateAsync(UserDomain userDomain, string id)
    {
        try {
            var member = await _context.Member.FirstOrDefaultAsync(member => member.UserId == id);
            await _context.SaveChangesAsync();
            return new ResponseBase { IsSuccess = false, Message = $"Member updated." };
        } catch (Exception e) {
            return new ResponseBase { IsSuccess = false, Message = $"Error --> {e.Message}" };
        }
    }

    public async Task<ResponseBase> DeleteAsync(string id)
    {
        try {
            var member = await _context.Member.FirstOrDefaultAsync(m => m.UserId == id);
            _context.Remove(member);
            await _context.SaveChangesAsync();
            return new ResponseBase { IsSuccess = true, Message = $"Member deleted." };
        } catch (Exception e) {
            return new ResponseBase { IsSuccess = false, Message = $"Error --> {e.Message}" };
        }
    }

}
