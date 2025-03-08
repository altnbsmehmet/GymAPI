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

    public async Task<ResponseBase> CreateAsync(MembershipDto membershipDto)
    {
        try {
            var membershipDomain = _mapper.Map<MembershipDto, MembershipDomain>(membershipDto);

            var membership = new Membership {
                Type = membershipDomain.Type,
                Duration = membershipDomain.Duration,
                Price = membershipDomain.Price
            };
            await _context.Membership.AddAsync(membership);
            await _context.SaveChangesAsync();
            return new ResponseBase { IsSuccess = true, Message = $"Membership with type {membership.Type} with duration {membership.Duration} and with price {membership.Price} successfully created." };
        } catch (Exception e) {
            return new ResponseBase { IsSuccess = false, Message = $"Error --> {e.Message}" };
        }
    }

    public async Task<GetMembershipsResponse> GetAllAsync()
    {
        try {
            var memberships = await _context.Membership.ToListAsync();
            return new GetMembershipsResponse { IsSuccess = true, Message = "Memberships read.", Memberships = memberships };
        } catch (Exception e) {
            return new GetMembershipsResponse { IsSuccess = false, Message = $"Error --> {e.Message}" };
        }
    }

    public async Task<GetMembershipResponse> GetByIdAsync(int id)
    {
        try {
            var membership = await _context.Membership.FirstOrDefaultAsync(membership => membership.Id == id);
            return new GetMembershipResponse { IsSuccess = true, Message = "Membership read.", Membership = membership };
        } catch (Exception e) {
            return new GetMembershipResponse { IsSuccess = false, Message = $"Error --> {e.Message}" };
        }
    }

    public async Task<ResponseBase> UpdateAsync(MembershipDto membershipDto, int id)
    {
        try {
            var membershipDomain = _mapper.Map<MembershipDto, MembershipDomain>(membershipDto);

            var membership = await _context.Membership.FirstOrDefaultAsync(membership => membership.Id == id);
            membership.Type = membershipDomain.Type;
            membership.Duration = membershipDomain.Duration;
            membership.Price = membershipDomain.Price;
            await _context.SaveChangesAsync();
            return new ResponseBase { IsSuccess = true, Message = $"Membership successfully patched to: Type {membership.Type}, Duration {membership.Duration}, Price {membership.Price}" };
        } catch (Exception e) {
            return new ResponseBase { IsSuccess = false, Message = $"Exception --> {e}" };
        }
    }

    public async Task<ResponseBase> DeleteAsync(int id)
    {
        try {
            var membership = await _context.Membership.FirstOrDefaultAsync(membership => membership.Id == id);
            if (membership == null) return new ResponseBase { IsSuccess = false, Message = "Membership not found." };
            _context.Remove(membership);
            int affectedRows =  await _context.SaveChangesAsync();
            if (affectedRows == 0) return new ResponseBase { IsSuccess = false, Message = "Failed to delete membership." };
            return new ResponseBase { IsSuccess = true, Message = $"Membership type {membership.Type} with duration {membership.Duration} and with price {membership.Price} successfully deleted." };
        } catch (Exception e) {
            return new ResponseBase { IsSuccess = false, Message = $"Exception --> {e}" };
        }
    }

}
