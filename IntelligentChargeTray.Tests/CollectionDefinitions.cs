namespace IntelligentChargeTray.Tests;

/// <summary>
/// Definiert die "STA"-Testkollektion.
/// Tests in dieser Kollektion laufen sequenziell im selben Thread –
/// notwendig für WinForms-Komponenten (NotifyIcon, ContextMenuStrip),
/// die einen STA-Thread voraussetzen.
/// </summary>
[CollectionDefinition("STA")]
public class StaCollectionDefinition { }
