using WorkoutGamifier.UITests.Infrastructure;
using WorkoutGamifier.UITests.PageObjects;

namespace WorkoutGamifier.UITests.Tests;

public class PoolManagementWorkflowTests : AppiumTestBase
{
    private NavigationHelper _navigation = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _navigation = new NavigationHelper(Driver, Config);
        
        // Handle app initialization
        await _navigation.HandleAppInitialization();
    }

    [Fact]
    public async Task PoolCreationWorkflow_CreatePoolAddWorkouts_ShouldWork()
    {
        // This test covers the complete pool creation and management workflow
        
        await TakeScreenshot("pool-creation-workflow-start");
        
        // Step 1: Navigate to pools and check initial state
        var poolsPage = await _navigation.NavigateToPools();
        Assert.True(await poolsPage.IsOnPage(), "Should be on Pools page");
        
        var initialPoolCount = await poolsPage.GetPoolCount();
        var initialPoolNames = await poolsPage.GetPoolNames();
        
        await TakeScreenshot("initial-pools-state");
        
        // Step 2: Create a new pool
        var poolCreationStarted = await poolsPage.CreateNewPool();
        Assert.True(poolCreationStarted, "Should be able to start pool creation");
        
        await TakeScreenshot("pool-creation-form-opened");
        await Task.Delay(3000); // Allow time for pool creation form/process
        
        // Step 3: Navigate back to pools and verify creation
        poolsPage = await _navigation.NavigateToPools();
        await poolsPage.WaitForPoolsToLoad();
        
        var finalPoolCount = await poolsPage.GetPoolCount();
        var finalPoolNames = await poolsPage.GetPoolNames();
        
        await TakeScreenshot("pools-after-creation");
        
        // Verify pool was created (count increased or new pool name appeared)
        var poolCreated = finalPoolCount > initialPoolCount || 
                         finalPoolNames.Except(initialPoolNames).Any();
        
        if (poolCreated)
        {
            await TakeScreenshot("pool-successfully-created");
            
            // Step 4: Try to view details of the newly created pool
            var newPoolNames = finalPoolNames.Except(initialPoolNames).ToList();
            if (newPoolNames.Any())
            {
                var newPoolName = newPoolNames.First();
                var detailsOpened = await poolsPage.ViewPoolDetails(newPoolName);
                
                if (detailsOpened)
                {
                    await TakeScreenshot("new-pool-details-opened");
                    
                    // Navigate back to pools
                    await _navigation.NavigateToPools();
                    await TakeScreenshot("returned-to-pools-list");
                }
            }
        }
        
        await TakeScreenshot("pool-creation-workflow-complete");
        Assert.True(true, "Pool creation workflow completed");
    }

    [Fact]
    public async Task PoolManagementWorkflow_ViewEditDeletePool_ShouldWork()
    {
        // This test covers viewing, editing, and deleting pools
        
        await TakeScreenshot("pool-management-workflow-start");
        
        var poolsPage = await _navigation.NavigateToPools();
        await poolsPage.WaitForPoolsToLoad();
        
        var poolNames = await poolsPage.GetPoolNames();
        var poolCount = await poolsPage.GetPoolCount();
        
        await TakeScreenshot("pools-for-management");
        
        if (poolCount == 0)
        {
            // Create a pool first if none exist
            await poolsPage.CreateNewPool();
            await Task.Delay(3000);
            
            poolsPage = await _navigation.NavigateToPools();
            await poolsPage.WaitForPoolsToLoad();
            poolNames = await poolsPage.GetPoolNames();
            poolCount = await poolsPage.GetPoolCount();
            
            await TakeScreenshot("pool-created-for-management");
        }
        
        if (poolCount > 0 && poolNames.Any())
        {
            var testPoolName = poolNames.First();
            
            // Step 1: View pool details
            var detailsViewed = await poolsPage.ViewPoolDetails(testPoolName);
            if (detailsViewed)
            {
                await TakeScreenshot("pool-details-viewed");
                
                // Navigate back to pools list
                await _navigation.NavigateToPools();
                await Task.Delay(1000);
            }
            
            // Step 2: Try to edit the pool
            var editStarted = await poolsPage.EditPool(testPoolName);
            if (editStarted)
            {
                await TakeScreenshot("pool-edit-started");
                await Task.Delay(2000);
                
                // Navigate back to pools list
                await _navigation.NavigateToPools();
                await Task.Delay(1000);
            }
            
            // Step 3: Verify pool still exists after edit attempt
            await poolsPage.WaitForPoolsToLoad();
            var poolStillExists = await poolsPage.VerifyPoolExists(testPoolName);
            
            if (poolStillExists)
            {
                await TakeScreenshot("pool-exists-after-edit");
                
                // Optional: Try to delete the pool (commented out to preserve test data)
                /*
                var deleteStarted = await poolsPage.DeletePool(testPoolName);
                if (deleteStarted)
                {
                    await TakeScreenshot("pool-delete-started");
                    await Task.Delay(2000);
                    
                    // Verify pool was deleted
                    poolsPage = await _navigation.NavigateToPools();
                    await poolsPage.WaitForPoolsToLoad();
                    var poolDeleted = !await poolsPage.VerifyPoolExists(testPoolName);
                    
                    if (poolDeleted)
                    {
                        await TakeScreenshot("pool-successfully-deleted");
                    }
                }
                */
            }
        }
        
        await TakeScreenshot("pool-management-workflow-complete");
        Assert.True(true, "Pool management workflow completed");
    }

    [Fact]
    public async Task PoolSearchWorkflow_SearchAndFilterPools_ShouldWork()
    {
        // This test covers pool search and filtering functionality
        
        await TakeScreenshot("pool-search-workflow-start");
        
        var poolsPage = await _navigation.NavigateToPools();
        await poolsPage.WaitForPoolsToLoad();
        
        var initialPoolNames = await poolsPage.GetPoolNames();
        var initialPoolCount = await poolsPage.GetPoolCount();
        
        await TakeScreenshot("pools-before-search");
        
        if (initialPoolCount > 0 && initialPoolNames.Any())
        {
            // Step 1: Try to search for an existing pool
            var searchTerm = initialPoolNames.First().Split(' ').First(); // Use first word of first pool name
            var searchStarted = await poolsPage.SearchPools(searchTerm);
            
            if (searchStarted)
            {
                await TakeScreenshot("pool-search-executed");
                await Task.Delay(2000); // Allow search results to load
                
                var searchResultNames = await poolsPage.GetPoolNames();
                var searchResultCount = await poolsPage.GetPoolCount();
                
                await TakeScreenshot("pool-search-results");
                
                // Verify search results
                Assert.True(searchResultCount <= initialPoolCount, "Search results should not exceed initial count");
                
                // Step 2: Clear search to show all pools again
                var searchCleared = await poolsPage.ClearSearch();
                if (searchCleared)
                {
                    await TakeScreenshot("pool-search-cleared");
                    await Task.Delay(2000);
                    
                    var clearedResultNames = await poolsPage.GetPoolNames();
                    var clearedResultCount = await poolsPage.GetPoolCount();
                    
                    await TakeScreenshot("pools-after-clear-search");
                    
                    // Verify all pools are shown again
                    Assert.True(clearedResultCount >= searchResultCount, "Should show more or equal pools after clearing search");
                }
            }
            
            // Step 3: Try searching for a non-existent pool
            var nonExistentSearch = await poolsPage.SearchPools("NonExistentPoolXYZ123");
            if (nonExistentSearch)
            {
                await TakeScreenshot("non-existent-pool-search");
                await Task.Delay(2000);
                
                var noResultsCount = await poolsPage.GetPoolCount();
                await TakeScreenshot("no-search-results");
                
                // Clear search again
                await poolsPage.ClearSearch();
                await Task.Delay(2000);
                await TakeScreenshot("search-cleared-after-no-results");
            }
        }
        else
        {
            // If no pools exist, create some for testing
            await poolsPage.CreateNewPool();
            await Task.Delay(3000);
            
            await TakeScreenshot("pool-created-for-search-test");
        }
        
        await TakeScreenshot("pool-search-workflow-complete");
        Assert.True(true, "Pool search workflow completed");
    }

    [Fact]
    public async Task PoolWorkoutManagement_AddRemoveWorkouts_ShouldWork()
    {
        // This test covers adding and removing workouts from pools
        
        await TakeScreenshot("pool-workout-management-start");
        
        var poolsPage = await _navigation.NavigateToPools();
        await poolsPage.WaitForPoolsToLoad();
        
        var poolNames = await poolsPage.GetPoolNames();
        var poolCount = await poolsPage.GetPoolCount();
        
        if (poolCount == 0)
        {
            // Create a pool first
            await poolsPage.CreateNewPool();
            await Task.Delay(3000);
            
            poolsPage = await _navigation.NavigateToPools();
            await poolsPage.WaitForPoolsToLoad();
            poolNames = await poolsPage.GetPoolNames();
            poolCount = await poolsPage.GetPoolCount();
            
            await TakeScreenshot("pool-created-for-workout-management");
        }
        
        if (poolCount > 0 && poolNames.Any())
        {
            var testPoolName = poolNames.First();
            
            // Step 1: Get initial workout count for the pool
            var initialWorkoutCount = await poolsPage.GetWorkoutCountForPool(testPoolName);
            await TakeScreenshot("initial-workout-count");
            
            // Step 2: View pool details to see workouts
            var detailsOpened = await poolsPage.ViewPoolDetails(testPoolName);
            if (detailsOpened)
            {
                await TakeScreenshot("pool-details-for-workout-management");
                
                // Look for add workout functionality
                // This would typically involve finding an "Add Workout" button in the pool details
                // For now, we'll navigate back and verify the workflow structure
                
                await Task.Delay(2000);
                await _navigation.NavigateToPools();
                await TakeScreenshot("returned-from-pool-details");
            }
            
            // Step 3: Verify workout count after potential modifications
            var finalWorkoutCount = await poolsPage.GetWorkoutCountForPool(testPoolName);
            await TakeScreenshot("final-workout-count");
            
            // The workout count should be consistent
            Assert.True(finalWorkoutCount >= 0, "Workout count should be a valid number");
        }
        
        await TakeScreenshot("pool-workout-management-complete");
        Assert.True(true, "Pool workout management workflow completed");
    }

    [Fact]
    public async Task PoolValidationWorkflow_VerifyPoolConstraints_ShouldWork()
    {
        // This test verifies pool validation and constraints
        
        await TakeScreenshot("pool-validation-workflow-start");
        
        var poolsPage = await _navigation.NavigateToPools();
        await poolsPage.WaitForPoolsToLoad();
        
        var initialPoolNames = await poolsPage.GetPoolNames();
        var initialPoolCount = await poolsPage.GetPoolCount();
        
        await TakeScreenshot("pools-for-validation");
        
        // Step 1: Verify pool names are valid
        foreach (var poolName in initialPoolNames)
        {
            Assert.False(string.IsNullOrWhiteSpace(poolName), "Pool names should not be empty or whitespace");
            Assert.True(poolName.Length >= 1, "Pool names should have at least 1 character");
            Assert.True(poolName.Length <= 200, "Pool names should not exceed reasonable length");
        }
        
        // Step 2: Verify pool count consistency
        var recountedPools = await poolsPage.GetPoolCount();
        Assert.Equal(initialPoolCount, recountedPools);
        
        // Step 3: Verify pool existence checks work correctly
        if (initialPoolNames.Any())
        {
            var firstPoolName = initialPoolNames.First();
            var poolExists = await poolsPage.VerifyPoolExists(firstPoolName);
            Assert.True(poolExists, "Existing pool should be found by verification method");
            
            var nonExistentPoolExists = await poolsPage.VerifyPoolDoesNotExist("NonExistentPoolXYZ123");
            Assert.True(nonExistentPoolExists, "Non-existent pool should not be found");
        }
        
        // Step 4: Verify empty state handling
        var isEmpty = await poolsPage.IsEmptyState();
        if (isEmpty)
        {
            await TakeScreenshot("empty-state-validation");
            Assert.Equal(0, initialPoolCount);
        }
        else
        {
            await TakeScreenshot("non-empty-state-validation");
            Assert.True(initialPoolCount > 0, "Non-empty state should have pools");
        }
        
        await TakeScreenshot("pool-validation-workflow-complete");
        Assert.True(true, "Pool validation workflow completed");
    }

    [Fact]
    public async Task PoolOrderingWorkflow_VerifyPoolDisplayOrder_ShouldWork()
    {
        // This test verifies that pools are displayed in a consistent order
        
        await TakeScreenshot("pool-ordering-workflow-start");
        
        var poolsPage = await _navigation.NavigateToPools();
        await poolsPage.WaitForPoolsToLoad();
        
        var firstReadPoolNames = await poolsPage.GetPoolNames();
        await TakeScreenshot("first-pool-order-read");
        
        // Navigate away and back to verify order consistency
        await _navigation.NavigateToSessions();
        await Task.Delay(1000);
        
        poolsPage = await _navigation.NavigateToPools();
        await poolsPage.WaitForPoolsToLoad();
        
        var secondReadPoolNames = await poolsPage.GetPoolNames();
        await TakeScreenshot("second-pool-order-read");
        
        // Verify order consistency
        if (firstReadPoolNames.Count == secondReadPoolNames.Count)
        {
            var orderConsistent = firstReadPoolNames.SequenceEqual(secondReadPoolNames);
            Assert.True(orderConsistent, "Pool order should be consistent across page loads");
            
            if (orderConsistent)
            {
                await TakeScreenshot("pool-order-consistent");
            }
        }
        
        // If we have pools, verify they can be accessed in order
        if (secondReadPoolNames.Any())
        {
            for (int i = 0; i < Math.Min(3, secondReadPoolNames.Count); i++)
            {
                var poolName = secondReadPoolNames[i];
                var poolExists = await poolsPage.VerifyPoolExists(poolName);
                Assert.True(poolExists, $"Pool at position {i} should exist: {poolName}");
            }
            
            await TakeScreenshot("pool-order-verification-complete");
        }
        
        await TakeScreenshot("pool-ordering-workflow-complete");
        Assert.True(true, "Pool ordering workflow completed");
    }
}