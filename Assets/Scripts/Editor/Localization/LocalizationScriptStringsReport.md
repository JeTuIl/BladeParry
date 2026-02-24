# Localization – Display strings in Assets/Scripts

This report lists strings in project scripts that are shown on screen and should be localized.  
Use **Tools > Localization > Analyze for localization** in Unity to re-scan and export CSV.

---

## 1. GameplayLoopController.cs

| Line | Kind | String / context |
|------|------|-------------------|
| 497 | `.text` (countdown number) | `i.ToString()` – numeric countdown (5, 4, 3, 2, 1). Consider localizing if you ever show a label like "Get ready" or reuse for other languages. |
| 501 | **Literal** | **`"Fight!"`** – countdown end message. **Localize.** |

---

## 2. FightSummaryDisplay.cs

| Line | Kind | String / context |
|------|------|-------------------|
| 32 | Assigned from method | `summaryText.text = FormatSummary(summary)` – content comes from format below. |
| 41–46 | **string.Format (multi-line)** | **Full summary template (French):**<br>• `"Duree du combat : {0} min {1} s\n"`<br>• `"Parades parfaites : {2}\n"`<br>• `"Parades non parfaites : {3}\n"`<br>• `"Plus grande serie de parades parfaites : {4}\n"`<br>• `"Nombre de combos ennemis : {5}\n"`<br>• `"Nombre coups reçus : {6}"`<br>**Localize** each line (or one key with placeholders). |

---

## 3. FloatingFeedbackText.cs

| Line | Kind | String / context |
|------|------|-------------------|
| 38 | **Display method** | **`"PARRY!"`** – parry feedback. **Localize.** |
| 46 | **Display method** | **`"Perfect !"`** – perfect parry feedback. **Localize.** |
| 54 | **Display method** | **`"MISS"`** – miss feedback. **Localize.** |
| 62 | **Display method** | **`"COMBO!"`** – combo feedback. **Localize.** |
| 77 | `.text` set from parameter | `tmp.text = text` – text comes from the public methods above; localize at call site (strings above). |

---

## 4. LifebarManager.cs

| Line | Kind | String / context |
|------|------|-------------------|
| 53 | **SerializeField format** | **`numericFormat = "{0} / {1}"`** – HP display template (current / max). **Localize** (e.g. `"{0} / {1}"` or language-specific format). |
| 177 | string.Format | `string.Format(numericFormat, ...)` – uses `numericFormat`; localize the format string. |
| 181 | Interpolated | `$"{currentLifeValue} / {maxLifeValue}"` – same meaning as above; consider using localized format string. |

---

## 5. PerfectParryComboDisplay.cs

| Line | Kind | String / context |
|------|------|-------------------|
| 56 | `.text` (number) | `text.text = _count.ToString()` – combo count. Only numbers; optional to add a prefix/suffix key (e.g. "Combo: {0}"). |

---

## 6. FpsCounter.cs

| Line | Kind | String / context |
|------|------|-------------------|
| 19 | `.text` (number) | `_fpsText.text = Mathf.RoundToInt(_smoothedFps).ToString()` – FPS number. Usually dev-only; localize only if shown in shipping builds. |

---

## 7. SlideDetection.cs

| Line | Kind | String / context |
|------|------|-------------------|
| 121 | **Enum ToString** | `directionLabel.text = direction.ToString()` – shows `Direction` enum (e.g. Up, Down, Left, Right). **Localize** enum display names if user-facing. |

---

## Summary – high priority to localize

- **GameplayLoopController.cs**: `"Fight!"`
- **FightSummaryDisplay.cs**: full fight summary template (all French lines).
- **FloatingFeedbackText.cs**: `"PARRY!"`, `"Perfect !"`, `"MISS"`, `"COMBO!"`
- **LifebarManager.cs**: `numericFormat` `"{0} / {1}"` (and the interpolated fallback).
- **SlideDetection.cs**: Direction enum labels if the direction UI is user-facing.

Scene text (e.g. MainMenu, FightingScene) is analyzed by the **Scenes (TMP)** tab in **Tools > Localization > Analyze for localization** (TextMeshProUGUI components and their current `m_text` in scenes).
