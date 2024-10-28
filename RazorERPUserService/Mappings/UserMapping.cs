using AutoMapper;
using RazorERPUserService.DTOs;
using RazorERPUserService.Models;

namespace RazorERPUserService.Mappings
{
    public class UserMapping : Profile
    {
        public UserMapping()
        {
            CreateMap<User, UserDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserID))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => "***"));
            CreateMap<UserDTO, User>()
                .ForMember(dest => dest.UserID, opt => opt.MapFrom(src => src.Id));
        }

    }
}
