# Atlas of Stars — Unity Project Context

## Resumen del proyecto

- **Nombre:** Atlas of Stars
- **Engine:** Unity (C#)
- **Plataforma principal:** Mobile (iOS / Android)
- **Plataforma secundaria:** PC (escalable — no romper nada mobile-first)
- **Orientación:** Landscape (horizontal) — Canvas resolución de referencia **1920 × 1080**
- **Versión:** 0.2.0
- **Estado:** Mecánicas de vuelo completas — construyendo loop de mundo abierto hacia EA
- **Meta:** Early Access — loop completo: explorar, sobrevivir ciclos solares, escapar del sistema

## Visión de juego

**Mundo abierto persistente, no niveles.** El jugador explora el Sector Auriga — un sistema solar que se está muriendo. Hay una sola partida activa. Al morir, respawn en la última estación visitada (pierdes recursos sueltos, conservas mejoras y mapa). El mundo no se reinicia.

La mecánica central es **leer y usar la gravedad como herramienta**: slingshots, refugio detrás de planetas durante pulsos solares, zonas de Lagrange con recursos. Un jugador experto gasta 20% del combustible de uno que vuela recto.

Ver `GAME_DESIGN.md` para la visión completa del universo, facciones, economía y diseño de curva de dificultad.

---

## Principios de arquitectura

### Mobile-first, PC-ready

- Input siempre a través de la capa `Scripts/Input/` — nunca hardcodear `Input.*` en gameplay.
- UI con Canvas Scaler `Scale With Screen Size`, 1920×1080, Match 0.5.
- Lógica de plataforma solo en handlers de input/plataforma (`#if UNITY_ANDROID`).
- Proyecto usa **nuevo Input System** — usar `UnityEngine.InputSystem`, nunca `UnityEngine.Input`.

### Modular y escalable

- Una responsabilidad por clase. Scripts pequeños y enfocados.
- **ScriptableObjects para datos** — nunca magic numbers en código.
- **Events/Delegates** para comunicación entre sistemas.
- No Singletons en gameplay — solo en Managers que persisten.

### Notas críticas de implementación

- `??=` NO funciona con objetos Unity — siempre usar `if (x == null)` para componentes.
- `AddComponent<T>()` dispara `Awake()` inmediatamente — no asignar campos después.
- Managers que dependen de `SaveManager` deben leer en `Awake()`, no `Start()`, porque `SaveManager` usa `RuntimeInitializeOnLoadMethod(BeforeSceneLoad)`.
- `LocalizationData` asset debe estar en `_Project/Resources/` con nombre exacto `LocalizationData`.
- `LocalizationManager.Awake()` null-checkea `SaveManager.Instance` — orden de `BeforeSceneLoad` no está garantizado entre managers.
- `GameManager.Awake()` fuerza `Time.timeScale = 1f` — el enum `GameState` arranca en `Playing` (0) y `SetState(Playing)` hace early return, nunca lo resetea sin esto.
- Unity pierde precisión de física con `float` a partir de ~7,000u del origen — usar **FloatingOrigin** cuando el jugador se aleja.

---

## Pipeline de escenas

```
Boot → MainMenu → Loading (3s async) → GameScene (mundo abierto persistente)
                                              ⇄ PauseMenu
```

> No hay LevelSelect. Hay una sola partida activa. El menú principal tiene: Nueva Partida / Continuar / Settings / Quit.

| Escena    | Carpeta       | Notas                                              |
| --------- | ------------- | -------------------------------------------------- |
| Boot      | Scenes/Core/  | Logos empresa + juego, arranque                    |
| MainMenu  | Scenes/Menu/  | Nueva Partida / Continuar / Settings / Quit        |
| Loading   | Scenes/Core/  | Carga async GameScene, mínimo 3s                   |
| GameScene | Scenes/World/ | El mundo abierto completo. Única escena de juego   |

---

## Scripts existentes

### Core

| Script                | Ruta          | Función                                                    |
| --------------------- | ------------- | ----------------------------------------------------------- |
| `SceneNames`        | Scripts/Core/ | Constantes de nombres de escena                             |
| `BootLoader`        | Scripts/Core/ | Secuencia splash (logos con fade/Animator o fallback)       |
| `LoadingController` | Scripts/Core/ | Carga async GameScene, mínimo 3s                            |

### Managers (DontDestroyOnLoad — todos se auto-crean con `RuntimeInitializeOnLoadMethod`)

| Script                  | Ruta              | Función                                                         |
| ----------------------- | ----------------- | ---------------------------------------------------------------- |
| `SceneLoader`         | Scripts/Managers/ | Carga de escenas sync/async                                      |
| `SaveManager`         | Scripts/Managers/ | JSON centralizado en `persistentDataPath/save.json`            |
| `AudioManager`        | Scripts/Managers/ | Playlist aleatoria (música) + SFX                               |
| `LocalizationManager` | Scripts/Managers/ | Idioma EN/ES, carga `LocalizationData` desde Resources         |
| `GameManager`         | Scripts/Managers/ | Estado del juego (Playing/Paused/GameOver). Sin LevelComplete   |

### Input

| Script             | Ruta           | Función                                                                  |
| ------------------ | -------------- | ------------------------------------------------------------------------ |
| `IInputHandler`  | Scripts/Input/ | Interfaz: OnTap, OnSwipe, OnHoldStart, OnHoldEnd, OnPointerDown/Up       |
| `InputHandler`   | Scripts/Input/ | Touch (mobile, primer dedo) + Mouse (PC/Editor). Agregar a un GameObject |

Parámetros configurables en Inspector: `_swipeThreshold` (px), `_tapMaxDuration` (s), `_holdDuration` (s).

### Menu

| Script                 | Ruta          | Función                                                       |
| ---------------------- | ------------- | ------------------------------------------------------------- |
| `MainMenuController` | Scripts/Menu/ | Botones Nueva Partida / Continuar / Settings / Quit           |
| `SettingsController` | Scripts/Menu/ | Sliders música/SFX + botones EN/ES. Llamar Open() / Close()  |

> `LevelSelectController` y `LevelButton` existen en el código pero son **legacy** — no aplican al diseño de mundo abierto. No usar para features nuevas.

### Gameplay

| Script                  | Ruta              | Función                                                                              |
| ----------------------- | ----------------- | ------------------------------------------------------------------------------------ |
| `PauseController`     | Scripts/Gameplay/ | Escucha GameManager.OnStateChanged, Escape/P toggle pausa                            |
| `GameOverController`  | Scripts/Gameplay/ | Panel GameOver (muerte) — Respawn / Menú                                             |
| `SpaceFabric`         | Scripts/Gameplay/ | Malla 3D dinámica que simula la tela del espacio-tiempo, se deforma con GravitySource |
| `GravitySource`       | Scripts/Gameplay/ | Marca un objeto como masa gravitacional. Lista estática `GravitySource.All`          |
| `Spaceship`           | Scripts/Gameplay/ | Nave del jugador: empuje, rotación, estabilizador y gravedad newtoniana              |
| `TrajectoryPredictor` | Scripts/Gameplay/ | Dos LineRenderers: inercia (azul) + empuje (naranja). Tab = toggle                   |
| `CameraFollow`        | Scripts/Gameplay/ | Cámara que sigue al player con offset y smooth                                       |
| `ShipWeapon`          | Scripts/Gameplay/ | Dispara proyectiles desde el muzzle. Click izq / F. Retroceso físico configurable   |
| `Projectile`          | Scripts/Gameplay/ | Proyectil con Rigidbody, velocidad inicial, lifetime, destruye al colisionar         |
| `ShipHealth`          | Scripts/Gameplay/ | Vida de la nave (0–100). TakeDamage/Heal. Eventos OnDied, OnHealthChanged            |

### UI

| Script            | Ruta        | Función                                                                          |
| ----------------- | ----------- | -------------------------------------------------------------------------------- |
| `LocalizedText` | Scripts/UI/ | TMP que se actualiza solo al cambiar idioma. Refresh en Start()                  |
| `HUDController` | Scripts/UI/ | HUD in-game: velocidad (u/s), combustible %, vida %. Solo TMP_Text              |

### Data (ScriptableObjects)

| Script               | Ruta          | Función                                                                          |
| -------------------- | ------------- | -------------------------------------------------------------------------------- |
| `SaveData`         | Scripts/Data/ | Modelo JSON: language, worldSeed, solarDay, resources, shipUpgrades, lastStationId, musicVolume, sfxVolume |
| `SoundData`        | Scripts/Data/ | AudioClip + volume + pitch + loop                                                |
| `LocalizationData` | Scripts/Data/ | Lista de LocalizationEntry (key / english / spanish)                             |

> `SaveData` tiene campos legacy (`unlockedLevels`) — ignorar, serán removidos cuando se refactorice el save.

---

## Patrones de uso

### Audio

```csharp
AudioManager.Instance.PlayPlaylist(AudioManager.Instance.musicGame);
AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxButtonClick);
AudioManager.Instance.SetMusicVolume(0.8f); // persiste en save.json
```

### Localización

```csharp
LocalizationManager.Instance.Get("btn_play"); // → "Play" o "Jugar"
LocalizationManager.Instance.SetLanguage(Language.Spanish);
// En UI: componente LocalizedText en TMP, asignar key en Inspector
```

### Guardar datos

```csharp
SaveManager.Instance.Data.solarDay = 5;
SaveManager.Instance.Save();
```

### Estado del juego

```csharp
GameManager.Instance.GameOver();           // muerte → panel respawn
GameManager.OnStateChanged += OnStateChanged;
```

### Input

```csharp
[SerializeField] private InputHandler _input;

private void OnEnable()  { _input.OnTap += HandleTap; }
private void OnDisable() { _input.OnTap -= HandleTap; }
private void HandleTap(Vector2 screenPos) { }
```

### SpaceFabric + GravitySource

```csharp
// Cualquier objeto que deba curvar la tela y atraer la nave:
// → agregar componente GravitySource, ajustar Mass en Inspector
// → astros/planetas: Mass 5–30   |   nave: Mass 0.05

// SpaceFabric lee GravitySource.All automáticamente — no requiere refs manuales

// Parámetros SpaceFabric: Resolution (50), Size, Max Depth, Falloff
// Parámetros Spaceship: Thrust Force (2), Rotation Speed (75°/s), Fuel Capacity,
//   Fuel Burn Rate (12/s), Stab Burn Rate (4/s), Stab Speed (8), Grav Constant

// Controles PC:
//   W/↑/Space → empuje   |   A/D/←/→ → rotar   |   S/Shift → estabilizador
//   Click/F → disparar   |   Tab → toggle trayectoria

// Rigidbody nave: Use Gravity=false, Drag=0, Angular Drag=0, Interpolate=Interpolate
//   Constraints: Freeze Z + Freeze Rot X + Freeze Rot Y

// TrajectoryPredictor: Steps × StepSize = segundos predicción (120 × 0.06 = ~7s)
// ShipWeapon: Muzzle = GO hijo en punta de nave (eje Y local = frente)
// CameraFollow: offset se calcula automáticamente en Start()
```

### Settings

```csharp
public void OnSettingsPressed() => _settings.Open();
// Conectar en Inspector: Slider.OnValueChanged → OnMusicVolumeChanged / OnSfxVolumeChanged
// Btn_English.onClick → OnEnglishPressed | Btn_Spanish.onClick → OnSpanishPressed
```

---

## Estructura de carpetas (`Assets/_Project/`)

```
Scripts/
├── Core/        → SceneNames, BootLoader, LoadingController
├── Gameplay/    → Spaceship, SpaceFabric, GravitySource, TrajectoryPredictor,
│                  CameraFollow, ShipWeapon, Projectile, ShipHealth, PauseController,
│                  GameOverController, [SolarCycle, FloatingOrigin, WorldGenerator, ...]
├── Input/       → IInputHandler, InputHandler
├── Managers/    → SceneLoader, SaveManager, AudioManager, LocalizationManager, GameManager
├── Menu/        → MainMenuController, SettingsController
├── UI/          → LocalizedText, HUDController
├── Data/        → SaveData, SoundData, LocalizationData
└── Utils/       → Helpers, Extensions, Constants (pendiente)

Resources/           → LocalizationData.asset
Scenes/
├── Core/        → Boot, Loading
├── Menu/        → MainMenu
└── World/       → GameScene (mundo abierto, única escena de juego)
ScriptableObjects/
├── Audio/       → SoundData assets
├── Settings/    → LocalizationData asset
└── Items/
```

---

## Convenciones de código

- **Namespaces:** `AtlasOfStars.Core`, `AtlasOfStars.Gameplay`, `AtlasOfStars.UI`, `AtlasOfStars.Managers`, `AtlasOfStars.Data`, `AtlasOfStars.Menu`
- **Naming:** PascalCase clases/métodos, `_camelCase` campos privados, prefijo `I` interfaces
- **ScriptableObjects:** sufijo `Data` o `SO` (ej. `SoundData`, `ShipUpgradeSO`)
- **Events:** prefijo `On` (ej. `OnPlayerDied`, `OnSolarPulse`)
- **Unity null:** siempre `if (x == null)`, nunca `??=` con UnityEngine.Object

---

## Checklist de desarrollo — Ruta a EA

### Fundación (completado)

- [X] Pipeline de escenas, SceneLoader, BootLoader, LoadingController
- [X] SaveManager, AudioManager, LocalizationManager, LocalizedText
- [X] GameManager (estados: Playing / Paused / GameOver)
- [X] PauseController, GameOverController
- [X] InputHandler (touch + mouse)
- [X] SettingsController (volumen + idioma)
- [X] MainMenuController

### Mecánicas de vuelo (completado)

- [X] SpaceFabric — tela del espacio-tiempo dinámica
- [X] GravitySource — masa gravitacional, atrae nave y deforma tela
- [X] Spaceship — empuje, rotación, estabilizador, gravedad newtoniana
- [X] TrajectoryPredictor — inercia (azul) + empuje (naranja), Tab = toggle
- [X] CameraFollow — sigue al player con offset y smooth
- [X] ShipWeapon — disparo con retroceso físico
- [X] Projectile — proyectil con Rigidbody y colisión
- [X] ShipHealth — vida 0–100, TakeDamage/Heal, OnDied/OnHealthChanged
- [X] HUDController — velocidad, combustible %, vida %

### Fase 1 — Mundo base jugable (siguiente)

- [ ] **Colisiones con astros** — OnCollisionEnter en planetas/asteroides → ShipHealth.TakeDamage
- [ ] **SolarCycleManager** — tiempo de juego (10 min = 1 día solar), contador, evento `OnSolarPulse`
- [ ] **SolarPulse** — daño masivo en campo abierto, safe zone detrás de planetas (raycast sombra)
- [ ] **FloatingOrigin** — reposiciona todo el mundo cuando el jugador supera ~500u del origen
- [ ] **FuelPickup** — objeto flotante recogible que restaura combustible al acercarse
- [ ] **RespawnSystem** — al OnDied: guardar estado, teleportar a lastStationId, restaurar vida/fuel

### Fase 2 — Economía y progresión

- [ ] **ResourceSystem** — inventario: combustible, cristales de hidrógeno, fragmentos de metal
- [ ] **Asteroid** — mineable con arma (OnProjectileHit → suelta FuelPickup/ResourcePickup)
- [ ] **Station** — punto de guardado + comercio básico (trueque). Setea lastStationId al llegar
- [ ] **ShipUpgradeSO** — ScriptableObject por mejora (costo, efecto). Al menos: depósito extra, casco reforzado
- [ ] **WorldGenerator** — 5 planetas handcrafted (Helio, Ferrón, Nexus, Umber, Ceniza) + seed para contenido procedural entre ellos

### Fase 3 — Mundo vivo

- [ ] **NPCShip base** — entidad con movimiento gravitacional propio
- [ ] **FactionSystem** — 4 facciones (Carroñeros, Coalición, Anclados, Pioneros), relaciones neutral/hostile/friendly
- [ ] **WormholePortal** — par de portales, teletransporte entre extremos del sistema
- [ ] **DerelictSpawner** — genera derelictos en zonas de influencia según seed + día solar
- [ ] **SolarPulse VFX** — efecto visual masivo cuando ocurre el pulso

### Fase 4 — Cierre EA

- [ ] **EscapeRoute** — condición de victoria: encontrar y usar la ruta de escape del sector
- [ ] **ChunkLoader** — zona activa (200u física completa) / dormida (200–500u solo posición) / descargada
- [ ] **Object Pooling** — asteroides, proyectiles, derelictos, pickups
- [ ] **Efectos de empuje** — partículas o trail en thruster
- [ ] **Misil dirigido** — nuevo tipo de proyectil con guiado (útil para minería a distancia y combate)
- [ ] **Refactor SaveData** — remover `unlockedLevels`, implementar campos: worldSeed, solarDay, resources, shipUpgrades, exploredBodies, lastStationId, factionRelations
- [ ] **Build iOS / Android** — primer build en dispositivo real

### Post-EA / Futuro

- [ ] Más idiomas
- [ ] Analytics / eventos de juego
- [ ] Notificaciones push (mobile)
- [ ] Monetización (cosmética — no pay-to-win)
- [ ] Spinoffs del universo Auriga
