using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using TK.Twitter.Crawl.TwitterAPI.Dto.Login;
using TK.TwitterAccount.Domain;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace TK.Twitter.Crawl.TwitterAPI
{
    public class TwitterAPIAuthService : DomainService
    {
        private const string LOG_PREFIX = "[TwitterAuthService]";
        private readonly ITwitterAccountRepository _twitterAccountRepository;
        private readonly IRepository<TwitterCrawlAccountEntity, long> _twitterCrawlAccountRepository;

        public TwitterAPIAuthService(
            ITwitterAccountRepository twitterAccountRepository,
            IRepository<TwitterCrawlAccountEntity, long> twitterCrawlAccountRepository)
        {
            _twitterAccountRepository = twitterAccountRepository;
            _twitterCrawlAccountRepository = twitterCrawlAccountRepository;
        }

        public async Task<(string, string, IEnumerable<string>)> Login(string username, string password)
        {
            var cookies = new CookieContainer();
            var handler = new HttpClientHandler();
            handler.CookieContainer = cookies;

            //var proxy = new WebProxy
            //{
            //    Address = new Uri($"http://172.104.241.29:8081"),
            //    BypassProxyOnLocal = false,
            //    UseDefaultCredentials = false,
            //};
            //handler.Proxy = proxy;

            var httpClient = new HttpClient(handler, disposeHandler: true);
            httpClient.DefaultRequestHeaders.Add("authorization", "Bearer AAAAAAAAAAAAAAAAAAAAANRILgAAAAAAnNwIzUejRCOuH5E6I8xnZz4puTs%3D1Zv7ttfk8LF81IUq16cHjhLTvJu4FA33AGWWjCpTnA");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Linux; Android 11; Nokia G20) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.88 Mobile Safari/537.36");
            httpClient.DefaultRequestHeaders.Add("x-twitter-client-language", "en");

            string guestToken = string.Empty;

            Logger.LogDebug(LOG_PREFIX + "Start Activate");
            var activateResponse = await Activate(httpClient);

            guestToken = activateResponse?.GuestToken;
            if (guestToken.IsEmpty())
            {
                throw new BusinessException(CrawlDomainErrorCodes.TwitterGuestTokenError, "Can not get GuestToken");
            }

            Logger.LogDebug(LOG_PREFIX + "Activate succeeded");

            Logger.LogDebug(LOG_PREFIX + "Start Call FlowStart API");
            // Gọi API Flow start để lấy flow_token
            var flowResponse = await Flow(httpClient, guestToken, new
            {
                input_flow_data = new
                {
                    flow_context = new
                    {
                        debug_overrides = new { },
                        start_location = new
                        {
                            location = "splash_screen"
                        }
                    }
                },
                subtask_versions = new { }
            }, start: true);

            string flowToken = string.Empty;

            flowToken = flowResponse?.FlowToken;

            if (flowToken.IsEmpty())
            {
                throw new BusinessException(CrawlDomainErrorCodes.TwitterFlowTokenError, "Can not get FlowToken from API Flow Start");
            }

            Logger.LogDebug(LOG_PREFIX + "Get FlowToken from FlowStart succeeded");

            Logger.LogDebug(LOG_PREFIX + "Start Call FlowInstrumentation API");
            // Gọi API Flow Instrumentation để lấy flow_token
            flowResponse = await Flow(httpClient, guestToken, new
            {
                flow_token = flowToken,
                subtask_inputs = new List<object>{
                    new {
                         subtask_id= "LoginJsInstrumentationSubtask",
                         js_instrumentation=new {
                             response= "{}",
                             link= "next_link"
                         }
                    }
                }
            });

            flowToken = flowResponse?.FlowToken;

            if (flowToken.IsEmpty())
            {
                throw new BusinessException(CrawlDomainErrorCodes.TwitterFlowTokenError, "Can not get FlowToken from API Flow Instrumentation");
            }

            Logger.LogDebug(LOG_PREFIX + "Get FlowToken from FlowInstrumentation succeeded");

            Logger.LogDebug(LOG_PREFIX + "Start Call FlowUsername API");
            // Gọi API Flow Username để lấy flow_token
            flowResponse = await Flow(httpClient, guestToken, new
            {
                flow_token = flowToken,
                subtask_inputs = new List<object>
                {
                    new {
                        subtask_id = "LoginEnterUserIdentifierSSO",
                        settings_list = new {
                            setting_responses = new List<object>
                            {
                                new
                                {
                                    key = "user_identifier",
                                    response_data = new {
                                        text_data = new {
                                            result= username
                                        }
                                    }
                                }
                            },
                            link="next_link"
                        }
                    }
                }
            });

            flowToken = flowResponse?.FlowToken;

            if (flowToken.IsEmpty())
            {
                throw new BusinessException(CrawlDomainErrorCodes.TwitterFlowTokenError, "Can not get FlowToken from API Flow Username");
            }

            Logger.LogDebug(LOG_PREFIX + "Get FlowToken from FlowUsername succeeded");


            Logger.LogDebug(LOG_PREFIX + "Start Call FlowPassword API");
            flowResponse = await Flow(httpClient, guestToken, new
            {
                flow_token = flowToken,
                subtask_inputs = new List<object>
                {
                    new {
                        subtask_id = "LoginEnterPassword",
                        enter_password = new {
                            password= password,
                            link= "next_link"
                        }
                    }
                }
            });

            flowToken = flowResponse?.FlowToken;

            if (flowToken.IsEmpty())
            {
                throw new BusinessException(CrawlDomainErrorCodes.TwitterFlowTokenError, "Can not get FlowToken from API Flow Password");
            }

            Logger.LogDebug(LOG_PREFIX + "Get FlowToken from FlowPassword succeeded");


            Logger.LogDebug(LOG_PREFIX + "Start Call FlowDupplication API");
            var flowDupplicationResponse = await Flow(httpClient, guestToken, new
            {
                flow_token = flowToken,
                subtask_inputs = new List<object>
                {
                    new {
                        subtask_id = "AccountDuplicationCheck",
                        check_logged_in_account = new {
                            link= "AccountDuplicationCheck_false"
                        }
                    }
                }
            });

            IEnumerable<Cookie> responseCookies = cookies.GetCookies(new Uri("https://twitter.com")).Cast<Cookie>();

            var ct0_Cookie = responseCookies?.FirstOrDefault(x => x.Name.EqualsIgnoreCase("ct0"));

            Logger.LogDebug(LOG_PREFIX + "Get Cookie ct0 from FlowDupplication succeeded");

            return (ct0_Cookie?.Value, guestToken, responseCookies.Select(x => $"{x.Name}={x.Value}"));
        }

        public async Task<TwitterAPIActivateResponse> Activate(HttpClient httpClient)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.twitter.com/1.1/guest/activate.json");
            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new BusinessException(CrawlDomainErrorCodes.TwitterActivateError);
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonHelper.Parse<TwitterAPIActivateResponse>(content);
        }

        public async Task<TwitterAPIFlowResponse> Flow(HttpClient httpClient, string guestToken, object body, bool start = false)
        {
            string url = "https://api.twitter.com/1.1/onboarding/task.json";
            if (start)
            {
                url += "?flow_name=login";
            }
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("x-guest-token", guestToken);
            request.Content = new StringContent(JsonHelper.Stringify(body), new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"));
            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new BusinessException(CrawlDomainErrorCodes.TwitterFlowTokenError);
            }

            var content = await response.Content.ReadAsStringAsync();
            var r = JsonHelper.Parse<TwitterAPIFlowResponse>(content);
            return r;
        }

        public async Task<TwitterCrawlAccountEntity> CheckLogin(string crawlAccountId)
        {
            var crawlAccount = await _twitterAccountRepository.FirstOrDefaultAsync(x => x.AccountId == crawlAccountId);
            if (crawlAccount == null)
            {
                throw new BusinessException(CrawlDomainErrorCodes.NotFound, "Crawl account not found");
            }

            var (ct0_value, guestToken, cookie) = await Login(crawlAccount.Name, crawlAccount.Password);
            if (ct0_value.IsEmpty())
            {
                throw new BusinessException(CrawlDomainErrorCodes.InsideLogicError, "Login failed");
            }

            // Thực hiện lưu dữ liệu vào bảng account để có thể dùng ở chỗ khác
            var current = await _twitterCrawlAccountRepository.FirstOrDefaultAsync(x => x.AccountId == crawlAccountId);
            if (current == null)
            {
                current = await _twitterCrawlAccountRepository.InsertAsync(new TwitterCrawlAccountEntity()
                {
                    AccountId = crawlAccountId,
                    CookieCtZeroValue = ct0_value,
                    GuestToken = guestToken,
                    Cookie = cookie.JoinAsString(";")
                }, autoSave: true);
            }
            else
            {
                current.CookieCtZeroValue = ct0_value;
                current.GuestToken = guestToken;
                current.Cookie = cookie.JoinAsString(";");
                await _twitterCrawlAccountRepository.UpdateAsync(current, autoSave: true);
        }

            return current;
        }
    }
}
