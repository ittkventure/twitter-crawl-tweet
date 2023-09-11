using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity.Dapper;
using TK.Twitter.Crawl.Repository;
using TK.TwitterAccount.Domain.Entities;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace TK.Twitter.Crawl.ConsoleApp.Test
{

    public class TestInteractWithTableWithSQL : ITransientDependency
    {
        private readonly ITwitterCrawlRelationDapperRepository _twitterCrawlRelationDapperRepository;
        private readonly IRepository<TwitterAccountAPIEntity, long> _repository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IClock _clock;

        public TestInteractWithTableWithSQL(
            ITwitterCrawlRelationDapperRepository twitterCrawlRelationDapperRepository,
            IRepository<TwitterAccountAPIEntity, long> repository,
            IUnitOfWorkManager unitOfWorkManager,
            IClock clock)
        {
            _twitterCrawlRelationDapperRepository = twitterCrawlRelationDapperRepository;
            _repository = repository;
            _unitOfWorkManager = unitOfWorkManager;
            _clock = clock;
        }

        public async Task Test()
        {
            string batchKey = "Test" + _clock.Now.ToString("yyyyMMddHHmmss");
            using (var uow = _unitOfWorkManager.Begin())
            {
#pragma warning disable CS0168 // Variable is declared but never used
                try
                {
                    await _twitterCrawlRelationDapperRepository.CreateTable(batchKey);

                    await _twitterCrawlRelationDapperRepository.InsertAsync(batchKey, new TwitterTweetCrawlTweetDapperEntity()
                    {
                        UserId = "1000209338101780481",
                        FollowingUserId = "2387447834",
                        //FollowingUserCreatedAt = new System.DateTime(2023, 6, 1, 11, 22, 33),
                        FollowingUserCreatedAt = null,
                        FollowingUserDescription = "Description",
                        FollowingUserFastFollowersCount = 1,
                        FollowingUserFavouritesCount = 2,
                        FollowingUserFollowersCount = 3,
                        FollowingUserFriendsCount = 4,
                        FollowingUserListedCount = 5,
                        FollowingUserName = "Name",
                        FollowingUserNormalFollowersCount = 6,
                        FollowingUserProfileImageUrl = "ProfileImageUrl",
                        FollowingUserScreenName = "ScreenName",
                        FollowingUserStatusesCount = 7,
                        DiscoveredTime = new System.DateTime(2023, 6, 20, 3, 2, 1),
                        EntitiesAsJson = "",
                        ProfessionalAsJson = null
                    });

                    //await _twitterCrawlRelationDapperRepository.DropTable(batchKey);

                    var data = await _twitterCrawlRelationDapperRepository.GetByUserId(batchKey, "1000209338101780481");
                }
                catch (System.Exception ex)
                {
                    await uow.RollbackAsync();
                    await _twitterCrawlRelationDapperRepository.DropTable(batchKey);
                }
#pragma warning restore CS0168 // Variable is declared but never used

                await uow.SaveChangesAsync();
            }
        }
    }
}
