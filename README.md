# WinRevive Control Center

Windows 11 için güvenli, geri alınabilir sistem yönetim paneli.

> Geliştirme aşamasındadır. Windows 11 üzerinde manuel test edilmeden üretim cihazlarında
> sistem değişikliği uygulanmamalıdır.

> **Özel yazılım:** Bu repository ve GitHub Release dosyaları private kullanım içindir.
> Tüm hakları saklıdır. Kaynak kodu veya `.exe` dosyaları başkalarıyla paylaşılamaz,
> yeniden dağıtılamaz veya değiştirilemez.

## Hedef

WinRevive; sistem analizi, güvenli optimizasyon, başlangıç ve Search yönetimi,
uygulama temizliği, güç profilleri, RAM analizi, görünüm kişiselleştirmesi ve
kalıcı geri alma geçmişini tek bir WPF panelinde birleştirmeyi hedefler.

Temel ilkeler:

- Her değişiklikten önce mevcut değer yedeklenir.
- Apply sonrası gerçek sistem değeri yeniden okunarak Verify yapılır.
- Başarısız işlemler başarı gibi gösterilmez.
- Yönetici yetkisi yalnızca gerçekten gerektiğinde istenir.
- Windows güvenlik mekanizmaları atlatılmaz.
- Ölçülmemiş performans kazancı iddia edilmez.

## Mevcut MVP

- Windows, işlemci, bellek ve sistem diski bilgisi
- Windows build, mimari, mantıksal işlemci ve capability detection
- Başlangıç kayıtları ve Windows Search yapılandırmasının salt-okunur analizi
- WPF/MVVM ana paneli
- Koyu ve açık tema seçimi
- Yerel JSON ayar ve JSONL işlem geçmişi
- Yapılandırılmış yerel log
- HKCU `EnableTransparency` için Detect, Backup, Apply, Verify ve Revert
- Linux-benzeri minimal profil: koyu mod, azaltılmış şeffaflık ve sol görev çubuğu hizası
- Minimal profil için JSON snapshot ve geri alma
- Ortak adlandırılmış snapshot altyapısı; kural yedekleri `LocalAppData\WinRevive\snapshots` altında tutulur
- Windows WMI üzerinden GPU, pil ve pagefile teşhisi; erişilemeyen bilgiler açıkça bilinmiyor olarak raporlanır
- Salt-okunur kurulu uygulama envanteri ve Defender, Secure Boot, TPM, BitLocker güvenlik özeti
- Snapshot'lı accent renk kişiselleştirmesi
- Search onarımı için ayrı, allowlist tabanlı elevated host sözleşmesi ve UAC akışı
- Dosya seçicili, snapshot'lı ve geri alınabilir duvar kâğıdı kişiselleştirmesi
- Explorer uzantı, gizli dosya, kompakt görünüm ve açılış konumu ayarları
- Tarama ve silmeyi ayıran güvenli geçici dosya temizleme merkezi
- xUnit test altyapısı
- Inno Setup başlangıç betiği

## Windows kurulumu

### GitHub Actions ile test build'i

1. Repository'nin **Actions** sekmesinden `Windows Release` workflow'unu çalıştırın.
2. Workflow tamamlandığında `winrevive-publish` artifact'ini indirin.
3. İçindeki `WinRevive.ControlCenter.exe` dosyasını kendi Windows 11 cihazınızda çalıştırın.

### Setup.exe release'i

Etiketlenmiş bir sürüm oluşturulduğunda workflow:

1. Windows runner üzerinde restore, build ve test çalıştırır.
2. Self-contained `win-x64` publish üretir.
3. Inno Setup ile `WinRevive-Setup.exe` oluşturur.
4. Publish klasörünü ve Setup.exe'yi GitHub Release'e ekler.

Kurulum paketi modern Inno Setup sihirbazı kullanır, varsayılan olarak kullanıcı profilindeki
`%LocalAppData%\Programs\WinRevive` konumuna kurulur ve yönetici yetkisi istemez.
Masaüstü kısayolu isteğe bağlıdır. Kurulum için önerilen yol: private GitHub Release içinden Setup.exe'yi indirip çalıştırın, hedef klasörü onaylayın,
kurulum tamamlandıktan sonra Başlat menüsünden WinRevive Control Center'ı açın.
Uygulama ilk açılışta yönetici yetkisi istemez; yalnızca yetki gerektiren özellikler
gelecekte ayrı ve açıklamalı bir UAC akışıyla eklenecektir.

> Bu Linux çalışma ortamında sahte bir `.exe` üretilmez. Resmi Windows artifact'i
> yalnızca Windows runner üzerinde derlenir ve testlerden sonra yayınlanır.

### Kısa kurulum tutorial'ı

1. GitHub hesabınızla private repository'nin **Releases** sayfasını açın.
2. Kullanmak istediğiniz sürümün altındaki `WinRevive-Setup.exe` dosyasını indirin.
3. İndirdiğiniz dosyaya sağ tıklayıp **Özellikler** bölümünü açın. Windows dosyayı engellediyse **Engellemeyi kaldır** seçeneğini işaretleyip **Uygula** düğmesine basın.
4. `WinRevive-Setup.exe` dosyasını çalıştırın ve kurulum sihirbazında lisans/kurulum konumunu onaylayın.
5. İsterseniz masaüstü kısayolunu seçin, ardından **Install** düğmesine basın.
6. Kurulum tamamlandığında **Finish** düğmesine basın ve uygulamayı Başlat menüsündeki **WinRevive Control Center** kısayolundan açın.
7. İlk açılışta sistem bilgilerini kontrol edin. Bir ayarı uygulamadan önce ilgili açıklamayı okuyun; geri alınabilir ayarlarda önce snapshot oluşturulduğunu doğrulayın.
8. Yönetici yetkisi gerektiren Search onarımı gibi işlemlerde Windows'un UAC penceresi gösterilir. İşlemi yalnızca bilinçli olarak başlatmışsanız onaylayın.

#### Güncelleme ve kaldırma

- Güncellemek için yeni sürümün `WinRevive-Setup.exe` dosyasını indirip mevcut kurulumun üzerine çalıştırın.
- Mevcut kullanıcı ayarlarını korumak için kurulum sırasında varsayılan seçenekleri kullanın.
- Kaldırmak için **Ayarlar > Uygulamalar > Yüklü uygulamalar > WinRevive Control Center > Kaldır** yolunu veya Başlat menüsündeki kaldırma kısayolunu kullanın.
- Kaldırmadan önce korumak istediğiniz snapshot ve log dosyalarını `%LocalAppData%\WinRevive` klasöründen güvenli bir konuma kopyalayın.

## Lisans

Bu proje MIT lisansı altında değildir. `LICENSE` dosyasındaki özel lisans geçerlidir;
tüm hakları saklıdır ve kullanım yalnızca copyright sahibine aittir.

## Yol haritası

1. Sistem snapshot ve işlem geçmişini genişletme
2. Windows build/capability detection
3. Başlangıç uygulamaları ve Search analiz modülleri
4. Güvenli temizlik ve güç profilleri
5. Tema/personalization merkezi
6. Yetkili işlemler için sınırlandırılmış elevated helper
7. Installer upgrade/repair/uninstall ve release pipeline

## Geliştirme

Bu proje .NET 8 WPF hedefler ve Windows üzerinde derlenmelidir:

```powershell
dotnet restore
dotnet build -c Release
dotnet test -c Release
```

İlk MVP; gerçek sistem bilgilerini, yerel JSON ayarlarını, yapılandırılmış loglamayı
ve HKCU `EnableTransparency` değerini yedekleyerek uygulayan/doğrulayan/geri alan
örnek kuralı içerir. Yönetici yetkisi gerektirmeyen bu ayar, Windows dışı sistemlerde
uygulanamaz olarak raporlanır.
