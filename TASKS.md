# Etkinlik Takvimi Projesi - Görev Listesi

## 📋 Proje Özeti
Kulüp etkinliklerini, akademik olayları ve duyuruları gösteren interaktif bir takvim sayfası geliştirme.

---

## 🔧 Backend Görevleri (API)

### 1. DTO Oluşturma
**Dosya:** `ClupApi/DTOs/CalendarEventDto.cs`

```csharp
public class CalendarEventDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Category { get; set; }  // "AkademikOlay", "KulupEtkinligi", "Duyuru"
    public string CategoryColor { get; set; }
    public bool IsAllDay { get; set; }
    public string? Location { get; set; }
}

public class CategoryDto
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Color { get; set; }
}
```

**Tahmini Süre:** 15 dakika

---

### 2. Repository Katmanı
**Dosya:** `ClupApi/Repositories/ICalendarRepository.cs` ve `CalendarRepository.cs`

**Interface:**
```csharp
public interface ICalendarRepository
{
    Task<List<CalendarEventDto>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<List<CalendarEventDto>> GetEventsByDateAsync(DateTime date);
    Task<List<CategoryDto>> GetCategoriesAsync();
}
```

**Yapılacaklar:**
- Activities, Announcements ve diğer ilgili tablolardan veri çek
- Tarih aralığına göre filtrele
- DTO'ya map et
- Kategorilere göre renk ata

**Tahmini Süre:** 1-2 saat

---

### 3. Service Katmanı (Opsiyonel)
**Dosya:** `ClupApi/Services/ICalendarService.cs` ve `CalendarService.cs`

**Yapılacaklar:**
- Business logic ekle (varsa)
- Repository'yi çağır
- Ek validasyon/filtreleme

**Tahmini Süre:** 30 dakika - 1 saat

---

### 4. Controller Oluşturma
**Dosya:** `ClupApi/Controllers/CalendarController.cs`

**Endpoint'ler:**

```csharp
[ApiController]
[Route("api/[controller]")]
public class CalendarController : ControllerBase
{
    // GET: api/calendar/events?startDate=2024-08-01&endDate=2024-08-31
    [HttpGet("events")]
    public async Task<ActionResult<List<CalendarEventDto>>> GetEvents(
        [FromQuery] DateTime startDate, 
        [FromQuery] DateTime endDate)
    
    // GET: api/calendar/events/daily?date=2024-08-19
    [HttpGet("events/daily")]
    public async Task<ActionResult<List<CalendarEventDto>>> GetDailyEvents(
        [FromQuery] DateTime date)
    
    // GET: api/calendar/categories
    [HttpGet("categories")]
    public async Task<ActionResult<List<CategoryDto>>> GetCategories()
}
```

**Validasyon:**
- startDate < endDate kontrolü
- Maksimum 3 aylık aralık sınırı (performans için)
- Tarih formatı kontrolü

**Tahmini Süre:** 1 saat

---

### 5. Mapping Yapılandırması
**Dosya:** `ClupApi/Mappings/CalendarMappingProfile.cs` (AutoMapper kullanıyorsanız)

**Yapılacaklar:**
- Activity -> CalendarEventDto
- Announcement -> CalendarEventDto
- Diğer event tipleri

**Tahmini Süre:** 30 dakika

---

### 6. Test Verisi Ekleme
**Yapılacaklar:**
- Farklı kategorilerde 20-30 test etkinliği ekle
- Farklı tarihlere yay (geçmiş, bugün, gelecek)
- Hem tam gün hem saatli etkinlikler ekle

**Tahmini Süre:** 30 dakika

---

### 7. API Testleri
**Araçlar:** Postman, Swagger, veya ClupApi.http

**Test Senaryoları:**
- ✅ Belirli tarih aralığında etkinlikleri getir
- ✅ Tek günün etkinliklerini getir
- ✅ Kategorileri getir
- ✅ Geçersiz tarih aralığı (hata dönmeli)
- ✅ Boş sonuç (etkinlik olmayan tarih)
- ✅ CORS ayarları (frontend farklı port'taysa)

**Tahmini Süre:** 1 saat

---

### 8. Dokümantasyon
**Yapılacaklar:**
- Swagger açıklamaları ekle
- README'ye endpoint bilgileri yaz
- Örnek request/response ekle

**Tahmini Süre:** 30 dakika

---

## 🎨 Frontend Görevleri (Blazor)

### 1. API Service Oluşturma
**Dosya:** `KampusEtkinlik/Services/CalendarApiService.cs`

**Yapılacaklar:**
- HttpClient ile API'yi çağır
- GetEventsAsync(startDate, endDate)
- GetDailyEventsAsync(date)
- GetCategoriesAsync()
- Error handling ekle

**Tahmini Süre:** 1 saat

---

### 2. Model/DTO Oluşturma
**Dosya:** `KampusEtkinlik/Models/CalendarEvent.cs`

**Yapılacaklar:**
- Backend DTO'larıyla aynı yapıda model oluştur
- JSON deserializasyon için attribute'lar ekle

**Tahmini Süre:** 15 dakika

---

### 3. Takvim Komponenti (Ana Sayfa)
**Dosya:** `KampusEtkinlik/Components/Pages/Calendar.razor`

**Yapılacaklar:**
- Ay/yıl seçici (önceki/sonraki butonlar)
- 7x6 grid layout (Pazartesi-Pazar)
- Her hücrede gün numarası
- Her hücrede o günün etkinlikleri (max 2-3 tane göster)
- "+X daha" göstergesi
- Güne tıklayınca modal aç

**State:**
- currentMonth, currentYear
- events (List<CalendarEvent>)
- selectedDate
- isLoading

**Tahmini Süre:** 3-4 saat

---

### 4. Günlük Detay Modal
**Dosya:** `KampusEtkinlik/Components/Shared/DayDetailModal.razor`

**Yapılacaklar:**
- Modal overlay/backdrop
- Tarih başlığı (örn: "19 Ağustos 2024, Pazartesi")
- Kategorilere göre grupla (Akademik Olaylar, Kulüp Etkinlikleri, Duyurular)
- Her etkinlik için:
  - Başlık
  - Saat (tam gün değilse)
  - Lokasyon
  - Açıklama
- Kapat butonu

**Tahmini Süre:** 2-3 saat

---

### 5. Etkinlik Kartı Komponenti
**Dosya:** `KampusEtkinlik/Components/Shared/EventCard.razor`

**Yapılacaklar:**
- Küçük etkinlik kartı (takvim hücrelerinde)
- Kategori rengi göstergesi
- Başlık (truncate)
- Saat (varsa)

**Tahmini Süre:** 1 saat

---

### 6. Stil/CSS
**Dosya:** `KampusEtkinlik/wwwroot/css/calendar.css`

**Yapılacaklar:**
- Grid layout
- Responsive tasarım (mobil uyumlu)
- Kategori renkleri
- Hover efektleri
- Modal animasyonları
- Loading spinner

**Tahmini Süre:** 2-3 saat

---

### 7. State Management
**Yapılacaklar:**
- Ay değişince API'yi çağır
- Etkinlikleri cache'le (aynı ay için tekrar çağırma)
- Loading state'i göster
- Error handling (toast/alert)

**Tahmini Süre:** 1-2 saat

---

### 8. Responsive Tasarım
**Yapılacaklar:**
- Mobil: Liste görünümü veya daha küçük grid
- Tablet: 7 günlük grid
- Desktop: Tam takvim görünümü

**Tahmini Süre:** 2 saat

---

### 9. Test & Debug
**Test Senaryoları:**
- ✅ Ay değiştirme
- ✅ Güne tıklama ve modal açma
- ✅ Boş günler (etkinlik yok)
- ✅ Çok etkinlikli günler
- ✅ Tam gün etkinlikleri
- ✅ Saatli etkinlikler
- ✅ Farklı kategoriler
- ✅ API hatası durumu
- ✅ Yavaş internet (loading)

**Tahmini Süre:** 2 saat

---

## 🤝 Entegrasyon Görevleri (Birlikte)

### 1. API URL Yapılandırması
**Dosya:** `KampusEtkinlik/appsettings.json`

```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:7001"
  }
}
```

**Tahmini Süre:** 15 dakika

---

### 2. CORS Ayarları
**Dosya:** `ClupApi/Program.cs`

**Yapılacaklar:**
- Frontend URL'ini CORS'a ekle
- Development/Production ayarları

**Tahmini Süre:** 15 dakika

---

### 3. End-to-End Test
**Yapılacaklar:**
- Backend'i çalıştır
- Frontend'i çalıştır
- Tüm senaryoları test et
- Bug'ları düzelt

**Tahmini Süre:** 1-2 saat

---

## 📊 Toplam Tahmini Süreler

**Backend:** 6-8 saat
**Frontend:** 14-18 saat
**Entegrasyon:** 2-3 saat

**TOPLAM:** 22-29 saat (yaklaşık 3-4 gün)

---

## 🎯 Öncelik Sırası

### Sprint 1 - Temel Yapı (Gün 1)
1. ✅ Backend: DTO ve Repository oluştur
2. ✅ Backend: Controller ve endpoint'leri yaz
3. ✅ Backend: Test verisi ekle
4. ✅ Backend: API testleri tamamlandı
5. ⏳ Frontend: API Service oluştur
6. ⏳ Frontend: Mock data ile basit takvim grid'i yap

### Sprint 2 - Entegrasyon (Gün 2)
1. ✅ CORS ayarları*
2. ✅ Frontend: API entegrasyonu*
3. ✅ Frontend: Ay değiştirme fonksiyonu**
4. ✅ Test: Veri akışını kontrol et

### Sprint 3 - Detaylar (Gün 3)
1. ✅ Frontend: Modal komponenti**
2. ✅ Frontend: Etkinlik kartları--
3. ✅ Frontend: Stil ve animasyonlar
4. ✅ Test: Tüm senaryolar

### Sprint 4 - Polish (Gün 4)
1. ✅ Responsive tasarım
2. ✅ Error handling
3. ✅ Loading states
4. ✅ Final testler ve bug fix

---

## 📝 Notlar

### Backend Notları
- Timezone: Türkiye saati (UTC+3) kullan
- Tarih formatı: ISO 8601 (`2024-08-19T14:00:00`)
- Performans: Maksimum 3 aylık veri çekme sınırı koy
- Kategori renkleri: Enum veya sabit değerler kullan

### Frontend Notları
- Kütüphane seçimi: Radzen Blazor (önerilen) veya sıfırdan
- Tarih işlemleri için: DateTime.AddMonths(), DateTime.DaysInMonth()
- Cache stratejisi: Dictionary<string, List<CalendarEvent>> (key: "2024-08")
- Mobil öncelikli tasarım yap

### Ortak Notlar
- Git branch stratejisi: `feature/calendar-backend` ve `feature/calendar-frontend`
- API contract'ı değişirse ikisine de haber ver
- Düzenli commit at (her görev sonrası)
- Pull request'lerde birbirinizin kodunu review edin

---

## 🐛 Olası Sorunlar ve Çözümleri

### Problem: CORS hatası
**Çözüm:** Program.cs'de frontend URL'ini ekle

### Problem: Tarih formatı uyuşmazlığı
**Çözüm:** ISO 8601 kullan, timezone'u netleştir

### Problem: Çok fazla etkinlik yavaşlatıyor
**Çözüm:** Pagination ekle veya tarih aralığını sınırla

### Problem: Modal açılmıyor
**Çözüm:** JavaScript interop gerekebilir (Blazor Server ise)

---

## ✅ Tamamlanma Kriterleri

Proje tamamlandı sayılır:
- [ ] Takvim grid'i doğru şekilde gösteriliyor
- [ ] Ay değiştirme çalışıyor
- [ ] Etkinlikler API'den geliyor
- [ ] Güne tıklayınca modal açılıyor
- [ ] Modal'da tüm etkinlikler kategorilere göre gruplu
- [ ] Responsive tasarım çalışıyor
- [ ] Loading ve error state'leri var
- [ ] Kod temiz ve dokümante edilmiş
- [ ] Testler geçiyor

---

**Son Güncelleme:** 29 Kasım 2024
**Proje Durumu:** Planlama Aşaması
