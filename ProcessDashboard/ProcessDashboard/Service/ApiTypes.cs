#region
using System;
using System.Net.Http;
using Fusillade;
using ModernHttpClient;
using Newtonsoft.Json;
using ProcessDashboard.Service.Interface;
using Refit;
#endregion
namespace ProcessDashboard.Service
{
    /*
     * ** DO NOT CHANGE **
     * Name: ApiTypes.cs
     * 
     * Purpose: This class is a concerete implementation for IApiTypes interface.
     * 
     * Description:
     * 
     * Fusilade provides three priority types: User Intiated, Background and Sepculative.
     * This class provides three objects for these which can be used while making API requests.
     * It also uses ModernHttpClient inorder to use native Http optimizations for Network calls. 
     *
    */

    public class ApiTypes : IApiTypes
    {
        // For any network requests that has to be done on the background
        private readonly Lazy<IPDashApi> _background;

        // Speculatively make requests by predicting what the user might need. This has the lowest priority.
        // There is a 5 mb limit on how much can be speculatively retrieved.
        private readonly Lazy<IPDashApi> _speculative;

        // Any network request that is because the user initiated it. This has the highest priority
        private readonly Lazy<IPDashApi> _userInitiated;
        //TODO: Change this to the APIBaseAddress stored in settings file.
		public string ApiBaseAddress = AccountStorage.BaseUrl;

        public ApiTypes(string apiBaseAddress = null)
        {
            Func<HttpMessageHandler, IPDashApi> createClient = messageHandler =>
            {
                var client = new HttpClient(messageHandler)
                {
                    BaseAddress = new Uri(apiBaseAddress ?? ApiBaseAddress)
                };

                // Return a concrete implementation of IPDashAPi
                return RestService.For<IPDashApi>(client

                , new RefitSettings
                {
                    JsonSerializerSettings = new JsonSerializerSettings
                    {
                        
                        DateTimeZoneHandling = DateTimeZoneHandling.Local

                    }
                });
                
            };

            // Use Native Message Handler to make use of ModernHttpClient

            _background = new Lazy<IPDashApi>(() => createClient(
                new RateLimitedHttpMessageHandler(new NativeMessageHandler(), Priority.Background)));

            _userInitiated = new Lazy<IPDashApi>(() => createClient(
                new RateLimitedHttpMessageHandler(new NativeMessageHandler(), Priority.UserInitiated)));

            _speculative = new Lazy<IPDashApi>(() => createClient(
                new RateLimitedHttpMessageHandler(new NativeMessageHandler(), Priority.Speculative)));
        }

        public IPDashApi Background => _background.Value;

        public IPDashApi UserInitiated => _userInitiated.Value;

        public IPDashApi Speculative => _speculative.Value;

        public IPDashApi GetApi(Priority priority)
        {
            switch (priority)
            {
                case Priority.Background:
                    return Background;
                case Priority.UserInitiated:
                    return UserInitiated;
                case Priority.Speculative:
                    return Speculative;
                case Priority.Explicit:
                    return UserInitiated;
                default:
                    return UserInitiated;
            }
        }
    }
}