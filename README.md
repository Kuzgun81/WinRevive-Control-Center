# WinRevive Control Center

Windows 11 için güvenli, geri alınabilir sistem yönetim paneli MVP'si.

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
