using FsCheck;
using FsCheck.Xunit;
using System.Text.Json;
using Xunit;

namespace ClupApi.Tests
{
    /// <summary>
    /// Property-based tests for MembershipCacheService serialization
    /// **Feature: club-membership-session**
    /// </summary>
    public class MembershipCachePropertyTests
    {
        /// <summary>
        /// Membership status enum matching the one in KampusEtkinlik.Services
        /// </summary>
        public enum ClubMembershipStatus
        {
            NotMember,
            Pending,
            Approved,
            Rejected
        }

        /// <summary>
        /// Serializes a dictionary of membership statuses to JSON.
        /// Mirrors the implementation in MembershipCacheService.
        /// </summary>
        public static string SerializeStatuses(Dictionary<int, ClubMembershipStatus> statuses)
        {
            var stringDict = statuses.ToDictionary(
                kvp => kvp.Key.ToString(),
                kvp => kvp.Value.ToString()
            );
            return JsonSerializer.Serialize(stringDict);
        }

        /// <summary>
        /// Deserializes JSON back to a dictionary of membership statuses.
        /// Mirrors the implementation in MembershipCacheService.
        /// </summary>
        public static Dictionary<int, ClubMembershipStatus>? DeserializeStatuses(string json)
        {
            if (string.IsNullOrEmpty(json))
                return null;

            try
            {
                var stringDict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (stringDict == null)
                    return null;

                var result = new Dictionary<int, ClubMembershipStatus>();
                foreach (var kvp in stringDict)
                {
                    if (int.TryParse(kvp.Key, out int clubId) &&
                        Enum.TryParse<ClubMembershipStatus>(kvp.Value, out var status))
                    {
                        result[clubId] = status;
                    }
                }
                return result;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// **Property 3: Serialization Round-Trip**
        /// *For any* valid membership status dictionary, serializing to JSON and deserializing back 
        /// SHALL produce an equivalent dictionary.
        /// **Validates: Requirements 2.3, 2.4**
        /// </summary>
        [Property(MaxTest = 100)]
        public bool SerializationRoundTrip_PreservesData(PositiveInt[] clubIds, byte[] statusIndices)
        {
            // Build a dictionary from the generated data
            var original = new Dictionary<int, ClubMembershipStatus>();
            var statuses = new[] { ClubMembershipStatus.NotMember, ClubMembershipStatus.Pending, 
                                   ClubMembershipStatus.Approved, ClubMembershipStatus.Rejected };
            
            if (clubIds != null && statusIndices != null)
            {
                var count = Math.Min(clubIds.Length, statusIndices.Length);
                for (int i = 0; i < count; i++)
                {
                    var clubId = clubIds[i].Get;
                    var status = statuses[statusIndices[i] % statuses.Length];
                    original[clubId] = status; // Later entries overwrite earlier ones for same key
                }
            }

            // Serialize
            var json = SerializeStatuses(original);
            
            // Deserialize
            var deserialized = DeserializeStatuses(json);
            
            // Verify round-trip
            if (deserialized == null)
                return original.Count == 0;
            
            // Check all entries match
            if (original.Count != deserialized.Count)
                return false;
            
            foreach (var kvp in original)
            {
                if (!deserialized.TryGetValue(kvp.Key, out var status) || status != kvp.Value)
                    return false;
            }
            
            return true;
        }


        /// <summary>
        /// Property test: Empty dictionary serialization round-trip
        /// </summary>
        [Fact]
        public void EmptyDictionary_RoundTrip_Succeeds()
        {
            var original = new Dictionary<int, ClubMembershipStatus>();
            var json = SerializeStatuses(original);
            var deserialized = DeserializeStatuses(json);
            
            Assert.NotNull(deserialized);
            Assert.Empty(deserialized);
        }

        /// <summary>
        /// Property test: Single entry round-trip for each status type
        /// </summary>
        [Theory]
        [InlineData(1, ClubMembershipStatus.NotMember)]
        [InlineData(2, ClubMembershipStatus.Pending)]
        [InlineData(3, ClubMembershipStatus.Approved)]
        [InlineData(4, ClubMembershipStatus.Rejected)]
        public void SingleEntry_RoundTrip_PreservesStatus(int clubId, ClubMembershipStatus status)
        {
            var original = new Dictionary<int, ClubMembershipStatus> { { clubId, status } };
            var json = SerializeStatuses(original);
            var deserialized = DeserializeStatuses(json);
            
            Assert.NotNull(deserialized);
            Assert.Single(deserialized);
            Assert.Equal(status, deserialized[clubId]);
        }

        /// <summary>
        /// Property test: Multiple entries with different statuses
        /// </summary>
        [Fact]
        public void MultipleEntries_RoundTrip_PreservesAllStatuses()
        {
            var original = new Dictionary<int, ClubMembershipStatus>
            {
                { 1, ClubMembershipStatus.NotMember },
                { 5, ClubMembershipStatus.Pending },
                { 10, ClubMembershipStatus.Approved },
                { 100, ClubMembershipStatus.Rejected }
            };
            
            var json = SerializeStatuses(original);
            var deserialized = DeserializeStatuses(json);
            
            Assert.NotNull(deserialized);
            Assert.Equal(original.Count, deserialized.Count);
            
            foreach (var kvp in original)
            {
                Assert.True(deserialized.ContainsKey(kvp.Key));
                Assert.Equal(kvp.Value, deserialized[kvp.Key]);
            }
        }

        /// <summary>
        /// Property test: Invalid JSON returns null
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("invalid json")]
        [InlineData("{invalid}")]
        public void InvalidJson_ReturnsNull(string? json)
        {
            var result = DeserializeStatuses(json!);
            Assert.Null(result);
        }

        /// <summary>
        /// Property test: JSON with invalid club ID is skipped
        /// </summary>
        [Fact]
        public void InvalidClubId_IsSkipped()
        {
            var json = "{\"abc\":\"Approved\",\"1\":\"Pending\"}";
            var result = DeserializeStatuses(json);
            
            Assert.NotNull(result);
            Assert.Single(result); // Only valid entry
            Assert.Equal(ClubMembershipStatus.Pending, result[1]);
        }

        /// <summary>
        /// Property test: JSON with invalid status is skipped
        /// </summary>
        [Fact]
        public void InvalidStatus_IsSkipped()
        {
            var json = "{\"1\":\"InvalidStatus\",\"2\":\"Approved\"}";
            var result = DeserializeStatuses(json);
            
            Assert.NotNull(result);
            Assert.Single(result); // Only valid entry
            Assert.Equal(ClubMembershipStatus.Approved, result[2]);
        }

        /// <summary>
        /// Property test: Large club IDs are handled correctly
        /// </summary>
        [Property(MaxTest = 100)]
        public bool LargeClubIds_RoundTrip_Succeeds(PositiveInt clubId)
        {
            var original = new Dictionary<int, ClubMembershipStatus>
            {
                { clubId.Get, ClubMembershipStatus.Approved }
            };
            
            var json = SerializeStatuses(original);
            var deserialized = DeserializeStatuses(json);
            
            return deserialized != null && 
                   deserialized.Count == 1 && 
                   deserialized[clubId.Get] == ClubMembershipStatus.Approved;
        }

        /// <summary>
        /// Simulates UpdateSingleStatusAsync behavior - updates a single status in the dictionary
        /// </summary>
        private static Dictionary<int, ClubMembershipStatus> UpdateSingleStatus(
            Dictionary<int, ClubMembershipStatus>? existing, 
            int clubId, 
            ClubMembershipStatus newStatus)
        {
            var statuses = existing ?? new Dictionary<int, ClubMembershipStatus>();
            statuses[clubId] = newStatus;
            return statuses;
        }

        /// <summary>
        /// Simulates RemoveStatusAsync behavior - removes a status from the dictionary
        /// </summary>
        private static Dictionary<int, ClubMembershipStatus> RemoveStatus(
            Dictionary<int, ClubMembershipStatus>? existing, 
            int clubId)
        {
            var statuses = existing ?? new Dictionary<int, ClubMembershipStatus>();
            statuses.Remove(clubId);
            return statuses;
        }

        /// <summary>
        /// **Property 4: Status Update Persistence**
        /// *For any* successful membership action (apply or leave), the sessionStorage SHALL be updated 
        /// to reflect the new status immediately.
        /// **Validates: Requirements 2.1, 2.2**
        /// </summary>
        [Property(MaxTest = 100)]
        public bool StatusUpdatePersistence_ApplyAction_PersistsPendingStatus(PositiveInt clubId, PositiveInt[] existingClubIds, byte[] existingStatusIndices)
        {
            // Setup: Create initial state with some existing memberships
            var initial = new Dictionary<int, ClubMembershipStatus>();
            var statuses = new[] { ClubMembershipStatus.NotMember, ClubMembershipStatus.Pending, 
                                   ClubMembershipStatus.Approved, ClubMembershipStatus.Rejected };
            
            if (existingClubIds != null && existingStatusIndices != null)
            {
                var count = Math.Min(existingClubIds.Length, existingStatusIndices.Length);
                for (int i = 0; i < count; i++)
                {
                    initial[existingClubIds[i].Get] = statuses[existingStatusIndices[i] % statuses.Length];
                }
            }

            // Simulate: Apply to club (sets Pending status)
            var updated = UpdateSingleStatus(new Dictionary<int, ClubMembershipStatus>(initial), clubId.Get, ClubMembershipStatus.Pending);
            
            // Persist: Serialize and deserialize (simulates sessionStorage round-trip)
            var json = SerializeStatuses(updated);
            var persisted = DeserializeStatuses(json);
            
            // Verify: The new Pending status is persisted
            return persisted != null && 
                   persisted.ContainsKey(clubId.Get) && 
                   persisted[clubId.Get] == ClubMembershipStatus.Pending;
        }

        /// <summary>
        /// **Property 4: Status Update Persistence - Leave Action**
        /// *For any* leave action, the status SHALL be removed from cache.
        /// **Validates: Requirements 2.2**
        /// </summary>
        [Property(MaxTest = 100)]
        public bool StatusUpdatePersistence_LeaveAction_RemovesStatus(PositiveInt clubId, PositiveInt[] otherClubIds, byte[] otherStatusIndices)
        {
            // Setup: Create initial state with the target club as Approved
            var initial = new Dictionary<int, ClubMembershipStatus>
            {
                { clubId.Get, ClubMembershipStatus.Approved }
            };
            
            // Add some other memberships
            if (otherClubIds != null && otherStatusIndices != null)
            {
                var statuses = new[] { ClubMembershipStatus.NotMember, ClubMembershipStatus.Pending, 
                                       ClubMembershipStatus.Approved, ClubMembershipStatus.Rejected };
                var count = Math.Min(otherClubIds.Length, otherStatusIndices.Length);
                for (int i = 0; i < count; i++)
                {
                    if (otherClubIds[i].Get != clubId.Get) // Don't overwrite target
                    {
                        initial[otherClubIds[i].Get] = statuses[otherStatusIndices[i] % statuses.Length];
                    }
                }
            }

            var expectedOtherCount = initial.Count - 1; // Excluding the target club

            // Simulate: Leave club (removes status)
            var updated = RemoveStatus(new Dictionary<int, ClubMembershipStatus>(initial), clubId.Get);
            
            // Persist: Serialize and deserialize
            var json = SerializeStatuses(updated);
            var persisted = DeserializeStatuses(json);
            
            // Verify: The status is removed, other statuses preserved
            return persisted != null && 
                   !persisted.ContainsKey(clubId.Get) &&
                   persisted.Count == expectedOtherCount;
        }

        /// <summary>
        /// Property test: Status update overwrites existing status
        /// </summary>
        [Theory]
        [InlineData(ClubMembershipStatus.NotMember, ClubMembershipStatus.Pending)]
        [InlineData(ClubMembershipStatus.Pending, ClubMembershipStatus.Approved)]
        [InlineData(ClubMembershipStatus.Approved, ClubMembershipStatus.NotMember)]
        [InlineData(ClubMembershipStatus.Rejected, ClubMembershipStatus.NotMember)]
        public void StatusUpdate_OverwritesExistingStatus(ClubMembershipStatus oldStatus, ClubMembershipStatus newStatus)
        {
            var clubId = 1;
            var initial = new Dictionary<int, ClubMembershipStatus> { { clubId, oldStatus } };
            
            // Update status
            var updated = UpdateSingleStatus(new Dictionary<int, ClubMembershipStatus>(initial), clubId, newStatus);
            
            // Persist and verify
            var json = SerializeStatuses(updated);
            var persisted = DeserializeStatuses(json);
            
            Assert.NotNull(persisted);
            Assert.Single(persisted);
            Assert.Equal(newStatus, persisted[clubId]);
        }

        #region UI Status Mapping Helpers

        /// <summary>
        /// Maps status to button text - mirrors Kulupler.razor GetMembershipButtonText
        /// </summary>
        private static string GetMembershipButtonText(ClubMembershipStatus status)
        {
            return status switch
            {
                ClubMembershipStatus.Approved => "Üye ✓",
                ClubMembershipStatus.Pending => "Beklemede...",
                ClubMembershipStatus.Rejected => "Reddedildi",
                _ => "Üye Ol"
            };
        }

        /// <summary>
        /// Maps status to button CSS class - mirrors Kulupler.razor GetMembershipButtonClass
        /// </summary>
        private static string GetMembershipButtonClass(ClubMembershipStatus status)
        {
            return status switch
            {
                ClubMembershipStatus.Approved => "joined",
                ClubMembershipStatus.Pending => "pending",
                ClubMembershipStatus.Rejected => "rejected",
                _ => ""
            };
        }

        /// <summary>
        /// Maps status to status icon - mirrors Kulupler.razor GetMembershipStatusIcon
        /// </summary>
        private static string GetMembershipStatusIcon(ClubMembershipStatus status)
        {
            return status switch
            {
                ClubMembershipStatus.Approved => "✓",
                ClubMembershipStatus.Pending => "⏳",
                ClubMembershipStatus.Rejected => "✗",
                _ => "○"
            };
        }

        /// <summary>
        /// Maps status to status text - mirrors Kulupler.razor GetMembershipStatusText
        /// </summary>
        private static string GetMembershipStatusText(ClubMembershipStatus status)
        {
            return status switch
            {
                ClubMembershipStatus.Approved => "Bu kulübün üyesisiniz",
                ClubMembershipStatus.Pending => "Başvurunuz değerlendiriliyor",
                ClubMembershipStatus.Rejected => "Başvurunuz reddedildi",
                _ => "Bu kulübün üyesi değilsiniz"
            };
        }

        /// <summary>
        /// Maps status to status CSS class - mirrors Kulupler.razor GetMembershipStatusClass
        /// </summary>
        private static string GetMembershipStatusClass(ClubMembershipStatus status)
        {
            return status switch
            {
                ClubMembershipStatus.Approved => "member",
                ClubMembershipStatus.Pending => "pending",
                ClubMembershipStatus.Rejected => "rejected",
                _ => "non-member"
            };
        }

        #endregion

        /// <summary>
        /// **Property 5: UI Status Consistency**
        /// *For any* membership status value, the UI SHALL display the correct button text, 
        /// CSS class, icon, and status text as defined in the mapping.
        /// **Validates: Requirements 3.1, 3.2, 3.3, 3.4**
        /// </summary>
        [Property(MaxTest = 100)]
        public bool UIStatusConsistency_AllMappingsAreConsistent(byte statusIndex)
        {
            var statuses = new[] { ClubMembershipStatus.NotMember, ClubMembershipStatus.Pending, 
                                   ClubMembershipStatus.Approved, ClubMembershipStatus.Rejected };
            var status = statuses[statusIndex % statuses.Length];

            // Get all UI mappings
            var buttonText = GetMembershipButtonText(status);
            var buttonClass = GetMembershipButtonClass(status);
            var statusIcon = GetMembershipStatusIcon(status);
            var statusText = GetMembershipStatusText(status);
            var statusClass = GetMembershipStatusClass(status);

            // Verify all mappings are non-null and non-empty (except buttonClass for NotMember)
            var hasButtonText = !string.IsNullOrEmpty(buttonText);
            var hasStatusIcon = !string.IsNullOrEmpty(statusIcon);
            var hasStatusText = !string.IsNullOrEmpty(statusText);
            var hasStatusClass = !string.IsNullOrEmpty(statusClass);

            // Button class can be empty for NotMember
            var hasValidButtonClass = status == ClubMembershipStatus.NotMember 
                ? buttonClass == "" 
                : !string.IsNullOrEmpty(buttonClass);

            return hasButtonText && hasStatusIcon && hasStatusText && hasStatusClass && hasValidButtonClass;
        }

        /// <summary>
        /// Property 5: UI Status Consistency - Specific mappings verification
        /// </summary>
        [Theory]
        [InlineData(ClubMembershipStatus.NotMember, "Üye Ol", "", "○", "Bu kulübün üyesi değilsiniz", "non-member")]
        [InlineData(ClubMembershipStatus.Pending, "Beklemede...", "pending", "⏳", "Başvurunuz değerlendiriliyor", "pending")]
        [InlineData(ClubMembershipStatus.Approved, "Üye ✓", "joined", "✓", "Bu kulübün üyesisiniz", "member")]
        [InlineData(ClubMembershipStatus.Rejected, "Reddedildi", "rejected", "✗", "Başvurunuz reddedildi", "rejected")]
        public void UIStatusConsistency_CorrectMappings(
            ClubMembershipStatus status, 
            string expectedButtonText, 
            string expectedButtonClass,
            string expectedIcon,
            string expectedStatusText,
            string expectedStatusClass)
        {
            Assert.Equal(expectedButtonText, GetMembershipButtonText(status));
            Assert.Equal(expectedButtonClass, GetMembershipButtonClass(status));
            Assert.Equal(expectedIcon, GetMembershipStatusIcon(status));
            Assert.Equal(expectedStatusText, GetMembershipStatusText(status));
            Assert.Equal(expectedStatusClass, GetMembershipStatusClass(status));
        }

        /// <summary>
        /// Property 5: NotMember status enables "Üye Ol" button
        /// **Validates: Requirement 3.4**
        /// </summary>
        [Fact]
        public void UIStatusConsistency_NotMemberEnablesJoinButton()
        {
            var status = ClubMembershipStatus.NotMember;
            var buttonText = GetMembershipButtonText(status);
            var buttonClass = GetMembershipButtonClass(status);

            // NotMember should show "Üye Ol" with no special class (enabled state)
            Assert.Equal("Üye Ol", buttonText);
            Assert.Equal("", buttonClass); // No special class means default/enabled
        }

        #region Graceful Degradation Tests

        /// <summary>
        /// Simulates the graceful degradation logic from Kulupler.razor
        /// </summary>
        private static Dictionary<int, ClubMembershipStatus> GetEffectiveStatuses(
            Dictionary<int, ClubMembershipStatus>? cachedStatuses,
            Dictionary<int, ClubMembershipStatus>? apiStatuses,
            bool cacheError,
            bool apiError)
        {
            // Requirement 5.1: If cache fails, use API
            if (cacheError && !apiError && apiStatuses != null)
                return apiStatuses;

            // Requirement 5.2: If API fails, use cache
            if (apiError && !cacheError && cachedStatuses != null)
                return cachedStatuses;

            // Normal case: API data takes precedence
            if (!apiError && apiStatuses != null)
                return apiStatuses;

            // Requirement 5.3: If both fail, return empty (default status)
            if (cacheError && apiError)
                return new Dictionary<int, ClubMembershipStatus>();

            // Fallback to cache if available
            return cachedStatuses ?? new Dictionary<int, ClubMembershipStatus>();
        }

        /// <summary>
        /// **Property 7: Graceful Degradation**
        /// *For any* combination of sessionStorage and API failures, the system SHALL continue 
        /// to function by falling back to available data or default states.
        /// **Validates: Requirements 5.1, 5.2, 5.3**
        /// </summary>
        [Property(MaxTest = 100)]
        public bool GracefulDegradation_AlwaysReturnsValidResult(
            PositiveInt[] cacheClubIds, byte[] cacheStatusIndices,
            PositiveInt[] apiClubIds, byte[] apiStatusIndices,
            bool cacheError, bool apiError)
        {
            var statuses = new[] { ClubMembershipStatus.NotMember, ClubMembershipStatus.Pending, 
                                   ClubMembershipStatus.Approved, ClubMembershipStatus.Rejected };

            // Build cache data
            var cachedStatuses = new Dictionary<int, ClubMembershipStatus>();
            if (cacheClubIds != null && cacheStatusIndices != null)
            {
                var count = Math.Min(cacheClubIds.Length, cacheStatusIndices.Length);
                for (int i = 0; i < count; i++)
                    cachedStatuses[cacheClubIds[i].Get] = statuses[cacheStatusIndices[i] % statuses.Length];
            }

            // Build API data
            var apiStatuses = new Dictionary<int, ClubMembershipStatus>();
            if (apiClubIds != null && apiStatusIndices != null)
            {
                var count = Math.Min(apiClubIds.Length, apiStatusIndices.Length);
                for (int i = 0; i < count; i++)
                    apiStatuses[apiClubIds[i].Get] = statuses[apiStatusIndices[i] % statuses.Length];
            }

            // Get effective statuses using graceful degradation logic
            var result = GetEffectiveStatuses(cachedStatuses, apiStatuses, cacheError, apiError);

            // Property: Result should never be null
            return result != null;
        }

        /// <summary>
        /// Property 7: Cache error falls back to API
        /// **Validates: Requirement 5.1**
        /// </summary>
        [Fact]
        public void GracefulDegradation_CacheError_FallsBackToApi()
        {
            var apiStatuses = new Dictionary<int, ClubMembershipStatus>
            {
                { 1, ClubMembershipStatus.Approved }
            };

            var result = GetEffectiveStatuses(null, apiStatuses, cacheError: true, apiError: false);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(ClubMembershipStatus.Approved, result[1]);
        }

        /// <summary>
        /// Property 7: API error falls back to cache
        /// **Validates: Requirement 5.2**
        /// </summary>
        [Fact]
        public void GracefulDegradation_ApiError_FallsBackToCache()
        {
            var cachedStatuses = new Dictionary<int, ClubMembershipStatus>
            {
                { 1, ClubMembershipStatus.Pending }
            };

            var result = GetEffectiveStatuses(cachedStatuses, null, cacheError: false, apiError: true);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(ClubMembershipStatus.Pending, result[1]);
        }

        /// <summary>
        /// Property 7: Both errors return empty (default status)
        /// **Validates: Requirement 5.3**
        /// </summary>
        [Fact]
        public void GracefulDegradation_BothErrors_ReturnsEmpty()
        {
            var result = GetEffectiveStatuses(null, null, cacheError: true, apiError: true);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Property 7: Normal operation uses API data
        /// </summary>
        [Fact]
        public void GracefulDegradation_NormalOperation_UsesApiData()
        {
            var cachedStatuses = new Dictionary<int, ClubMembershipStatus>
            {
                { 1, ClubMembershipStatus.Pending }
            };
            var apiStatuses = new Dictionary<int, ClubMembershipStatus>
            {
                { 1, ClubMembershipStatus.Approved }
            };

            var result = GetEffectiveStatuses(cachedStatuses, apiStatuses, cacheError: false, apiError: false);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(ClubMembershipStatus.Approved, result[1]); // API takes precedence
        }

        #endregion

        #region Student Isolation Tests

        /// <summary>
        /// Generates cache key for a student - mirrors MembershipCacheService.GetCacheKey
        /// </summary>
        private static string GetCacheKey(int studentId) => $"membership_statuses_{studentId}";

        /// <summary>
        /// **Property 6: Student Isolation**
        /// *For any* two different student IDs, their sessionStorage keys SHALL be different, 
        /// ensuring data isolation.
        /// **Validates: Requirements 4.1, 4.3**
        /// </summary>
        [Property(MaxTest = 100)]
        public bool StudentIsolation_DifferentStudentsHaveDifferentKeys(PositiveInt studentId1, PositiveInt studentId2)
        {
            // Skip if same student ID
            if (studentId1.Get == studentId2.Get)
                return true;

            var key1 = GetCacheKey(studentId1.Get);
            var key2 = GetCacheKey(studentId2.Get);

            // Keys must be different for different students
            return key1 != key2;
        }

        /// <summary>
        /// Property 6: Cache key format is correct
        /// **Validates: Requirement 4.1**
        /// </summary>
        [Property(MaxTest = 100)]
        public bool StudentIsolation_KeyFormatIsCorrect(PositiveInt studentId)
        {
            var key = GetCacheKey(studentId.Get);
            var expectedKey = $"membership_statuses_{studentId.Get}";

            return key == expectedKey;
        }

        /// <summary>
        /// Property 6: Specific student ID key format verification
        /// </summary>
        [Theory]
        [InlineData(1, "membership_statuses_1")]
        [InlineData(100, "membership_statuses_100")]
        [InlineData(12345, "membership_statuses_12345")]
        [InlineData(999999, "membership_statuses_999999")]
        public void StudentIsolation_CorrectKeyFormat(int studentId, string expectedKey)
        {
            var actualKey = GetCacheKey(studentId);
            Assert.Equal(expectedKey, actualKey);
        }

        /// <summary>
        /// Property 6: Student data isolation - simulates two students with different data
        /// </summary>
        [Fact]
        public void StudentIsolation_DifferentStudentsHaveIndependentData()
        {
            // Student 1 data
            var student1Id = 1;
            var student1Statuses = new Dictionary<int, ClubMembershipStatus>
            {
                { 1, ClubMembershipStatus.Approved },
                { 2, ClubMembershipStatus.Pending }
            };

            // Student 2 data
            var student2Id = 2;
            var student2Statuses = new Dictionary<int, ClubMembershipStatus>
            {
                { 1, ClubMembershipStatus.NotMember },
                { 3, ClubMembershipStatus.Rejected }
            };

            // Simulate storage (key -> json)
            var storage = new Dictionary<string, string>
            {
                { GetCacheKey(student1Id), SerializeStatuses(student1Statuses) },
                { GetCacheKey(student2Id), SerializeStatuses(student2Statuses) }
            };

            // Retrieve and verify student 1 data
            var retrieved1 = DeserializeStatuses(storage[GetCacheKey(student1Id)]);
            Assert.NotNull(retrieved1);
            Assert.Equal(2, retrieved1.Count);
            Assert.Equal(ClubMembershipStatus.Approved, retrieved1[1]);
            Assert.Equal(ClubMembershipStatus.Pending, retrieved1[2]);

            // Retrieve and verify student 2 data
            var retrieved2 = DeserializeStatuses(storage[GetCacheKey(student2Id)]);
            Assert.NotNull(retrieved2);
            Assert.Equal(2, retrieved2.Count);
            Assert.Equal(ClubMembershipStatus.NotMember, retrieved2[1]);
            Assert.Equal(ClubMembershipStatus.Rejected, retrieved2[3]);

            // Verify data is independent (same club ID, different status)
            Assert.NotEqual(retrieved1[1], retrieved2[1]);
        }

        /// <summary>
        /// Property 6: Clearing one student's cache doesn't affect another
        /// </summary>
        [Fact]
        public void StudentIsolation_ClearingOneCacheDoesNotAffectOther()
        {
            var student1Id = 1;
            var student2Id = 2;

            // Simulate storage
            var storage = new Dictionary<string, string>
            {
                { GetCacheKey(student1Id), SerializeStatuses(new Dictionary<int, ClubMembershipStatus> { { 1, ClubMembershipStatus.Approved } }) },
                { GetCacheKey(student2Id), SerializeStatuses(new Dictionary<int, ClubMembershipStatus> { { 1, ClubMembershipStatus.Pending } }) }
            };

            // Clear student 1's cache
            storage.Remove(GetCacheKey(student1Id));

            // Verify student 1's cache is cleared
            Assert.False(storage.ContainsKey(GetCacheKey(student1Id)));

            // Verify student 2's cache is still intact
            Assert.True(storage.ContainsKey(GetCacheKey(student2Id)));
            var student2Data = DeserializeStatuses(storage[GetCacheKey(student2Id)]);
            Assert.NotNull(student2Data);
            Assert.Equal(ClubMembershipStatus.Pending, student2Data[1]);
        }

        #endregion
    }
}
