# WinRevive Control Center

Windows 11 için güvenli ve geri alınabilir sistem yönetim paneli. Ayarlar
uygulanmadan önce snapshot alınır; desteklenmeyen veya başarısız işlemler
başarı gibi gösterilmez.

**Güncel sürüm:** `v0.4.0`

## Kurulum

### Önerilen yöntem: Setup

1. GitHub'da **Releases** sayfasını açın ve `v0.4.0` sürümünü seçin.
2. Bu sürümün **Assets** bölümünden `WinRevive-Setup.exe` dosyasını indirin.
3. Dosyaya sağ tıklayın, **Özellikler** bölümünde **Engellemeyi kaldır** görünüyorsa
   işaretleyip **Uygula**'ya basın.
4. `WinRevive-Setup.exe` dosyasını çalıştırın ve kurulum sihirbazında **Install**
   düğmesine basın.
5. Kurulum bitince Başlat menüsünden veya oluşturduğunuz masaüstü kısayolundan
   **WinRevive Control Center** uygulamasını açın.

Kurulum yönetici yetkisi istemez ve varsayılan olarak
`%LocalAppData%\Programs\WinRevive` klasörüne yapılır. Uygulama açılmazsa
`%LocalAppData%\WinRevive\logs\app.log` dosyasını kontrol edin.

### Actions artifact'i

Geliştirici derlemesi için **Actions > Windows Release** iş akışını çalıştırın.
Tamamlanınca `winrevive-publish` artifact'ini indirin, ZIP'i çıkartın ve içindeki
`WinRevive.ControlCenter.exe` dosyasını çalıştırın. GitHub sayfasındaki dosyaya
tıklamak uygulamayı tarayıcı içinde açmaz; önce indirmeniz gerekir.

## Neler var?

- Windows, donanım, disk, bellek ve güvenlik özeti
- Başlangıç uygulamaları ve Windows Search analizi
- Geri alınabilir sistem, güç, Explorer ve görünüm ayarları
- Duvar kâğıdı ve accent rengi kişiselleştirmesi
- Geçici dosya tarama ve güvenli temizlik
- RAM ve kullanıcı süreçleri teşhisi
- Yerel snapshot, işlem geçmişi ve loglama

## Gereksinimler

- Windows 11
- 64-bit x64 işlemci
- `v0.4.0` Release kurulumu için `WinRevive-Setup.exe`

Uygulama yalnızca gerçekten gerekli işlemlerde UAC ister. Windows güvenlik
mekanizmalarını atlatmaz. Geliştirme aşamasında olduğu için sistem ayarlarını
değiştirmeden önce açıklamaları okuyun ve önemli verilerinizi yedekleyin.

## Geliştirme

Proje .NET 8 WPF kullanır ve Windows üzerinde derlenmelidir:

```powershell
dotnet restore
dotnet build WinRevive.sln -c Release
dotnet test tests/WinRevive.Tests/WinRevive.Tests.csproj -c Release
```

Etiketli sürüm oluşturulduğunda `.github/workflows/windows-release.yml` dosyası
Windows üzerinde build, test, self-contained publish ve Inno Setup paketlemesini
çalıştırır.

## Katkıda bulunanlar

- [Kuzgun81](https://github.com/Kuzgun81)
- [GitHub Copilot](https://github.com/apps/github-copilot-cli)

## Lisans

`LICENSE` dosyasındaki özel lisans geçerlidir. Kaynak kodu ve release dosyaları
izin olmadan yeniden dağıtılamaz veya değiştirilemez.
