using AutoMapper;
using ClupApi.Models;
using ClupApi.DTOs;

namespace ClupApi.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Club mappings
            //CreateMap<Club, ClubResponseDto>()
            //    .ForMember(dest => dest.MemberCount, opt => opt.MapFrom(src => src.ClubMemberships.Count(cm => cm.IsApproved == true)))
            //    .ForMember(dest => dest.ActivityCount, opt => opt.MapFrom(src => src.Activities.Count(a => a.IsActive)));

            //CreateMap<ClubCreateDto, Club>()
            //    .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            //    .ForMember(dest => dest.ClubID, opt => opt.Ignore())
            //    .ForMember(dest => dest.Activities, opt => opt.Ignore())
            //    .ForMember(dest => dest.Announcements, opt => opt.Ignore())
            //    .ForMember(dest => dest.ClubMemberships, opt => opt.Ignore());

            //CreateMap<ClubUpdateDto, Club>()
            //    .ForMember(dest => dest.ClubID, opt => opt.Ignore())
            //    .ForMember(dest => dest.ClubNumber, opt => opt.Ignore())
            //    .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            //    .ForMember(dest => dest.Activities, opt => opt.Ignore())
            //    .ForMember(dest => dest.Announcements, opt => opt.Ignore())
            //    .ForMember(dest => dest.ClubMemberships, opt => opt.Ignore());

            //// Student mappings
            //CreateMap<Student, StudentResponseDto>()
            //    .ForMember(dest => dest.ClubMembershipCount, opt => opt.MapFrom(src => src.ClubMemberships.Count(cm => cm.IsApproved == true)))
            //    .ForMember(dest => dest.ActivityParticipationCount, opt => opt.MapFrom(src => src.ActivityParticipations.Count()));

            //CreateMap<StudentCreateDto, Student>()
            //    .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            //    .ForMember(dest => dest.StudentID, opt => opt.Ignore())
            //    .ForMember(dest => dest.ActivityParticipations, opt => opt.Ignore())
            //    .ForMember(dest => dest.ClubMemberships, opt => opt.Ignore());

            //CreateMap<StudentUpdateDto, Student>()
            //    .ForMember(dest => dest.StudentID, opt => opt.Ignore())
            //    .ForMember(dest => dest.StudentNumber, opt => opt.Ignore())
            //    .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            //    .ForMember(dest => dest.ActivityParticipations, opt => opt.Ignore())
            //    .ForMember(dest => dest.ClubMemberships, opt => opt.Ignore());

            // Activity mappings
            CreateMap<Activity, ActivityResponseDto>()
                .ForMember(dest => dest.OrganizingClubName, opt => opt.MapFrom(src => src.OrganizingClub.ClubName));

            CreateMap<ActivityCreateDto, Activity>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.CreationDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.NumberOfParticipants, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.ActivityID, opt => opt.Ignore())
                .ForMember(dest => dest.OrganizingClub, opt => opt.Ignore())
                .ForMember(dest => dest.ActivityParticipations, opt => opt.Ignore());

            CreateMap<ActivityUpdateDto, Activity>()
                .ForMember(dest => dest.ActivityID, opt => opt.Ignore())
                .ForMember(dest => dest.OrganizingClubID, opt => opt.Ignore())
                .ForMember(dest => dest.NumberOfParticipants, opt => opt.Ignore())
                .ForMember(dest => dest.CreationDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.OrganizingClub, opt => opt.Ignore())
                .ForMember(dest => dest.ActivityParticipations, opt => opt.Ignore());

            // Announcement mappings
            CreateMap<Announcement, AnnouncementResponseDto>()
                .ForMember(dest => dest.ClubName, opt => opt.MapFrom(src => src.Club.ClubName));

            CreateMap<AnnouncementCreateDto, Announcement>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.CreationDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.AnnouncementID, opt => opt.Ignore())
                .ForMember(dest => dest.DeletionDate, opt => opt.Ignore())
                .ForMember(dest => dest.Club, opt => opt.Ignore());

            CreateMap<AnnouncementUpdateDto, Announcement>()
                .ForMember(dest => dest.AnnouncementID, opt => opt.Ignore())
                .ForMember(dest => dest.ClubID, opt => opt.Ignore())
                .ForMember(dest => dest.DeletionDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreationDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.Club, opt => opt.Ignore());

            // ClubMembership mappings
        //    CreateMap<ClubMembership, ClubMembershipResponseDto>()
        //        .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student.StudentName))
        //        .ForMember(dest => dest.StudentSurname, opt => opt.MapFrom(src => src.Student.StudentSurname))
        //        .ForMember(dest => dest.StudentNumber, opt => opt.MapFrom(src => src.Student.StudentNumber))
        //        .ForMember(dest => dest.ClubName, opt => opt.MapFrom(src => src.Club.ClubName));

        //    CreateMap<ClubMembershipCreateDto, ClubMembership>()
        //        .ForMember(dest => dest.JoinDate, opt => opt.MapFrom(src => DateTime.UtcNow))
        //        .ForMember(dest => dest.IsApproved, opt => opt.MapFrom(src => (bool?)null))
        //        .ForMember(dest => dest.MembershipID, opt => opt.Ignore())
        //        .ForMember(dest => dest.Club, opt => opt.Ignore())
        //        .ForMember(dest => dest.Student, opt => opt.Ignore());

        //    // ActivityParticipation mappings
        //    CreateMap<ActivityParticipation, ActivityParticipationResponseDto>()
        //        .ForMember(dest => dest.ActivityName, opt => opt.MapFrom(src => src.Activity.ActivityName))
        //        .ForMember(dest => dest.ActivityDate, opt => opt.MapFrom(src => src.Activity.StartDate))
        //        .ForMember(dest => dest.ClubName, opt => opt.MapFrom(src => src.Activity.OrganizingClub.ClubName))
        //        .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student.StudentName))
        //        .ForMember(dest => dest.StudentSurname, opt => opt.MapFrom(src => src.Student.StudentSurname))
        //        .ForMember(dest => dest.StudentNumber, opt => opt.MapFrom(src => src.Student.StudentNumber));

        //    CreateMap<ActivityParticipationCreateDto, ActivityParticipation>()
        //        .ForMember(dest => dest.JoinDate, opt => opt.MapFrom(src => DateTime.UtcNow))
        //        .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => (int?)null))
        //        .ForMember(dest => dest.ParticipationID, opt => opt.Ignore())
        //        .ForMember(dest => dest.Activity, opt => opt.Ignore())
        //        .ForMember(dest => dest.Student, opt => opt.Ignore());
        }
    }
}