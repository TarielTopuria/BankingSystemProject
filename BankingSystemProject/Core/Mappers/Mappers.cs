using AutoMapper;
using BankingSystemProject.Core.DTOs;
using BankingSystemProject.Data.Tables;

namespace BankingSystemProject.Core.Mappers
{
    public class Mappers : Profile
    {
        public Mappers()
        {
            CreateMap<CardCreateDTO, Card>();
            CreateMap<BankAccountCreateDTO, BankAccount>();
        }
    }
}
