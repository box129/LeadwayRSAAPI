using Leadway_RSA_API.Data;
using Leadway_RSA_API.DTOs;
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
        //private readonly ApplicationDbContext _context;

        public ApplicantsController(IApplicantService applicantService, IIdentificationService identificationService, IBeneficiaryService beneficiaryService, IAssetService assetService, IAssetAllocationService assetAllocationService, IExecutorService executorService, IGuardianService guardianService, IPaymentTransactionService paymentTransactionService)
        {
           // _context = context;
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
        public async Task<IActionResult> CreateApplicant([FromBody] CreateApplicantDto applicantDto)
        {
            // Controller's job: Check validation.
            // --- Server-Side Validation (based on Data Annotation in your model) ---
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Correct: Pass the DTO to the service. The service is now responsible for the mapping.
            // This is a cleaner approach as the service owns the business logic for creating the model.
            var newApplicant = await _applicantService.CreateApplicantAsync(applicantDto);

            // Controller's job: Handle the service's result and return the appropriate response.
            if (newApplicant == null)
            {
                ModelState.AddModelError("EmailAddress", "An applicant with this email address already exists.");
                return Conflict(ModelState); // HTTP 409 Conflict
            }
            // Correct: Map the full model from the service back to a DTO for the response.
            var applicantDtoResponse = new ApplicantDto
            {
                Id = newApplicant.Id,
                RSAPin = newApplicant.RSAPin,
                FirstName = newApplicant.FirstName,
                LastName = newApplicant.LastName,
                PhoneNumber = newApplicant.PhoneNumber,
                EmailAddress = newApplicant.EmailAddress,
                DateOfBirth = newApplicant.DateOfBirth,
                CurrentStep = newApplicant.CurrentStep,
                IsComplete = newApplicant.IsComplete
            };

            // The 'CreatedAtAction' method automatically sets the Location header
            return CreatedAtAction(nameof(GetApplicant), new { id = applicantDtoResponse.Id }, applicantDtoResponse);
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

            // Map the full model to the DTO for the response.
            var applicantDto = new ApplicantDto
            {
                Id = applicant.Id,
                RSAPin = applicant.RSAPin,
                FirstName = applicant.FirstName,
                LastName = applicant.LastName,
                PhoneNumber = applicant.PhoneNumber,
                EmailAddress = applicant.EmailAddress,
                DateOfBirth = applicant.DateOfBirth,
                CurrentStep = applicant.CurrentStep,
                IsComplete = applicant.IsComplete
            };

            return Ok(applicantDto); // HTTP 200 OK with the applicant data
        }

        /// <summary>
        /// Updates an existing Applicant record.
        /// </summary>
        [HttpPut("{id}")] // e.g., PUT /api/applicants/1
        public async Task<IActionResult> UpdateApplicant(int id, [FromBody] UpdateApplicantDto applicantDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Pass the DTO and ID to the service. The service handles the lookup and update.
            
            // The service now handles all the find, update, and concurrency logic.
            var updatedApplicant = await _applicantService.UpdateApplicantAsync(id, applicantDto);

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
        public async Task<IActionResult> AddIdentification(int applicantId, [FromBody] CreateIdentificationDto identificationDto)
        {
            // 1. Validate incoming data using model state
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Returns 400 with validation errors
            }

            // Sends request to the identification service to add the identification
            var addedIdentification = await _identificationService.AddIdentificationAsync(applicantId, identificationDto);
            if (addedIdentification == null)
            {
                return NotFound($"Applicant with ID {applicantId} not found.");
            }

            // Map the added model back to the DTO for the response
            var addedIdentificationDto = new IdentificationDto
            {
                Id = addedIdentification.Id,
                ApplicantId = addedIdentification.ApplicantId,
                IdentificationType = addedIdentification.IdentificationType,
                DocumentNumber = addedIdentification.DocumentNumber,
                ImagePath = addedIdentification.ImagePath,
                UploadDate = addedIdentification.UploadDate
            };

            // 4. Return success response (201 Created)
            // Use nameof(GetIdentification) to point to the action that retrieves a single identification by its own ID.
            return CreatedAtAction(nameof(GetIdentification), new { id = addedIdentificationDto.Id }, addedIdentificationDto);
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

            // Map the list of full models to a list of DTOs
            var identificationDtos = identifications.Select(i => new IdentificationDto
            {
                Id = i.Id,
                ApplicantId = i.ApplicantId,
                IdentificationType = i.IdentificationType,
                DocumentNumber = i.DocumentNumber,
                ImagePath = i.ImagePath,
                UploadDate = i.UploadDate
            }).ToList();

            return Ok(identificationDtos); // Returns 200 OK with the list of DTOs
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
            // Map the full model to the DTO for the response
            var identificationDto = new IdentificationDto
            {
                Id = identification.Id,
                ApplicantId = identification.ApplicantId,
                IdentificationType = identification.IdentificationType,
                DocumentNumber = identification.DocumentNumber,
                ImagePath = identification.ImagePath,
                UploadDate = identification.UploadDate
            };

            return Ok(identificationDto); // Returns 200 OK with the identification DTO
        }

        /// <summary>
        /// Updates an existing Identification record for a specific Applicant.
        /// </summary>
        [HttpPut("{applicantId}/identifications/{id}")] // e,g., PUT api/applicants/1/identifications/1
        public async Task<IActionResult> UpdateIdentification(int applicantId, int id, [FromBody] UpdateIdentificationDto identificationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // The service layer should handle finding the entity and applying the updates
            var updatedIdentification = await _identificationService.UpdateIdentificationAsync(applicantId, id, identificationDto);
            if (updatedIdentification == null)
            {
                return NotFound($"Identification with ID {id} not found or does not belong to Applicant ID {applicantId}.");
            }
            // Return a 204 No Content for a successful update
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
        public async Task<IActionResult> AddBeneficiary(int applicantId, [FromBody] CreateBeneficiaryDto beneficiaryDto)
        {
            // 1. Validate incoming data using model state
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Returns 400 with validation errors
            }

            var addedBeneficiary = await _beneficiaryService.AddBeneficiaryAsync(applicantId, beneficiaryDto);
            if (addedBeneficiary == null)
            {
                return NotFound($"Applicant with ID {applicantId} not found.");
            }


            // maps from service response to DTO for the response
            var addedBeneficiaryDto = new BeneficiaryDto
            {
                Id = addedBeneficiary.Id,
                ApplicantId = addedBeneficiary.ApplicantId,
                FirstName = addedBeneficiary.FirstName,
                LastName = addedBeneficiary.LastName,
                Gender = addedBeneficiary.Gender
            };

            // 4. Return success response (201 Created)
            // Use nameof(GetBeneficiary) to point to the action that retrieves a single beneficiary by its own ID.
            return CreatedAtAction(nameof(GetBeneficiary), new { id = addedBeneficiaryDto.Id }, addedBeneficiaryDto);
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

            var beneficiaryDtos = beneficiaries.Select(b => new BeneficiaryDto
            {
                Id = b.Id,
                ApplicantId = b.ApplicantId,
                FirstName = b.FirstName,
                LastName = b.LastName,
                Gender = b.Gender
            }).ToList();

            return Ok(beneficiaryDtos); // Returns 200 OK with the list of beneficiaries
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

            var beneficiaryDto = new BeneficiaryDto
            {
                Id = beneficiary.Id,
                ApplicantId = beneficiary.ApplicantId,
                FirstName = beneficiary.FirstName,
                LastName = beneficiary.LastName,
                Gender = beneficiary.Gender
            };

            return Ok(beneficiaryDto); // Returns 200 OK with the beneficiary data
        }

        /// <summary>
        /// Updates an existing Beneficiary record for a specific Applicant.
        /// </summary>
        [HttpPut("{applicantId}/beneficiaries/{id}")] // e.g., PUT api/applicnats/1/beneficiaries/1
        public async Task<IActionResult> UpdateBeneficiary(int applicantId, int id, [FromBody] UpdateBeneficiaryDto beneficiaryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Returns 400 with validation errors
            }
            var updatedBeneficiary = await _beneficiaryService.UpdateBeneficiaryAsync(applicantId, id, beneficiaryDto);
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
        public async Task<IActionResult> AddAsset(int applicantId, [FromBody] CreateAssetDto assetDto)
        {
            // 1. Valdate incoming data using madel state
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Returns 400 with validation errors
            }

            var addedAsset = await _assetService.AddAssetAsync(applicantId, assetDto);
            // NOTE: This check has been added to handle the case where the applicantId doesn't exist.
            if (addedAsset == null)
            {
                return NotFound($"Applicant with ID {applicantId} not found.");
            }

            var addedAssetDto = new AssetDto
            {
                Id = addedAsset.Id,
                ApplicantId = addedAsset.ApplicantId,
                AssetType = addedAsset.AssetType,
                Name = addedAsset.Name,
                Value = addedAsset.Value,
                RSAPin = addedAsset.RSAPin,
                PFA = addedAsset.PFA,
                BankName = addedAsset.BankName,
                AccountNumber = addedAsset.AccountNumber,
                AccountType = addedAsset.AccountType
            };

            // 4. Return success response (201 Created)
            // Use nameof(GetAsset) to point to the action that retrieves a single asset by its own ID.
            return CreatedAtAction(nameof(GetAsset), new { id = addedAssetDto.Id }, addedAssetDto);
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

            var assetDtos = assets.Select(a => new AssetDto
            {
                Id = a.Id,
                ApplicantId = a.ApplicantId,
                AssetType = a.AssetType,
                Name = a.Name,
                Value = a.Value,
                RSAPin = a.RSAPin,
                PFA = a.PFA,
                BankName = a.BankName,
                AccountNumber = a.AccountNumber,
                AccountType = a.AccountType
            }).ToList();

            return Ok(assetDtos); // Returns 200 Ok with the list of assets
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

            var assetDto = new AssetDto
            {
                Id = asset.Id,
                ApplicantId = asset.ApplicantId,
                AssetType = asset.AssetType,
                Name = asset.Name,
                Value = asset.Value,
                RSAPin = asset.RSAPin,
                PFA = asset.PFA,
                BankName = asset.BankName,
                AccountNumber = asset.AccountNumber,
                AccountType = asset.AccountType
            };

            return Ok(assetDto); // Returns 200 OK with the asset data
        }

        /// <summary>
        /// Updates an existing Asset record for a specific Applicant.
        /// </summary>
        [HttpPut("{applicantId}/assets/{id}")] // e.g., PUT /api/applicants/1/assets/1
        public async Task<IActionResult> UpdateAsset(int applicantId, int id,[FromBody] UpdateAssetDto assetDto)
        {
            // To check if any of the data annotations flooded the model state with errors
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var updatedAsset = await _assetService.UpdateAssetAsync(applicantId, id, assetDto);
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
        public async Task<IActionResult> AddAssetAllocation(int applicantId, [FromBody] CreateAssetAllocationDto allocationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var addedAllocation = await _assetAllocationService.AddAssetAllocationAsync(applicantId, allocationDto);
            // Corrected logic to check for null response from the service.
            if (addedAllocation == null)
            {
                return NotFound($"Applicant with ID {applicantId} not found, or the Asset/Beneficiary does not belong to this Applicant.");
            }

            // CORRECTED: Map the service response to the DTO before returning.
            var addedAllocationDto = new AssetAllocationDto
            {
                Id = addedAllocation.Id,
                ApplicantId = addedAllocation.ApplicantId,
                AssetId = addedAllocation.AssetId,
                BeneficiaryId = addedAllocation.BeneficiaryId,
                Percentage = addedAllocation.Percentage
                // Note: The service doesn't return the nested objects, so we don't map them here.
            };

            // Corrected: Use the ID from the newly created object, not the incoming request object.
            return CreatedAtAction(nameof(GetAssetAllocation), new { id = addedAllocationDto.Id }, addedAllocationDto);
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

            // Use a LINQ Select to project the full models into a list of DTOs.
            var allocationDtos = allocations.Select(aa => new AssetAllocationDto
            {
                Id = aa.Id,
                ApplicantId = aa.ApplicantId,
                AssetId = aa.AssetId,
                BeneficiaryId = aa.BeneficiaryId,
                Percentage = aa.Percentage,
                Asset = aa.Asset == null ? null : new SimpleAssetDto
                {
                    Id = aa.Asset.Id,
                    Name = aa.Asset.Name,
                    AssetType = aa.Asset.AssetType
                },
                Beneficiary = aa.Beneficiary == null ? null : new SimpleBeneficiaryDto
                {
                    Id = aa.Beneficiary.Id,
                    FirstName = aa.Beneficiary.FirstName,
                    LastName = aa.Beneficiary.LastName
                }
            }).ToList();

            return Ok(allocationDtos);
        }

        /// <summary>
        /// Retrieves a single BeneficiaryAssetAllocation record by its own Id.
        /// Includes details of the associated Asset and Beneficiary.
        /// </summary>
        [HttpGet("assetallocations/{id}")]
        public async Task<IActionResult> GetAssetAllocation(int id)
        {
            // Get the full model from the service layer.
            var allocation = await _assetAllocationService.GetAssetAllocationByIdAsync(id);

            if (allocation == null)
            {
                return NotFound($"Beneficiary Asset Allocation with ID {id} not found.");
            }

            // Map the full model to the DTO.
            var allocationDto = new AssetAllocationDto
            {
                Id = allocation.Id,
                ApplicantId = allocation.ApplicantId,
                AssetId = allocation.AssetId,
                BeneficiaryId = allocation.BeneficiaryId,
                Percentage = allocation.Percentage,
                Asset = (allocation.Asset != null) ? new SimpleAssetDto
                {
                    Id = allocation.Asset.Id,
                    Name = allocation.Asset.Name,
                    AssetType = allocation.Asset.AssetType
                } : null,
                Beneficiary = (allocation.Beneficiary != null) ? new SimpleBeneficiaryDto
                {
                    Id = allocation.Beneficiary.Id,
                    FirstName = allocation.Beneficiary.FirstName,
                    LastName = allocation.Beneficiary.LastName
                } : null
            };

            // Return the DTO.
            return Ok(allocationDto);
        }

        /// <summary>
        /// Updates an existing BeneficiaryAssetAllocation record for a specific Applicant.
        /// </summary>
        [HttpPut("{applicantId}/assetallocations/{id}")]
        public async Task<IActionResult> UpdateBeneficiaryAssetAllocation(int applicantId, int id, [FromBody] UpdateAssetAllocationDto allocationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedAllocation = await _assetAllocationService.UpdateBeneficiaryAssetAllocationAsync(applicantId, id, allocationDto);
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
        public async Task<IActionResult> AddExecutor(int applicantId, [FromBody] CreateExecutorsDtos executorDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Returns 400 with validation errors
            }

            var addedExecutor = await _executorService.AddExecutorAsync(applicantId, executorDto);
            if (addedExecutor == null)
            {
                return NotFound($"Applicant with ID {applicantId} not found.");
            }

            var addedExecutorDto = new ExecutorsDtos
            {
                Id = addedExecutor.Id,
                ApplicantId = addedExecutor.ApplicantId,
                ExecutorType = addedExecutor.ExecutorType,
                FirstName = addedExecutor.FirstName,
                LastName = addedExecutor.LastName,
                CompanyName = addedExecutor.CompanyName,
                PhoneNumber = addedExecutor.PhoneNumber,
                Address = addedExecutor.Address,
                City = addedExecutor.City,
                State = addedExecutor.State
            };

            // 4. Return success response (201 Created)
            // Use nameof(GetExecutor) to point to the action that retrieves a single executor by its own ID.
            return CreatedAtAction(nameof(GetExecutor), new { Id = addedExecutorDto.Id }, addedExecutorDto);
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
                return NotFound($"No executors found for Applicant with ID {applicantId}.");
            }

            var executorDtos = executors.Select(e => new ExecutorsDtos()
            {
                Id = e.Id,
                ApplicantId = e.ApplicantId,
                ExecutorType = e.ExecutorType,
                FirstName = e.FirstName,
                LastName = e.LastName,
                CompanyName = e.CompanyName,
                PhoneNumber = e.PhoneNumber,
                Address = e.Address,
                City = e.City,
                State = e.State
            }).ToList();

            return Ok(executorDtos); // Returns 200 Ok with the List of executors
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

            var executorDto = new ExecutorsDtos()
            {
                Id = executor.Id,
                ApplicantId = executor.ApplicantId,
                ExecutorType = executor.ExecutorType,
                FirstName = executor.FirstName,
                LastName = executor.LastName,
                CompanyName = executor.CompanyName,
                PhoneNumber = executor.PhoneNumber,
                Address = executor.Address,
                City = executor.City,
                State = executor.State
            };
            return Ok(executorDto); // Returns 200 Ok with the executor data 
        }

        /// <summary>
        /// Updates an existing Executor record for a specific Applicant.
        /// </summary>
        [HttpPut("{applicantId}/executors/{id}")] // e.g., PUT /api/applicants/1/executors/1
        public async Task<IActionResult> UpdateExecutor(int applicantId, int id, [FromBody] UpdateExecutorsDtos executorDto)
        {
            // 2. Check for errors flooded into the ModelState, because of the Data Annotations used
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedExecutor = await _executorService.UpdateExecutorAsync(applicantId, id, executorDto);
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
        public async Task<IActionResult> AddGuardian(int applicantId, [FromBody] CreateGuardiansDtos guardianDto)
        {
            // 1. Validate incoming data using model state
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var addedGuardian = await _guardianService.AddGuardianAsync(applicantId, guardianDto);
            if (addedGuardian == null)
            {
                return NotFound($"Applicant with ID {applicantId} not found.");
            }

            var addedGuardianDto = new GuardiansDtos()
            {
                Id = addedGuardian.Id,
                ApplicantId = addedGuardian.ApplicantId,
                FirstName = addedGuardian.FirstName,
                LastName = addedGuardian.LastName,
                PhoneNumber = addedGuardian.PhoneNumber,
                Address = addedGuardian.Address,
                City = addedGuardian.City,
                State = addedGuardian.State
            };

            // 4. Return success response (201 Created)
            // Use nameof(GetGuardian) to point to the action that retrieves a single guardian by its own ID.
            return CreatedAtAction(nameof(GetGuardian), new { Id = addedGuardianDto.Id }, addedGuardianDto);
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

            var guardianDtos = guardians.Select(g => new GuardiansDtos()
            {
                Id = g.Id,
                ApplicantId = g.ApplicantId,
                FirstName = g.FirstName,
                LastName = g.LastName,
                PhoneNumber = g.PhoneNumber,
                Address = g.Address,
                City = g.City,
                State = g.State
            }).ToList(); 

            return Ok(guardianDtos); // Returns 200 OK with the list of guardians
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

            // CORRECTED: Map the model to a DTO before returning
            var guardianDto = new GuardiansDtos()
            {
                Id = guardian.Id,
                ApplicantId = guardian.ApplicantId,
                FirstName = guardian.FirstName,
                LastName = guardian.LastName,
                PhoneNumber = guardian.PhoneNumber,
                Address = guardian.Address,
                City = guardian.City,
                State = guardian.State
            }; ;

            return Ok(guardianDto); // Returns 200 Ok with the guardian data
        }

        /// <summary>
        /// Updates an existing Guardian record for a specific Applicant.
        /// </summary>
        [HttpPut("{applicantId}/guardians/{id}")]
        public async Task<IActionResult> UpdateGuardian(int applicantId, int id, [FromBody] UpdateGuardiansDtos guardianDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var updatedGuardian = await _guardianService.UpdateGuardianAsync(applicantId, id, guardianDto);
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
        [HttpPost("{applicantId}/paymenttransactions")]
        public async Task<IActionResult> AddPaymentTransaction(int applicantId, [FromBody] CreatePaymentTransactionDto paymentTransactionDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var addedTransaction = await _paymentTransactionService.AddPaymentTransactionAsync(applicantId, paymentTransactionDto);
            if (addedTransaction == null)
            {
                return NotFound($"Applicant with ID {applicantId} not found.");
            }

            var addedTransactionDto = new PaymentTransactionDto
            {
                Id = addedTransaction.Id,
                ApplicantId = addedTransaction.ApplicantId,
                Amount = addedTransaction.Amount,
                Currency = addedTransaction.Currency,
                Status = addedTransaction.Status,
                GatewayReferenceId = addedTransaction.GatewayReferenceId,
                PaymentMethod = addedTransaction.PaymentMethod,
                TransactionDate = addedTransaction.TransactionDate,
                Message = addedTransaction.Message
            };

            return CreatedAtAction(nameof(GetPaymentTransaction), new { id = addedTransactionDto.Id }, addedTransactionDto);
        }

        /// <summary>
        /// Retrieves all PaymentTransaction records for a specific Applicant.
        /// </summary>
        [HttpGet("{applicantId}/paymenttransactions")]
        public async Task<IActionResult> GetPaymentTransactionsForApplicant(int applicantId)
        {
            var transactions = await _paymentTransactionService.GetPaymentTransactionByApplicantIdAsync(applicantId);
            if (!transactions.Any())
            {
                return NotFound($"No payment transactions found for Applicant with ID {applicantId}.");
            }

            var transactionDtos = transactions.Select(t => new PaymentTransactionDto
            {
                Id = t.Id,
                ApplicantId = t.ApplicantId,
                Amount = t.Amount,
                Currency = t.Currency,
                Status = t.Status,
                GatewayReferenceId = t.GatewayReferenceId,
                PaymentMethod = t.PaymentMethod,
                TransactionDate = t.TransactionDate,
                Message = t.Message
            }).ToList();

            return Ok(transactionDtos);
        }

        /// <summary>
        /// Retrieves a single PaymentTransaction record by its own Id.
        /// </summary>
        [HttpGet("paymenttransactions/{id}")]
        public async Task<IActionResult> GetPaymentTransaction(int id)
        {
            var transaction = await _paymentTransactionService.GetPaymentTransactionAsync(id);

            if (transaction == null)
            {
                return NotFound($"Payment Transaction with ID {id} not found.");
            }

            var transactionDto = new PaymentTransactionDto
            {
                Id = transaction.Id,
                ApplicantId = transaction.ApplicantId,
                Amount = transaction.Amount,
                Currency = transaction.Currency,
                Status = transaction.Status,
                GatewayReferenceId = transaction.GatewayReferenceId,
                PaymentMethod = transaction.PaymentMethod,
                TransactionDate = transaction.TransactionDate,
                Message = transaction.Message
            };

            return Ok(transactionDto);
        }

        /// <summary>
        /// Updates an existing PaymentTransaction record for a specific Applicant.
        /// </summary>
        [HttpPut("{applicantId}/paymenttransactions/{id}")]
        public async Task<IActionResult> UpdatePaymentTransaction(int applicantId, int id, [FromBody] UpdatePaymentTransactionDto paymentTransactionDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var updatedTransaction = await _paymentTransactionService.UpdatePaymentTransactionAsync(applicantId, id, paymentTransactionDto);
            if (updatedTransaction == null)
            {
                return NotFound($"Payment Transaction with ID {id} not found or does not belong to Applicant ID {applicantId}.");
            }

            return NoContent();
        }

        /// <summary>
        /// Deletes a PaymentTransaction record for a specific Applicant.
        /// </summary>
        [HttpDelete("{applicantId}/paymenttransactions/{id}")]
        public async Task<IActionResult> DeletePaymentTransaction(int applicantId, int id)
        {
            var isDeleted = await _paymentTransactionService.DeletePaymentTransactionAsync(applicantId, id);
            if (!isDeleted)
            {
                return NotFound($"Payment Transaction with ID {id} not found or does not belong to Applicant ID {applicantId}.");
            }

            return NoContent();
        }
    }
}
