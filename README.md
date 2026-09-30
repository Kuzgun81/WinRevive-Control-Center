# WinRevive Control Center

Windows 11 için güvenli, geri alınabilir sistem yönetim paneli.

> Geliştirme aşamasındadır. Windows 11 üzerinde manuel test edilmeden üretim cihazlarında
> sistem değişikliği uygulanmamalıdır.

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
- WPF/MVVM ana paneli
- Koyu ve açık tema seçimi
- Yerel JSON ayar ve JSONL işlem geçmişi
- Yapılandırılmış yerel log
- HKCU `EnableTransparency` için Detect, Backup, Apply, Verify ve Revert
- xUnit test altyapısı
- Inno Setup başlangıç betiği

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
