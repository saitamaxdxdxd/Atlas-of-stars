# Atlas of Stars — Game Design Document

---

## Concepto central

**Eres un navegante en un sistema solar que se está muriendo.**
La estrella es inestable. Cada ciclo solar el sistema se vuelve más caótico y hostil.
No eres el único — hay otros sobrevivientes, comerciantes, piratas y desesperados haciendo lo mismo.
Todos saben que el sistema tiene los días contados. La pregunta es quién escapa primero y con qué.

La habilidad principal no es disparar ni construir: es **leer el espacio como un sistema de fuerzas**
y usarlo a tu favor. Dominar la gravedad es lo que separa a quien sobrevive de quien se queda varado.

---

## El universo como IP (visión a largo plazo)

Atlas of Stars no es solo un juego — es un universo.
El sistema solar que se muere es el **Sector Auriga**. Hay más sectores.
Cada juego futuro puede explorar otro fragmento del mismo universo:

- **Atlas of Stars** (este juego) — el sector moribundo, la huida, los primeros sobrevivientes
- Spinoff posible: un juego de estrategia sobre las facciones que lograron escapar y fundar colonias
- Spinoff posible: otro sector, otro tipo de nave, mismas leyes físicas pero diferente escenario
- Spinoff posible: prequel — ¿cómo empezó a morir la estrella? ¿hubo sabotaje?

La física gravitacional, los ciclos solares y la economía de combustible son el **lenguaje del universo**.
Cualquier juego del mismo universo los usa — el jugador ya sabe cómo moverse.

---

## Loop de juego

```
Despertar en ciclo 1 → Explorar zona segura → Encontrar recursos/contactos
→ Sobrevivir primer pulso → Mejorar nave → Explorar más lejos → Encontrar facciones
→ Decidir: comerciar, atacar o aliarse → Ciclos se acumulan → Sistema se deteriora
→ Buscar la ruta de escape → Salir antes de que todo colapse
```

El mundo es persistente. No hay pantalla de game over con reinicio.
Si mueres: pierdes recursos no guardados, respawneas en la última estación visitada.
El mundo sigue existiendo y deteriorándose sin ti.

---

## Curva de dificultad — que se sienta natural, no forzado

El juego **nunca te explica con texto**. Te enseña con el mundo.

### Ciclos 1–5: el sistema te guía sin decírtelo

- El primer planeta que ves está cerca, es grande, y su gravedad *se nota visualmente* en la tela
- No hay enemigos todavía — solo el silencio del espacio y un asteroide flotando cerca con combustible
- El primer pulso solar ocurre cuando ya has tenido tiempo de ver planetas — el jugador instintivamente
  se mete detrás del planeta más cercano. Si no lo hace, recibe daño leve, no fatal. Aprende.
- Los primeros objetos flotantes son fáciles de alcanzar y dan recursos generosos
- La nave empieza con el depósito lleno — el combustible tarda en ser un problema real

### Ciclos 6–15: el espacio empieza a hablar

- Aparecen los primeros derelictos y restos de naves — señales de que algo pasó aquí
- Primer contacto con NPC neutral (un comerciante) que te ofrece intercambio simple
- Primer asteroide mineable — el juego no te dice cómo, pero ves que tiene cristales brillantes
  y tienes un arma. La conclusión es obvia.
- Los pulsos se vuelven más frecuentes — el jugador ya sabe qué hacer

### Ciclos 16–30: la tensión se vuelve real

- Primer NPC hostil — y no es invencible. Si lo evades o lo derrotas, tienes opciones
- Aparece el primer agujero de gusano. Es visible, llamativo, peligroso de aspecto.
  Nadie te obliga a usarlo. Pero un jugador curioso lo intentará.
- El combustible por primera vez puede escasear si no prestaste atención en los primeros ciclos

### Ciclos 31+: caos natural

- El sistema se deteriora porque narrativamente lo está haciendo, no porque el juego "aumentó el nivel"
- El jugador ahora tiene herramientas para manejar la complejidad — las consiguió orgánicamente

**Regla de diseño:** cada mecánica nueva aparece primero en versión segura/pequeña, luego escala.
Nunca se introduce algo nuevo en su forma más peligrosa.

---

## El ciclo solar

**1 día solar = 10 minutos de juego activo.**
El tiempo NO avanza cuando el juego está cerrado. La presión es dentro de la sesión.
Una partida completa de 100 días = ~16 horas. Pocos llegan al día 100 en su primera partida — está bien.

| Días   | Estado del sistema | Duración aprox. |
|--------|--------------------|-----------------|
| 1–20   | Silencio. Aprendes moviéndote. Recursos generosos | ~3h |
| 21–40  | Primeros contactos. Pulsos leves. Facciones aparecen | ~3h |
| 41–60  | Fenómenos activos. Recursos escasos. Conflictos | ~3h |
| 61–80  | Agujeros negros. Facciones desesperadas. Pulsos fuertes | ~3h |
| 81–100 | Caos total. La estrella agoniza. Carrera final de escape | ~3h |

**Pulso solar** — evento global periódico. La estrella emite una onda de energía.
- En campo abierto → daño masivo
- Detrás de un planeta → a salvo
- Obliga al jugador a conocer las posiciones de los cuerpos celestes como refugios naturales

---

## Economía del combustible

Nunca se regenera solo. Siempre hay forma de conseguirlo, pero requiere esfuerzo o habilidad.

1. **Gravity assist** — el ángulo correcto cerca de un planeta te acelera gratis.
   Un jugador que domina esto gasta ~20% del combustible de uno que vuela recto.
2. **Minería de asteroides** — dispara, recoge cristales de hidrógeno.
3. **Depósitos flotantes** — restos de naves, cápsulas eyectadas, cargamentos perdidos.
4. **Estaciones y comerciantes** — compras directamente a cambio de otros materiales.
5. **Zonas de Lagrange** — puntos gravitacionales estables donde se acumula materia orgánicamente.

**Nunca dejar al jugador varado:** siempre hay un asteroide o depósito cerca. El juego genera
recursos suficientes para que quien explora no se quede sin nada — pero quien malgasta, sufre.

---

## Objetos flotantes e intercambios

El espacio está lleno de cosas que levitan — naves abandonadas, cápsulas, satélites, derelictos.

### Tipos de objetos flotantes

| Objeto | Qué contiene | Cómo interactuar |
|--------|-------------|------------------|
| Cápsula de escape | Combustible, materiales básicos | Acércate y recoge automáticamente |
| Derelicto de nave | Piezas raras, planos de mejoras | Dispara para abrir, entra a la zona |
| Satélite abandonado | Datos del sistema (mapa parcial) | Colisiona suavemente o dispara |
| Baliza comercial | NPC con oferta de intercambio | Acércate, menú de trueque |
| Asteroide rico | Cristales de hidrógeno, metales | Minería con arma |
| Contenedor sellado | Desconocido hasta abrir — puede ser bueno o trampa | Dispara para abrir |

### Sistema de trueque con derelictos/NPCs flotantes

No hay dinero universal. Todo es trueque.
Ejemplos de intercambios:
- 3 cristales de hidrógeno → 1 plano de mejora de propulsor
- 1 fragmento de aleación rara → recarga completa de combustible
- Información de ruta de escape → alianza temporal con facción

Esto crea decisiones genuinas: ¿guardo los cristales para combustible o los cambio por la mejora?

---

## Facciones — otros sobrevivientes

Todo el mundo sabe que el sistema se está muriendo. Cada grupo reacciona diferente.

### Los Carroñeros (hostiles por defecto)
Naves que abandonaron cualquier código moral. Atacan a cualquiera para robar recursos.
Al principio son pocos y débiles. En ciclos avanzados son el mayor peligro.
*Motivación:* sobrevivir a cualquier costo. No buscan escapar — no creen que sea posible.

### La Coalición de Tránsito (neutrales / comerciantes)
Mantienen estaciones de intercambio. No atacan pero tampoco regalan nada.
Tienen la mejor información sobre rutas de escape — a precio.
*Motivación:* acumular suficiente para comprar su propia salida.

### Los Anclados (neutrales / filosóficos)
Decidieron no escapar. Creen que el sistema tiene más ciclos de los que todos piensan.
Conocen el sistema solar mejor que nadie — llevan generaciones aquí.
Son la mejor fuente de conocimiento sobre física gravitacional (tutoriales naturales disfrazados de diálogo).
*Motivación:* vivir mientras puedan, compartir lo que saben.

### Los Pioneros (aliados potenciales)
Igual que tú — buscan escapar. Compiten contigo por recursos pero pueden aliarse.
Si los ayudas, te dan ventajas. Si los traicionas, te recuerdan.
*Motivación:* llegar a otro sector. La misma que tú.

### El Vigía (misterioso)
Una sola nave que no encaja con ninguna facción. Aparece, observa, desaparece.
Nadie sabe quién es ni de dónde vino. Deja objetos valiosos sin explicación.
*Motivación:* desconocida. Quizás sabe algo que los demás no.
*(Semilla para expansiones / juegos futuros del universo)*

---

## Fenómenos espaciales

| Fenómeno | Comportamiento |
|----------|----------------|
| Asteroides | Campos en órbita. Fuente de materiales. Colisión = daño |
| Meteoritos | Proyectiles del espacio profundo. Aumentan con ciclos |
| Agujero de gusano | Par de portales orbitando. Teletransporte al otro lado del sistema. Inestables en ciclos altos |
| Agujero negro | Ciclos avanzados. Trampa mortal o slingshot extremo si lo dominas |
| Pulso solar | Evento global periódico. Busca sombra |
| Nube de plasma | Drena combustible al atravesarla. Rodeala o cruza rápido |
| Campo de escombros | Restos de naves destruidas. Peligroso pero rico en recursos |
| Resonancia gravitacional | Dos planetas alineados crean corredor de fuerza brutal |
| Zona de Lagrange | Punto estable entre dos cuerpos. Recursos acumulados naturalmente |
| Eyección coronal | Evento visual masivo. Empuja todo en una dirección — úsalo o escápate |

---

## Progresión de la nave

No hay niveles de personaje. La nave mejora con materiales encontrados/comprados.

| Mejora | Efecto |
|--------|--------|
| Depósito de combustible | Mayor capacidad máxima |
| Propulsor eficiente | Menos consumo al empujar |
| Casco reforzado | Aguanta más colisiones y pulsos |
| Escáner gravitacional | Visualiza intensidad de campos de gravedad en el mapa |
| Imán de recolección | Recoge fragmentos a distancia automáticamente |
| Misil dirigido | Nuevo proyectil con guiado — útil para minería a distancia y combate |
| Blindaje solar | Reduce daño de pulsos solares |
| Reactor de emergencia | Si te quedas sin combustible, genera un pulso mínimo una vez |
| Detector de anomalías | Señala en el mapa wormholes y zonas de Lagrange cercanas |

---

## Lo que persiste al morir

**Se conserva:**
- Mejoras instaladas en la nave
- Mapa explorado (cuerpos celestes descubiertos)
- Relaciones con facciones (quién te debe, quién te odia)
- Ciclo solar actual (el mundo no se reinicia)

**Se pierde:**
- Recursos no depositados en estación
- Posición → respawn en última estación visitada

---

## Por qué este juego funciona y no es lo de siempre

1. **La física es la mecánica, no el obstáculo.** Casi todos los juegos espaciales ignoran la gravedad
   o la hacen invisible. Aquí se ve, se siente y se usa como herramienta.

2. **El jugador aprende física real sin darse cuenta.** Los slingshots gravitacionales existen.
   La NASA los usa. Cuando el jugador ejecuta uno, aprendió algo verdadero.

3. **La tensión es narrativa, no arbitraria.** El sistema se muere porque tiene sentido en la historia,
   no porque el juego "subió el nivel de dificultad".

4. **El espacio se siente vivo.** Hay facciones con motivaciones propias. El Vigía aparece y
   desaparece. Los Anclados saben cosas. No estás solo en un mapa vacío.

5. **La habilidad se ve.** Un novato vuela torpe y quema combustible. Un experto danza entre
   planetas y llega con el depósito lleno. La diferencia es visible e impresionante para ambos.

6. **Tiene algo que decir.** Un sistema solar que se muere mientras sus habitantes se ayudan,
   se traicionan o simplemente intentan sobrevivir — eso es una historia. No es solo un pretexto
   para mecánicas.

---

## Arquitectura del mundo — cómo se construye sin romperse

### El problema: mundos grandes vs. precisión de Unity
Unity pierde precisión de física con `float` a partir de ~7,000 unidades del origen.
Solución: **Floating Origin** — cuando el jugador se aleja del centro, el juego mueve
silenciosamente todo el mundo de vuelta al origen. El jugador no nota nada.

### Estructura del sistema solar

```
5–7 planetas con órbitas FIJAS y personalidad definida (handcrafted)
    └── Zona de influencia de cada planeta (radio fijo)
         └── Contenido interior PROCEDURAL por partida:
              - Posición de asteroides y campos de escombros
              - Qué facción ocupa qué zona en este día
              - Qué contienen los derelictos
              - Cuándo ocurren los eventos locales
```

El jugador en su segunda partida reconoce los planetas pero no lo que hay entre ellos.
La familiaridad reduce la barrera de entrada. La variabilidad mantiene la sorpresa.

### Los 5 planetas — personalidades sugeridas

| Planeta | Rol | Característica |
|---------|-----|----------------|
| Helio (cercano) | Tutorial natural | Grande, gravedad visible, primer refugio del pulso |
| Ferrón | Minería | Rodeado de campos de asteroides ricos |
| Nexus | Comercio | Estación de la Coalición orbita aquí |
| Umber | Misterio | Los Anclados viven aquí. Fenómenos extraños |
| Ceniza | Muerto | Ya colapsó. Campo de escombros masivo. Peligroso/valioso |

### Carga por distancia (chunks)

```
Zona activa     → radio ~200u   — física completa, enemigos activos
Zona dormida    → radio 200-500u — solo posiciones, sin física
Zona descargada → más de 500u   — existe solo en el estado (seed + día solar)
```

Object Pooling para asteroides, enemigos y derelictos — nunca se crean/destruyen,
se activan y desactivan del pool según la zona del jugador.

### Seed por partida

Cada nueva partida genera una semilla (número aleatorio).
Esa semilla determina de forma reproducible dónde está cada cosa del contenido procedural.
El mundo es diferente en cada partida pero consistente dentro de la misma sesión.

### Por qué se siente más grande de lo que es

- **El viaje entre planetas ES contenido** — no es tiempo muerto, es donde viven las decisiones
- **Información asimétrica** — el mapa empieza vacío, lo descubres tú. Siempre hay algo por ver
- **Los días limitan qué puedes ver** — en 100 días no alcanza para explorar todo. En la siguiente
  partida eliges diferente
- **Las facciones se reubican** — el comerciante de la zona 3 esta partida estará en otro lado la próxima
- **No necesitas un mundo grande, necesitas sistemas que interactúen** — igual que Minecraft
