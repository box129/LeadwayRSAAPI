using Leadway_RSA_API.Data;
using Leadway_RSA_API.Models;
using Leadway_RSA_API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Transactions;

namespace Leadway_RSA_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // /api/applicants
    public class ApplicantsController : ControllerBase
    {
        private readonly IApplicantService _applicantService;
        private readonly IIdentificationService _identificationService;
        private readonly IBeneficiaryService _beneficiaryService;
        private readonly IAssetService _assetService;
        private readonly IAssetAllocationService _assetAllocationService;
        private readonly IExecutorService _executorService;
        private readonly IGuardianService _guardianService;
        private readonly IPaymentTransactionService _paymentTransactionService;
        // You might keep the DbContext here if other methods you haven't refactored yet still need it.
        private readonly ApplicationDbContext _context;

        public ApplicantsController(ApplicationDbContext context, IApplicantService applicantService, IIdentificationService identificationService, IBeneficiaryService beneficiaryService, IAssetService assetService, IAssetAllocationService assetAllocationService, IExecutorService executorService, IGuardianService guardianService, IPaymentTransactionService paymentTransactionService)
        {
            _context = context;
            _applicantService = applicantService;
            _identificationService = identificationService;
            _beneficiaryService = beneficiaryService;
            _assetService = assetService;
            _assetAllocationService = assetAllocationService;
            _executorService = executorService;
            _guardianService = guardianService;
            _paymentTransactionService = paymentTransactionService;
        }

        /// <summary>
        /// Creates a new Applicant record (Initial step for personal details).
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateApplicant([FromBody] Applicant applicant)
        {
            // Controller's job: Check validation.
            // --- Server-Side Validation (based on Data Annotation in your model) ---
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Controller's job: Delegate the business logic to the service.
            var newApplicant = await _applicantService.CreateApplicantAsync(applicant);

            // Controller's job: Handle the service's result and return the appropriate response.
            if (newApplicant == null)
            {
                ModelState.AddModelError("EmailAddress", "An applicant with this email address already exists.");
                return Conflict(ModelState); // HTTP 409 Conflict
            }

            // The 'CreatedAtAction' method automatically sets the Location header
            return CreatedAtAction(nameof(GetApplicant), new { id = newApplicant.Id }, newApplicant);
        }

        /// <summary>
        /// Retrieves an Applicant by their Id.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetApplicant(int id)
        {
            // Controller's job: Delegate the lookup to the service.
            var applicant = await _applicantService.GetApplicantAsync(id);

            // Controller's job: Return 404 if the service returns null.
            if (applicant == null)
            {
                return NotFound($"Applicant with ID {id} not found.");
            }

            return Ok(applicant); // HTTP 200 OK with the applicant data
        }

        /// <summary>
        /// Updates an existing Applicant record.
        /// </summary>
        [HttpPut("{id}")] // e.g., PUT /api/applicants/1
        public async Task<IActionResult> UpdateApplicant(int id, [FromBody] Applicant applicant)
        {
            // 1. Validate: Ensure the ID in the route matches the ID in the request body.
            if (id != applicant.Id)
            {
                return BadRequest("Applicant ID in the route does not match the ID in the request body.");
            }

            // 2. Validate incoming data using model state
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Returns 400 with validation errors
            }


            // The service now handles all the find, update, and concurrency logic.
            var updatedApplicant = await _applicantService.UpdateApplicantAsync(id, applicant);

            if (updatedApplicant == null)
            {
                return NotFound($"Applicant with ID {id} not found.");
            }

            // 8. Return 204 No Content for successful update (REST best practice)
            return NoContent();
        }


        /// <summary>
        /// Deletes an Applicant record and all its associated data (Identifications, Beneficiaries, Assets, etc.).
        /// </summary>
        [HttpDelete("{id}")] // e.g., DELETE /api/applicants/1
        public async Task<IActionResult> DeleteApplicant(int id)
        {
            // The service now handles the find and delete logic.
            var isDeleted = await _applicantService.DeleteApplicantAsync(id);

            if (!isDeleted)
            {
                return NotFound($"Applicant with ID {id} not found.");
            }

            return NoContent();
        }


        /// <summary>
        /// Adds a new Identification record for a specific Applicant.
        /// </summary>
        [HttpPost("{applicantId}/identifications")] // e.g., POST /api/applicants/1/identifications
        public async Task<IActionResult> AddIdentification(int applicantId, [FromBody] Identification identification)
        {
            // 1. Validate incoming data using model state
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Returns 400 with validation errors
            }
            // Sends request to the identification service to add the identification
            var addedIdentification = await _identificationService.AddIdentificationAsync(applicantId, identification);
            if (addedIdentification == null)
            {
                return NotFound($"Applicant with ID {applicantId} not found.");
            }

            // 4. Return success response (201 Created)
            // Use nameof(GetIdentification) to point to the action that retrieves a single identification by its own ID.
            return CreatedAtAction(nameof(GetIdentification), new { id = addedIdentification.Id }, addedIdentification);
        }

        /// <summary>
        /// Retrieves all Identification records for a specific Applicant.
        /// </summary>
        [HttpGet("{applicantId}/identifications")] // e.g., GET /api/applicants/1/identifications
        public async Task<IActionResult> GetIdentificationsForApplicant(int applicantId)
        {
            var identifications = await _identificationService.GetIdentificationsByApplicantIdAsync(applicantId);
            if (!identifications.Any())
            {
                return NotFound($"No identifications found for Applicant with ID {applicantId}.");
            }
            return Ok(identifications); // Returns 200 OK with the list of identifications
        }

        /// <summary>
        /// Retrieves a single Identification record by its own Id.
        /// </summary>
        [HttpGet("identifications/{id}")] // e.g., GET /api/applicants/identifications/1
        public async Task<IActionResult> GetIdentification(int id)
        {
            var identification = await _identificationService.GetIdentificationAsync(id);
            if (identification == null)
            {
                return NotFound($"Identification with ID {id} not found.");
            }
            return Ok(identification); // Returns 200 OK with the identification data
        }

        /// <summary>
        /// Updates an existing Identification record for a specific Applicant.
        /// </summary>
        [HttpPut("{applicantId}/identifications/{id}")] // e,g., PUT api/applicants/1/identifications/1
        public async Task<IActionResult> UpdateIdentification(int applicantId, int id, [FromBody] Identification identification)
        {
            if (id != identification.Id)
            {
                return BadRequest("Identification ID in the route does not match the ID in the request body.");
            }
            if (applicantId != identification.ApplicantId)
            {
                return BadRequest("Applicant ID in the route does not match the ApplicantId in the request body.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var updatedIdentification = await _identificationService.UpdateIdentificationAsync(applicantId, id, identification);
            if (updatedIdentification == null)
            {
                return NotFound($"Identification with ID {id} not found or does not belong to Applicant ID {applicantId}.");
            }
            return NoContent();
        }

        /// <summary>
        /// Deletes an Identification record for a specific Applicant.
        /// </summary>
        [HttpDelete("{applicantId}/identifications/{id}")] // e.g., DELETE /api/applicants/1/identifications/1
        public async Task<IActionResult> DeleteIdentification(int applicantId, int id)
        {
            var isDeleted = await _identificationService.DeleteIdentificationAsync(applicantId, id);
            if (!isDeleted)
            {
                return NotFound($"Identification with ID {id} not found or does not belong to Applicant ID {applicantId}.");
            }
            return NoContent();
        }

        /// <summary>
        /// Adds a new Beneficiary record for a specific Applicant.
        /// </summary>
        [HttpPost("{applicantId}/beneficiaries")] // e.g., POST /api/applicants/1/beneficiaries
        public async Task<IActionResult> AddBeneficiary(int applicantId, [FromBody] Beneficiary beneficiary)
        {
            // 1. Validate incoming data using model state
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Returns 400 with validation errors
            }

            var addedBeneficiary = await _beneficiaryService.AddBeneficiaryAsync(applicantId, beneficiary);
            if (addedBeneficiary == null)
            {
                return NotFound($"Applicant with ID {applicantId} not found.");
            }

            // 4. Return success response (201 Created)
            // Use nameof(GetBeneficiary) to point to the action that retrieves a single beneficiary by its own ID.
            return CreatedAtAction(nameof(GetBeneficiary), new { id = addedBeneficiary.Id }, addedBeneficiary);
        }

        /// <summary>
        /// Retrieves all Beneficiary records for a specific Applicant.
        /// </summary>
        [HttpGet("{applicantId}/beneficiaries")] // e.g., GET /api/applicants/1/beneficiaries
        public async Task<IActionResult> GetBeneficiariesForApplicant(int applicantId)
        {
            var beneficiaries = await _beneficiaryService.GetBeneficiariesByApplicantIdAsync(applicantId);
            // The service returns an empty list if nothing is found, so we only need to check Any().
            if (!beneficiaries.Any())
            {
                return NotFound($"No beneficiaries found for Applicant with ID {applicantId}.");
            }
            return Ok(beneficiaries); // Returns 200 OK with the list of beneficiaries
        }

        /// <summary>
        /// Retrieves a single Beneficiary record by its own Id.
        /// </summary>
        [HttpGet("beneficiaries/{id}")] // e.g., GET /api/applicants/beneficiaries/1
        public async Task<IActionResult> GetBeneficiary(int id)
        {
            var beneficiary = await _beneficiaryService.GetBeneficiaryAsync(id);
            if (beneficiary == null)
            {
                return NotFound($"Beneficiary with ID {id} not found.");
            }

            return Ok(beneficiary); // Returns 200 OK with the beneficiary data
        }

        /// <summary>
        /// Updates an existing Beneficiary record for a specific Applicant.
        /// </summary>
        [HttpPut("{applicantId}/beneficiaries/{id}")] // e.g., PUT api/applicnats/1/beneficiaries/1
        public async Task<IActionResult> UpdateBeneficiary(int applicantId, int id, [FromBody] Beneficiary beneficiary)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Returns 400 with validation errors
            }
            var updatedBeneficiary = await _beneficiaryService.UpdateBeneficiaryAsync(applicantId, id, beneficiary);
            if (updatedBeneficiary == null)
            {
                return NotFound($"Beneficiary with ID {id} not found or does not belong to Applicant ID {applicantId}");
            }

            // 6. Return 204 No Content for successful update.
            return NoContent();
        }

        /// <summary>
        /// Deletes a Beneficiary record for a specific Applicant.
        /// </summary>
        [HttpDelete("{applicantId}/beneficiaries/{id}")] // e.g., DELETE /api/applicants/1/beneficiaries/1
        public async Task<IActionResult> DeleteBeneficiary(int applicantId, int id)
        {
            var isDeleted = await _beneficiaryService.DeleteBeneficiaryAsync(applicantId, id);
            if (!isDeleted)
            {
                return NotFound($"Beneficiary with ID {id} not found or does not belong to Applicant ID {applicantId}.");
            }

            // 3. Return 204 No Content for successful deletion.
            return NoContent();
        }

        /// <summary>
        /// Adds a new Asset record for a specific Applicant.
        /// </summary>
        [HttpPost("{applicantId}/assets")] // POST /api/applicants/1/assets
        public async Task<IActionResult> AddAsset(int applicantId, [FromBody] Asset asset)
        {
            // 1. Valdate incoming data using madel state
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Returns 400 with validation errors
            }

            var addedAsset = await _assetService.AddAssetAsync(applicantId, asset);
            // NOTE: This check has been added to handle the case where the applicantId doesn't exist.
            if (addedAsset == null)
            {
                return NotFound($"Applicant with ID {applicantId} not found.");
            }

            // 4. Return success response (201 Created)
            // Use nameof(GetAsset) to point to the action that retrieves a single asset by its own ID.
            return CreatedAtAction(nameof(GetAsset), new { id = addedAsset.Id }, addedAsset);
        }

        /// <summary>
        /// Retrieves all Asset records for a specific Applicant.
        /// </summary>
        [HttpGet("{applicantId}/assets")] // e.g., GET /api/applicants/1/assets
        public async Task<IActionResult> GetAssetsForApplicant(int applicantId)
        {
            var assets = await _assetService.GetAssetsByApplicantIdAsync(applicantId);
            if (!assets.Any())
            {
                return NotFound($"No assets found for Applicant with ID {applicantId}");
            }
            return Ok(assets); // Returns 200 Ok with the list of assets
        }

        /// <summary>
        /// Retrieves a single Asset record by its own Id.
        /// </summary>
        [HttpGet("assets/{id}")] // e.g., GET /api/applicants/assets/1
        public async Task<IActionResult> GetAsset(int id)
        {
            var asset = await _assetService.GetAssetAsync(id);
            if (asset == null)
            {
                return NotFound($"Asset with ID {id} is not found.");
            }

            return Ok(asset); // Returns 200 OK with the asset data
        }

        /// <summary>
        /// Updates an existing Asset record for a specific Applicant.
        /// </summary>
        [HttpPut("{applicantId}/assets/{id}")] // e.g., PUT /api/applicants/1/assets/1
        public async Task<IActionResult> UpdateAsset(int applicantId, int id,[FromBody] Asset asset)
        {
            // 2. To check if any of the data annotations flooded the model state with errors
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var updatedAsset = await _assetService.UpdateAssetAsync(applicantId, id, asset);
            if (updatedAsset == null)
            {
                return NotFound($"Asset with ID {id} not found or does not belong to Applicant ID {applicantId}");
            }

            // 5. Return 204 for successful operation
            return NoContent();
        }

        /// <summary>
        /// Deletes an Asset record for a specific Applicant.
        /// </summary>
        [HttpDelete("{applicantId}/assets/{id}")] // e.g., DELETE /api/applicants/1/assets/1
        public async Task<IActionResult> DeleteAsset(int applicantId, int id)
        {
            var isDeleted = await _assetService.DeleteAssetAsync(applicantId, id);
            if (!isDeleted)
            {
                return NotFound($"Asset with ID {id} not found or does not belong to Applicant ID {applicantId}");
            }
            // 4. Returns 204 if successful
            return NoContent();
        }

        /// <summary>
        /// Creates a new asset allocation for a specific applicant.
        /// Requires existing AssetId and BeneficiaryId that belong to the specified Applicant.
        /// </summary>
        [HttpPost("{applicantId}/assetallocations")] // e.g., POST /api/applicants/1/assetallocations
        public async Task<IActionResult> AddAssetAllocation(int applicantId, [FromBody] BeneficiaryAssetAllocation allocation)
        {
            // 1. Validate incoming data using model state (e.g., percentage range, required fields)
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Returns 400 with validation errors
            }

            var addedAllocation = await _assetAllocationService.AddAssetAllocationAsync(applicantId, allocation);
            // Corrected logic to check for null response from the service.
            if (addedAllocation == null)
            {
                return NotFound($"Applicant with ID {applicantId} not found, or the Asset/Beneficiary does not belong to this Applicant.");
            }

            // Corrected: Use the ID from the newly created object, not the incoming request object.
            return CreatedAtAction(nameof(GetAssetAllocation), new { id = addedAllocation.Id }, addedAllocation);
        }

        /// <summary>
        /// Retrieves all BeneficiaryAssetAllocation records for a specific Applicant.
        /// Includes details of the associated Asset and Beneficiary.
        /// </summary>
        [HttpGet("{applicantId}/assetallocations")] // e.g., GET /api/applicants/1/assetallocations
        public async Task<IActionResult> GetAssetAllocationsForApplicant(int applicantId)
        {
            var allocations = await _assetAllocationService.GetAssetAllocationsByApplicantIdAsync(applicantId);
            if (!allocations.Any())
            {
                return NotFound($"No asset allocations found for Applicant with ID {applicantId}");
            }

            return Ok(allocations);
        }

        /// <summary>
        /// Retrieves a single BeneficiaryAssetAllocation record by its own Id.
        /// Includes details of the associated Asset and Beneficiary.
        /// </summary>
        [HttpGet("assetallocations/{id}")]
        public async Task<IActionResult> GetAssetAllocation(int id)
        {
            var allocation = await _context.BeneficiaryAssetAllocations
                .Include(ba => ba.Asset)
                .Include(ba => ba.Beneficiary)
                .Include(ba => ba.Applicant)
                .FirstOrDefaultAsync(ba => ba.Id == id);

            if (allocation == null)
            {
                return NotFound($"Beneficiary Asset Allocation with ID {id} not found.");
            }

            // --- ADD THIS BLOCK FOR CLEANUP ---
            

            // Clean up the Applicant object nested inside the Asset
            if (allocation.Asset != null && allocation.Asset.Applicant != null)
            {
                // Fix the circular reference from the Asset -> Applicant -> AssetAllocations
                allocation.Asset.AssetAllocations = null;

                allocation.Asset.Applicant.Identifications = null;
                allocation.Asset.Applicant.Beneficiaries = null;
                allocation.Asset.Applicant.Executors = null;
                allocation.Asset.Applicant.Guardians = null;
                allocation.Asset.Applicant.Assets = null;
                allocation.Asset.Applicant.AssetAllocations = null;
                allocation.Asset.Applicant.PaymentTransactions = null;
            }

            
            // --- END OF CLEANUP BLOCK ---

            return Ok(allocation);
        }

        /// <summary>
        /// Updates an existing BeneficiaryAssetAllocation record for a specific Applicant.
        /// </summary>
        [HttpPut("{applicantId}/assetallocations/{id}")]
        public async Task<IActionResult> UpdateBeneficiaryAssetAllocation(int applicantId, int id, [FromBody] BeneficiaryAssetAllocation allocation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var updatedAllocation = await _assetAllocationService.UpdateBeneficiaryAssetAllocationAsync(applicantId, id, allocation);
            if (updatedAllocation == null)
            {
                return NotFound($"Beneficiary Asset Allocation with ID {id} not found or does not belong to Applicant ID {applicantId}");
            }

            return NoContent();
        }

        /// <summary>
        /// Deletes a BeneficiaryAssetAllocation record for a specific Applicant.
        /// </summary>
        [HttpDelete("{applicantId}/assetallocations/{id}")]
        public async Task<IActionResult> DeleteBeneficiaryAssetAllocation(int applicantId, int id)
        {
            var isDeleted = await _assetAllocationService.DeleteBeneficiaryAssetAllocationAsync(applicantId, id);
            if (!isDeleted)
            {
                return NotFound($"Beneficiary Asset Allocation with ID {id} does not exist or does not belong to Applicant ID {applicantId}.");
            }

            return NoContent();
        }

        /// <summary>
        /// Adds a new Executor record for a specific Applicant.
        /// </summary>
        [HttpPost("{applicantId}/executors")]
        public async Task<IActionResult> AddExecutor(int applicantId, [FromBody] Executor executor)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Returns 400 with validation errors
            }

            var addedExecutor = await _executorService.AddExecutorAsync(applicantId, executor);
            if (addedExecutor == null)
            {
                return NotFound($"Applicant with ID {applicantId} not found.");
            }

            // 4. Return success response (201 Created)
            // Use nameof(GetExecutor) to point to the action that retrieves a single executor by its own ID.
            return CreatedAtAction(nameof(GetExecutor), new { Id = addedExecutor.Id }, addedExecutor);
        }

        /// <summary>
        /// Retrieves all Executor records for a specific Applicant.
        /// </summary>
        [HttpGet("{applicantId}/executors")] // e.g., GET /api/applicants/1/executors
        public async Task<IActionResult> GetExecutorForApplicant(int applicantId)
        {
            var executors = await _executorService.GetExecutorByApplicantIdAsync(applicantId);
            if (!executors.Any())
            {
                return NotFound($"No executors found for Appllicant with ID {applicantId}.");
            }

            return Ok(executors); // Returns 200 Ok with the List of executors
        }

        /// <summary>
        /// Retrieves a single Executor record by its own Id.
        /// </summary>
        [HttpGet("executors/{id}")] // e.g., GET /applicant/executors/1
        public async Task<IActionResult> GetExecutor(int id)
        {
            var executor = await _executorService.GetExecutorAsync(id);
            if (executor == null)
            {
                return NotFound($"Executor with ID {id} not found.");
            }

            return Ok(executor); // Returns 200 Ok with the executor data 
        }

        /// <summary>
        /// Updates an existing Executor record for a specific Applicant.
        /// </summary>
        [HttpPut("{applicantId}/executors/{id}")] // e.g., PUT /api/applicants/1/executors/1
        public async Task<IActionResult> UpdateExecutor(int applicantId, int id, [FromBody] Executor executor)
        {
            // 2. Check for errors flooded into the ModelState, because of the Data Annotations used
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedExecutor = await _executorService.UpdateExecutorAsync(applicantId, id, executor);
            if (updatedExecutor == null)
            {
                return NotFound($"Executor with ID {id} not found or does not belong to Applicant ID {applicantId}.");
            }

            return NoContent();
        }

        /// <summary>
        /// Deletes an Executor record for a specific Applicant.
        /// </summary>
        [HttpDelete("{applicantId}/executors/{id}")] // e.g., DELETE /api/applicants/1/executors/1
        public async Task<IActionResult> DeleteExecutor(int applicantId, int id)
        {
            var isDeleted = await _executorService.DeleteExecutorAsync(applicantId, id);
            if (!isDeleted)
            {
                return NotFound($"Executor with ID {id} not found or does nt belong to Applicant ID {applicantId}");
            }

            return NoContent();
        }


        /// <summary>
        /// Adds a new Guardian record for a specific Applicant.
        /// </summary>
        [HttpPost("{applicantId}/guardians")] // e.g., POST api/applicants/1/guardians
        public async Task<IActionResult> AddGuardian(int applicantId, [FromBody] Guardian guardian)
        {
            // 1. Validate incoming data using model state
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var addedGuardian = await _guardianService.AddGuardianAsync(applicantId, guardian);

            // 4. Return success response (201 Created)
            // Use nameof(GetGuardian) to point to the action that retrieves a single guardian by its own ID.
            return CreatedAtAction(nameof(GetGuardian), new { Id = addedGuardian.Id }, addedGuardian);
        }

        /// <summary>
        /// Retrieves all Guardian records for a specific Applicant.
        /// </summary>
        [HttpGet("{applicantId}/guardians")] // e.g., GET /api/applicants/1/guardians
        public async Task<IActionResult> GetGuardiansForApplicant(int applicantId)
        {
            var guardians = await _guardianService.GetGuardiansByApplicantIdAsync(applicantId);
            // The service now returns an empty list, so we only need to check if the list is empty.
            // Returning a 404 is a valid design choice for an empty collection associated with a parent resource.
            if (!guardians.Any())
            {
                return NotFound($"No guardians found for Applicant with ID {applicantId}.");
            }

            return Ok(guardians); // Returns 200 OK with the list of guardians
        }

        /// <summary>
        /// Retrieves a single Guardian record by its own Id.
        /// </summary>
        [HttpGet("guardians/{id}")] // e.g., GET /api/applicants/guardians/1
        public async Task<IActionResult> GetGuardian(int id)
        {
            var guardian = await _guardianService.GetGuardianAsync(id);

            if (guardian == null)
            {
                return NotFound($"Guardian with ID {id} not found.");
            }

            return Ok(guardian); // Returns 200 Ok with the guardian data
        }

        /// <summary>
        /// Updates an existing Guardian record for a specific Applicant.
        /// </summary>
        [HttpPut("{applicantId}/guardians/{id}")]
        public async Task<IActionResult> UpdateGuardian(int applicantId, int id, [FromBody] Guardian guardian)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var updatedGuardian = await _guardianService.UpdateGuardianAsync(applicantId, id, guardian);
            if (updatedGuardian == null)
            {
                return NotFound($"Guardian with ID {id} not found or does not belong to Applicant ID {applicantId}.");
            }

            return NoContent();
        }

        /// <summary>
        /// Deletes a Guardian record for a specific Applicant.
        /// </summary>
        [HttpDelete("{applicantId}/guardians/{id}")]
        public async Task<IActionResult> DeleteGuardian(int applicantId, int id)
        {
            var isDeleted = await _guardianService.DeleteGuardianAsync(applicantId, id);
            if (!isDeleted)
            {
                return NotFound($"Guardian with Id {id} not found or does not belong to Applicant ID {applicantId}.");
            }

            return NoContent();
        }

        /// <summary>
        /// Adds a new PaymentTransaction record for a specific Applicant.
        /// </summary>
        [HttpPost("{applicantId}/paymenttransactions")] // e.g., POST /api/applicants/1/paymenttransactions
        public async Task<IActionResult> AddPaymentTransaction(int applicantId, [FromBody] PaymentTransaction paymentTransaction)
        {
            // 1. Validate incoming data using model state
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Returns 400 with validation errors
            }

            var addedTransaction = await _paymentTransactionService.AddPaymentTransactionAsync(applicantId, paymentTransaction);
            if (addedTransaction == null)
            {
                return NotFound($"Applicant with ID {applicantId} not found.");
            }

            // 4. Return success response (201 Created)
            // Use nameof(GetPaymentTransaction) to point to the action that retrieves a single transaction by its own ID.
            return CreatedAtAction(nameof(GetPaymentTransaction), new { id = addedTransaction.Id }, addedTransaction);
        }

        /// <summary>
        /// Retrieves all PaymentTransaction records for a specific Applicant.
        /// </summary>
        [HttpGet("{applicantId}/paymenttransactions")] // e.g., GET /api/applicants/1/paymenttransactions
        public async Task<IActionResult> GetPaymentTransactionForApplicant(int applicantId)
        {
            var transactions = await _paymentTransactionService.GetPaymentTransactionByApplicantIdAsync(applicantId);
            // The service now returns an empty list, so we only need to check if the list is empty.
            // Returning a 404 is a valid design choice for an empty collection associated with a parent resource.

            if (!transactions.Any())
            {
                return NotFound($"No payment transactions found for Applicant with ID {applicantId}.");
            }

            return Ok(transactions); // Returns 200 Ok with the list of transactions
        }

        /// <summary>
        /// Retrieves a single PaymentTransaction record by its own Id.
        /// </summary>
        [HttpGet("paymenttransactions/{id}")] // e.g., GET /api/applicants/paymenttransactions/1
        public async Task<IActionResult> GetPaymentTransaction(int id)
        {
            var transaction = await _paymentTransactionService.GetPaymentTransactionAsync(id);
            if (transaction == null)
            {
                return NotFound($"PaymentTransaction with ID {id} not found.");
            }

            return Ok(transaction); // Returns 200 Ok with the transaction data
        }


        /// <summary>
        /// Updates an existing PaymentTransaction record for a specific Applicant.
        /// </summary>
        [HttpPut("{applicantId}/paymentTransactions/{id}")]
        public async Task<IActionResult> UpdatePaymentTransaction(int applicantId, int id, [FromBody] PaymentTransaction paymentTransaction)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedTransaction = await _paymentTransactionService.UpdatePaymentTransactionAsync(applicantId, id, paymentTransaction);
            if (updatedTransaction == null)
            {
                return NotFound($"PaymentTransaction with ID {id} not found or does not belong to Applicant ID {applicantId}.");
            }

            return NoContent();
        }

        /// <summary>
        /// Deletes a PaymentTransaction record for a specific Applicant.
        /// </summary>
        [HttpDelete("{applicantId}/paymentTransactions/{id}")]
        public async Task<IActionResult> DeletePaymentTransaction(int applicantId, int id)
        {
            var isDeleted = await _paymentTransactionService.DeletePaymentTransactionAsync(applicantId, id);
            if (!isDeleted)
            {
                return NotFound($"PaymentTransaction with ID {id} not found or does not belong to Applicant ID {applicantId}.");
            }

            return NoContent();
        }
    }
}
