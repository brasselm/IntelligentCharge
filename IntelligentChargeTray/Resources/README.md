# Icons für IntelligentChargeTray

Hier werden zwei `.ico`-Dateien erwartet:

| Datei        | Bedeutung                              | Empfohlene Farbe |
|--------------|----------------------------------------|------------------|
| icon_on.ico  | Ladelimit ist **aktiv**                | Grün / Batterie  |
| icon_off.ico | Ladelimit ist **nicht aktiv** (normal) | Grau / Weiß      |

## Icon erstellen

- [RealWorld Icon Editor](https://rw-designer.com/) (kostenlos)
- [GIMP](https://www.gimp.org/) mit ICO-Plugin
- [IcoFX](https://icofx.ro/)
- Oder einfach eine SVG in ICO konvertieren (16x16 + 32x32 Größen empfohlen)

## Nach dem Hinzufügen

In `IntelligentChargeTray.csproj` die auskommentierten `<EmbeddedResource>`-Einträge
wieder aktivieren:

```xml
<ItemGroup>
  <EmbeddedResource Include="Resources\icon_on.ico" />
  <EmbeddedResource Include="Resources\icon_off.ico" />
</ItemGroup>
```

Ohne Icons verwendet die App automatisch Windows-Systemicons (`SystemIcons.Shield` /
`SystemIcons.Information`) als Fallback.
