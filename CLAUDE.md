# Atlas of Stars — Unity Project Context

## Resumen del proyecto

- **Nombre:** Atlas of Stars
- **Engine:** Unity (C#)
- **Plataforma principal:** PC (Steam)
- **Plataforma secundaria:** Mobile (iOS / Android) — a considerar post-EA, no condicionar decisiones actuales
- **Resolución de referencia:** 1920 × 1080, Canvas Scaler Scale With Screen Size
- **Versión:** 0.2.0
- **Estado:** Mecánicas de vuelo completas — construyendo loop de mundo abierto hacia EA
- **Meta:** Early Access — loop completo: explorar, sobrevivir ciclos solares, escapar del sistema

## Visión de juego

**Mundo abierto persistente, no niveles.** El jugador explora el Sector Auriga — un sistema solar que se está muriendo. Hay una sola partida activa. Al morir, respawn en la última estación visitada (pierdes recursos sueltos, conservas mejoras y mapa). El mundo no se reinicia.

La mecánica central es **leer y usar la gravedad como herramienta**: slingshots, refugio detrás de planetas durante pulsos solares, zonas de Lagrange con recursos. Un jugador experto gasta 20% del combustible de uno que vuela recto.

Ver `GAME_DESIGN.md` para la visión completa del universo, facciones, economía y diseño de curva de dificultad.

---

## Principios de arquitectura

### PC-first

- Input siempre a través de la capa `Scripts/Input/` — nunca hardcodear `Input.*` en gameplay.
- UI con Canvas Scaler `Scale With Screen Size`, 1920×1080, Match 0.5.
- No usar `#if UNITY_ANDROID` ni lógica de plataforma en gameplay — si se porta a mobile después, se agrega en la capa de input.
- Proyecto usa **nuevo Input System** — usar `UnityEngine.InputSystem`, nunca `UnityEngine.Input`.
- Sin restricciones de mobile en rendering: geometry shaders, bloom, post-processing, vertex count alto son todos válidos.

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
| `SpaceFabricManager`  | Scripts/Gameplay/ | Grid 3×3 de chunks centrado en el player, reciclaje automático al moverse             |
| `SpaceFabricChunk`    | Scripts/Gameplay/ | Un chunk de la tela: dos meshes de quad strips (hilos H + V entrelazados en Z)        |
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

### SpaceFabricManager + GravitySource

```csharp
// Setup en escena:
//   1. GameObject vacío → SpaceFabricManager → asignar _fabricMaterial
//   2. Material: Unlit, Cull Off, color blanco. Para glow: HDR color + Bloom post-processing.
//   3. _player se auto-asigna desde Spaceship si se deja vacío.
//   4. Cámara con rotación X 20–35° para ver profundidad de los pozos en perspectiva.
//   5. SpaceFabric.cs (legacy) debe eliminarse de la escena.

// GravitySource: agregar a cualquier objeto masivo. Mass controla profundidad Y ancho del pozo.
// Tabla de masas de referencia (relativas al jugador como 1x):
//
//   Objeto                  Mass      _maxDepth sugerido   Notas
//   ──────────────────────────────────────────────────────────────────
//   Nave del jugador        0.05      —                    Solo para GravFísica, no curva la tela visualmente
//   Proyectil / misil       0         —                    Sin GravitySource
//   Asteroide pequeño       1–3       12                   Curva mínima, decorativa
//   Asteroide grande        4–8       15                   Pozo visible pero sutil
//   Luna / satélite         10–18     18                   Influencia local notable
//   Planeta pequeño (Helio) 20–35     25                   Primer planeta del tutorial
//   Planeta mediano         40–60     30                   Referencia: valores default del manager
//   Planeta gigante         70–120    45                   Pozo muy amplio, influencia lejana
//   Estrella enana          150–250   60                   Peligroso acercarse
//   Agujero negro           400–800   100                  Trampa mortal o slingshot extremo
//
// Regla: si el fondo del pozo se ve PLANO, el _maxDepth del manager es menor que la Mass del objeto.
//        Subir _maxDepth hasta que el pozo tenga pico pronunciado en el centro.

// Parámetros SpaceFabricManager (Inspector):
//   _chunkSize       → tamaño de cada chunk (60 default). 3×3 = 180u de cobertura total.
//   _threadCount     → hilos por dirección por chunk. Menos = más espacio entre hilos.
//                      15–20 hilos = aireado (recomendado para inicio)
//                      30–40 hilos = denso
//   _segmentCount    → puntos por hilo (suavidad de la curva en los pozos). 40 = bueno.
//   _threadHalfWidth → medio grosor del hilo. 0.04–0.08 = rango visual bueno.
//   _interlaceOffset → separación Z entre H y V. 0.05 = entrelazado sutil.
//   _falloff         → velocidad de decaimiento del pozo GLOBAL (para objetos sin override).
//                      0.004–0.005 = recomendado para asteroides  |  0.008+ = pozo estrecho
//                      Para planetas/estrellas usar FalloffOverride en GravitySource (ver abajo).
//   _maxDepth        → tope de seguridad. Debe ser >= Mass del objeto más grande en escena.
//                      Default recomendado: 30. Planetas grandes: 80–120. Agujero negro: 150+.

// GravitySource — campo FalloffOverride (0 = usar global del manager):
//   El problema con objetos grandes: con falloff global 0.004, el pozo cae muy rápido.
//   Para un planeta de radio 50u, toda la parte profunda queda DENTRO de la esfera (oculta).
//   Solución: FalloffOverride bajo = pozo más ancho que se ve alrededor del objeto.
//
//   Objeto                  FalloffOverride   Pozo visible hasta
//   ──────────────────────────────────────────────────────────────
//   Asteroide               0 (global)        ~80u
//   Luna / satélite         0 (global)        ~80u
//   Planeta pequeño         0.002             ~120u
//   Planeta mediano         0.001             ~180u
//   Planeta gigante         0.0005            ~300u
//   Estrella                0.0002            sistema entero
//   Agujero negro           0.0001            sistema entero (trampa)
//
// Cámara — ángulo recomendado para ver profundidad de pozos:
//   20–35° X rotation = sutil, bueno para asteroides
//   45–55° X rotation = dramático, recomendado para planetas y estrellas

// Deformación — fórmula interna por vértice:
//   falloff = FalloffOverride > 0 ? FalloffOverride : _falloff (global)
//   depth += Mass / (1 + dist² × falloff)   ← curva Lorentziana, realista y suave
//   La profundidad se acumula de todas las GravitySources — pozos se suman entre sí.
//   Un objeto MUY masivo produce un pozo profundo y ancho. Si llega al cap (_maxDepth),
//   el fondo aparece plano — subir _maxDepth para evitarlo.

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
├── Gameplay/    → Spaceship, SpaceFabricManager, SpaceFabricChunk, GravitySource, TrajectoryPredictor,
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

- [X] SpaceFabricManager + SpaceFabricChunk — grid 3×3 de hilos H+V entrelazados, sigue al player
- [X] GravitySource — masa gravitacional, atrae nave y deforma tela
- [X] Spaceship — empuje, rotación, estabilizador, gravedad newtoniana
- [X] TrajectoryPredictor — inercia (azul) + empuje (naranja), Tab = toggle
- [X] CameraFollow — sigue al player con offset y smooth
- [X] ShipWeapon — disparo con retroceso físico
- [X] Projectile — proyectil con Rigidbody y colisión
- [X] ShipHealth — vida 0–100, TakeDamage/Heal, OnDied/OnHealthChanged
- [X] HUDController — velocidad, combustible %, vida %

### Fase 1 — Sistema solar + mundo base jugable ← AQUÍ VAMOS

**Lo que decidimos:**
- **Sistema binario**: dos estrellas orbitando su centro de masa común
  - Estrella A (moribunda) — fuente del caos, pulsos solares, visualmente más grande e inestable
  - Estrella B (sana) — más pequeña, estable, su Lagrange L1 con A = ruta de escape narrativa
  - En astrofísica real: una estrella sana puede acelerar la muerte de la otra por fuerzas de marea ✓
- **5 planetas handcrafted** con órbitas fijas y predefinidas (se mueven, pero su path es constante)
- **Contenido entre planetas** procedural por seed (derelictos, facciones, pickups)

**Los 3 scripts a construir (en orden):**

```
1. OrbitalBody      → mueve un GO en órbita circular alrededor de un punto/Transform
2. CelestialBody    → datos de un cuerpo celeste (nombre, tipo, radio de colisión, descripción)
3. WorldGenerator   → instancia y posiciona todo el sistema solar al cargar la partida
```

**Diseño de OrbitalBody:**
- `[SerializeField] Transform _center` — punto de órbita (puede ser otro GO, ej. centro de masa)
- `[SerializeField] float _radius` — distancia al centro
- `[SerializeField] float _period` — segundos por vuelta completa (negativo = sentido horario)
- `[SerializeField] float _startAngle` — ángulo inicial en grados (para que no salgan todos del mismo punto)
- En `FixedUpdate`: actualiza posición en círculo. No usa física — MovePosition directo.
- Los planetas tienen GravitySource → la tela se deforma y se mueve con ellos en tiempo real ✓

**Diseño de CelestialBody:**
- Tipo enum: `Star`, `Planet`, `Moon`, `AsteroidField`, `DeadPlanet`
- Datos: nombre localizado, descripción, radio de colisión, zona de influencia (para spawn de contenido)
- Evento: `OnPlayerEnterInfluence`, `OnPlayerExitInfluence` — para trigger de contenido procedural

**Diseño de WorldGenerator:**
- Lee el worldSeed de SaveManager
- Instancia desde prefabs: 2 estrellas + 5 planetas (todos con OrbitalBody + CelestialBody + GravitySource)
- Genera contenido procedural en cada zona de influencia usando el seed
- Spawna nave en posición orbital válida (con velocidad tangencial inicial para que no caiga)

**Parámetros del sistema solar (a ajustar en escena):**

| Cuerpo | Tipo | Mass | Radio orbital | Período | Scale | Collision R | Influence R | Falloff Override | Notas |
|--------|------|------|--------------|---------|-------|-------------|-------------|-----------------|-------|
| Estrella A | Star | 200 | 20u (orbita CM) | 120s | 20 | 10 | 90 | 0.0002 | Moribunda, pulsos |
| Estrella B | Star | 120 | 30u (orbita CM) | 120s | 14 | 7 | 70 | 0.0003 | Sana, más pequeña |
| Helio | Planet | 25 | 120u | 280s | 8 | 4 | 50 | 0.001 | Tutorial, cercano |
| Ferrón | Planet | 18 | 300u | 600s | 6 | 3 | 40 | 0.001 | Minería, asteroides |
| Nexus | Planet | 20 | 900u | 1500s | 7 | 3.5 | 45 | 0.001 | Comercio, estación |
| Umber | Planet | 15 | 1600u | 3000s | 5 | 2.5 | 35 | 0.0015 | Misterio, Anclados |
| Ceniza | Planet | 5 | 2800u | 6500s | 3 | 1.5 | 25 | 0 | Muerto, escombros |
| Coloso | Planet | 45 | 4500u | 12000s | 12 | 6 | 80 | 0.0008 | Gigante gaseoso, opcional |
| Limbo | Planet | 30 | 7000u | 22000s | 9 | 4.5 | 65 | 0.001 | Gigante de hielo, borde del sistema, opcional |

Zonas reservadas para cinturones de asteroides:
- **~200u** — belt menor (entre Helio y Ferrón)
- **~500–700u** — belt principal (entre Ferrón y Nexus)

> Los radios son de referencia. El _systemScale del WorldGenerator los multiplica globalmente.

**Colisiones — qué pasa al chocar:**
- Asteroide pequeño → TakeDamage(10)
- Planeta → TakeDamage(100) → muerte instantánea
- Estrella → TakeDamage(999) → muerte instantánea

**Velocidad tangencial inicial de la nave:**
```csharp
// Fórmula para órbita circular: v = sqrt(G * M / r)
// Con G=6, M=25 (Helio), r=30u desde Helio:
// v = sqrt(6 * 25 / 30) = sqrt(5) ≈ 2.24 u/s perpendicular a la gravedad
// Setear en Awake() de Spaceship o desde WorldGenerator al spawnear
```

- [ ] **OrbitalBody** — componente que mueve GOs en órbita circular, configurable desde Inspector
- [ ] **CelestialBody** — datos y eventos de cuerpos celestes. Enum tipo, zona de influencia
- [ ] **WorldGenerator** — instancia el sistema binario + 5 planetas + spawn de nave con velocidad orbital
- [ ] **Colisiones con astros** — OnCollisionEnter → ShipHealth.TakeDamage según tipo de cuerpo
- [ ] **SolarCycleManager** — tiempo de juego (10 min = 1 día solar), contador, evento `OnSolarPulse`
- [ ] **SolarPulse** — daño masivo en campo abierto, safe zone detrás de planetas (raycast de sombra)
- [ ] **FloatingOrigin** — reposiciona todo cuando el jugador supera ~500u del origen
- [ ] **FuelPickup** — objeto flotante recogible que restaura combustible al acercarse
- [ ] **RespawnSystem** — al OnDied: guardar estado, teleportar a lastStationId, restaurar vida/fuel

### Fase 2 — Economía y progresión

- [ ] **ResourceSystem** — inventario: combustible, cristales de hidrógeno, fragmentos de metal
- [ ] **Asteroid** — mineable con arma (OnProjectileHit → suelta FuelPickup/ResourcePickup)
- [ ] **Station** — punto de guardado + comercio básico (trueque). Setea lastStationId al llegar
- [ ] **ShipUpgradeSO** — ScriptableObject por mejora (costo, efecto). Al menos: depósito extra, casco reforzado

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
