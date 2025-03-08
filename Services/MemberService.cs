using Microsoft.EntityFrameworkCore;
using Data;
using AutoMapper;


public class MemberService : IMemberService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    public MemberService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
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
            return new GetMemberResponse { IsSuccess = true, Message = "Member signed up.", Member = member };
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
            return new GetMembersResponse { IsSuccess = true, Message = "Members read.", Members = members };
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
            return new GetMemberResponse { IsSuccess = true, Message = "Member read.", Member = member };
        } catch (Exception e) {
            return new GetMemberResponse { IsSuccess = false, Message = $"Error --> {e.Message}" };
        }
    }

    public async Task<GetMemberResponse> GetByUserIdAsync (string id)
    {
        try {
            var member = await _context.Member
                .Include(e => e.User)
                .FirstOrDefaultAsync(member => member.UserId == id);
            if (member == null) return new GetMemberResponse { IsSuccess = false, Message = "No member associated with given UserId." };
            return new GetMemberResponse { IsSuccess = true, Message = "member read.", Member = member};
        } catch (Exception e) {
            return new GetMemberResponse { IsSuccess = false, Message = $"Exception --> {e.Message}" };
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
