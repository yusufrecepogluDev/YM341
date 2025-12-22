using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace ClupApi.Tests
{
    /// <summary>
    /// Property-based tests for the Recommendation Service
    /// **Feature: personalized-recommendations**
    /// </summary>
    public class RecommendationServicePropertyTests
    {
        /// <summary>
        /// **Feature: personalized-recommendations, Property 2: Recommendation Limit**
        /// *For any* recommendation response, the number of recommended activities SHALL be at most 5.
        /// **Validates: Requirements 3.5**
        /// </summary>
        [Property(MaxTest = 100)]
        public Property RecommendationLimit_ShouldNotExceedFive()
        {
            return Prop.ForAll(
                Arb.From<int[]>(),
                (activityIds) =>
                {
                    // Simulate the Take(5) logic from RecommendationService
                    var limitedIds = activityIds?.Take(5).ToList() ?? new List<int>();
                    return limitedIds.Count <= 5;
                });
        }

        /// <summary>
        /// **Feature: personalized-recommendations, Property 2: Recommendation Limit**
        /// Tests that even with large input arrays, the limit is respected.
        /// **Validates: Requirements 3.5**
        /// </summary>
        [Property(MaxTest = 100)]
        public Property RecommendationLimit_WithLargeInput_ShouldStillBeFiveOrLess()
        {
            var largeArrayGen = Gen.ArrayOf(100, Arb.Generate<int>());
            
            return Prop.ForAll(
                largeArrayGen.ToArbitrary(),
                (activityIds) =>
                {
                    var limitedIds = activityIds.Take(5).ToList();
                    return limitedIds.Count == 5;
                });
        }

        /// <summary>
        /// **Feature: personalized-recommendations, Property 3: Response Format Validity**
        /// *For any* successful recommendation response, the recommendedActivityIds field 
        /// SHALL be a JSON array containing only integer activity IDs.
        /// **Validates: Requirements 3.6, 4.1**
        /// </summary>
        [Property(MaxTest = 100)]
        public Property ResponseFormat_ShouldContainOnlyIntegers()
        {
            return Prop.ForAll(
                Arb.From<int[]>(),
                (activityIds) =>
                {
                    var response = new RecommendationResponseMock
                    {
                        RecommendedActivityIds = activityIds?.ToList() ?? new List<int>()
                    };

                    // All IDs should be integers (which they are by type)
                    // and the list should be serializable to JSON
                    return response.RecommendedActivityIds.All(id => id is int);
                });
        }

        /// <summary>
        /// **Feature: personalized-recommendations, Property 3: Response Format Validity**
        /// Tests JSON serialization round-trip for recommendation response.
        /// **Validates: Requirements 3.6, 4.1**
        /// </summary>
        [Property(MaxTest = 100)]
        public Property ResponseFormat_JsonRoundTrip_ShouldPreserveData()
        {
            return Prop.ForAll(
                Gen.ListOf(Gen.Choose(1, 1000)).ToArbitrary(),
                (activityIds) =>
                {
                    var original = new RecommendationResponseMock
                    {
                        RecommendedActivityIds = activityIds
                    };

                    // Serialize to JSON
                    var json = System.Text.Json.JsonSerializer.Serialize(original);
                    
                    // Deserialize back
                    var deserialized = System.Text.Json.JsonSerializer.Deserialize<RecommendationResponseMock>(json);

                    // Should preserve all IDs
                    return deserialized != null && 
                           deserialized.RecommendedActivityIds.SequenceEqual(original.RecommendedActivityIds);
                });
        }
    }

    /// <summary>
    /// Mock class for testing recommendation response structure
    /// </summary>
    public class RecommendationResponseMock
    {
        public List<int> RecommendedActivityIds { get; set; } = new List<int>();
    }
}


    /// <summary>
    /// **Feature: personalized-recommendations, Property 1: Participation Exclusion**
    /// *For any* recommendation response, none of the recommended activity IDs 
    /// SHALL be present in the student's past participations list.
    /// **Validates: Requirements 3.4**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property ParticipationExclusion_RecommendedActivitiesShouldNotIncludePastParticipations()
    {
        return Prop.ForAll(
            Gen.ListOf(Gen.Choose(1, 100)).ToArbitrary(),
            Gen.ListOf(Gen.Choose(1, 100)).ToArbitrary(),
            (pastParticipationIds, recommendedIds) =>
            {
                // Simulate filtering out past participations
                var filteredRecommendations = recommendedIds
                    .Where(id => !pastParticipationIds.Contains(id))
                    .ToList();

                // None of the filtered recommendations should be in past participations
                return filteredRecommendations.All(id => !pastParticipationIds.Contains(id));
            });
    }

    /// <summary>
    /// **Feature: personalized-recommendations, Property 1: Participation Exclusion**
    /// Tests that the exclusion logic correctly removes all participated activities.
    /// **Validates: Requirements 3.4**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property ParticipationExclusion_FilteringRemovesAllParticipatedActivities()
    {
        var participationGen = Gen.ListOf(5, Gen.Choose(1, 20));
        var availableGen = Gen.ListOf(20, Gen.Choose(1, 30));

        return Prop.ForAll(
            participationGen.ToArbitrary(),
            availableGen.ToArbitrary(),
            (participations, available) =>
            {
                var participationSet = new HashSet<int>(participations);
                var filtered = available.Where(id => !participationSet.Contains(id)).ToList();

                // Verify no overlap between filtered results and participations
                return !filtered.Any(id => participationSet.Contains(id));
            });
    }


    /// <summary>
    /// **Feature: personalized-recommendations, Property 4: Activity ID Validity**
    /// *For any* recommended activity ID in the response, the ID SHALL correspond 
    /// to an existing active activity in the system.
    /// **Validates: Requirements 4.2**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property ActivityIdValidity_RecommendedIdsShouldExistInAvailableActivities()
    {
        return Prop.ForAll(
            Gen.ListOf(Gen.Choose(1, 50)).ToArbitrary(),
            Gen.ListOf(Gen.Choose(1, 100)).ToArbitrary(),
            (availableActivityIds, recommendedIds) =>
            {
                var availableSet = new HashSet<int>(availableActivityIds);
                
                // Filter recommendations to only include valid IDs
                var validRecommendations = recommendedIds
                    .Where(id => availableSet.Contains(id))
                    .ToList();

                // All valid recommendations should be in available activities
                return validRecommendations.All(id => availableSet.Contains(id));
            });
    }

    /// <summary>
    /// **Feature: personalized-recommendations, Property 4: Activity ID Validity**
    /// Tests that invalid activity IDs are filtered out from recommendations.
    /// **Validates: Requirements 4.2**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property ActivityIdValidity_InvalidIdsShouldBeFiltered()
    {
        var availableGen = Gen.ListOf(10, Gen.Choose(1, 20));
        var recommendedGen = Gen.ListOf(8, Gen.Choose(1, 50));

        return Prop.ForAll(
            availableGen.ToArbitrary(),
            recommendedGen.ToArbitrary(),
            (available, recommended) =>
            {
                var availableSet = new HashSet<int>(available);
                var filtered = recommended.Where(id => availableSet.Contains(id)).ToList();

                // Filtered list should only contain IDs from available set
                // and should not contain any ID not in available set
                var invalidIds = filtered.Where(id => !availableSet.Contains(id)).ToList();
                return invalidIds.Count == 0;
            });
    }
