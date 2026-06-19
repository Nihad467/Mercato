# Mercato 🛒

Mercato, müasir arxitektura prinsipləri (.NET, Docker, Redis, MinIO) əsasında qurulmuş, ödəniş sistemləri və təhlükəsiz autentifikasiya mexanizmləri ilə təchiz olunmuş yüksək performanslı bir backend platformasıdır (E-ticarət / Marketplace API).

---

## 🚀 Texnoloji Stack & Alətlər

Layihənin əsasını təşkil edən texnologiyalar və istifadə məqsədləri:

### 💻 Core & Architecture
* **Platforma:** .NET (C#)
* **Dizayn Patternləri:** CQRS (MediatR ilə), Repository Pattern, Dependency Injection.
* **Data Mapping & Validation:** AutoMapper / Mapster, FluentValidation.

### 🔐 Autentifikasiya & Təhlükəsizlik
* **ASP.NET Core Identity:** İstifadəçi və rol idarəetməsi.
* **JWT (JSON Web Tokens):** Sürətli və təhlükəsiz sessiya idarəetməsi.
* **Refresh Token:** Redis üzərində qurulmuş fasiləsiz sessiya yeniləmə mexanizmi.

### 💳 Xarici Xidmətlər (External Services)
* **Ödəniş Sistemləri:** Stripe / Yerli Bank API inteqrasiyaları (Kartla ödəniş, uğurlu/uğursuz tranzaksiyaların idarə olunması).
* **Bildiriş Servisləri:** SMS provider (OTP kodlar üçün) və SendGrid/SMTP (E-poçt bildirişləri üçün).

### 📦 İnfrastruktur & Data
* **Verilənlər Bazası & ORM:** PostgreSQL / MS SQL Server, Entity Framework Core.
* **Keşləmə (Caching):** [Redis](https://redis.io/) (Data caching və paylanmış sessiya idarəetməsi).
* **Obyekt Depolama:** [MinIO](https://min.io/) (Məhsul şəkilləri və sənədlər üçün S3 uyğunluqlu bulud anbarı).
* **Konteynerləşdirmə:** [Docker](https://www.docker.com/) & Docker Compose.

---

## 🛠️ Layihə Strukturunun Arxitekturası (`./src`)

Layihə **Clean Architecture** prinsiplərinə uyğun olaraq aşağıdakı qatlardan ibarətdir:
* **Domain:** Entitilər, enumlar və əsas biznes qaydaları.
* **Application:** Biznes məntiqləri, CQRS Handler-ləri, DTO-lar və interfeyslər.
* **Infrastructure:** Redis, MinIO, Verilənlər bazası konfiqurasiyaları və Xarici xidmətlər (Identity, Payment, SMS).
* **API:** Controller-lər, Middleware-lər və endpoint sazlamaları.

---

## ⚙️ Qurulum və İşə Salma (Setup)

### Prereqvizitlər
* [.NET SDK](https://dotnet.microsoft.com/download)
* [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Addımlar

1. **Repozitoriyanı klonlayın:**
   ```bash
   git clone [https://github.com/Nihad467/Mercato.git](https://github.com/Nihad467/Mercato.git)
   cd Mercato
