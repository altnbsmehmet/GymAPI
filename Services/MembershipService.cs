using Microsoft.EntityFrameworkCore;
using Data;
using AutoMapper;


public class MembershipService : IMembershipService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    public MembershipService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<string> CreateAsync(MembershipDto membershipDto)
    {
        var membershipDomain = _mapper.Map<MembershipDto, MembershipDomain>(membershipDto);

        var membership = new Membership {
            Type = membershipDomain.Type,
            Duration = membershipDomain.Duration,
            Price = membershipDomain.Price
        };
        await _context.Membership.AddAsync(membership);
        await _context.SaveChangesAsync();
        return $"Membership with type {membership.Type} with duration {membership.Duration} and with price {membership.Price} successfully created.";
    }

    public async Task<List<Membership>> GetAllAsync()
    {
        var memberships = await _context.Membership.ToListAsync();
        return memberships;
    }

    public async Task<Membership> GetByIdAsync(int id)
    {
        var membership = await _context.Membership.FirstOrDefaultAsync(membership => membership.Id == id);
        return membership;
    }

    public async Task<string> UpdateAsync(MembershipDto membershipDto, int id)
    {
        try {
            var membershipDomain = _mapper.Map<MembershipDto, MembershipDomain>(membershipDto);

            var membership = await _context.Membership.FirstOrDefaultAsync(membership => membership.Id == id);
            membership.Type = membershipDomain.Type;
            membership.Duration = membershipDomain.Duration;
            membership.Price = membershipDomain.Price;
            await _context.SaveChangesAsync();
            return $"Membership successfully patched to: Type {membership.Type}, Duration {membership.Duration}, Price {membership.Price}";
        } catch (Exception e) {
            return $"Exception --> {e}";
        }
    }

    public async Task<string> DeleteAsync(int id)
    {
        try {
        var membership = await _context.Membership.FirstOrDefaultAsync(membership => membership.Id == id);
        _context.Remove(membership);
        await _context.SaveChangesAsync();
        return $"Membership type {membership.Type} with duration {membership.Duration} and with price {membership.Price} successfully deleted.";
        } catch (Exception e) {
            return $"Exception --> {e}";
        }
    }

}
