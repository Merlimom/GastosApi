using Core.DTOs;
using Core.Entities;
using Core.Requests;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Mappings;

public class UserMappingConfiguration : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateUserRequest, User>()
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.Password, src => src.Password)
            .Map(dest => dest.CreationDate, src => DateTime.UtcNow) // Asigna la fecha de creación actual
            .Map(dest => dest.UpdateDate, src => DateTime.UtcNow);


        config.NewConfig<User, UserDTO>()
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.CreationDate, src => src.CreationDate.ToShortDateString())
            .Map(dest => dest.UpdateDate, src => src.UpdateDate.ToShortDateString());
    }
}
