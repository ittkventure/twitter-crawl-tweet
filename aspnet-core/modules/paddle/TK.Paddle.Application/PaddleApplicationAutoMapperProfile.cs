using AutoMapper;
using TK.Paddle.Domain.Dto;
using TK.Paddle.Domain.Entity;

namespace TK.Paddle.Application
{
    public class PaddleApplicationAutoMapperProfile : Profile
    {
        public PaddleApplicationAutoMapperProfile()
        {
            /* You can configure your AutoMapper mapping configuration here.
             * Alternatively, you can split your mapping configurations
             * into multiple profile classes for a better organization. */

            CreateMap<PaddleWebhookSubscriptionPaymentSuccessInput, PaddleWebhookLogEntity>().ReverseMap();
        }
    }
}