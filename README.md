# YM341 - Kampüs Etkinlik Yönetim Sistemi

## Proje Hakkında

KampusEtkinlik, üniversite kampüslerinde düzenlenen etkinlikleri, kulüp faaliyetlerini ve duyuruları yönetmek için geliştirilmiş bir web uygulamasıdır.

## Teknolojiler

- **Frontend**: Blazor Server (.NET 8)
- **Backend**: ASP.NET Core Web API (.NET 8)
- **Veritabanı**: SQL Server
- **Authentication**: JWT Bearer Token
- **AI Chatbot**: N8n Webhook Integration

## Proje Yapısı

```
├── ClupApi/              # Backend Web API
├── KampusEtkinlik/       # Frontend Blazor Server
├── ClupApi.Tests/        # Test projeleri
└── .kiro/specs/          # Feature specifications
```

## İlk Kurulum

### 1. Gereksinimler

- .NET 8 SDK
- SQL Server (LocalDB veya Express)
- Visual Studio 2022 veya VS Code
- Git

### 2. Projeyi Klonlayın

```bash
git clone https://github.com/your-username/YM341.git
cd YM341
```

### 3. Configuration Dosyalarını Oluşturun

⚠️ **ÖNEMLİ**: `appsettings.json` dosyaları güvenlik nedeniyle Git'e commit edilmemiştir.

```bash
cd ClupApi
copy appsettings.json.example appsettings.json
copy appsettings.Development.json.example appsettings.Development.json
```

Detaylı configuration talimatları için: [ClupApi/CONFIGURATION.md](ClupApi/CONFIGURATION.md)

#### N8n Chatbot Yapılandırması

N8n chatbot'u kullanmak için `ClupApi/appsettings.Development.json` dosyasında N8n webhook URL'sini yapılandırın:

```json
{
  "N8nSettings": {
    "WebhookUrl": "https://your-n8n-instance.com/webhook/your-webhook-id/chat",
    "TimeoutSeconds": 30,
    "RetryCount": 2,
    "ApiKey": ""
  }
}
```

**N8n Webhook Gereksinimleri:**
- Webhook HTTPS protokolü kullanmalıdır
- Request format: `{ "Message": "string", "UserId": "string", "SessionId": "string", "Timestamp": "datetime" }`
- Response format: `{ "Response": "string", "SessionId": "string", "Metadata": {} }`

### 4. Veritabanını Oluşturun

```bash
cd ClupApi
dotnet ef database update
```

### 5. Uygulamayı Çalıştırın

**Backend (ClupApi):**
```bash
cd ClupApi
dotnet run
```
API: https://localhost:7001

**Frontend (KampusEtkinlik):**
```bash
cd KampusEtkinlik
dotnet run
```
Web: https://localhost:7065

## Özellikler

- ✅ Kullanıcı kimlik doğrulama (JWT)
- ✅ Etkinlik yönetimi
- ✅ Kulüp yönetimi
- ✅ Duyuru sistemi
- ✅ Takvim görünümü
- ✅ AI Chatbot entegrasyonu (N8n)

### AI Chatbot Özellikleri

- 💬 Gerçek zamanlı sohbet arayüzü
- 🤖 N8n webhook tabanlı AI yanıtları
- 💾 Session storage ile mesaj geçmişi (50 mesaj limiti)
- 🔒 JWT authentication ile güvenli iletişim
- 📱 Responsive tasarım (mobil uyumlu)
- ⚡ Typing indicator ve loading states
- 🗑️ Geçmiş temizleme özelliği

## Geliştirme

### Test Çalıştırma

```bash
cd ClupApi.Tests
dotnet test
```

### Yeni Migration Oluşturma

```bash
cd ClupApi
dotnet ef migrations add MigrationName
dotnet ef database update
```

## Güvenlik

- **ASLA** `appsettings.json` dosyalarını Git'e commit etmeyin
- **ASLA** API key'leri, secret key'leri veya şifreleri kodda hardcode etmeyin
- **HER ZAMAN** güçlü şifreler kullanın
- **SADECE** HTTPS kullanın (production)

Detaylı güvenlik bilgileri: [ClupApi/CONFIGURATION.md](ClupApi/CONFIGURATION.md)

## Katkıda Bulunma

1. Fork yapın
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Değişikliklerinizi commit edin (`git commit -m 'Add amazing feature'`)
4. Branch'inizi push edin (`git push origin feature/amazing-feature`)
5. Pull Request açın

## Lisans

Bu proje eğitim amaçlı geliştirilmiştir.

## İletişim

Sorularınız için issue açabilirsiniz.