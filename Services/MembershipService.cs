using Microsoft.EntityFrameworkCore;
using Data;
using AutoMapper;
using Newtonsoft.Json;


public class MembershipService : IMembershipService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IServiceProvider _serviceProvider;
    public MembershipService(AppDbContext context, IMapper mapper, IServiceProvider serviceProvider)
    {
        _context = context;
        _mapper = mapper;
        _serviceProvider = serviceProvider;
    }
    public IMemberService GetMemberService()
    {
        return _serviceProvider.GetRequiredService<IMemberService>();
    }

    public async Task<ResponseBase> CreateAsync(MembershipDto membershipDto)
    {
        var membershipDomain = _mapper.Map<MembershipDto, MembershipDomain>(membershipDto);

        var membership = new Membership {
            Type = membershipDomain.Type,
            Duration = membershipDomain.Duration,
            Price = membershipDomain.Price,
            IsActive = true
        };
        await _context.Membership.AddAsync(membership);
        await _context.SaveChangesAsync();
        return new ResponseBase { IsSuccess = true, Message = $"Membership with type {membership.Type} with duration {membership.Duration} and with price {membership.Price} created." };
    }

    public async Task<GetMembershipsResponse> GetAllAsync()
    {
        var _memberService = GetMemberService();
        var memberships = await _context.Membership.ToListAsync();
        var membershipsDto = _mapper.Map<List<Membership>, List<MembershipDto>>(memberships);
        foreach (var membershipDto in membershipsDto) {
            var subscribersResponse = await _memberService.GetAllByMembershipIdAsync((int)membershipDto.Id);
            membershipDto.Subscribers = subscribersResponse.Members;
        }
        return new GetMembershipsResponse { IsSuccess = true, Message = "Memberships read.", Memberships = membershipsDto };
    }

    public async Task<GetMembershipResponse> GetByIdAsync(int id)
    {
        var _memberService = GetMemberService();
        var membership = await _context.Membership.FirstOrDefaultAsync(membership => membership.Id == id);
        var membershipDto = _mapper.Map<Membership, MembershipDto>(membership);
        var subscribersResponse = await _memberService.GetAllByMembershipIdAsync(membership.Id);
        membershipDto.Subscribers = subscribersResponse.Members;
        return new GetMembershipResponse { IsSuccess = true, Message = "Membership read.", Membership = membershipDto };
    }

    public async Task<ResponseBase> ToggleActivationByIdAsync(int id)
    {
        var membership = await _context.Membership.FirstOrDefaultAsync(membership => membership.Id == id);
        if ((bool)membership.IsActive) {
            membership.IsActive = false;
            await _context.SaveChangesAsync();
            return new ResponseBase { IsSuccess = true, Message = $"Membership deactivated." };
        }
        membership.IsActive = true;
        await _context.SaveChangesAsync();
        return new ResponseBase { IsSuccess = true, Message = $"Membership activated." };
    }

    public async Task<ResponseBase> UpdateAsync(MembershipDto membershipDto, int id)
    {
        var membershipDomain = _mapper.Map<MembershipDto, MembershipDomain>(membershipDto);

        var membership = await _context.Membership.FirstOrDefaultAsync(membership => membership.Id == id);
        membership.Type = membershipDomain.Type;
        membership.Duration = membershipDomain.Duration;
        membership.Price = membershipDomain.Price;
        await _context.SaveChangesAsync();
        return new ResponseBase { IsSuccess = true, Message = $"Membership successfully patched to: Type {membership.Type}, Duration {membership.Duration}, Price {membership.Price}" };
    }

    public async Task<ResponseBase> DeleteAsync(int id)
    {
        var membership = await _context.Membership.FirstOrDefaultAsync(membership => membership.Id == id);
        if (membership == null) return new ResponseBase { IsSuccess = false, Message = "Membership not found." };
        _context.Remove(membership);
        int affectedRows =  await _context.SaveChangesAsync();
        if (affectedRows == 0) return new ResponseBase { IsSuccess = false, Message = "Failed to delete membership." };
        return new ResponseBase { IsSuccess = true, Message = $"Membership type {membership.Type} with duration {membership.Duration} and with price {membership.Price} successfully deleted." };
    }

}
