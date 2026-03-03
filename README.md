# IntelligentCharge

Windows-Systemtray-Anwendung zur Verwaltung von Akku-Ladeschwellenwerten. Verlängert die Akkulaufzeit, indem der Ladevorgang auf einen konfigurierbaren Bereich begrenzt wird (z. B. 60–80 % oder 75–80 %).

## Funktionen

- Systemtray-Symbol mit kontextsensitivem Status-Icon (aktiv / inaktiv)
- Zwei vorkonfigurierte Ladeschwellwert-Profile:
  - **75 %–80 %**: Minimaler Ladepuffer, hohe Lebensdauer
  - **60 %–80 %**: Erweiterter Puffer für längere Nutzungszeiten ohne Strom
- Ladebegrenzung deaktivieren (volle Ladung auf 100 %)
- Autostart beim Windows-Login (ohne Administratorrechte)
- Automatische Statusaktualisierung alle 30 Sekunden

## Projektstruktur

```
IntelligentCharge/
├── IntelligentCharge.sln                  # Visual Studio Solution
├── ChargeThreshold.exe                    # Externes CLI-Tool zur Schwellwertsteuerung
├── Toggle-AkkuPowerMode.ps1               # PowerShell-Wrapper für CLI-Nutzung
│
├── IntelligentChargeTray/                 # Hauptanwendung
│   ├── Program.cs                         # Einstiegspunkt
│   ├── TrayApplicationContext.cs          # Tray-Logik und Menüsteuerung
│   └── Services/
│       ├── IChargeThresholdService.cs     # Interface für Ladeschwellwert-Operationen
│       ├── ChargeThresholdService.cs      # Implementierung (wraps ChargeThreshold.exe)
│       ├── IAutostartService.cs           # Interface für Autostart-Verwaltung
│       └── AutostartService.cs            # Registry-basierte Autostart-Implementierung
│
└── IntelligentChargeTray.Tests/           # Testprojekt
    ├── ChargeThresholdServiceTests.cs     # Unit-Tests für Output-Parser
    ├── TrayApplicationContextTests.cs     # Unit-Tests für UI-Logik
    ├── AutostartServiceTests.cs           # Integrationstests (Windows Registry)
    └── CollectionDefinitions.cs           # xUnit STA-Threading-Konfiguration
```

## Voraussetzungen

- Windows 10/11
- [.NET 10.0 Runtime](https://dotnet.microsoft.com/download/dotnet/10.0) (Windows)
- `ChargeThreshold.exe` im selben Verzeichnis wie `IntelligentChargeTray.exe`

## Installation

1. Repository klonen:
   ```
   git clone https://github.com/brasselm/IntelligentCharge.git
   ```

2. Solution bauen:
   ```
   dotnet build IntelligentCharge.sln -c Release
   ```

3. `ChargeThreshold.exe` in das Ausgabeverzeichnis legen:
   ```
   IntelligentChargeTray/bin/Release/net10.0-windows/
   ```

4. `IntelligentChargeTray.exe` starten – das Anwendungssymbol erscheint im Systemtray.

## Nutzung

### Systemtray-Menü

Rechtsklick auf das Tray-Symbol öffnet das Kontextmenü:

| Menüpunkt | Beschreibung |
|---|---|
| **Status** | Zeigt aktuellen Ladestatus (nur lesbar) |
| **Aktivieren (75–80 %)** | Schwellwert: Start bei 75 %, Stop bei 80 % |
| **Aktivieren (60–80 %)** | Schwellwert: Start bei 60 %, Stop bei 80 % |
| **Deaktivieren** | Ladebegrenzung aufheben (Akku lädt auf 100 %) |
| **Autostart** | Autostart beim Windows-Login aktivieren/deaktivieren |
| **Beenden** | Anwendung schließen |

## Technologie-Stack

| Kategorie | Technologie |
|---|---|
| Laufzeitumgebung | .NET 10.0 (Windows) |
| UI-Framework | Windows Forms |
| Autostart | Windows Registry (HKCU) |
| Testframework | xUnit 2.9.3 |
| Mocking | NSubstitute 5.3.0 |
| Sprache | C# mit Nullable-Reference-Types |

## Tests ausführen

```
dotnet test IntelligentCharge.sln
```

Die Testsuite umfasst:
- **Unit-Tests**: Output-Parser für `ChargeThreshold.exe` und UI-Logik (mit Mocks)
- **Integrationstests**: Lesen und Schreiben in die Windows Registry (HKCU, isolierter Testschlüssel)

## Architektur

Die Anwendung folgt einer klaren Schichtentrennung:

```
TrayApplicationContext (UI-Schicht)
        │
        ├── IChargeThresholdService  →  ChargeThresholdService
        │       └── ruft ChargeThreshold.exe auf (via Process)
        │
        └── IAutostartService        →  AutostartService
                └── liest/schreibt Windows Registry (HKCU)
```

Alle Abhängigkeiten werden über Interfaces injiziert, wodurch die UI-Logik vollständig ohne externe Prozesse oder Registry-Zugriffe getestet werden kann.

## Symbole einrichten

Die Anwendung erwartet zwei optionale Ressourcen-Icons:

- `icon_on.ico` – Aktiver Zustand (z. B. grünes Batterie-Symbol)
- `icon_off.ico` – Inaktiver Zustand (z. B. graues Symbol)

Sind keine Icons vorhanden, verwendet die Anwendung Windows-Systemsymbole als Fallback. Weitere Details zur Icon-Einbindung: [`IntelligentChargeTray/Resources/README.md`](IntelligentChargeTray/Resources/README.md)

## Lizenz

Dieses Projekt ist privat. Alle Rechte vorbehalten.
