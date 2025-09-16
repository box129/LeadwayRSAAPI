using Leadway_RSA_API.DTOs;
using Leadway_RSA_API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;

namespace Leadway_RSA_API.Controllers
{
    [Route("api/registration")] // Unique route for applicant registration
    [ApiController]
    public class ApplicantRegistrationController : ControllerBase
    {
        private readonly IApplicantService _applicantService;
        private readonly IPersonalDetailsService _personalDetailsService;
        private readonly IIdentificationService _identificationService;
        private readonly IBeneficiaryService _beneficiaryService;
        private readonly IAssetService _assetService;
        private readonly IAssetAllocationService _assetAllocationService;
        private readonly IExecutorService _executorService;
        private readonly IGuardianService _guardianService;

        private readonly IPaymentTransactionService _paymentTransactionService;
        private readonly IOtpService _otpService;

        private readonly IRegistrationService _registrationService;


        public ApplicantRegistrationController(IApplicantService applicantService, IPersonalDetailsService personalDetails, IIdentificationService identificationService, IAssetService assetService, IBeneficiaryService beneficiaryService, IAssetAllocationService assetAllocationService, IExecutorService executorService, IGuardianService guardianService, IPaymentTransactionService paymentTransactionService, IOtpService otpService, IRegistrationService registrationService)
        {
            _applicantService = applicantService;
            _personalDetailsService = personalDetails;
            _identificationService = identificationService;
            _beneficiaryService = beneficiaryService;
            _assetService = assetService;
            _assetAllocationService = assetAllocationService;
            _executorService = executorService;
            _guardianService = guardianService;
            _paymentTransactionService = paymentTransactionService;
            _otpService = otpService;
            _registrationService = registrationService;
        }

        /// <summary>
        /// Creates a new Applicant record (Initial step for personal details).
        /// </summary>
        [HttpPost("Start")]
        public async Task<IActionResult> StartRegistration([FromBody] CreateApplicantDto applicantDto)
        {
            // Controller's job: Check validation.
            // --- Server-Side Validation (based on Data Annotation in your model) ---
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Pass the DTO to the service. The service is now responsible for the mapping.
            // This is a cleaner approach as the service owns the business logic for creating the model.
            var newApplicant = await _applicantService.CreateApplicantAsync(applicantDto);

            // Controller's job: Handle the service's result and return the appropriate response.
            if (newApplicant == null)
            {
                ModelState.AddModelError("EmailAddress", "An applicant with this email address already exists.");
                return Conflict(ModelState); // HTTP 409 Conflict
            }

            var registrationKey = await _registrationService.GenerateAndSaveKey(newApplicant.Id);

            return Ok(new { ApplicationId = newApplicant.Id, registrationKey });
        }


        // --- New Endpoint for Sponsored Registration ---


        /// <summary>
        /// Starts the sponsored registration flow by submitting an email and sponsorship key.
        /// An OTP will be sent to the provided email for verification.
        /// </summary>
        [HttpPost("sponsored/submit-email")]
        public async Task<IActionResult> SubmitSponsoredEmail([FromBody] SubmitEmailDto emailDto)
        {
            // 1. Validate the one-time sponsorship key.
            var isValidSponsorshipKey = await _registrationService.ValidateSponsorshipKeyAsync(emailDto.SponsorshipKey);
            if (!isValidSponsorshipKey)
            {
                return Unauthorized("Invalid or expired sponsorship key.");
            }

            // 2. Validate the incoming DTO.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 3. Send the OTP to the provided email.
            var otpSent = await _otpService.SendOtpAsync(emailDto.EmailAddress);
            if (!otpSent)
            {
                // This could be due to a service error or email already in use for a different reason.
                ModelState.AddModelError("Email", "Could not send OTP. Please try again.");
                return StatusCode(StatusCodes.Status500InternalServerError, ModelState);
            }

            return Ok("An OTP has been sent to your email address. Please verify to continue.");
        }

        /// <summary>
        /// Verifies the OTP sent to the email and, upon success, provides the applicant's registration key.
        /// </summary>
        [HttpPost("sponsored/verify-otp")]
        public async Task<IActionResult> VerifySponsoredOtp([FromBody] VerifyOtpDto otpDto)
        {
            // 1. Validate the incoming DTO.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 2. Validate the one-time sponsorship key again.
            var isValidSponsorshipKey = await _registrationService.ValidateSponsorshipKeyAsync(otpDto.SponsorshipKey);
            if (!isValidSponsorshipKey)
            {
                return Unauthorized("Invalid or expired sponsorship key.");
            }

            // 3. Verify the OTP.
            var isOtpValid = await _otpService.VerifyOtpAsync(otpDto.Email, otpDto.Otp);
            if (!isOtpValid)
            {
                return BadRequest("Invalid OTP. Please try again.");
            }

            // 4. Find the applicant record that was created in the "Start" step.
            var existingApplicant = await _applicantService.FindApplicantByEmailAsync(otpDto.Email);

            if (existingApplicant == null)
            {
                // This indicates a flow error, as the applicant should already exist.
                return NotFound("Applicant not found. Please start the registration process again.");
            }

            // 5. Generate a unique registration key for all future steps for this user.
            var registrationKey = await _registrationService.GenerateAndSaveKey(existingApplicant.Id);

            // 6. Return the existing applicant's ID and the new registration key.
            return Ok(new { ApplicationId = existingApplicant.Id, registrationKey });
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

            // Logic to check if the applicantId is valid or authorized for this user
            // would go here (e.g., using a registration key). For now, we assume it's valid.

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

            return CreatedAtAction(nameof(AddPaymentTransaction), new { id = addedTransactionDto.Id }, addedTransactionDto);
        }


        /// <summary>
        /// Allows the applicant to create their personal details using a secure registration key.
        /// </summary>
        [HttpPost("add-personal-details")]
        public async Task<IActionResult> CreatePersonalDetails([FromHeader(Name = "X-Registration-Key")] string key, [FromBody] CreatePersonalDetailsDto detailsDto)
        {
            var applicantId = await _registrationService.ValidateKey(key);
            if (applicantId == null)
            {
                return Unauthorized("Invalid or expired registration key.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newDetails = await _personalDetailsService.CreatePersonalDetailsAsync(applicantId.Value, detailsDto);
            if (newDetails == null)
            {
                // Correctly handles the case where details already exist.
                return BadRequest("Personal details for this applicant already exist.");
            }

            var personalDetailsDto = new PersonalDetailsDto
            {
                Id = newDetails.Id,
                ApplicantId = newDetails.ApplicantId,
                PlaceOfBirth = newDetails.PlaceOfBirth,
                Religion = newDetails.Religion,
                Gender = newDetails.Gender,
                HomeAddress = newDetails.HomeAddress,
                State = newDetails.State,
                City = newDetails.City
            };

            return Created($"/api/registration/personal-details/{personalDetailsDto.Id}", personalDetailsDto);
        }


        // --- File Upload Endpoints ---

        /// <summary>
        /// Allows the applicant to upload or update their passport photo.
        /// </summary>
        [HttpPut("passport-photo")]
        public async Task<IActionResult> UploadOrUpdatePassportPhoto([FromHeader(Name = "X-Registration-Key")] string key, [FromForm] PassportPhotoDto photoDto)
        {
            var applicantId = await _registrationService.ValidateKey(key);
            if (applicantId == null)
            {
                return Unauthorized("Invalid or expired registration key.");
            }

            if (photoDto.file == null || photoDto.file.Length == 0)
            {
                return BadRequest("File is empty or not provided.");
            }

            // This single service call now handles both create and update logic.
            var result = await _personalDetailsService.UploadOrUpdatePassportPhotoAsync(applicantId.Value, photoDto.file);
            if (!result)
            {
                return BadRequest("Could not upload or update passport photo.");
            }

            return Ok("Passport photo uploaded or updated successfully.");
        }

        /// <summary>
        /// Allows the applicant to upload or update their signature.
        /// </summary>
        [HttpPut("signature")]
        public async Task<IActionResult> UploadOrUpdateSignature([FromHeader(Name = "X-Registration-Key")] string key, [FromForm] SignatureDto signatureDto)
        {
            var applicantId = await _registrationService.ValidateKey(key);
            if (applicantId == null)
            {
                return Unauthorized("Invalid or expired registration key.");
            }

            if (signatureDto.file == null || signatureDto.file.Length == 0)
            {
                return BadRequest("File is empty or not provided.");
            }

            // This single service call now handles both create and update logic.
            var result = await _personalDetailsService.UploadOrUpdateSignatureAsync(applicantId.Value, signatureDto.file);
            if (!result)
            {
                return BadRequest("Could not upload or update signature.");
            }

            return Ok("Signature uploaded or updated successfully.");
        }


        /// <summary>
        /// Adds a new Identification record, including the image, for a specific Applicant.
        /// </summary>
        [HttpPost("identifications")] // e.g., POST /api/applicants/1/identifications
        public async Task<IActionResult> AddIdentification([FromHeader(Name = "X-Registration-Key")] string key, [FromForm] CreateIdentificationDto identificationDto, [FromForm] DriversLicensePhotoDto driversLicensePhotoDto)
        {
            var applicantId = await _registrationService.ValidateKey(key);
            if (applicantId == null)
            {
                return Unauthorized("Invalid or expired registration key.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (driversLicensePhotoDto.file == null || driversLicensePhotoDto.file.Length == 0)
            {
                return BadRequest("File is empty or not provided.");
            }

            var addedIdentification = await _identificationService.AddIdentificationAsync(applicantId.Value, identificationDto, driversLicensePhotoDto.file);
            if (addedIdentification == null)
            {
                return BadRequest($"Could not add identification for applicant ID {applicantId}. It may already exist.");
            }

            var addedIdentificationDto = new IdentificationDto
            {
                Id = addedIdentification.Id,
                ApplicantId = addedIdentification.ApplicantId,
                IdentificationType = addedIdentification.IdentificationType.ToString(),
                DocumentNumber = addedIdentification.DocumentNumber,
                ImagePath = addedIdentification.ImagePath,
                UploadDate = addedIdentification.UploadDate
            };

            return Created($"api/registration/identifications/{addedIdentificationDto.Id}", addedIdentificationDto);
        }



        /// <summary>
        /// Adds a new Beneficiary record for a specific Applicant.
        /// </summary>
        [HttpPost("beneficiaries")] // e.g., POST /api/applicants/1/beneficiaries
        public async Task<IActionResult> AddBeneficiary([FromHeader(Name = "X-Registration-Key")] string key, [FromBody] CreateBeneficiaryDto beneficiaryDto)
        {
            var applicantId = await _registrationService.ValidateKey(key);
            if (applicantId == null)
            {
                return Unauthorized("Invalid or expired registration key.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var addedBeneficiary = await _beneficiaryService.AddBeneficiaryAsync(applicantId.Value, beneficiaryDto);
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

            return Created($"api/registration/beneficiaries/{addedBeneficiaryDto.Id}", addedBeneficiaryDto);
        }



        /// <summary>
        /// Adds a new Asset record for a specific Applicant.
        /// </summary>
        [HttpPost("assets")] // POST /api/applicants/1/assets
        public async Task<IActionResult> AddAsset([FromHeader(Name = "X-Registration-Key")] string key, [FromBody] CreateAssetDto assetDto)
        {
            var applicantId = await _registrationService.ValidateKey(key);
            if (applicantId == null)
            {
                return Unauthorized("Invalid or expired registration key.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var addedAsset = await _assetService.AddAssetAsync(applicantId.Value, assetDto);
            // NOTE: This check has been added to handle the case where the applicantId doesn't exist.
            if (addedAsset == null)
            {
                return NotFound($"Applicant with ID {applicantId} not found.");
            }

            var addedAssetDto = new AssetDto
            {
                Id = addedAsset.Id,
                ApplicantId = addedAsset.ApplicantId,
                Name = addedAsset.Name,
                RSAPin = addedAsset.RSAPin,
                PFA = addedAsset.PFA,
                SalaryBankName = addedAsset.SalaryBankName,
                SalaryAccountNumber = addedAsset.SalaryAccountNumber,
            };

            return Created($"api/registration/assets/{addedAssetDto.Id}", addedAssetDto);
        }



        /// <summary>
        /// Retrieves the beneficiaries for the applicant identified by the registration key.
        /// This is a public-facing endpoint for a non-authenticated applicant.
        /// </summary>
        [HttpGet("beneficiaries")]
        public async Task<IActionResult> GetBeneficiariesForApplicant([FromHeader(Name = "X-Registration-Key")] string key)
        {
            // 1. Validate the registration key. This identifies the applicant.
            var applicantId = await _registrationService.ValidateKey(key);
            if (applicantId == null)
            {
                return Unauthorized("Invalid or expired registration key.");
            }

            // 2. Use the valid applicantId to retrieve the beneficiaries.
            var beneficiaries = await _beneficiaryService.GetBeneficiariesByApplicantIdAsync(applicantId.Value);

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

            return Ok(beneficiaryDtos);
        }

        /// <summary>
        /// Retrieves all Asset records for a specific Applicant.
        /// </summary>
        [HttpGet("assets")] // e.g., GET /api/applicants/1/assets
        public async Task<IActionResult> GetAssetsForApplicant([FromHeader(Name = "X-Registration-Key")] string key)
        {
            // 1. Validate the registration key. This identifies the applicant.
            var applicantId = await _registrationService.ValidateKey(key);
            if (applicantId == null)
            {
                return Unauthorized("Invalid or expired registration key.");
            }

            var assets = await _assetService.GetAssetsByApplicantIdAsync(applicantId.Value);
            if (!assets.Any())
            {
                return NotFound($"No assets found for Applicant with ID {applicantId}");
            }

            var assetDtos = assets.Select(a => new AssetDto
            {
                Id = a.Id,
                ApplicantId = a.ApplicantId,
                Name = a.Name,
                RSAPin = a.RSAPin,
                PFA = a.PFA,
                SalaryBankName = a.SalaryBankName,
                SalaryAccountNumber = a.SalaryAccountNumber,
            }).ToList();

            return Ok(assetDtos); // Returns 200 Ok with the list of assets
        }



        /// <summary>
        /// Creates a new asset allocation for a specific applicant.
        /// Requires existing AssetId and BeneficiaryId that belong to the specified Applicant.
        /// </summary>
        [HttpPost("assetallocations")] // e.g., POST /api/applicants/1/assetallocations
        public async Task<IActionResult> AddAssetAllocation([FromHeader(Name = "X-Registration-Key")] string key, [FromBody] CreateAssetAllocationDto allocationDto)
        {
            var applicantId = await _registrationService.ValidateKey(key);
            if (applicantId == null)
            {
                return Unauthorized("Invalid or expired registration key.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var addedAllocation = await _assetAllocationService.AddAssetAllocationAsync(applicantId.Value, allocationDto);
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

            return Created($"api/registration/assetallocations/{addedAllocationDto.Id}", addedAllocationDto);
        }



        /// <summary>
        /// Adds a new Executor record for a specific Applicant.
        /// </summary>
        [HttpPost("executors")]
        public async Task<IActionResult> AddExecutor([FromHeader(Name = "X-Registration-Key")] string key, [FromBody] CreateExecutorsDtos executorDto)
        {
            var applicantId = await _registrationService.ValidateKey(key);
            if (applicantId == null)
            {
                return Unauthorized("Invalid or expired registration key.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var addedExecutor = await _executorService.AddExecutorAsync(applicantId.Value, executorDto);
            if (addedExecutor == null)
            {
                return NotFound($"Applicant with ID {applicantId} not found.");
            }

            var addedExecutorDto = new ExecutorsDtos
            {
                Id = addedExecutor.Id,
                ApplicantId = addedExecutor.ApplicantId,
                IsDefault = addedExecutor.IsDefault, // Map the new property
                // Map either the Name or the ExecutorType based on IsDefault
                Name = addedExecutor.IsDefault ? addedExecutor.Name : null,
                ExecutorType = addedExecutor.IsDefault ? null : addedExecutor.ExecutorType.ToString(),
                FirstName = addedExecutor.FirstName,
                LastName = addedExecutor.LastName,
                CompanyName = addedExecutor.CompanyName,
                PhoneNumber = addedExecutor.PhoneNumber,
                Address = addedExecutor.Address,
                City = addedExecutor.City,
                State = addedExecutor.State
            };

            return Created($"api/registration/executors/{addedExecutorDto.Id}", addedExecutorDto);
        }



        /// <summary>
        /// Adds a new Guardian record for a specific Applicant.
        /// </summary>
        [HttpPost("guardians")] // e.g., POST api/applicants/1/guardians
        public async Task<IActionResult> AddGuardian([FromHeader(Name = "X-Registration-Key")] string key, [FromBody] CreateGuardiansDtos guardianDto)
        {
            var applicantId = await _registrationService.ValidateKey(key);
            if (applicantId == null)
            {
                return Unauthorized("Invalid or expired registration key.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var addedGuardian = await _guardianService.AddGuardianAsync(applicantId.Value, guardianDto);
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
                Relationship = guardianDto.Relationship,
                Address = addedGuardian.Address,
                City = addedGuardian.City,
                State = addedGuardian.State
            };

            return Created($"api/registration/guardians/{addedGuardianDto.Id}", addedGuardianDto);
        }




        /// <summary>
        /// Allows the applicant to update their personal details using a secure registration key.
        /// </summary>
        [HttpPut("personal-details")]
        public async Task<IActionResult> UpdatePersonalDetails([FromHeader(Name = "X-Registration-Key")] string key, [FromBody] UpdatePersonalDetailsDto detailsDto)
        {
            var applicantId = await _registrationService.ValidateKey(key);
            if (applicantId == null)
            {
                return Unauthorized("Invalid or expired registration key.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedDetails = await _personalDetailsService.UpdatePersonalDetailsAsync(applicantId.Value, detailsDto);
            if (updatedDetails == null)
            {
                return NotFound("Personal details not found for this applicant.");
            }

            return Ok(updatedDetails);
        }

        /// <summary>
        /// Allows the applicant to update an existing identification record, including the image if provided.
        /// </summary>
        [HttpPut("identifications/{id}")]
        public async Task<IActionResult> UpdateIdentification([FromHeader(Name = "X-Registration-Key")] string key, int id, [FromForm] UpdateIdentificationDto identificationDto, [FromForm] DriversLicensePhotoDto driversLicensePhotoDto)
        {
            var applicantId = await _registrationService.ValidateKey(key);
            if (applicantId == null)
            {
                return Unauthorized("Invalid or expired registration key.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (driversLicensePhotoDto.file == null || driversLicensePhotoDto.file.Length == 0)
            {
                return BadRequest("File is empty or not provided.");
            }

            var updatedIdentification = await _identificationService.UpdateIdentificationAsync(applicantId.Value, id, identificationDto, driversLicensePhotoDto.file);
            if (updatedIdentification == null)
            {
                return NotFound("Identification record not found for this applicant.");
            }

            var updatedIdentificationDto = new IdentificationDto
            {
                Id = updatedIdentification.Id,
                ApplicantId = updatedIdentification.ApplicantId,
                IdentificationType = updatedIdentification.IdentificationType.ToString(),
                DocumentNumber = updatedIdentification.DocumentNumber,
                ImagePath = updatedIdentification.ImagePath,
                UploadDate = updatedIdentification.UploadDate
            };

            return Ok(updatedIdentificationDto);
        }

        /// <summary>
        /// Updates an existing Beneficiary record for a specific Applicant.
        /// </summary>
        [HttpPut("beneficiaries/{id}")] // e.g., PUT api/applicnats/1/beneficiaries/1
        public async Task<IActionResult> UpdateBeneficiary([FromHeader(Name = "X-Registration-Key")] string key, int id, [FromBody] UpdateBeneficiaryDto beneficiaryDto)
        {
            var applicantId = await _registrationService.ValidateKey(key);
            if (applicantId == null)
            {
                return Unauthorized("Invalid or expired registration key.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var updatedBeneficiary = await _beneficiaryService.UpdateBeneficiaryAsync(applicantId.Value, id, beneficiaryDto);
            if (updatedBeneficiary == null)
            {
                return NotFound($"Beneficiary with ID {id} not found or does not belong to Applicant ID {applicantId}");
            }

            // 6. Return 204 No Content for successful update.
            return NoContent();
        }



        /// <summary>
        /// Updates an existing Asset record for a specific Applicant.
        /// </summary>
        [HttpPut("assets/{id}")] // e.g., PUT /api/applicants/1/assets/1
        public async Task<IActionResult> UpdateAsset([FromHeader(Name = "X-Registration-Key")] string key, int id, [FromBody] UpdateAssetDto assetDto)
        {
            var applicantId = await _registrationService.ValidateKey(key);
            if (applicantId == null)
            {
                return Unauthorized("Invalid or expired registration key.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var updatedAsset = await _assetService.UpdateAssetAsync(applicantId.Value, id, assetDto);
            if (updatedAsset == null)
            {
                return NotFound($"Asset with ID {id} not found or does not belong to Applicant ID {applicantId}");
            }

            // 5. Return 204 for successful operation
            return NoContent();
        }

        /// <summary>
        /// Updates an existing BeneficiaryAssetAllocation record for a specific Applicant.
        /// </summary>
        [HttpPut("assetallocations/{id}")]
        public async Task<IActionResult> UpdateBeneficiaryAssetAllocation([FromHeader(Name = "X-Registration-Key")] string key, int id, [FromBody] UpdateAssetAllocationDto allocationDto)
        {
            var applicantId = await _registrationService.ValidateKey(key);
            if (applicantId == null)
            {
                return Unauthorized("Invalid or expired registration key.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedAllocation = await _assetAllocationService.UpdateBeneficiaryAssetAllocationAsync(applicantId.Value, id, allocationDto);
            if (updatedAllocation == null)
            {
                return NotFound($"Beneficiary Asset Allocation with ID {id} not found or does not belong to Applicant ID {applicantId}");
            }

            return NoContent();
        }

        /// <summary>
        /// Updates an existing Executor record for a specific Applicant.
        /// </summary>
        [HttpPut("executors/{id}")] // e.g., PUT /api/applicants/1/executors/1
        public async Task<IActionResult> UpdateExecutor([FromHeader(Name = "X-Registration-Key")] string key, int id, [FromBody] UpdateExecutorsDtos executorDto)
        {
            var applicantId = await _registrationService.ValidateKey(key);
            if (applicantId == null)
            {
                return Unauthorized("Invalid or expired registration key.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedExecutor = await _executorService.UpdateExecutorAsync(applicantId.Value, id, executorDto);
            if (updatedExecutor == null)
            {
                return NotFound($"Executor with ID {id} not found or does not belong to Applicant ID {applicantId}.");
            }

            return NoContent();
        }

        /// <summary>
        /// Updates an existing Guardian record for a specific Applicant.
        /// </summary>
        [HttpPut("guardians/{id}")]
        public async Task<IActionResult> UpdateGuardian([FromHeader(Name = "X-Registration-Key")] string key, int id, [FromBody] UpdateGuardiansDtos guardianDto)
        {
            var applicantId = await _registrationService.ValidateKey(key);
            if (applicantId == null)
            {
                return Unauthorized("Invalid or expired registration key.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var updatedGuardian = await _guardianService.UpdateGuardianAsync(applicantId.Value, id, guardianDto);
            if (updatedGuardian == null)
            {
                return NotFound($"Guardian with ID {id} not found or does not belong to Applicant ID {applicantId}.");
            }

            return NoContent();
        }

        [HttpPost("resend-key")]
        public async Task<IActionResult> ResendRegistrationKey([FromBody] ResendKeyDto dto)
        {
            // The service handles all the business logic and secure key delivery.
            var success = await _registrationService.ResendRegistrationKeyAsync(dto.EmailAddress);

            if (success)
            {
                // Return a generic success message to avoid giving away information to a malicious actor.
                return Ok("If an active registration exists, a link has been sent to the provided email address.");
            }

            // We can still return a generic success message to prevent user enumeration attacks.
            return Ok("If an active registration exists, a link has been sent to the provided email address.");
        }
    }
}
