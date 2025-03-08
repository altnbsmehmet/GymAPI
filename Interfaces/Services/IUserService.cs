using Data;

public interface IUserService
{
    Task<GetUserResponse> GetCurrentUserAsync();
    Task<UserAuthorizationResponse> AuthorizeUserAsync();

    Task<SignInResponse> SignInAsync(SignInDto signInInfo);

    Task<ResponseBase> SignUpAsync(SignUpDto signUpData);

    Task<ResponseBase> SignOutAsync();

    Task<GetUsersResponse> GetAllAsync();

    Task<GetUserResponse> GetByIdAsync(string userId);

    Task<ResponseBase> UpdateAsync(SignUpDto userInfo, string id);

    Task<ResponseBase> DeleteAsnyc(string id);
}