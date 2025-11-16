# Requirements Document

## Introduction

Bu özellik, Student ve Club kullanıcılarının sistemde kimlik doğrulaması yaparak giriş yapmalarını sağlar. Kullanıcılar, Student için StudentNumber ve StudentPassword, Club için ClubNumber ve ClubPassword bilgilerini kullanarak sisteme giriş yapabileceklerdir.

## Glossary

- **Authentication System**: Kullanıcı kimlik doğrulama ve giriş yönetim sistemi
- **Student**: Öğrenci kullanıcı tipi (Student tablosunda kayıtlı)
- **Club**: Kulüp kullanıcı tipi (Club tablosunda kayıtlı)
- **StudentNumber**: Öğrencinin benzersiz numarası (long tipinde)
- **StudentPassword**: Öğrencinin şifresi (maksimum 20 karakter)
- **ClubNumber**: Kulübün benzersiz numarası (long tipinde)
- **ClubPassword**: Kulübün şifresi (maksimum 20 karakter)
- **IsActive**: Kullanıcının aktif durumunu belirten boolean alan

## Requirements

### Requirement 1

**User Story:** Bir öğrenci olarak, StudentNumber ve StudentPassword bilgilerimi kullanarak sisteme giriş yapmak istiyorum, böylece öğrenci özelliklerine erişebilirim.

#### Acceptance Criteria

1. WHEN bir kullanıcı Student giriş formunu gönderdiğinde, THE Authentication System SHALL Student tablosunda StudentNumber ile eşleşen bir kayıt arar
2. IF StudentNumber Student tablosunda bulunursa, THEN THE Authentication System SHALL girilen StudentPassword ile veritabanındaki StudentPassword değerini karşılaştırır
3. WHEN StudentNumber ve StudentPassword eşleştiğinde VE IsActive değeri true olduğunda, THE Authentication System SHALL başarılı giriş yanıtı döner
4. IF StudentNumber bulunamazsa VEYA StudentPassword eşleşmezse VEYA IsActive değeri false ise, THEN THE Authentication System SHALL hata mesajı ile giriş reddeder

### Requirement 2

**User Story:** Bir kulüp yöneticisi olarak, ClubNumber ve ClubPassword bilgilerimi kullanarak sisteme giriş yapmak istiyorum, böylece kulüp yönetim özelliklerine erişebilirim.

#### Acceptance Criteria

1. WHEN bir kullanıcı Club giriş formunu gönderdiğinde, THE Authentication System SHALL Club tablosunda ClubNumber ile eşleşen bir kayıt arar
2. IF ClubNumber Club tablosunda bulunursa, THEN THE Authentication System SHALL girilen ClubPassword ile veritabanındaki ClubPassword değerini karşılaştırır
3. WHEN ClubNumber ve ClubPassword eşleştiğinde VE IsActive değeri true olduğunda, THE Authentication System SHALL başarılı giriş yanıtı döner
4. IF ClubNumber bulunamazsa VEYA ClubPassword eşleşmezse VEYA IsActive değeri false ise, THEN THE Authentication System SHALL hata mesajı ile giriş reddeder

### Requirement 3

**User Story:** Bir sistem kullanıcısı olarak, giriş yaparken hangi kullanıcı tipinde (Student veya Club) giriş yapmak istediğimi belirtmek istiyorum, böylece doğru kimlik doğrulama işlemi gerçekleşir.

#### Acceptance Criteria

1. THE Authentication System SHALL Student ve Club için ayrı giriş endpoint'leri sağlar
2. WHEN bir giriş isteği alındığında, THE Authentication System SHALL kullanıcı tipine göre ilgili tabloda kimlik doğrulama yapar
3. THE Authentication System SHALL her kullanıcı tipi için uygun yanıt formatı döner

### Requirement 4

**User Story:** Bir sistem yöneticisi olarak, başarısız giriş denemelerinin güvenli bir şekilde işlenmesini istiyorum, böylece sistem güvenliği sağlanır.

#### Acceptance Criteria

1. WHEN geçersiz kimlik bilgileri girildiğinde, THE Authentication System SHALL kullanıcıya genel bir hata mesajı gösterir
2. THE Authentication System SHALL şifre bilgilerini yanıt mesajlarında açığa çıkarmaz
3. WHEN bir giriş denemesi başarısız olduğunda, THE Authentication System SHALL hangi alanın (numara veya şifre) hatalı olduğunu belirtmez
4. THE Authentication System SHALL tüm şifre karşılaştırmalarını güvenli bir şekilde gerçekleştirir

### Requirement 5

**User Story:** Bir kullanıcı olarak, başarılı giriş yaptığımda kullanıcı bilgilerimi almak istiyorum, böylece uygulamada kişiselleştirilmiş deneyim yaşayabilirim.

#### Acceptance Criteria

1. WHEN bir Student başarılı giriş yaptığında, THE Authentication System SHALL StudentID, StudentName, StudentSurname ve StudentMail bilgilerini döner
2. WHEN bir Club başarılı giriş yaptığında, THE Authentication System SHALL ClubID ve ClubName bilgilerini döner
3. THE Authentication System SHALL yanıtta şifre bilgilerini içermez
4. THE Authentication System SHALL yanıtta IsActive durumunu içerir
