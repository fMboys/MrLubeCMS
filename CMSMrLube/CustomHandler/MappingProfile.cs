using AutoMapper;
using CMS.Core.DTOs;
using CMS.Core.Entities;

namespace MrLubeCMS.CustomHandler
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<bannerModel, banners>().ReverseMap();
            CreateMap<ShopTireDto, ShopTire>().ReverseMap();

        }
    }
}
