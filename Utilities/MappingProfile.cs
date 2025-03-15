using Data;
using AutoMapper;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<SignInDto, SignInDomain>();
        CreateMap<SignInDomain, SignInDto>();

        CreateMap<SignUpDto, UserDomain>();
        CreateMap<UserDomain, SignUpDto>();

        CreateMap<EmployeeDto, EmployeeDomain>();
        CreateMap<EmployeeDomain, EmployeeDto>();

        CreateMap<MemberDto, MemberDomain>();
        CreateMap<MemberDomain, MemberDto>();

        CreateMap<SubscriptionDto, SubscriptionDomain>();
        CreateMap<SubscriptionDomain, SubscriptionDto>();

        CreateMap<MembershipDto, MembershipDomain>();
        CreateMap<MembershipDomain, MembershipDto>();

        CreateMap<ApplicationUser, UserDto>()
            .ForMember(dest => dest.ProfilePhoto, 
                opt => opt.MapFrom(src => src.ProfilePhotoPath != null 
                    ? $"{EnvironmentVariables.ApiDomainUrl}{src.ProfilePhotoPath}" 
                    : null));
        CreateMap<UserDto, ApplicationUser>()
            .ForMember(dest => dest.ProfilePhoto, 
                opt => opt.MapFrom(src => src.ProfilePhotoPath != null 
                    ? $"{EnvironmentVariables.ApiDomainUrl}{src.ProfilePhotoPath}" 
                    : null));

        CreateMap<Employee, EmployeeDto>();
        CreateMap<EmployeeDto, Employee>();

        CreateMap<Member, MemberDto>();
        CreateMap<MemberDto, Member>();

        CreateMap<Subscription, SubscriptionDto>();
        CreateMap<SubscriptionDto, Subscription>();

        CreateMap<Membership, MembershipDto>();
        CreateMap<MembershipDto, Membership>();
    }
}