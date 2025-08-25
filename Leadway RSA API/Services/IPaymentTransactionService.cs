using Leadway_RSA_API.Models;
using Leadway_RSA_API.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Leadway_RSA_API.Services
{
    public interface IPaymentTransactionService
    {
        Task<PaymentTransaction?> AddPaymentTransactionAsync(int applicantId, CreatePaymentTransactionDto paymentTransactionDto);
        Task<List<PaymentTransaction>> GetPaymentTransactionByApplicantIdAsync(int applicantId);
        Task<PaymentTransaction?> GetPaymentTransactionAsync(int id);
        Task<PaymentTransaction?> UpdatePaymentTransactionAsync(int applicantId, int id, UpdatePaymentTransactionDto paymentTransactionDto);
        Task<bool> DeletePaymentTransactionAsync(int applicantId, int id);
    }
}
