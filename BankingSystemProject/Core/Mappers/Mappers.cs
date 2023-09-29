using AutoMapper;
using BankingSystemProject.Core.DTOs;
using BankingSystemProject.Data.Models;
using BankingSystemProject.Data.Tables;

namespace BankingSystemProject.Core.Mappers
{
    public class Mappers : Profile
    {
        public Mappers()
        {
            CreateMap<RegisterUserDTO, User>();
            CreateMap<CardCreateDTO, Card>();
            CreateMap<BankAccountCreateDTO, BankAccount>();
            CreateMap<BankAccount, BankAccountResponseDTO>()
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.CurrencyCode.ToString()));
            CreateMap<Card, CardResponseDTO>();
        }
    }
}
