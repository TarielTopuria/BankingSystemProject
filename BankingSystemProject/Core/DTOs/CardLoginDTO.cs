using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace BankingSystemProject.Core.DTOs
{
    public class CardLoginDTO
    {
        public string CardNumber { get; set; }
        public string PIN { get; set; }
    }
}
