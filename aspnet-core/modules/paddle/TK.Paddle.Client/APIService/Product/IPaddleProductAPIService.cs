using TK.Paddle.Client.APIService.Product.Dto;
using TK.Paddle.Client.APIService.Product.Response;

namespace TK.Paddle.Client.APIService.Product
{
    public interface IPaddleProductAPIService
    {
        #region Pay Links

        Task<PaddleProductGeneratePayLinkReponse> GeneratePayLinkAsync(PaddleProductGeneratePayLinkInput input);

        #endregion

        #region Products

        Task<PaddleProductListProductsReponse> ListProductsAsync();

        #endregion
    }
}
