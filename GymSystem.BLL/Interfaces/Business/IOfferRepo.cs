using GymSystem.BLL.Dtos.Offer;
using GymSystem.BLL.Errors;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymSystem.BLL.Interfaces.Business
{
    public interface IOfferRepo
    {
        Task<ApiResponse> AddOffer(OfferDto offerDto);
        Task<ApiResponse> UpdateOffer(int id, OfferDto offerDto);
        Task<ApiResponse> DeleteOffer(int id);
        Task<ApiResponse> GetOffer(int id);
        Task<ApiResponse> GetOffers();
    }
}