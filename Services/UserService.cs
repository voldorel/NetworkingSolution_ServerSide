using GameServer.Models;
using GameServer.Repositories;
using SharedLibrary;

namespace Server.Services;

public class UserService
{
    private readonly UserRepository _userRepository;
    public UserService(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User> FindUserByToken(string loginToken)
    {
        return await _userRepository.GetAsyncByLoginToken(loginToken);
    }
}