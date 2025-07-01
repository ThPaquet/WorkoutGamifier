using AutoMapper;
using WorkoutGamifier.Application.DTOs;
using WorkoutGamifier.Domain.Entities;

namespace WorkoutGamifier.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Workout mappings
        CreateMap<Workout, WorkoutDto>();
        CreateMap<CreateWorkoutDto, Workout>();
        CreateMap<UpdateWorkoutDto, Workout>();

        // Session mappings
        CreateMap<Session, SessionDto>();
        CreateMap<CreateSessionDto, Session>();
        CreateMap<UpdateSessionDto, Session>();

        // WorkoutPool mappings
        CreateMap<WorkoutPool, WorkoutPoolDto>()
            .ForMember(dest => dest.Workouts, opt => opt.MapFrom(src => 
                src.WorkoutPoolWorkouts.Select(wpw => wpw.Workout)));
        CreateMap<WorkoutPool, WorkoutPoolSummaryDto>()
            .ForMember(dest => dest.WorkoutCount, opt => opt.MapFrom(src => 
                src.WorkoutPoolWorkouts.Count));
        CreateMap<CreateWorkoutPoolDto, WorkoutPool>();
        CreateMap<UpdateWorkoutPoolDto, WorkoutPool>();

        // UserAction mappings
        CreateMap<UserAction, UserActionDto>();
        CreateMap<CreateUserActionDto, UserAction>();
        CreateMap<UpdateUserActionDto, UserAction>();

        // User mappings
        CreateMap<User, UserDto>();
        CreateMap<CreateUserDto, User>();
        CreateMap<UpdateUserDto, User>();

        // SessionAction mappings
        CreateMap<SessionAction, SessionActionDto>();

        // SessionWorkout mappings
        CreateMap<SessionWorkout, SessionWorkoutDto>();
    }
}