using Microsoft.JSInterop;
using System.Text.Json;

namespace KampusEtkinlik.Services
{
    /// <summary>
    /// SessionStorage üzerinden üyelik durumlarını cache'leyen servis.
    /// IJSRuntime kullanarak JavaScript interop ile sessionStorage'a erişir.
    /// </summary>
    public class MembershipCacheService
    {
        private readonly IJSRuntime _jsRuntime;
        private const string CacheKeyPrefix = "membership_statuses_";

        public MembershipCacheService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        /// <summary>
        /// Belirli bir öğrenci için cache key'ini oluşturur.
        /// Format: membership_statuses_{studentId}
        /// </summary>
        private static string GetCacheKey(int studentId) => $"{CacheKeyPrefix}{studentId}";

        /// <summary>
        /// SessionStorage'dan öğrencinin üyelik durumlarını okur.
        /// </summary>
        /// <param name="studentId">Öğrenci ID'si</param>
        /// <returns>ClubID -> ClubMembershipStatus dictionary veya null</returns>
        public async Task<Dictionary<int, ClubMembershipStatus>?> GetCachedStatusesAsync(int studentId)
        {
            try
            {
                var key = GetCacheKey(studentId);
                var json = await _jsRuntime.InvokeAsync<string?>("membershipCache.get", key);
                
                if (string.IsNullOrEmpty(json))
                    return null;

                // JSON'dan Dictionary<string, string> olarak deserialize et
                var stringDict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (stringDict == null)
                    return null;

                // String dictionary'yi int -> ClubMembershipStatus dictionary'ye dönüştür
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
            catch (Exception ex)
            {
                Console.WriteLine($"SessionStorage read error: {ex.Message}");
                return null;
            }
        }


        /// <summary>
        /// Öğrencinin tüm üyelik durumlarını sessionStorage'a yazar.
        /// </summary>
        /// <param name="studentId">Öğrenci ID'si</param>
        /// <param name="statuses">ClubID -> ClubMembershipStatus dictionary</param>
        public async Task SetCachedStatusesAsync(int studentId, Dictionary<int, ClubMembershipStatus> statuses)
        {
            try
            {
                var key = GetCacheKey(studentId);
                
                // Dictionary<int, ClubMembershipStatus>'u Dictionary<string, string>'e dönüştür
                var stringDict = statuses.ToDictionary(
                    kvp => kvp.Key.ToString(),
                    kvp => kvp.Value.ToString()
                );
                
                var json = JsonSerializer.Serialize(stringDict);
                await _jsRuntime.InvokeVoidAsync("membershipCache.set", key, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SessionStorage write error: {ex.Message}");
            }
        }

        /// <summary>
        /// Tek bir kulübün üyelik durumunu günceller.
        /// Mevcut cache'i okur, günceller ve geri yazar.
        /// </summary>
        /// <param name="studentId">Öğrenci ID'si</param>
        /// <param name="clubId">Kulüp ID'si</param>
        /// <param name="status">Yeni üyelik durumu</param>
        public async Task UpdateSingleStatusAsync(int studentId, int clubId, ClubMembershipStatus status)
        {
            try
            {
                var statuses = await GetCachedStatusesAsync(studentId) ?? new Dictionary<int, ClubMembershipStatus>();
                statuses[clubId] = status;
                await SetCachedStatusesAsync(studentId, statuses);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SessionStorage update error: {ex.Message}");
            }
        }

        /// <summary>
        /// Belirli bir kulübün üyelik durumunu cache'den kaldırır.
        /// </summary>
        /// <param name="studentId">Öğrenci ID'si</param>
        /// <param name="clubId">Kulüp ID'si</param>
        public async Task RemoveStatusAsync(int studentId, int clubId)
        {
            try
            {
                var statuses = await GetCachedStatusesAsync(studentId);
                if (statuses != null && statuses.ContainsKey(clubId))
                {
                    statuses.Remove(clubId);
                    await SetCachedStatusesAsync(studentId, statuses);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SessionStorage remove error: {ex.Message}");
            }
        }

        /// <summary>
        /// Öğrencinin tüm üyelik cache'ini temizler.
        /// Logout sırasında çağrılmalıdır.
        /// </summary>
        /// <param name="studentId">Öğrenci ID'si</param>
        public async Task ClearCacheAsync(int studentId)
        {
            try
            {
                var key = GetCacheKey(studentId);
                await _jsRuntime.InvokeVoidAsync("membershipCache.remove", key);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SessionStorage clear error: {ex.Message}");
            }
        }

        /// <summary>
        /// Serialization için kullanılan statik yardımcı metod.
        /// Dictionary'yi JSON string'e dönüştürür.
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
        /// Deserialization için kullanılan statik yardımcı metod.
        /// JSON string'i Dictionary'ye dönüştürür.
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
    }
}
