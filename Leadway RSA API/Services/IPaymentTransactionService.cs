using Leadway_RSA_API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Leadway_RSA_API.Services
{
    public interface IPaymentTransactionService
    {
        Task<PaymentTransaction?> AddPaymentTransactionAsync(int applicantId, PaymentTransaction paymentTransaction);
        Task<List<PaymentTransaction>> GetPaymentTransactionByApplicantIdAsync(int applicantId);
        Task<PaymentTransaction?> GetPaymentTransactionAsync(int id);
        Task<PaymentTransaction?> UpdatePaymentTransactionAsync(int applicantId, int id, PaymentTransaction paymentTransaction);
        Task<bool> DeletePaymentTransactionAsync(int applicantId, int id);
    }
}
