using AutoMapper;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel.BaseIdentityUser;

namespace WebTrackED_CHED_MIMAROPA.Model.Service
{
    public class ModelMapper:Profile
    {
        public ModelMapper()
        {
            CreateMap<CategoryInputModel, Category>().ReverseMap();
            CreateMap<SubCategoryInputModel, SubCategory>().ReverseMap();
          
            CreateMap<SubCategoryInputModel, SubCategory>().ReverseMap();
          
            CreateMap<DesignationInputModel, Designation>().ReverseMap();
            CreateMap<CHEDPersonelInputModel, CHEDPersonel>().ReverseMap();
            CreateMap<SenderInputModel, Sender>().ReverseMap();
            CreateMap<DocumentAttachment, ComposeInputModel>().ReverseMap();
        
            CreateMap<Message, MessageInputModel>().ReverseMap();
            CreateMap<Settings, SettingsInputModel>().ReverseMap();
            CreateMap<Notification, NotificationInputModel>().ReverseMap();
        }
    }
}
