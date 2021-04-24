using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using archivesystemDomain.Entities;
using archivesystemDomain.Services;
using archivesystemWebUI.Models;
using archivesystemWebUI.Models.DataLayers;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using archivesystemWebUI.Models.FolderModels;



namespace archivesystemWebUI.Infrastructures
{
    public class MappingProfile : Profile
    {
        private HttpContext Context => HttpContext.Current;
        private ApplicationUserManager UserManager => Context.GetOwinContext().GetUserManager<ApplicationUserManager>();

        public MappingProfile()
        {


            Mapper.CreateMap<EnrollViewModel, AppUser>()
                .ForMember(
                    dest => dest.Name,
                    opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}")
                )
                .ForMember(dest => dest.CreatedAt, opt => opt.UseValue(DateTime.Now))
                .ForMember(dest => dest.UpdatedAt, opt => opt.UseValue(DateTime.Now));


            Mapper.CreateMap<EnrollViewModel, UserUniqueProps>().ForMember(
                dest => dest.Name,
                opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}")
            );

            Mapper.CreateMap<UpdateUserVm, UserUniqueProps>().ForMember(
                dest => dest.Name,
                opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}")
            );
            Mapper.CreateMap<UpdateUserVm, AppUser>().ForMember(
                dest => dest.Name,
                opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}")
            );

            Mapper.CreateMap<AppUser, UserDataView>()
                .ForMember(dest => dest.Department, opt => opt.MapFrom(src => src.Department.Name))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("dd MMM, yy")))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt.ToString("dd MMM, yy")))
                .ForMember(dest => dest.Roles, opt =>
                    {
                        opt.PreCondition(src => !string.IsNullOrWhiteSpace(src.UserId));
                        opt.MapFrom(src => UserManager.GetRoles(src.UserId));
                    })
                .ForMember(dest => dest.Gender, opt =>
                {
                    opt.MapFrom(src => src.Gender == Gender.Male ? "Male": "Female");
                })
                .ForMember(dest => dest.Completed, opt =>
                {
                    opt.MapFrom(src => src.Completed ? "Completed" : "Incomplete");
                });

            Mapper.CreateMap<AppUser, UpdateUserVm>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Name.Split().First()))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Name.Split().Last()));
              
            Mapper.CreateMap<CreateAccessLevelViewModel, AccessLevel>()
                .ForMember(dest => dest.CreatedAt, opt => opt.UseValue<DateTime>(DateTime.Now))
                .ForMember(m => m.UpdatedAt, opt => opt.UseValue<DateTime>(DateTime.Now));

           
            Mapper.CreateMap<EnrollViewModel, AppUser>();
            Mapper.CreateMap<Department, DepartmentViewModel>().ReverseMap();
            Mapper.CreateMap<Faculty, FacultyViewModel>().ReverseMap();
            Mapper.CreateMap<CreateAccessLevelViewModel, AccessLevel>()
                .ForMember(dest => dest.CreatedAt, opt => opt.UseValue<DateTime>(DateTime.Now))
                .ForMember(m => m.UpdatedAt, opt => opt.UseValue<DateTime>(DateTime.Now));
            Mapper.CreateMap<FolderPageViewModel, Folder>().ReverseMap();

            Mapper.CreateMap<Department,Folder>()
                .ForMember(dest => dest.Id ,opt => opt.Ignore())
                .ForMember(dest => dest.DepartmentId ,opt => opt.MapFrom(src=> src.Id))
                .ForMember(dest => dest.UpdatedAt, opt => opt.UseValue(DateTime.Now));
            Mapper.CreateMap<Faculty, Folder>()
               .ForMember(dest => dest.Id, opt => opt.Ignore())
               .ForMember(dest => dest.UpdatedAt, opt => opt.UseValue(DateTime.Now))
               .ForMember(dest => dest.FacultyId, opt => opt.MapFrom(src=> src.Id));

            Mapper.CreateMap<EditFolderViewModel, Folder>()
               .ForMember(dest => dest.Id, opt => opt.Ignore())
               .ForMember(dest => dest.UpdatedAt, opt => opt.UseValue(DateTime.Now));

            Mapper.CreateMap<Folder, FolderPageViewModel>()
                .ForMember(x=> x.DirectChildren,opt=> opt.MapFrom(src=> src.Subfolders));

            Mapper.CreateMap<EditAccessLevelViewModel, AccessLevel>().ReverseMap();
        }
    }
}