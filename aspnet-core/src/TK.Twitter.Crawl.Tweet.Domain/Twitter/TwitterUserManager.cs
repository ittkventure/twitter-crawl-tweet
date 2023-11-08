using System.Threading.Tasks;
using TK.Twitter.Crawl.Entity;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace TK.Twitter.Crawl
{
    public class TwitterUserManager : DomainService
    {
        private readonly IRepository<TwitterUserEntity, long> _repository;

        public TwitterUserManager(IRepository<TwitterUserEntity, long> repository)
        {
            _repository = repository;
        }

        public async Task AddOrUpdateUserAsync(TwitterUserEntity dto, bool autoSave = false)
        {
            var user = await _repository.FirstOrDefaultAsync(x => x.UserId == dto.UserId);
            if (user != null)
            {
                user.UserId = dto.UserId;
                user.Name = dto.Name;
                user.ScreenName = dto.ScreenName;
                user.Description = dto.Description;
                user.ProfileImageUrl = dto.ProfileImageUrl;
                await _repository.UpdateAsync(user);
                return;
            }

            await _repository.InsertAsync(dto, autoSave);
        }

    }
}
