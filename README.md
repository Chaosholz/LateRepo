# 🧩 LateJoin Mod for R.E.P.O. v0.3.2

**LateRepo** is a LateJoin mod for **R.E.P.O.**  
It allows players to join an active lobby **during the shop phase and after exiting the shop.**,  
without requiring the host to restart the entire session.

---

## 🎮 Detailed Explanation

After leaving the shop or while in the shop, the game will automatically reopen the lobby. 
Players who join later can connect during this phase, before the next level starts.

The mod is **server-side** and only required by the **host**.  
If issues occur — for example, the game becomes **desynced** —  
the joining player should also install the mod to ensure proper synchronization.

This mod should work with other mods, such as:  
- [MorePlayersImproved](https://thunderstore.io/c/repo/p/Spindles/MorePlayersImproved)

---

## 🧭 Planned Features

- LateJoin **during an active level**
- Better password skip
- All features will be configurable through the config file

---

## ⚙️ Installation

### 🔧 Via Thunderstore

1. Create or open a **mod profile** where you want to install the mod.  
2. Install it through your browser or search for **LateRepo** in the Thunderstore mod list.  
👉 [Download on Thunderstore](https://thunderstore.io/c/repo/p/Chaosholz/LateRepo)

---

### 📦 Manual Installation (BepInEx)

- **Requires BepInEx 5.x**  
  👉 [Download BepInEx from GitHub](https://github.com/BepInEx/BepInEx/releases)

#### Step 1 – Set up BepInEx

1. Download **BepInEx 5.x**.  
2. Extract the contents directly into your **R.E.P.O.** directory:  
```
REPO/
├── REPO_Data/
├── BepInEx/
├── doorstop_config.ini
├── REPO.exe
├── UnityPlayer.dll
└── winhttp.dll
```

3. Launch the game once — this will automatically create the following folders:
- `BepInEx/plugins/`
- `BepInEx/config/`

#### Step 2 – Add the Mod

1. Copy `LateRepo.dll` into:  
BepInEx/plugins/

2. Launch the game again.

---

## 💬 Notes

This mod is **unofficial** and **not supported by the original game developers**.  
Use at your own risk.  

If you experience issues:
- Make sure all players are using the **same mod version**  
- If other mods are installed, test **LateRepo** on its own to rule out conflicts

---

---

# 🧩 LateJoin mod für R.E.P.O. v0.3.2

**LateRepo** ist eine LateJoin-Mod für **R.E.P.O.**,  
die es Spielern erlaubt, einer laufenden Lobby **nach dem Shop** beizutreten,  
ohne dass der Host die Lobby neu starten muss.

---

## 🎮 Genauere Erklärung

Nach dem Verlassen des Shops oder im Shop öffnet das Spiel die Lobby automatisch neu.  
Spieler, die später beitreten möchten, können dann beitreten, bevor das nächste Level gestartet wird.

Die Mod ist serverseitig und wird nur vom Host benötigt.  
Falls es zu Problemen kommt, z. B. das Spiel ist **desynced**,  
sollte die andere Person die Mod ebenfalls installieren.

Diese Mod sollte mit anderen Mods kompatibel sein, zum Beispiel:
- [MorePlayersImproved](https://thunderstore.io/c/repo/p/Spindles/MorePlayersImproved)

---

## 🧭 Geplante Funktionen

- Beitreten **während eines laufenden Levels**  
- Beide sollen dann in den Configs ein- und ausschaltbar sein.

---

## ⚙️ Installation

### 🔧 Per Thunderstore

1. Erstelle oder öffne ein **Mod-Profil**, in dem du die Mod installieren möchtest.  
2. Installiere sie über den Browser oder suche auf Thunderstore nach **LateRepo**.  
👉 [Download auf Thunderstore](https://thunderstore.io/c/repo/p/Chaosholz/LateRepo)

---

### 📦 Manuell mit BepInEx

- **BepInEx 5.x**  
  👉 [BepInEx herunterladen](https://github.com/BepInEx/BepInEx/releases)

#### Schritt 1 – BepInEx einrichten

1. Lade **BepInEx 5.x** herunter.  
2. Entpacke den Inhalt in dein **R.E.P.O.**-Verzeichnis:
```
REPO/
├── REPO_Data/
├── BepInEx/
├── doorstop_config.ini
├── REPO.exe
├── UnityPlayer.dll
└── winhttp.dll
```

4. Starte das Spiel einmal, damit die Unterordner angelegt werden:
- `BepInEx/plugins/`
- `BepInEx/config/`

#### Schritt 2 – Mod hinzufügen
1. Kopiere `LateRepo.dll` in:
BepInEx/plugins/

2. Starte das Spiel erneut.

---

## 💬 Hinweise

Diese Mod ist **nicht offiziell** und wird **nicht vom ursprünglichen Entwickler unterstützt**.  
Benutzung erfolgt auf eigene Verantwortung.  

Wenn Probleme auftreten:
- Schau, dass alle dieselbe Mod-Version benutzen.  
- Falls du andere Mods nutzt, teste LateRepo allein, um Konflikte auszuschließen.

---

### Credits
Based on [LateJoin](https://thunderstore.io/c/repo/p/felinusfish/LateJoin/) by **felinusfish**  
Modified by **Chaosholz** to restore LateJoin functionality after the latest R.E.P.O. update.  
Licensed under the **GNU GPLv3**

---

