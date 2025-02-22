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

    public async Task<string> CreateAsync(MemberDomain memberDomain)
    {
        try {
        var member = new Member { 
            UserId = memberDomain.UserId
        };
        _context.Member.Add(member);
        await _context.SaveChangesAsync();
        return "Member signed up.";
        } catch (Exception e) {
            return $"Error --> {e.Message}";
        }
    }

    public async Task<List<Member>> GetAllAsync()
    {
        var members = await _context.Member
            .Include(m => m.User)
            .ToListAsync();
        return members;
    }

    public async Task<Member> GetByIdAsync(int id)
    {
        var member = await _context.Member
            .Include(m => m.User)
            .FirstOrDefaultAsync(member => member.Id == id);
        return member;
    }

    public async Task<string> UpdateAsync(UserDomain userDomain, string id)
    {
        try {
            var member = await _context.Member.FirstOrDefaultAsync(member => member.UserId == id);
            await _context.SaveChangesAsync();
            return $"Member successfully updated.";
        } catch (Exception e) {
            return $"Exception --> {e}";
        }
    }

    public async Task<string> DeleteAsync(string id)
    {
        var member = await _context.Member.FirstOrDefaultAsync(m => m.UserId == id);
        _context.Remove(member);
        await _context.SaveChangesAsync();
        return $"Member successfully deleted.";
    }

}
