using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Steemix.Library.HttpClient;
using Steemix.Library.Models.Requests;
using Steemix.Library.Models.Responses;

namespace Steemstagram.Shared
{
    public class Manager
    {
        private readonly SteemixApiClient ApiClient;

        public Manager()
        {
            ApiClient = new SteemixApiClient();
        }

        public Task<List<UserPost>> GetTopPosts(string offset, int limit)
        {
            return Task<List<UserPost>>.Factory.StartNew(() =>
            {
                try
                {
                    var request = new TopPostRequest(offset, limit);
                    var response = ApiClient.GetTopPosts(request);
                    return response.Results;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return null;
                }
            });
        }

        public Task<RegisterResponse> Register(RegisterRequest request)
        {
            return Task<RegisterResponse>.Factory.StartNew(() =>
            {
                try
                {
                    return ApiClient.Register(request);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return null;
                }
            });
        }

        public Task<LoginResponse> Login(LoginRequest request)
        {
            return Task<LoginResponse>.Factory.StartNew(() =>
            {
                try
                {
                    return ApiClient.Login(request);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return null;
                }
            });
        }

        public Task<UserInfoResponse> GetUserInfo(UserInfoRequest request)
        {
            return Task<UserInfoResponse>.Factory.StartNew(() =>
            {
                try
                {
                    return ApiClient.GetUserInfo(request);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return null;
                }
            });
        }

        public Task<VoteResponse> Vote(VoteRequest request, int liked)
        {
            return Task<VoteResponse>.Factory.StartNew(() =>
            {
                try
                {
                    if (liked == 1)
                        return ApiClient.DownVote(request);
                    else
                        return ApiClient.UpVote(request);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return null;
                }
            });
        }
    }
}
