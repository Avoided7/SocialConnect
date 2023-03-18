using AutoMapper;
using SocialConnect.Domain.Entities;
using SocialConnect.WebUI.ViewModels;

namespace SocialConnect.WebUI.Mapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<RegisterVM, User>();
        }
    }
}
