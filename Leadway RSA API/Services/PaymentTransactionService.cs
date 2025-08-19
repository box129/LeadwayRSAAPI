using Leadway_RSA_API.Data;
using Leadway_RSA_API.Models;
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

        public async Task<PaymentTransaction?> AddPaymentTransactionAsync(int applicantId, PaymentTransaction paymentTransaction)
        {
            // 2. Check if the Applicant exists
            var applicant = await _context.Applicants.FindAsync(applicantId);
            if (applicant == null)
            {
                return null;
                //return NotFound($"Applicant with ID {applicantId} not found."); // Returns a 404
            }

            // 3. Associate PaymentTransaction with Applicant
            paymentTransaction.ApplicantId = applicantId; // Ensure the foreign key is correctly set

            // The TransactionDate defaults to DateTime.UtcNow in the model. If you want to allow
            // the client to provide it, remove the default from the model and validate here.
            // For now, it will use the model's default if not provided by the client.

            // Important: Clear any nested navigation property for Applicant in the incoming object
            paymentTransaction.Applicant = null;

            _context.PaymentTransactions.Add(paymentTransaction);
            await _context.SaveChangesAsync();

            // Optional: Update the Applicant's LastModifiedDate as well
            applicant.LastModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync(); // Save the update to the applicant 

            return paymentTransaction;
        }

        public async Task<List<PaymentTransaction>> GetPaymentTransactionByApplicantIdAsync(int applicantId)
        {
            // Retrieve payment transactions for the given applicant.
            var transactions = await _context.PaymentTransactions
                .Where(pt => pt.ApplicantId == applicantId)
                .ToListAsync();

            // --- ADD THIS BLOCK ---
            // If lazy loading is enabled, the Applicant property will be populated.
            // We explicitly set it to null to prevent serialization cycles.
            foreach (var transaction in transactions)
            {
                transaction.Applicant = null;
            }
            // --- END OF ADDED BLOCK ---

            return transactions;
        }

        public async Task<PaymentTransaction?> GetPaymentTransactionAsync(int id)
        {
            // Find the transaction by its primary key
            var transaction = await _context.PaymentTransactions
                .Include(pt => pt.Applicant)
                .FirstOrDefaultAsync(pt => pt.Id == id);

            if (transaction == null)
            {
                return null;
                //return NotFound($"Payment Transaction with id {id} is not found.");
            }

            // Clean up the circular reference before returning.
            if (transaction.Applicant != null)
            {
                // The Applicant model might have a collection of PaymentTransactions.
                // Setting it to null prevents a serialization loop.
                transaction.Applicant.PaymentTransactions = null;
            }

            return transaction;
        }
        public async Task<PaymentTransaction?> UpdatePaymentTransactionAsync(int applicantId, int id, PaymentTransaction paymentTransaction)
        {
            if (id != paymentTransaction.Id || applicantId != paymentTransaction.ApplicantId)
            {
                return null;
            }

            var existingPaymentTransaction = await _context.PaymentTransactions
                                              .FirstOrDefaultAsync(p => p.Id == id && p.ApplicantId == applicantId);

            if (existingPaymentTransaction == null)
            {
                return null;
                //return NotFound($"PaymentTransaction with ID {id} not found or does not belong to Applicant ID {applicantId}.");
            }
            // Important: Clear the navigation property to prevent EF from trying to attach a new Applicant.
            paymentTransaction.Applicant = null;

            _context.Entry(existingPaymentTransaction).CurrentValues.SetValues(paymentTransaction);
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
                    //return NotFound($"PaymentTransaction with ID {id} not found or does not belong to Applicant ID {applicantId}.");
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<bool> DeletePaymentTransactionAsync(int applicantId, int id)
        {
            var paymentTransaction = await _context.PaymentTransactions
                                      .FirstOrDefaultAsync(p => p.Id == id && p.ApplicantId == applicantId);

            if (paymentTransaction == null)
            {
                return false;
                //return NotFound($"PaymentTransaction with ID {id} not found or does not belong to Applicant ID {applicantId}.");
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


        // Helper method
        private bool PaymentTransactionExists(int id, int applicantId)
        {
            return _context.PaymentTransactions.Any(e => e.Id == id && e.ApplicantId == applicantId);
        }
    }
}
