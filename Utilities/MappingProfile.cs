using System.Diagnostics;
using AutoMapper;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<EmployeeDto, EmployeeDomain>();
        CreateMap<EmployeeDomain, EmployeeDto>();

        CreateMap<MemberDto, MemberDomain>();
        CreateMap<MemberDomain, MemberDto>();

        CreateMap<MembershipDto, MembershipDomain>();
        CreateMap<MembershipDomain, MembershipDto>();

        CreateMap<SignInDto, SignInDomain>();
        CreateMap<SignInDomain, SignInDto>();

        CreateMap<SubscriptionDto, SubscriptionDomain>();
        CreateMap<SubscriptionDomain, SubscriptionDto>();

        CreateMap<UserDto, UserDomain>();
        CreateMap<UserDomain, UserDto>();
    }
}
