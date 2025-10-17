# Database Migration Required

## ProductReview.IsApproved Column Change

### Change Summary
The `IsApproved` column in the `ProductReviews` table has been changed from `bool` (NOT NULL) to `bool?` (NULL).

### Previous State
```sql
IsApproved bit NOT NULL DEFAULT 0
```

### New State
```sql
IsApproved bit NULL DEFAULT NULL
```

### Three-State Logic
- `NULL` = Pending (default for new reviews)
- `TRUE` = Approved
- `FALSE` = Rejected

### Migration Steps

#### Option 1: Using EF Core Migrations
```bash
# Add migration
dotnet ef migrations add ChangeIsApprovedToNullable --project AutoPartsStore.Infrastructure --startup-project AutoPartsStore.Web

# Update database
dotnet ef database update --project AutoPartsStore.Infrastructure --startup-project AutoPartsStore.Web
```

#### Option 2: Manual SQL Script
```sql
-- Step 1: Update existing data (optional - set all existing false values to null for pending)
UPDATE ProductReviews 
SET IsApproved = NULL 
WHERE IsApproved = 0;

-- Step 2: Alter column to allow NULL
ALTER TABLE ProductReviews 
ALTER COLUMN IsApproved bit NULL;

-- Step 3: Update default constraint
ALTER TABLE ProductReviews 
DROP CONSTRAINT IF EXISTS DF_ProductReviews_IsApproved;

ALTER TABLE ProductReviews 
ADD CONSTRAINT DF_ProductReviews_IsApproved DEFAULT NULL FOR IsApproved;
```

### Code Changes Summary

#### Files Modified:
1. **AutoPartsStore.Core/Entities/ProductReview.cs**
   - Changed `IsApproved` from `bool` to `bool?`
   - Added `SetPending()` method
   - Added `GetApprovalStatus()` helper method
   - Updated constructor to set `IsApproved = null`

2. **AutoPartsStore.Core/Models/ProductReviews/ProductReviewDto.cs**
   - Changed `IsApproved` from `bool` to `bool?`

3. **AutoPartsStore.Core/Models/ProductReviews/ReviewApprovalRequest.cs**
   - Changed `IsApproved` from `bool` to `bool?`

4. **AutoPartsStore.Infrastructure/Configuration/ProductReviewConfiguration.cs**
   - Changed `.IsRequired()` to `.IsRequired(false)`
   - Changed default value from `false` to `null`

5. **AutoPartsStore.Infrastructure/Repositories/ProductReviewRepository.cs**
   - Updated all Status assignments to handle three states
   - Fixed `GetPendingReviewsAsync()` to filter `IsApproved == null`
   - Updated all approved reviews queries to `IsApproved == true`

6. **AutoPartsStore.Infrastructure/Repositories/CarPartRepository.cs**
   - Fixed all `IsApproved` comparisons to explicitly compare with `true`

7. **AutoPartsStore.Infrastructure/Services/ProductReviewService.cs**
   - Updated `ApproveReviewAsync()` to accept `bool?` parameter
   - Added handling for three states (approve, reject, pending)

8. **AutoPartsStore.Core/Interfaces/IServices/IProductReviewService.cs**
   - Updated interface signature for `ApproveReviewAsync()`

9. **AutoPartsStore.Web/Controllers/ProductReviewsController.cs**
   - Updated success message to handle three states

### API Changes

#### PATCH /api/reviews/{reviewId}/approval
**Request Body:**
```json
{
  "isApproved": true   // true = Approve, false = Reject, null = Set to Pending
}
```

**Response Examples:**
```json
// Approve
{
  "success": true,
  "message": "Review approved successfully",
  "data": null,
  "errors": []
}

// Reject
{
  "success": true,
  "message": "Review rejected successfully",
  "data": null,
  "errors": []
}

// Set to Pending
{
  "success": true,
  "message": "Review set to pending successfully",
  "data": null,
  "errors": []
}
```

### Testing Checklist
- [ ] Create new review (should be pending by default)
- [ ] Approve review (IsApproved = true)
- [ ] Reject review (IsApproved = false)
- [ ] Set review back to pending (IsApproved = null)
- [ ] Get pending reviews (only null values)
- [ ] Get approved reviews (only true values)
- [ ] Get all reviews
- [ ] Check review counts and ratings (only approved)

### Rollback Plan
If needed, revert to previous state:
```sql
-- Set all NULL values to false
UPDATE ProductReviews 
SET IsApproved = 0 
WHERE IsApproved IS NULL;

-- Make column NOT NULL again
ALTER TABLE ProductReviews 
ALTER COLUMN IsApproved bit NOT NULL;
```
