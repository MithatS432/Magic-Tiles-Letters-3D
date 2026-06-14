# 🅰️ Magic Tiles Letters 3D
**A 3D mobile word puzzle game** — drag letter tiles to spell hidden words.

![Unity](https://img.shields.io/badge/Unity-2022.3+-black?logo=unity)
![Platform](https://img.shields.io/badge/Platform-iOS%20%7C%20Android-blue)
![Status](https://img.shields.io/badge/Status-Core%20Loop%20Complete-brightgreen)

---

## 🎬 Inspired By
This project takes cues from **tile‑based word explorers** (like *Tile Explorer*), blending free‑form letter arrangement with real‑time word matching in a 3D mobile setting.

---

## 🎯 Core Gameplay Loop
1. Random letter tiles spawn on screen.
2. The player taps tiles to place them into **UI word slots** in order.
3. The current sequence is **checked live** against three hidden target words.
4. When a target word is found inside the sequence, **matched letters burst and fade**, and remaining letters shift left.
5. Clear all **three target words** to win the level.
6. Fill the slots without matching → **game over**.

---

## 🧠 Core Gameplay Systems

### 🔤 Word Management (`WordManager` & `WordData`)
- Words are stored in `ScriptableObject` assets (`WordData`).
- `GetValidWords()` filters to **3–6 letters, alphabet‑only** strings.
- Each level picks **3 active words** from shuffled word pools.
- `CheckLive(input, callback)` scans the player’s string using `IndexOf` for a match. On success it returns the match start index & length.
- `matchedFlags[]` track word completion; slots update with strikethrough (`<s>word</s>`) via TextMeshPro.
- When all three words are cleared, `LevelManager.OnLevelWon()` is triggered. New words are loaded after a short delay.

### 🧩 Letter Spawning & Pooling (`LetterSpawner`)
- Builds a letter pool from the characters of the three active words **plus padding letters** (R,S,L,N,E,A,I,O).
- Excludes `T` and `V` from the required pool to vary difficulty.
- Total spawn count: e.g., **20 letters**.
- Positions are randomly distributed inside UI bounds with a **minimum distance** check to avoid overlap.
- Prefabs are mapped via a `Dictionary<char, GameObject>` for instant lookup (`GetLetterPrefab`).

### 🖱️ Letter Interaction (`ClickableLetter`)
- Attached to each letter tile; handles **tap detection** via `Button`.
- On click → `LevelManager.Instance.OnLetterClicked(this)`.
- `MoveToSlot(RectTransform)` animates the tile to the first empty slot using `Lerp` in `Update()`.
- Once the tile reaches its slot (distance < 0.1f), `LevelManager.OnLetterArrived()` triggers a live word check.

### 🔍 Real‑time Word Checking
- `LevelManager.CheckLiveWord()` builds the current word from the slot children using `BuildWordFromSlots()`.
- Minimum 3 characters required to validate; otherwise a full board triggers a loss.
- If `WordManager.CheckLive` finds a match, `ClearMatchedLetters(startIndex, matchLength)` removes only the matched segment and shifts the remaining left.

### 💥 VFX & Feedback (`LetterMatchVFX`, `LevelManager`)
- Matched letters undergo a **burst‑scale** then fade‑out coroutine (`BurstAndFade`).
- Optional particle prefabs spawn at slot positions.
- Win/lose effects (`winEffect`, `loseEffect`) instantiated with 3‑second self‑destruction.
- Audio clips: win, lose, and touch sounds played via `AudioSource.PlayClipAtPoint` or `AudioSource.PlayOneShot`.

### ⚡ Skills / Power‑Ups (`LevelManager`)
Three skills with limited uses (max 3 each), rewarded randomly on level completion:
1. **Auto‑Match** – instantly clears one random unmatched target word.
2. **Undo** – removes the last placed letter from the slots.
3. **Hint Fill** – picks a random unmatched word, clears the slots, and auto‑fills them with the correct letters, then triggers a match.

Skill buttons are auto‑found by name if not assigned, and interactable states update based on remaining count.

### 📊 Level Progression & Game Over
- `currentLevel` displayed via `TextMeshProUGUI` (LEVEL X).
- On win → level incremented, random skill rewarded (if not maxed), and `WordManager` advances to the next words.
- On lose → a restart coroutine reloads the current scene after a 2‑second delay.

---

## ⚙️ Engineering & Architecture

### 🧩 Design Patterns
- **Singleton** – `WordManager`, `LevelManager` ensure central access.
- **ScriptableObject** – Word data decoupled from scenes, easy to edit.
- **Event‑Driven** – `OnWordsAssigned`, `OnWordMatchedVisual` decouple spawner/UI updates.
- **Modular Components** – Spawning, VFX, skills all reside in separate classes.

### 🧠 Object Lifecycle
- Letters are instantiated by `LetterSpawner` and destroyed when matched or on level restart.
- `filledLetters` list tracks slot occupancy; matched letters are removed and shifted.
- `DontDestroyOnLoad` not used for core managers (they exist per scene), keeping scenes self‑contained.

### 🧭 UI & Scene Management
- Slot images (`wordSlots`) are pre‑assigned in the Inspector; letters are parented to them.
- Scene reload on game over (`SceneManager.LoadScene`).
- Word display slots update in real time with strikethrough effect for completed words.

### 🚀 Performance Considerations
- Lightweight `Dictionary` for letter prefab lookup.
- No per‑frame allocations in animations (Lerp based).
- Coroutines used for timed sequences instead of `Update` polling.
- Particle effects pooled/destroyed quickly to avoid memory creep.

---

## 🛠️ Tech Stack
- **Engine:** Unity (3D mode, UI‑driven gameplay)
- **Language:** C#
- **UI:** Unity UI (uGUI) + TextMeshPro
- **Architecture:** Singleton + ScriptableObject + Modular
- **Target Platform:** iOS / Android

---

## 📸 Screenshots
*[Add your gameplay screenshots here]*  
![Gameplay](screenshots/gameplay.png)  
![Skills](screenshots/skills.png)

## 🎬 Gameplay Preview
*[Add a GIF or video link here]*  
![Preview](gifs/gameplay.gif)

---

## 🚀 Status
- ✅ Core gameplay loop complete
- ✅ Real‑time word checking
- ✅ Letter spawner with padding logic
- ✅ 3 skill power‑ups with UI
- ✅ Win / lose flow & level progression
- ✅ VFX & audio feedback
- ⏳ Additional polish & level balancing
- ⏳ In‑app store / monetization (future)

---

## 📂 Project Structure (Main Scripts)
