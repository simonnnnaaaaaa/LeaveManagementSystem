using AutoMapper;
using LeaveManagementSystem.Web.Data;
using LeaveManagementSystem.Web.Models.LeaveTypes;

namespace LeaveManagementSystem.Web.MappingProfiles
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<LeaveType, IndexVM>()
                .ForMember(dest => dest.Days, opt =>opt.MapFrom(src => src.NumberOfDays));
        }
    }
}
