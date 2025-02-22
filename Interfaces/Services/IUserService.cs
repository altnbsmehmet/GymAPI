using Data;

public interface IUserService
{
    Task<ApplicationUser> GetCurrentUserAsync();

    Task<Object> SignInAsync(SignInDto signInInfo);

    Task<string> SignUpAsync(UserDto signUpData);

    Task SignOutAsync();

    Task<List<ApplicationUser>> GetAllAsync();

    Task<ApplicationUser> GetByIdAsync(string userId);

    Task<string> UpdateAsync(UserDto userInfo, string id);

    Task<string> DeleteAsnyc(string id);
}