using Leadway_RSA_API.Data;
using Leadway_RSA_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Transactions;
using System.Diagnostics.Eventing.Reader;

namespace Leadway_RSA_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // /api/applicants
    public class ApplicantsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ApplicantsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a new Applicant record (Initial step for personal details).
        /// </summary>
        /// <param name="applicant">The Applicant object containing personal details.</param>
        /// <returns>A 201 Created response with the newly created Applicant or a 400 Bad Request.</returns>

        [HttpPost]
        public async Task<IActionResult> CreateApplicant([FromBody] Applicant applicant)
        {
            // --- Server-Side Validation (based on Data Annotation in your model) ---
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // --- Business Logic & Data Assignment ---
            // Set initial audit fields (though DbContext override also handles CreatedDate)
            applicant.CreatedDate = DateTime.UtcNow;
            applicant.LastModifiedDate = DateTime.UtcNow;
            applicant.CurrentStep = 1; // Explicitly set current step for a new applicant
            applicant.IsComplete = false;

            try
            {
                // Add the new applicant to the DbContext
                _context.Applicants.Add(applicant);
                // Save changes to the database
                await _context.SaveChangesAsync();

                // --- Response ---
                // Return a 201 Created response.
                // The 'CreatedAtAction' method automatically sets the Location header
                return CreatedAtAction(nameof(GetApplicant), new { id = applicant.Id }, applicant);
            }
            catch (DbUpdateException ex)
            {
                // Handle potential database errors, e.g., unique constraint violations
                // For 'EmailAddress' unique constraint:
                if (ex.InnerException?.Message.Contains("duplicate key value violates unique constraint") == true &&
                    ex.InnerException?.Message.Contains("IX_Applicants_EmailAddress") == true)
                {
                    ModelState.AddModelError("EmailAddress", "An applicant with this email address already exists.");
                    return Conflict(ModelState); // HTTP 409 Conflict
                }

                // Log the exception for debugging purposes (in a real app)
                // _logger.LogError(ex, "Error creating applicant.");

                // For generic database errors, return a 500 Internal Server Error
                return StatusCode(500, "An error occurred while saving the applicant. Please try again.");
            }
            catch (Exception)
            {
                // Catch any other unexpected errors
                // _logger.LogError(ex, "An unexpected error occurred.");
                return StatusCode(500, "An unexpected error occurred. Please try again.");
            }
        }

        /// <summary>
        /// Retrieves an Applicant by their Id.
        /// </summary>
        /// <param name="id">The Id of the Applicant.</param>
        /// <returns>A 200 OK response with the Applicant, or 404 Not Found.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetApplicant(int id)
        {
            // Find the applicant by Id. Use FindAsync for primary key lookups, or FirstOrDefaultAsync.
            var applicant = await _context.Applicants.FindAsync(id);

            if (applicant == null)
            {
                return NotFound($"Applicant with ID {id} not found.");
            }

            return Ok(applicant); // HTTP 200 OK with the applicant data
        }

        // You'll add more GET/PUT/POST endpoints for other steps (e.g., /api/applicants/{id}/identifications) here
        // as we progress through the phases.


        /// <summary>
        /// Adds a new Identification record for a specific Applicant.
        /// </summary>
        /// <param name="applicantId">The ID of the Applicant.</param>
        /// <param name="identification">The Identification object to add.</param>
        /// <returns>A 201 Created response or 400 Bad Request/404 Not Found.</returns>
        [HttpPost("{applicantId}/identifications")] // e.g., POST /api/applicants/1/identifications
        public async Task<IActionResult> AddIdentification(int applicantId, [FromBody] Identification identification)
        {
            // 1. Validate incoming data using model state
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Returns 400 with validation errors
            }

            // 2. Check if the Applicant exists
            var applicant = await _context.Applicants.FindAsync(applicantId);
            if (applicant == null)
            {
                return NotFound($"Applicant with ID {applicantId} not found."); // Returns 404
            }

            // 3. Associate Identification with Applicant and set default values
            identification.ApplicantId = applicantId; // Ensure the FK is correctly set
            identification.UploadDate = DateTime.UtcNow; // Set upload date on the server

            try
            {
                _context.Identifications.Add(identification);
                await _context.SaveChangesAsync();

                // Optional: Update the Applicant's LastModifiedDate as well
                applicant.LastModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync(); // Save the update to the applicant

                // 4. Return success response (201 Created)
                // Use nameof(GetIdentification) to point to the action that retrieves a single identification by its own ID.
                return CreatedAtAction(nameof(GetIdentification), new { id = identification.Id }, identification);
            }
            catch (DbUpdateException ex)
            {
                // Log the exception for debugging (in a real app, use a logger)
                // _logger.LogError(ex, "Error adding identification.");
                return StatusCode(500, $"An error occurred while saving the identification: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Catch any other unexpected errors
                // _logger.LogError(ex, "An unexpected error occurred.");
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all Identification records for a specific Applicant.
        /// </summary>
        /// <param name="applicantId">The ID of the Applicant.</param>
        /// <returns>A 200 OK response with a list of Identifications, or 404 Not Found.</returns>
        [HttpGet("{applicantId}/identifications")] // e.g., GET /api/applicants/1/identifications
        public async Task<IActionResult> GetIdentificationsForApplicant(int applicantId)
        {
            // Check if the Applicant exists
            var applicant = await _context.Applicants.FindAsync(applicantId);
            if (applicant == null)
            {
                return NotFound($"Applicant with ID {applicantId} not found.");
            }

            // Retrieve identifications for the given applicant. Use ToListAsync() to execute the query.
            var identifications = await _context.Identifications
                .Where(i => i.ApplicantId == applicantId)
                .ToListAsync();

            // --- ADD THIS BLOCK ---
            // If lazy loading is enabled, the Applicant property will be populated.
            // Explicitly set it to null before returning to prevent serialization cycles.
            foreach (var id in identifications)
            {
                id.Applicant = null;
            }
            // --- END OF ADDED BLOCK ---

            // If no identifications are found, return a 404.
            if (identifications == null || !identifications.Any())
            {
                return NotFound($"No identifications found for Applicant with ID {applicantId}");
            }
            return Ok(identifications); // Returns 200 OK with the list of identifications
        }

        /// <summary>
        /// Retrieves a single Identification record by its own Id.
        /// </summary>
        /// <param name="id">The ID of the Identification record.</param>
        /// <returns>A 200 OK response with the Identification, or 404 Not Found.</returns>
        [HttpGet("identifications/{id}")] // e.g., GET /api/applicants/identifications/1
        public async Task<IActionResult> GetIdentification(int id)
        {
            // Find the identification by its primary key
            var identification = await _context.Identifications.FindAsync(id);

            if (identification == null)
            {
                return Ok($"Identification with ID {id} not found.");
            }

            return Ok(identification); // Returns 200 OK with the identification data
        }

        /// <summary>
        /// Updates an existing Identification record for a specific Applicant.
        /// </summary>
        /// <param name="applicantId">The ID of the Applicant to whom the Identification belongs.</param>
        /// <param name="id">The ID of the Identification record to update.</param>
        /// <param name="identification">The Identification object with updated data.</param>
        /// <returns>A 204 No Content response if successful, 400 Bad Request, or 404 Not Found.</returns>
        [HttpPut("{applicantId}/identifications/{id}")] // e,g., PUT api/applicants/1/identifications/1
        public async Task<IActionResult> UpdateIdentification(int applicantId, int id, [FromBody] Identification identification)
        {
            // 1. Validate: Ensure IDs in route and body match, and ApplicantId matches.
            if (id != identification.Id)
            {
                return BadRequest("Identification ID in the route does not match the ID in the request body.");
            }
            if (applicantId != identification.ApplicantId)
            {
                return BadRequest("Applicant ID in the route does not match the ApplicantId in the request body.");
            }

            // 2. Validate incoming data using model state
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 3. Find the existing Identification record in the database
            var existingIdentification = await _context.Identifications.FirstOrDefaultAsync(i => i.Id == id && i.ApplicantId == applicantId);

            if (existingIdentification == null)
            {
                // Identification not found OR it doesn't belong to the specified applicant
                return NotFound($"Identification with ID {id} not found or does not belong to Applicant ID {applicantId}.");
            }

            // 4. Update the properties of the *existing tracked entity*.
            _context.Entry(existingIdentification).CurrentValues.SetValues(identification);

            try
            {
                // 5. Save changes to the database.
                await _context.SaveChangesAsync();

                // Optional: Update the Applicant's LastModifiedDate as well
                var applicant = await _context.Applicants.FindAsync(applicantId);
                if (applicant != null)
                {
                    applicant.LastModifiedDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync(); // Save the update to the applicant
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!IdentificationExists(id, applicantId)) // Helper to check if it exists and belongs to applicant
                {
                    return NotFound($"Identification with ID {id} not found or does not belong to Applicant ID {applicantId}.");
                }
                else
                {
                    throw; // A real concurrency conflict occurred.
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating the identification: {ex.Message}. Inner Exception: {ex.InnerException?.Message}");
            }

            // 6. Return 204 No Content for successful update.
            return NoContent();
        }

        // Helper method (add this inside your ApplicantsController class if not already there)
        // Make sure you have this helper:
        private bool IdentificationExists(int id, int applicantId)
        {
            return _context.Identifications.Any(e => e.Id == id && e.ApplicantId == applicantId);
        }

        /// <summary>
        /// Deletes an Identification record for a specific Applicant.
        /// </summary>
        /// <param name="applicantId">The ID of the Applicant to whom the Identification belongs.</param>
        /// <param name="id">The ID of the Identification record to delete.</param>
        /// <returns>A 204 No Content response if successful, or 404 Not Found.</returns>
        [HttpDelete("{applicantId}/identifications/{id}")] // e.g., DELETE /api/applicants/1/identifications/1
        public async Task<IActionResult> DeleteIdentification(int applicantId, int id)
        {
            // 1. Find the Identification to delete, ensuring it belongs to the specified Applicant.
            var identification = await _context.Identifications.FirstOrDefaultAsync(i => i.Id == id && i.ApplicantId == applicantId);

            if (identification == null)
            {
                return NotFound($"Identification with ID {id} not found or does not belong to Applicant ID {applicantId}.");
            }

            try
            {
                // 2. Remove the Identification. EF Core will handle removal from the database.
                _context.Identifications.Remove(identification);
                await _context.SaveChangesAsync();

                // Optional: Update the Applicant's LastModifiedDate as well
                var applicant = await _context.Applicants.FindAsync(applicantId);
                if (applicant != null)
                {
                    applicant.LastModifiedDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync(); // Save the update to the applicant
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while deleting the identification: {ex.Message}. Inner Exception: {ex.InnerException?.Message}");
            }

            // 3. Return 204 No Content for successful deletion.
            return NoContent();
        }




        /// <summary>
        /// Adds a new Beneficiary record for a specific Applicant.
        /// </summary>
        /// <param name="applicantId">The ID of the Applicant.</param>
        /// <param name="beneficiary">The Beneficiary object to add.</param>
        /// <returns>A 201 Created response or 400 Bad Request/404 Not Found/500 Internal Server Error.</returns>
        [HttpPost("{applicantId}/beneficiaries")] // e.g., POST /api/applicants/1/beneficiaries
        public async Task<IActionResult> AddBeneficiary(int applicantId, [FromBody] Beneficiary beneficiary)
        {
            // 1. Validate incoming data using model state
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Returns 400 with validation errors
            }

            // 2. Check if th Applicant exists
            var applicant = await _context.Applicants.FindAsync(applicantId);
            if (applicant == null)
            {
                return NotFound($"Applicant with ID {applicantId} not found."); // Returns 404
            }

            // 3. Associate Beneficiary with Applicant
            beneficiary.ApplicantId = applicantId; // Ensure the foreign Key is correctly set

            // We are only creating the Beneficiary itself in this POST request.
            // Asset allocations are handled separately.
            beneficiary.AssetAllocations = new List<BeneficiaryAssetAllocation>();

            try
            {
                _context.Beneficiaries.Add(beneficiary);
                await _context.SaveChangesAsync();

                // Optional: Update the Applicant's LastModifiedDate as well
                applicant.LastModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync(); // Save the update to the applicant

                // 4. Return success response (201 Created)
                // Use nameof(GetBeneficiary) to point to the action that retrieves a single beneficiary by its own ID.
                return CreatedAtAction(nameof(GetBeneficiary), new { id = beneficiary.Id }, beneficiary);
            }
            catch (DbUpdateException ex)
            {
                // Handle potential database errors (e.g., if there were unique constraints)
                return StatusCode(500, $"An error occurred while saving the beneficiary: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Catch any other unexpected errors
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all Beneficiary records for a specific Applicant.
        /// </summary>
        /// <param name="applicantId">The ID of the Applicant.</param>
        /// <returns>A 200 OK response with a list of Beneficiaries, or 404 Not Found.</returns>
        [HttpGet("{applicantId}/beneficiaries")] // e.g., GET /api/applicants/1/beneficiaries
        public async Task<IActionResult> GetBeneficiariesForApplicant(int applicantId)
        {
            // Check if the Applicant exists
            var applicant = await _context.Applicants.FindAsync(applicantId);
            if (applicant == null)
            {
                return NotFound($"Applicant with ID {applicantId} not found.");
            }

            // Retrieve beneficiaries for the given applicant. Use ToListAsync() to execute the query.
            var beneficiaries = await _context.Beneficiaries
                .Where(b => b.ApplicantId == applicantId)
                .ToListAsync();

            // --- ADD THIS BLOCK ---
            // If lazy loading is enabled, the Applicant property will be populated.
            // Explicitly set it to null before returning to prevent serialization cycles.
            foreach (var beneficiary in beneficiaries)
            {
                beneficiary.Applicant = null;
            }
            // --- END OF ADDED BLOCK ---

            // If no beneficiaries are found, return a 404
            if (beneficiaries == null || !beneficiaries.Any())
            {
                return NotFound($"No beneficiaries found for Applicant with ID {applicantId}.");
            }

            return Ok(beneficiaries); // Returns 200 OK with the list of beneficiaries
        }

        /// <summary>
        /// Retrieves a single Beneficiary record by its own Id.
        /// </summary>
        /// <param name="id">The ID of the Beneficiary record.</param>
        /// <returns>A 200 OK response with the Beneficiary, or 404 Not Found.</returns>
        [HttpGet("beneficiaries/{id}")] // e.g., GET /api/applicants/beneficiaries/1
        public async Task<IActionResult> GetBeneficiary(int id)
        {
            // Find the beneficiary by its primary Key
            var beneficiary = await _context.Beneficiaries.FindAsync(id);

            if (beneficiary == null)
            {
                return NotFound($"Beneficiary with ID {id} not found.");
            }

            return Ok(beneficiary); // Returns 200 OK with the beneficiary data
        }

        /// <summary>
        /// Updates an existing Beneficiary record for a specific Applicant.
        /// </summary>
        /// <param name="applicantId">The ID of the Applicant to whom the Beneficiary belongs.</param>
        /// <param name="id">The ID of the Beneficiary record to update.</param>
        /// <param name="beneficiary">The Beneficiary object with updated data.</param>
        /// <returns>A 204 No Content response if successful, 400 Bad Request, or 404 Not Found.</returns>
        [HttpPut("{applicantId}/beneficiaries/{id}")] // e.g., PUT api/applicnats/1/beneficiaries/1
        public async Task<IActionResult> UpdateBeneficiary(int applicantId, int id, [FromBody] Beneficiary beneficiary)
        {
            // 1. Validate: Ensure IDs in route and body match, and ApplicantId matches.
            if (id != beneficiary.Id)
            {
                return BadRequest("Beneficiary ID in the Route does not match the ID in the request boby");
            }
            if (applicantId != beneficiary.ApplicantId)
            {
                return BadRequest("Applicant ID in the Route does not match the Applicant ID in the request body");
            }

            // 2. Validate incoming data using model state
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 3. Find the existing Beneficiary record in the database.
            var existingBeneficiary = await _context.Beneficiaries.FirstOrDefaultAsync(b => b.Id == id && b.ApplicantId == applicantId);
            if (existingBeneficiary == null)
            {
                // Beneficiary not found OR it doesn't belong to the specified applicant
                return NotFound($"Beneficiary with ID {id} not found or does not belong to Applicant ID {applicantId}.");
            }

            // 4. Update the properties of the *existing tracked entity*.
            _context.Entry(existingBeneficiary).CurrentValues.SetValues(beneficiary);

            try
            {
                // 5. Save changes to the database.
                await _context.SaveChangesAsync();

                // Optional: Update the Applicant's LastModifiedDate as well
                var applicant = await _context.Applicants.FindAsync(applicantId);
                if (applicant != null)
                {
                    applicant.LastModifiedDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync(); // Save the update to the applicant
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BeneficiaryExists(id, applicantId)) // Helper to check if it exists and belongs to applicant
                {
                    return NotFound($"Beneficiary with ID {id} not found or does not belong to Applicant ID {applicantId}.");
                }
                else
                {
                    throw; // A real concurrency issue occurred.
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating the beneficiary: {ex.Message}. Inner Exception: {ex.InnerException?.Message}");
            }

            // 6. Return 204 No Content for successful update.
            return NoContent();
        }

        // Helper method (add this inside your ApplicantsController class if not already there)
        // Make sure you have this helper:
        private bool BeneficiaryExists(int id, int applicantId)
        {
            return _context.Beneficiaries.Any(e => e.Id == id && e.ApplicantId == applicantId);
        }

        /// <summary>
        /// Deletes a Beneficiary record for a specific Applicant.
        /// </summary>
        /// <param name="applicantId">The ID of the Applicant to whom the Beneficiary belongs.</param>
        /// <param name="id">The ID of the Beneficiary record to delete.</param>
        /// <returns>A 204 No Content response if successful, or 404 Not Found.</returns>
        [HttpDelete("{applicantId}/beneficiaries/{id}")] // e.g., DELETE /api/applicants/1/beneficiaries/1
        public async Task<IActionResult> DeleteBeneficiary(int applicantId, int id)
        {
            // 1. Find the Beneficiary to delete, ensuring it belongs to the specified Applicant.
            var beneficiary = await _context.Beneficiaries.FirstOrDefaultAsync(b => b.Id == id && b.ApplicantId == applicantId);

            if (beneficiary == null)
            {
                // Beneficiary not found OR it doesn't belong to the specified applicant
                return NotFound($"Beneficiary with ID {id} not found or does not belong to Applicant ID {applicantId}.");
            }

            try
            {
                // 2. Remove the Beneficiary. EF Core will handle removal from the database.
                _context.Beneficiaries.Remove(beneficiary);
                await _context.SaveChangesAsync();

                // Optional: Update the Applicant's LastModifiedDate as well
                var applicant = await _context.Applicants.FindAsync(applicantId);
                if (applicant != null)
                {
                    applicant.LastModifiedDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync(); // Save the update to the applicant
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while deleting the beneficiary: {ex.Message}. Inner Exception: {ex.InnerException?.Message}");
            }

            // 3. Return 204 No Content for successful deletion.
            return NoContent();
        }


        /// <summary>
        /// Adds a new Asset record for a specific Applicant.
        /// </summary>
        /// <param name="applicantId">The ID of the Applicant.</param>
        /// <param name="asset">The Asset object to add.</param>
        /// <returns>A 201 Created response or 400 Bad Request/404 Not Found/500 Internal Server Error.</returns>
        [HttpPost("{applicantId}/assets")] // POST /api/applicants/1/assets
        public async Task<IActionResult> AddAsset(int applicantId, [FromBody] Asset asset)
        {
            // 1. Valdate incoming data using madel state
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Returns 400 with validation errors
            }

            // 2. Check if the Applicant exists
            var applicant = await _context.Applicants.FindAsync(applicantId);
            if (applicant == null)
            {
                return NotFound($"Applicant with ID {applicantId} not found."); // Returns 404
            }

            // 3. Associate Asset with Applicant
            asset.ApplicantId = applicantId; // Ensure the foreign key is correctly set

            // Important: Clear any nested collections like AssetAllocations
            // We are only creating the Asset itself in this POST request.
            // Asset allocations are handled separately.
            asset.AssetAllocations = new List<BeneficiaryAssetAllocation>();

            try
            {
                _context.Assets.Add(asset);
                await _context.SaveChangesAsync();

                // Optional: Update the Applicant's LastModifiedDate as well
                applicant.LastModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync(); // Save the update to the applicant

                // 4. Return success response (201 Created)
                // Use nameof(GetAsset) to point to the action that retrieves a single asset by its own ID.
                return CreatedAtAction(nameof(GetAsset), new { id = asset.Id }, asset);
            }
            catch (DbUpdateException ex)
            {
                // Handle potential database errors
                return StatusCode(500, $"An error occurred while saving the asset: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Catch any other unexpected errors
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all Asset records for a specific Applicant.
        /// </summary>
        /// <param name="applicantId">The ID of the Applicant.</param>
        /// <returns>A 200 OK response with a list of Assets, or 404 Not Found.</returns>
        [HttpGet("{applicantId}/assets")] // e.g., GET /api/applicants/1/assets
        public async Task<IActionResult> GetAssetsForApplicant(int applicantId)
        {
            // Check if the Applicant exists
            var applicant = await _context.Applicants.FindAsync(applicantId);
            if (applicant == null)
            {
                return NotFound($"Applicant with ID {applicantId} not found.");
            }
            var assets = await _context.Assets
                .Where(a => a.ApplicantId == applicantId)
                .ToListAsync();

            // --- ADD THIS BLOCK ---
            // If lazy loading is enabled, the Applicant property will be populated.
            // Explicitly set it to null before returning to prevent serialization cycles.
            foreach (var asset in assets)
            {
                asset.Applicant = null;
            }
            // --- END OF ADDED BLOCK ---

            // If no assets are found, return a 404.
            if (assets == null || !assets.Any())
            {
                return NotFound($"No assets found for Applicant with ID {applicantId}.");
            }

            return Ok(assets); // Returns 200 OK with the list of assets
        }

        /// <summary>
        /// Retrieves a single Asset record by its own Id.
        /// </summary>
        /// <param name="id">The ID of the Asset record.</param>
        /// <returns>A 200 OK response with the Asset, or 404 Not Found.</returns>
        [HttpGet("assets/{id}")] // e.g., GET /api/applicants/assets/1
        public async Task<IActionResult> GetAsset(int id)
        {
            // Find the asset by its primary Key
            var asset = await _context.Assets.FindAsync(id);

            if (asset == null)
            {
                return NotFound($"Asset with ID {id} not found.");
            }

            return Ok(asset); // Returns 200 OK with the asset data
        }

        /// <summary>
        /// Updates an existing Asset record for a specific Applicant.
        /// </summary>
        /// <param name="applicantId">The ID of the Applicant to whom the Asset belongs.</param>
        /// <param name="id">The ID of the Asset record to update.</param>
        /// <param name="asset">The Asset object with updated data.</param>
        /// <returns>A 204 No Content response if successful, 400 Bad Request, or 404 Not Found.</returns>
        [HttpPut("{applicantId}/assets/{id}")] // e.g., PUT /api/applicants/1/assets/1
        public async Task<IActionResult> UpdateAsset(int applicantId, int id,[FromBody] Asset asset)
        {
            // 1. Check if the Id and ApplicantId in the route matches that of the request body
            if (id != asset.Id)
            {
                return BadRequest("Asset ID in the route does not match the ID in the request body.");
            }
            if (applicantId != asset.ApplicantId)
            {
                return BadRequest("Applicant ID in the route does not match the ApplicantId in the request body.");
            }

            // 2. To check if any of the data annotations flooded the model state with errors
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 3. Find the existing Assset in the database, if not found return 404
            var existingAsset = await _context.Assets.FirstOrDefaultAsync(a => a.Id == id && a.ApplicantId == applicantId);

            if (existingAsset == null)
            {
                return NotFound($"Asset with ID{id} not found or does not belong to Applicant ID {applicantId}.");
            }

            // 4. Update the properties of the existing Asset property
            _context.Entry(existingAsset).CurrentValues.SetValues(asset);

            try
            {
                // Save changes to the database
                await _context.SaveChangesAsync();

                var applicant = await _context.Applicants.FindAsync(applicantId);

                if (applicant != null)
                {
                    applicant.LastModifiedDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AssetExists(id, applicantId)) // Helper to check if the asset actually exist
                {
                    return NotFound($"Assets with ID {id} not found or does not belong to the Applicant ID {applicantId}.");
                }
                else
                {
                    throw;
                }
            }
            // 5. Return 204 for successful operation
            return NoContent();
        }

        // Helper method (add this inside your ApplicantsController class)
        private bool AssetExists(int id, int applicantId)
        {
            return _context.Assets.Any(e => e.Id == id && e.ApplicantId == applicantId);
        }

        /// <summary>
        /// Deletes an Asset record for a specific Applicant.
        /// </summary>
        /// <param name="applicantId">The ID of the Applicant to whom the Asset belongs.</param>
        /// <param name="id">The ID of the Asset record to delete.</param>
        /// <returns>A 204 No Content response if successful, or 404 Not Found.</returns>
        [HttpDelete("{applicantId}/assets/{id}")] // e.g., DELETE /api/applicants/1/assets/1
        public async Task<IActionResult> DeleteAsset(int applicantId, int id)
        {
            // 1. Check if the asset exists in the database, if not return 404
            var asset = await _context.Assets.FirstOrDefaultAsync(a => a.Id == id && a.ApplicantId == applicantId);
            if (asset == null)
            {
                return NotFound($"Asset with ID {id} not found or does not belong to Applicant ID {applicantId}.");
            }

            // 2. Remove the Asset. EF Core handles the removal from the database
            _context.Assets.Remove(asset);
            await _context.SaveChangesAsync();

            // 3. Find the Applicant using the Aplicant ID and modify the lastModifiedDate
            var applicant = await _context.Applicants.FindAsync(applicantId);
            if (applicant != null)
            {
                applicant.LastModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            // 4. Returns 204 if successful
            return NoContent();
        }



        /// <summary>
        /// Creates a new asset allocation for a specific applicant.
        /// Requires existing AssetId and BeneficiaryId that belong to the specified Applicant.
        /// </summary>
        /// <param name="applicantId">The ID of the applicant for whom the allocation is being made.</param>
        /// <param name="allocation">The allocation details (AssetId, BeneficiaryId, Percentage).</param>
        /// <returns>A 201 Created response, or 400 Bad Request / 404 Not Found / 409 Conflict / 500 Internal Server Error if errors occur.</returns>
        [HttpPost("{applicantId}/assetallocations")] // e.g., POST /api/applicants/1/assetallocations
        public async Task<IActionResult> AddAssetAllocation(int applicantId, [FromBody] BeneficiaryAssetAllocation allocation)
        {
            // 1. Validate incoming data using model state (e.g., percentage range, required fields)
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Returns 400 with validation errors
            }

            // 2. Verify Applicant exists
            var applicant = await _context.Applicants.FindAsync(applicantId);
            if (applicant == null)
            {
                return NotFound($"Applicant with ID {applicantId} not found."); // Returns 404
            }

            // 3. Verify Asset exists AND belongs to this Applicant
            var asset = await _context.Assets
                .Where(a => a.Id == allocation.AssetId && a.ApplicantId == applicantId)
                .FirstOrDefaultAsync();

            if (asset == null)
            {
                return NotFound($"Asset with ID {allocation.AssetId} not found or does not belong to Application ID {applicantId}.");
            }

            // 4. Verify Beneficiary exists AND belongs to this Applicant
            // Use .Where() clauses for both Id and ApplicantId to ensure ownership
            var beneficiary = await _context.Beneficiaries
                .Where(b => b.Id == allocation.BeneficiaryId && b.ApplicantId == applicantId)
                .FirstOrDefaultAsync();

            if (beneficiary == null)
            {
                return NotFound($"Beneficiary with ID {allocation.BeneficiaryId} not found or does not belong to Applicant ID {applicantId}.");
            }

            // 5. Set the ApplicantId on the allocation object from the route parameter
            allocation.ApplicantId = applicantId;

            // 6. Check for duplicate allocation (same asset to same beneficiary for the same applicant)
            // This prevents creating identical allocation records.
            var existingAllocation = await _context.BeneficiaryAssetAllocations
                .Where(ba => ba.ApplicantId == applicant.Id &&
                            ba.AssetId == allocation.AssetId &&
                            ba.BeneficiaryId == allocation.BeneficiaryId)
                .FirstOrDefaultAsync();

            if (existingAllocation != null)
            {
                return Conflict($"Asset with ID {allocation.AssetId} is already allocated to Beneficiary with ID {allocation.BeneficiaryId} for Applicant ID {applicantId}.");
            }

            // Important: Ensure navigation properties are not set in the incoming JSON payload,
            // and clear them if they were to prevent EF Core from trying to create new related entities.
            // Instead, we link to the existing entities found above.
            allocation.Applicant = null; // Clear if accidentally set in input
            allocation.Asset = null;     // Clear if accidentally set in input
            allocation.Beneficiary = null; // Clear if accidentally set in input

            try
            {
                _context.BeneficiaryAssetAllocations.Add(allocation);
                await _context.SaveChangesAsync();

                // Optional: Update the Applicant's LastModifiedDate
                applicant.LastModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // To ensure the returned object includes the full Asset and Beneficiary details,
                // we re-load them or include them (ReferenceHandler.IgnoreCycles will handle the rest).
                // The _context.Entry().Reference().LoadAsync() ensures navigation properties are loaded
                // if they weren't implicitly included by EF Core.
                await _context.Entry(allocation).Reference(a => a.Asset).LoadAsync();
                await _context.Entry(allocation).Reference(a => a.Beneficiary).LoadAsync();
                // await _context.Entry(allocation).Reference(a => a.Applicant).LoadAsync(); // Optional: if you want applicant nested

                // 7. Return success response (201 Created)
                return CreatedAtAction(nameof(GetAssetAllocation), new { id = allocation.Id }, allocation);
            }
            catch (DbUpdateException ex)
            {
                // Log the full exception for debugging in a real application
                return StatusCode(500, $"An error occurred while saving the asset allocation: {ex.Message}. Inner Exception: {ex.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                // Catch any other unexpected errors
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all BeneficiaryAssetAllocation records for a specific Applicant.
        /// Includes details of the associated Asset and Beneficiary.
        /// </summary>
        /// <param name="applicantId">The ID of the Applicant.</param>
        /// <returns>A 200 OK response with a list of allocations, or 404 Not Found.</returns>
        [HttpGet("{applicantId}/assetallocations")] // e.g., GET /api/applicants/1/assetallocations
        public async Task<IActionResult> GetAssetAllocationsForApplicant(int applicantId)
        {
            var applicant = await _context.Applicants.FindAsync(applicantId);
            if (applicant == null)
            {
                return NotFound($"Applicant with ID {applicantId} not found.");
            }

            // Eager load related Asset and Beneficiary data for a more complete response
            var allocations = await _context.BeneficiaryAssetAllocations
                                            .Where(ba => ba.ApplicantId == applicantId)
                                            .Include(ba => ba.Asset)        // Include the Asset details
                                            .Include(ba => ba.Beneficiary)  // Include the Beneficiary details
                                            .ToListAsync();

            // --- ADD THIS BLOCK ---
            // If lazy loading is enabled, the Applicant property will be populated.
            // Explicitly set it to null before returning to prevent serialization cycles.

            foreach (var allocation in allocations)
            {
                allocation.Applicant = null;

                // Clean up the nested Asset object
                if (allocation.Asset != null)
                {
                    // --- NEW: Fixes the 'assetAllocations' null ---
                    allocation.Asset.AssetAllocations = null;
                    // --- END NEW ---

                    if (allocation.Asset.Applicant != null)
                    {
                        allocation.Asset.Applicant.Identifications = null;
                        allocation.Asset.Applicant.Beneficiaries = null;
                        allocation.Asset.Applicant.Executors = null;
                        allocation.Asset.Applicant.Guardians = null;
                        allocation.Asset.Applicant.Assets = null;
                        allocation.Asset.Applicant.PaymentTransactions = null;
                        allocation.Asset.Applicant.AssetAllocations = null;
                    }
                }

                // Clean up the nested Beneficiary object
                if (allocation.Beneficiary != null)
                {
                    // --- NEW: Fixes the 'assetAllocations' null ---
                    allocation.Beneficiary.AssetAllocations = null;
                    // --- END NEW ---

                    if (allocation.Beneficiary.Applicant != null)
                    {
                        allocation.Beneficiary.Applicant.Identifications = null;
                        allocation.Beneficiary.Applicant.Beneficiaries = null;
                        allocation.Beneficiary.Applicant.Executors = null;
                        allocation.Beneficiary.Applicant.Guardians = null;
                        allocation.Beneficiary.Applicant.Assets = null;
                        allocation.Beneficiary.Applicant.PaymentTransactions = null;
                    }
                }
            }

            return Ok(allocations);
        }


        /// <summary>
        /// Retrieves a single BeneficiaryAssetAllocation record by its own Id.
        /// Includes details of the associated Asset and Beneficiary.
        /// </summary>
        /// <param name="id">The ID of the BeneficiaryAssetAllocation record.</param>
        /// <returns>A 200 OK response with the allocation, or 404 Not Found.</returns>
        
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
            // Clean up the deeply nested objects to prevent serialization cycles
            // Start with the main Applicant object
            if (allocation.Applicant != null)
            {
                allocation.Applicant.Identifications = null;
                allocation.Applicant.Beneficiaries = null;
                allocation.Applicant.Executors = null;
                allocation.Applicant.Guardians = null;
                allocation.Applicant.Assets = null;
                allocation.Applicant.AssetAllocations = null;
                allocation.Applicant.PaymentTransactions = null;
            }

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

            // Clean up the Applicant object nested inside the Beneficiary
            if (allocation.Beneficiary != null && allocation.Beneficiary.Applicant != null)
            {
                // Fix the circular reference from the Beneficiary -> Applicant -> AssetAllocations
                allocation.Beneficiary.AssetAllocations = null;

                allocation.Beneficiary.Applicant.Identifications = null;
                allocation.Beneficiary.Applicant.Beneficiaries = null;
                allocation.Beneficiary.Applicant.Executors = null;
                allocation.Beneficiary.Applicant.Guardians = null;
                allocation.Beneficiary.Applicant.Assets = null;
                allocation.Beneficiary.Applicant.AssetAllocations = null;
                allocation.Beneficiary.Applicant.PaymentTransactions = null;
            }
            // --- END OF CLEANUP BLOCK ---

            return Ok(allocation);
        }


        /// <summary>
        /// Updates an existing BeneficiaryAssetAllocation record for a specific Applicant.
        /// </summary>
        /// <param name="applicantId">The ID of the Applicant to whom the allocation belongs.</param>
        /// <param name="id">The ID of the BeneficiaryAssetAllocation record to update.</param>
        /// <param name="allocation">The BeneficiaryAssetAllocation object with updated data.</param>
        /// <returns>A 204 No Content response if successful, 400 Bad Request, or 404 Not Found.</returns>
        [HttpPut("{applicantId}/beneficiaryAssetAllocations/{id}")]
        public async Task<IActionResult> UpdateBeneficiaryAssetAllocation(int applicantId, int id, [FromBody] BeneficiaryAssetAllocation allocation)
        {
            if (id != allocation.Id)
            {
                return BadRequest("Allocation ID in the route does not match the ID in the request body.");
            }
            if (applicantId != allocation.ApplicantId)
            {
                return BadRequest("Applicant ID in the route does not match the ApplicantId in the request body.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingAllocation = await _context.BeneficiaryAssetAllocations
                                              .FirstOrDefaultAsync(a => a.Id == id && a.ApplicantId == applicantId);

            if (existingAllocation == null)
            {
                return NotFound($"Allocation with ID {id} not found or does not belong to Applicant ID {applicantId}.");
            }

            _context.Entry(existingAllocation).CurrentValues.SetValues(allocation);

            try
            {
                await _context.SaveChangesAsync();
                var applicant = await _context.Applicants.FindAsync(applicantId);
                if (applicant != null)
                {
                    applicant.LastModifiedDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BeneficiaryAssetAllocationExists(id, applicantId))
                {
                    return NotFound($"Allocation with ID {id} not found or does not belong to Applicant ID {applicantId}.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Deletes a BeneficiaryAssetAllocation record for a specific Applicant.
        /// </summary>
        /// <param name="applicantId">The ID of the Applicant to whom the allocation belongs.</param>
        /// <param name="id">The ID of the BeneficiaryAssetAllocation record to delete.</param>
        /// <returns>A 204 No Content response if successful, or 404 Not Found.</returns>
        [HttpDelete("{applicantId}/beneficiaryAssetAllocations/{id}")]
        public async Task<IActionResult> DeleteBeneficiaryAssetAllocation(int applicantId, int id)
        {
            var allocation = await _context.BeneficiaryAssetAllocations
                                      .FirstOrDefaultAsync(a => a.Id == id && a.ApplicantId == applicantId);

            if (allocation == null)
            {
                return NotFound($"Allocation with ID {id} not found or does not belong to Applicant ID {applicantId}.");
            }

            _context.BeneficiaryAssetAllocations.Remove(allocation);
            await _context.SaveChangesAsync();

            var applicant = await _context.Applicants.FindAsync(applicantId);
            if (applicant != null)
            {
                applicant.LastModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        // Helper method
        private bool BeneficiaryAssetAllocationExists(int id, int applicantId)
        {
            return _context.BeneficiaryAssetAllocations.Any(e => e.Id == id && e.ApplicantId == applicantId);
        }



        /// <summary>
        /// Adds a new Executor record for a specific Applicant.
        /// </summary>
        /// <param name="applicantId">The ID of the Applicant.</param>
        /// <param name="executor">The Executor object to add.</param>
        /// <returns>A 201 Created response or 400 Bad Request/404 Not Found/500 Internal Server Error.</returns>
        [HttpPost("{applicantId}/executors")]
        public async Task<IActionResult> AddExecutor(int applicantId, [FromBody] Executor executor)
        {
            if (!ModelState.IsValid)
            {
                // Check for specific conditional validation here if desired
                // For example:
                if (executor.ExecutorType == "Individual" && (string.IsNullOrEmpty(executor.FirstName) || string.IsNullOrEmpty(executor.LastName)))
                {
                    ModelState.AddModelError(string.Empty, "First Name and Last Name are required for Individual ExecutorType.");
                    return BadRequest(ModelState);
                }
                if (executor.ExecutorType == "Company" && string.IsNullOrEmpty(executor.CompanyName))
                {
                    ModelState.AddModelError(string.Empty, "Company Name is required for Company ExecutorType.");
                    return BadRequest(ModelState);
                }
                // END OF EXAMPLE

                return BadRequest(ModelState); // Returns 400 with validation errors
            }

            // 2. Check if the Applicant Exist
            var applicant = await _context.Applicants.FindAsync(applicantId);
            if (applicant == null)
            {
                return NotFound($"Applicant with ID {applicantId} not found."); // Returns 404
            }

            // 3. Associate Executor with Applicant
            executor.ApplicantId = applicantId; // Ensure the foreign key is correctly set

            // Important: Clear any nested navigation property for Applicant in the incoming object
            // We're linking to an existing Applicant, not creating a new one.
            executor.Applicant = null;

            try
            {
                _context.Executors.Add(executor);
                await _context.SaveChangesAsync();

                // Optional: Update the Applicant's LastModifiedDate as well
                applicant.LastModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync(); // Save the update to the applicants

                // 4. Return success response (201 Created)
                // Use nameof(GetExecutor) to point to the action that retrieves a single executor by its own ID.
                return CreatedAtAction(nameof(GetExecutor), new { Id = executor.Id }, executor);
            }
            catch (DbUpdateException ex)
            {
                // Handle potential database errors
                return StatusCode(500, $"An error occurred while saving the executor: {ex.Message}. InnerException: {ex.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                // Catch any other unexpected errors
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all Executor records for a specific Applicant.
        /// </summary>
        /// <param name="applicantId">The ID of the Applicant.</param>
        /// <returns>A 200 OK response with a list of Executors, or 404 Not Found.</returns>
        [HttpGet("{applicantId}/executors")] // e.g., GET /api/applicants/1/executors
        public async Task<IActionResult> GetExecutorForApplicant(int applicantId)
        {
            // Check if the applicant exist
            var applicant = await _context.Applicants.FindAsync(applicantId);
            if (applicant == null)
            {
                return NotFound($"Applicant with ID {applicantId} not found.");
            }

            // Retrieve executors for the given applicant. Use ToListAsync() to execute the query.
            var executors = await _context.Executors
                .Where(e => e.ApplicantId == applicantId)
                .ToListAsync(); // Executes the query

            // --- ADD THIS BLOCK ---
            // If lazy loading is enabled, the Applicant property will be populated.
            // Explicitly set it to null before returning to prevent serialization cycles.
            foreach (var executor in executors)
            {
                executor.Applicant = null;
            }
            // --- END OF ADDED BLOCK ---

            // If no executors are found, return a 404.
            if (executors == null || !executors.Any())
            {
                return NotFound($"No executors found for Applicant with ID {applicantId}.");
            }

            return Ok(executors); // Returns 200 Ok with the List of executors
        }

        /// <summary>
        /// Retrieves a single Executor record by its own Id.
        /// </summary>
        /// <param name="id">The ID of the Executor record.</param>
        /// <returns>A 200 OK response with the Executor, or 404 Not Found.</returns>
        [HttpGet("executors/{id}")] // e.g., GET /applicant/executors/1
        public async Task<IActionResult> GetExecutor(int id)
        {
            // Find the executor by its primary key
            var executor = await _context.Executors
                .Include(e => e.Applicant)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (executor == null)
            {
                return NotFound($"Executor with ID {id} not found.");
            }

            return Ok(executor); // Returns 200 Ok with the executor data 
        }

        /// <summary>
        /// Updates an existing Executor record for a specific Applicant.
        /// </summary>
        /// <param name="applicantId">The ID of the Applicant to whom the Executor belongs.</param>
        /// <param name="id">The ID of the Executor record to update.</param>
        /// <param name="executor">The Executor object with updated data.</param>
        /// <returns>A 204 No Content response if successful, 400 Bad Request,
        [HttpPut("{applicantId}/executors/{id}")] // e.g., PUT /api/applicants/1/executors/1
        public async Task<IActionResult> UpdateExecutor(int applicantId, int id, [FromBody] Executor executor)
        {
            // 1. Valid if the executor id and applican id in the route match th ID int the request body.
            if (id != executor.Id)
            {
                return BadRequest("Executor ID in the route does not match the ID in the request body.");
            }
            if (applicantId != executor.ApplicantId)
            {
                return BadRequest("Applicant ID in the route does not match the ApplicantId in the request body.");
            }
            // 2. Check for errors flooded into the ModelState, because of the Data Annotations used
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 3. Check for the existing Executor in the database, if not return 404
            var existingExecutor = await _context.Executors.FirstOrDefaultAsync(e => e.Id == id && e.ApplicantId == applicantId);
            if (existingExecutor == null)
            {
                return NotFound($"Executor with ID {id} not found or does nor belong to Applicant ID {applicantId}.");
            }

            // 4. Update properties of the existing tracked entity
            _context.Entry(existingExecutor).CurrentValues.SetValues(executor);

            try
            {
                // 5. Save changes to database andnupdate the last modified date
                await _context.SaveChangesAsync();
                var applicant = await _context.Applicants.FindAsync(applicantId);
                if (applicant != null)
                {
                    applicant.LastModifiedDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExecutorExists(id, applicantId)) // Helper method
                {
                    return NotFound($"Executor with ID {id} not found or does not belong to Applicant ID {applicantId}.");
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        // Helper method (add this inside your ApplicantsController class)
        private bool ExecutorExists(int id, int applicantId)
        {
            return _context.Executors.Any(e => e.Id == id && e.ApplicantId == applicantId);
        }

        /// <summary>
        /// Deletes an Executor record for a specific Applicant.
        /// </summary>
        /// <param name="applicantId">The ID of the Applicant to whom the Executor belongs.</param>
        /// <param name="id">The ID of the Executor record to delete.</param>
        /// <returns>A 204 No Content response if successful, or 404 Not Found.</returns>
        [HttpDelete("{applicantId}/executors/{id}")] // e.g., DELETE /api/applicants/1/executors/1
        public async Task<IActionResult> DeleteExecutor(int applicantId, int id)
        {
            var executor = await _context.Executors
                                      .FirstOrDefaultAsync(e => e.Id == id && e.ApplicantId == applicantId);

            if (executor == null)
            {
                return NotFound($"Executor with ID {id} not found or does not belong to Applicant ID {applicantId}.");
            }

            _context.Executors.Remove(executor);
            await _context.SaveChangesAsync();

            var applicant = await _context.Applicants.FindAsync(applicantId);
            if (applicant != null)
            {
                applicant.LastModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }


        /// <summary>
        /// Adds a new Guardian record for a specific Applicant.
        /// </summary>
        /// <param name="applicantId">The ID of the Applicant.</param>
        /// <param name="guardian">The Guardian object to add.</param>
        /// <returns>A 201 Created response or 400 Bad Request/404 Not Found/500 Internal Server Error.</returns>
        [HttpPost("{applicantId}/guardiians")] // e.g., POST api/applicants/1/guardians
        public async Task<IActionResult> AddGuardian(int applicantId, Guardian guardian)
        {
            // 1. Validate incoming data using model state
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 2. Check if the Applicant exists
            var applicant = await _context.Applicants.FindAsync(applicantId);
            if (applicant == null)
            {
                return NotFound($"Applicant with ID {applicantId} not found."); // Returns 404
            }

            // 3. Associate Guardian with Applicant
            guardian.ApplicantId = applicantId; // Enssure the foreign is correctly set

            // Important: CLear any nested navigation property for Applicnat in the incoming object
            // We're linking to an existing Applicant, not creating a new one.
            guardian.Applicant = null;

            try
            {
                _context.Guardians.Add(guardian);
                await _context.SaveChangesAsync();

                // Optional: Update the Applicant's LastModifiedDate as well
                applicant.LastModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync(); // Save the update to the applicant

                // 4. Return success response (201 Created)
                // Use nameof(GetGuardian) to point to the action that retrieves a single guardian by its own ID.
                return CreatedAtAction(nameof(GetGuardian), new { Id = guardian.Id }, guardian);
            }
            catch (DbUpdateException ex)
            {
                // Handle potential database errors (e.g., if there unique constraints)
                // Log the exception for debugging (in a real app, use a logger)
                return StatusCode(500, $"An error occured while saving the guardian: {ex.Message}. Inner Exception: {ex.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                // Catch any other unexpected errors
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all Guardian records for a specific Applicant.
        /// </summary>
        /// <param name="applicantId">The ID of the Applicant.</param>
        /// <returns>A 200 OK response with a list of Guardians, or 404 Not Found.</returns>
        [HttpGet("{applicantId}/guardians")] // e.g., GET /api/applicants/1/guardians
        public async Task<IActionResult> GetGuardiansForApplicant(int applicantId)
        {
            // Check if the Applicant exists
            var applicant = await _context.Applicants.FindAsync(applicantId);
            if (applicant == null)
            {
                return NotFound($"Applicant with ID {applicantId} not found.");
            }

            // Retrieve guardians for the given applicant. Use ToListAsync() to execute the query.
            var guardians = await _context.Guardians
                .Where(g => g.ApplicantId == applicantId)
                .ToListAsync();

            // --- ADD THIS BLOCK ---
            // If lazy loading is enabled, the Applicant property will be populated.
            // We explicitly set it to null to prevent serialization cycles.
            foreach (var guardian in guardians)
            {
                guardian.Applicant = null;
            }
            // --- END OF ADDED BLOCK ---

            // If no guardians are found, return a 404.
            if (guardians == null || !guardians.Any())
            {
                return NotFound($"No guardians found for Applicant with ID {applicantId}.");
            }

            return Ok(guardians); // Returns 200 OK with the list of guardians
        }

        /// <summary>
        /// Retrieves a single Guardian record by its own Id.
        /// </summary>
        /// <param name="id">The ID of the Guardian record.</param>
        /// <returns>A 200 OK response with the Guardian, or 404 Not Found.</returns>
        [HttpGet("guardians/{id}")] // e.g., GET /api/applicants/guardians/1
        public async Task<IActionResult> GetGuardian(int id)
        {
            // Find the guardian by its primary key
            var guardian = await _context.Guardians
                .Include(g => g.Applicant)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (guardian == null)
            {
                return NotFound($"Guardian with ID {id} not found.");
            }

            return Ok(guardian); // Returns 200 Ok with the guardian data
        }

        /// <summary>
        /// Updates an existing Guardian record for a specific Applicant.
        /// </summary>
        /// <param name="applicantId">The ID of the Applicant to whom the Guardian belongs.</param>
        /// <param name="id">The ID of the Guardian record to update.</param>
        /// <param name="guardian">The Guardian object with updated data.</param>
        /// <returns>A 204 No Content response if successful, 400 Bad Request, or 404 Not Found.</returns>
        [HttpPut("{applicantId}/guardians/{id}")]
        public async Task<IActionResult> UpdateGuardian(int applicantId, int id, [FromBody] Guardian guardian)
        {
            if (id != guardian.Id)
            {
                return BadRequest("Guardian ID in the route does not match the ID in the request body.");
            }
            if (applicantId != guardian.ApplicantId)
            {
                return BadRequest("Applicant ID in the route does not match the ApplicantId in the request body.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingGuardian = await _context.Guardians
                                              .FirstOrDefaultAsync(g => g.Id == id && g.ApplicantId == applicantId);

            if (existingGuardian == null)
            {
                return NotFound($"Guardian with ID {id} not found or does not belong to Applicant ID {applicantId}.");
            }

            _context.Entry(existingGuardian).CurrentValues.SetValues(guardian);

            try
            {
                await _context.SaveChangesAsync();
                var applicant = await _context.Applicants.FindAsync(applicantId);
                if (applicant != null)
                {
                    applicant.LastModifiedDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GuardianExists(id, applicantId))
                {
                    return NotFound($"Guardian with ID {id} not found or does not belong to Applicant ID {applicantId}.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Deletes a Guardian record for a specific Applicant.
        /// </summary>
        /// <param name="applicantId">The ID of the Applicant to whom the Guardian belongs.</param>
        /// <param name="id">The ID of the Guardian record to delete.</param>
        /// <returns>A 204 No Content response if successful, or 404 Not Found.</returns>
        [HttpDelete("{applicantId}/guardians/{id}")]
        public async Task<IActionResult> DeleteGuardian(int applicantId, int id)
        {
            var guardian = await _context.Guardians
                                      .FirstOrDefaultAsync(g => g.Id == id && g.ApplicantId == applicantId);

            if (guardian == null)
            {
                return NotFound($"Guardian with ID {id} not found or does not belong to Applicant ID {applicantId}.");
            }

            _context.Guardians.Remove(guardian);
            await _context.SaveChangesAsync();

            var applicant = await _context.Applicants.FindAsync(applicantId);
            if (applicant != null)
            {
                applicant.LastModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        // Helper method
        private bool GuardianExists(int id, int applicantId)
        {
            return _context.Guardians.Any(e => e.Id == id && e.ApplicantId == applicantId);
        }



        /// <summary>
        /// Adds a new PaymentTransaction record for a specific Applicant.
        /// </summary>
        /// <param name="applicantId">The ID of the Applicant.</param>
        /// <param name="paymentTransaction">The PaymentTransaction object to add.</param>
        /// <returns>A 201 Created response or 400 Bad Request/404 Not Found/500 Internal Server Error.</returns>
        [HttpPost("{applicantId}/paymenttransactions")] // e.g., POST /api/applicants/1/paymenttransactions
        public async Task<IActionResult> AddPaymentTransaction(int applicantId, [FromBody] PaymentTransaction paymentTransaction)
        {
            // 1. Validate incoming data using model state
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Returns 400 with validation errors
            }

            // 2. Check if the Applicant exists
            var applicant = await _context.Applicants.FindAsync(applicantId);
            if (applicant == null)
            {
                return NotFound($"Applicant with ID {applicantId} not found."); // Returns a 404
            }

            // 3. Associate PaymentTransaction with Applicant
            paymentTransaction.ApplicantId = applicantId; // Ensure the foreign key is correctly set

            // The TransactionDate defaults to DateTime.UtcNow in the model. If you want to allow
            // the client to provide it, remove the default from the model and validate here.
            // For now, it will use the model's default if not provided by the client.

            // Important: Clear any nested navigation property for Applicant in the incoming object
            paymentTransaction.Applicant = null;

            try
            {
                _context.PaymentTransactions.Add(paymentTransaction);
                await _context.SaveChangesAsync();

                // Optional: Update the Applicant's LastModifiedDate as well
                applicant.LastModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync(); // Save the update to the applicant 

                // 4. Return success response (201 Created)
                // Use nameof(GetPaymentTransaction) to point to the action that retrieves a single transaction by its own ID.
                return CreatedAtAction(nameof(GetPaymentTransaction), new { id = paymentTransaction.Id }, paymentTransaction);
            }
            catch (DbUpdateException ex)
            {
                // Handle potential database errors
                return StatusCode(500, $"An error occured while saving the payment transaction: {ex.Message}. Inner Exception: {ex.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                // Catch any other unexpexted errors
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all PaymentTransaction records for a specific Applicant.
        /// </summary>
        /// <param name="applicantId">The ID of the Applicant.</param>
        /// <returns>A 200 OK response with a list of PaymentTransactions, or 404 Not Found.</returns>
        [HttpGet("{applicantId}/paymenttransactions")] // e.g., GET /api/applicants/1/paymenttransactions
        public async Task<IActionResult> GetPaymentTransactionForApplicant(int applicantId)
        {
            // Check if the Applicant exists
            var applicant = await _context.Applicants.FindAsync(applicantId);
            if (applicant == null)
            {
                return NotFound($"Applicant with ID {applicantId} not found.");
            }

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

            // If no transactions are found, return 404.
            if (transactions == null || !transactions.Any())
            {
                return NotFound($"No payment transactions found for Applicant with ID {applicantId}.");
            }

            return Ok(transactions); // Returns 200 Ok with the list of transactions
        }

        /// <summary>
        /// Retrieves a single PaymentTransaction record by its own Id.
        /// </summary>
        /// <param name="id">The ID of the PaymentTransaction record.</param>
        /// <returns>A 200 OK response with the PaymentTransaction, or 404 Not Found.</returns>
        [HttpGet("paymenttransactions/{id}")] // e.g., GET /api/applicants/paymenttransactions/1
        public async Task<IActionResult> GetPaymentTransaction(int id)
        {
            // Find the transaction by its primary key
            var transaction = await _context.PaymentTransactions
                .Include(pt => pt.Applicant)
                .FirstOrDefaultAsync(pt => pt.Id == id);

            if (transaction == null)
            {
                return NotFound($"Payment Transaction with id {id} is not found.");
            }

            return Ok(transaction); // Returns 200 Ok with the transaction data
        }


        /// <summary>
        /// Updates an existing PaymentTransaction record for a specific Applicant.
        /// </summary>
        /// <param name="applicantId">The ID of the Applicant to whom the PaymentTransaction belongs.</param>
        /// <param name="id">The ID of the PaymentTransaction record to update.</param>
        /// <param name="paymentTransaction">The PaymentTransaction object with updated data.</param>
        /// <returns>A 204 No Content response if successful, 400 Bad Request, or 404 Not Found.</returns>
        [HttpPut("{applicantId}/paymentTransactions/{id}")]
        public async Task<IActionResult> UpdatePaymentTransaction(int applicantId, int id, [FromBody] PaymentTransaction paymentTransaction)
        {
            if (id != paymentTransaction.Id)
            {
                return BadRequest("PaymentTransaction ID in the route does not match the ID in the request body.");
            }
            if (applicantId != paymentTransaction.ApplicantId)
            {
                return BadRequest("Applicant ID in the route does not match the ApplicantId in the request body.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingPaymentTransaction = await _context.PaymentTransactions
                                              .FirstOrDefaultAsync(p => p.Id == id && p.ApplicantId == applicantId);

            if (existingPaymentTransaction == null)
            {
                return NotFound($"PaymentTransaction with ID {id} not found or does not belong to Applicant ID {applicantId}.");
            }

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
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PaymentTransactionExists(id, applicantId))
                {
                    return NotFound($"PaymentTransaction with ID {id} not found or does not belong to Applicant ID {applicantId}.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Deletes a PaymentTransaction record for a specific Applicant.
        /// </summary>
        /// <param name="applicantId">The ID of the Applicant to whom the PaymentTransaction belongs.</param>
        /// <param name="id">The ID of the PaymentTransaction record to delete.</param>
        /// <returns>A 204 No Content response if successful, or 404 Not Found.</returns>
        [HttpDelete("{applicantId}/paymentTransactions/{id}")]
        public async Task<IActionResult> DeletePaymentTransaction(int applicantId, int id)
        {
            var paymentTransaction = await _context.PaymentTransactions
                                      .FirstOrDefaultAsync(p => p.Id == id && p.ApplicantId == applicantId);

            if (paymentTransaction == null)
            {
                return NotFound($"PaymentTransaction with ID {id} not found or does not belong to Applicant ID {applicantId}.");
            }

            _context.PaymentTransactions.Remove(paymentTransaction);
            await _context.SaveChangesAsync();

            var applicant = await _context.Applicants.FindAsync(applicantId);
            if (applicant != null)
            {
                applicant.LastModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        // Helper method
        private bool PaymentTransactionExists(int id, int applicantId)
        {
            return _context.PaymentTransactions.Any(e => e.Id == id && e.ApplicantId == applicantId);
        }



        /// <summary>
        /// Updates an existing Applicant record.
        /// </summary>
        /// <param name="id">The ID of the Applicant to update (from route).</param>
        /// <param name="applicant">The Applicant object with updated data (from body).</param>
        /// <returns>A 204 No Content response if successful, 400 Bad Request, or 404 Not Found.</returns>
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

            // 3. Find the existing applicant in the database first.
            // This ensures EF Core is tracking the actual entity you want to modify.
            var existingApplicant = await _context.Applicants.FindAsync(id);

            if (existingApplicant == null)
            {
                return NotFound($"Applicant with ID {id} not found."); // Returns 404 if not found
            }

            // 4. Update the properties of the *existing tracked entity* with the new values
            // from the 'applicant' object received in the request body.
            // CurrentValues.SetValues() is an efficient way to copy all scalar/complex properties.
            _context.Entry(existingApplicant).CurrentValues.SetValues(applicant);

            // 5. Ensure LastModifiedDate is updated to the current time, as the record has changed.
            existingApplicant.LastModifiedDate = DateTime.UtcNow;

            // Note: If you have properties like 'CreatedDate' that should *never* be updated
            // after initial creation, you might explicitly mark them as not modified:
            _context.Entry(existingApplicant).Property(a => a.CreatedDate).IsModified = false;

            try
            {
                // 6. Save changes to the database. EF Core will detect the modifications to 'existingApplicant'.
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // 7. Handle concurrency conflicts (if another user updated simultaneously)
                if (!ApplicantExists(id)) // Use your helper method
                {
                    return NotFound($"Applicant with ID {id} not found.");
                }
                else
                {
                    // If it still exists but there's a concurrency issue, re-throw or implement retry logic.
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating the applicant: {ex.Message}. Inner Exception: {ex.InnerException?.Message}");
            }

            // 8. Return 204 No Content for successful update (REST best practice)
            return NoContent();
        }



        // Helper method (add this inside your ApplicantsController class)
        private bool ApplicantExists(int id)
        {
            return _context.Applicants.Any(e => e.Id == id);
        }


        /// <summary>
        /// Deletes an Applicant record and all its associated data (Identifications, Beneficiaries, Assets, etc.).
        /// </summary>
        /// <param name="id">The ID of the Applicant to delete.</param>
        /// <returns>A 204 No Content response if successful, or 404 Not Found.</returns>
        [HttpDelete("{id}")] // e.g., DELETE /api/applicants/1
        public async Task<IActionResult> DeleteApplicant(int id)
        {
            // 1. Find the Applicant to delete
            var applicant = await _context.Applicants.FindAsync(id);
            if (applicant == null)
            {
                return NotFound($"Applicant with ID {id} not found."); // Returns 404
            }

            try
            {
                // 2. Remove the Applicant
                _context.Applicants.Remove(applicant);
                await _context.SaveChangesAsync();

                // Important: Due to how you've set up relationships (one-to-many, e.g., Applicant has ICollection<Beneficiary>),
                // Entity Framework Core will, by default, handle cascading deletes.
                // This means when you delete an Applicant, all its associated Identifications, Beneficiaries, Assets,
                // Executors, Guardians, BeneficiaryAssetAllocations, and PaymentTransactions will also be deleted from the database.
                // This is generally desired for parent-child relationships like this, but be aware of its impact.

                // 3. Return 204 No Content for successful deletion
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while deleting the applicant: {ex.Message}. Inner Exception: {ex.InnerException?.Message}");
            }
        }


        
    }
}
