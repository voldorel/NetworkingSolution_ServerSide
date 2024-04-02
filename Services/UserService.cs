using GameServer.Models;
using GameServer.Repositories;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharedLibrary;
using SharedLibrary.Requests;

namespace Server.Services;

public class UserService
{
    private static int usingUserResources = 0;
    private static int usingUserMetaData = 0;
    private static int usingUserNickName = 0;
    private static int usingUserPrfileMetaData = 0;
    private readonly UserRepository _userRepository;
    public UserService(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    public async Task EarnResource(string userId,JToken content,Action<string> OnComplete)
    {
        if (Interlocked.Exchange(ref usingUserResources , 1) == 0)
        {
            try
            {
                var request = JsonConvert.DeserializeObject<EarnResourceRequest>((string)content); 
                await _userRepository.EarnResource(ObjectId.Parse(userId), request.ResourceType,request.Value, request.Description,OnComplete);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            Interlocked.Exchange(ref usingUserResources, 0);
        }
  
    }

    public async Task ChangeUserMetaData(string userId,JToken content)
    {
        if (Interlocked.Exchange(ref usingUserMetaData, 1) == 0)
        {
            try
            {
                Console.WriteLine("##### :  "+((string)content));
                var request = JsonConvert.DeserializeObject<ChangeUserMetaDataRequest>(((string)content)!);
                if (request != null) await _userRepository.ChangeUserMetaData(ObjectId.Parse(userId), request);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            Interlocked.Exchange(ref usingUserMetaData, 0);
        }
    }

    public async Task ChangeUserNickName(string userId,JObject content)
    {
        if (Interlocked.Exchange(ref usingUserPrfileMetaData, 1) == 0)
        {
            try
            {
                await _userRepository.ChangeUserNickName(ObjectId.Parse(userId),content["NickName"].ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            Interlocked.Exchange(ref usingUserPrfileMetaData, 0);
        }
    }
    public async Task ChangeAvatar(string userId,JObject content,Action<string> OnComplete)
    {
        if (Interlocked.Exchange(ref usingUserPrfileMetaData, 1) == 0)
        {
            try
            {
                await _userRepository.ChangeAvatar(ObjectId.Parse(userId),int.Parse(content["AvatarId"].ToString()),OnComplete);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            Interlocked.Exchange(ref usingUserPrfileMetaData, 0);
        }
    }
}