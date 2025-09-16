using Leadway_RSA_API.Data;
using Leadway_RSA_API.Models;
using Leadway_RSA_API.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Leadway_RSA_API.Services
{
    public class PaymentTransactionService : IPaymentTransactionService
    {
        private readonly ApplicationDbContext _context;
        public PaymentTransactionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaymentTransaction?> AddPaymentTransactionAsync(int applicantId, CreatePaymentTransactionDto paymentTransactionDto)
        {
            var applicant = await _context.Applicants.FindAsync(applicantId);
            if (applicant == null)
            {
                return null;
            }

            var paymentTransaction = new PaymentTransaction
            {
                ApplicantId = applicantId,
                Amount = paymentTransactionDto.Amount,
                Currency = paymentTransactionDto.Currency,
                Status = paymentTransactionDto.Status,
                GatewayReferenceId = paymentTransactionDto.GatewayReferenceId,
                PaymentMethod = paymentTransactionDto.PaymentMethod,
                //Message = paymentTransactionDto.Message,
                TransactionDate = DateTime.UtcNow // Set on the server
            };

            _context.PaymentTransactions.Add(paymentTransaction);
            await _context.SaveChangesAsync();

            applicant.LastModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return paymentTransaction;
        }

        public async Task<List<PaymentTransaction>> GetPaymentTransactionByApplicantIdAsync(int applicantId)
        {
            var transactions = await _context.PaymentTransactions
                .Where(pt => pt.ApplicantId == applicantId)
                .ToListAsync();

            // Do not clean up circular reference here. Let the controller handle mapping to DTO.
            return transactions;
        }

        public async Task<PaymentTransaction?> GetPaymentTransactionAsync(int id)
        {
            // The include is fine here; the controller will map to the DTO
            var transaction = await _context.PaymentTransactions
                .Include(pt => pt.Applicant)
                .FirstOrDefaultAsync(pt => pt.Id == id);

            return transaction;
        }

        public async Task<PaymentTransaction?> UpdatePaymentTransactionAsync(int applicantId, int id, UpdatePaymentTransactionDto paymentTransactionDto)
        {
            var existingPaymentTransaction = await _context.PaymentTransactions
                                                           .FirstOrDefaultAsync(p => p.Id == id && p.ApplicantId == applicantId);

            if (existingPaymentTransaction == null)
            {
                return null;
            }

            // Update only the properties that are allowed to be updated by the DTO
            existingPaymentTransaction.Status = paymentTransactionDto.Status;
            existingPaymentTransaction.GatewayReferenceId = paymentTransactionDto.GatewayReferenceId;
            existingPaymentTransaction.PaymentMethod = paymentTransactionDto.PaymentMethod;
            existingPaymentTransaction.Message = paymentTransactionDto.Message;

            try
            {
                await _context.SaveChangesAsync();
                var applicant = await _context.Applicants.FindAsync(applicantId);
                if (applicant != null)
                {
                    applicant.LastModifiedDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
                return existingPaymentTransaction;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PaymentTransactionExists(id, applicantId))
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Updates a payment transaction's status. This is a dedicated method for simple status updates,
        /// such as from 'Pending' to 'Success' after a gateway callback or OTP verification.
        /// </summary>
        public async Task<PaymentTransaction?> UpdateTransactionStatusAsync(int applicantId, int id, string newStatus, string? gatewayReferenceId = null)
        {
            var existingTransaction = await _context.PaymentTransactions
                                                    .FirstOrDefaultAsync(p => p.Id == id && p.ApplicantId == applicantId);

            if (existingTransaction == null)
            {
                return null;
            }

            existingTransaction.Status = newStatus;
            existingTransaction.GatewayReferenceId = gatewayReferenceId; // Update reference ID if provided
            existingTransaction.LastModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return existingTransaction;
        }

        public async Task<bool> DeletePaymentTransactionAsync(int applicantId, int id)
        {
            var paymentTransaction = await _context.PaymentTransactions
                                                   .FirstOrDefaultAsync(p => p.Id == id && p.ApplicantId == applicantId);

            if (paymentTransaction == null)
            {
                return false;
            }

            _context.PaymentTransactions.Remove(paymentTransaction);
            await _context.SaveChangesAsync();

            var applicant = await _context.Applicants.FindAsync(applicantId);
            if (applicant != null)
            {
                applicant.LastModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return true;
        }

        private bool PaymentTransactionExists(int id, int applicantId)
        {
            return _context.PaymentTransactions.Any(e => e.Id == id && e.ApplicantId == applicantId);
        }
    }
}
