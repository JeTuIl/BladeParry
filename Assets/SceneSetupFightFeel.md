# Scene Setup: Fight Feel and Polish

Instructions to wire the new fight-feel features in **FightingScene**.

---

## 1. Eased attack motion

- Select the GameObject that has **CharacterAttaqueSequence** (enemy attack sequence).
- In the Inspector, find **Wind Up Curve** and **Wind Down Curve**.
- Leave empty for linear motion, or assign Animation Curves:
  - **Wind Up Curve:** Ease-in (e.g. create in Project: Right-click → Create → Animation Curve, or use a curve that starts slow).
  - **Wind Down Curve:** Ease-out so the strike snaps at the end (curve that ends steep).

---

## 2. Perlin screen shake

- No extra setup. **ScreenshackManager** still uses the same **Target Rect Transform** as before; shake is now multi-frame Perlin noise.

---

## 4. Hit stop

- Select the GameObject with **GameplayLoopController**.
- In the **Hit stop** section, adjust:
  - **Hit Stop Time Scale** (e.g. 0.92).
  - **Hit Stop Duration** (e.g. 0.05 seconds).

---

## 5. Squash and stretch

### Player

- Ensure the player character has a Transform that can be scaled (e.g. the Image that shows the player sprite, or a parent of it).
- Add a **Squash Stretch Animator** component to the player hierarchy (e.g. on the same object as **PlayerSpriteManager** or on a parent of the sprite).
- Set **Scale Target** to the RectTransform/Transform to animate (player Image or a parent).
- In **GameplayLoopController**, assign **Player Squash Stretch** to this component.

### Enemy

- The enemy uses **CharacterSpriteDirection**, which already sets the Image’s scale. So the **Scale Target** for the enemy must be a **parent** of that Image (e.g. a “SpriteContainer” or the root of the enemy character).
- Create a parent GameObject above the enemy Image if needed, then add **Squash Stretch Animator** to that parent (or to the enemy root) and set **Scale Target** to that same parent.
- In **GameplayLoopController**, assign **Enemy Squash Stretch** to this component.

---

## 6. Post-processing (vignette)

1. **Create a Global Volume**
   - In FightingScene: GameObject → Volume → Global Volume (or create an empty GameObject and add the **Volume** component).
   - Set **Profile** to a new or existing Volume Profile (e.g. Create → Volume Profile).

2. **Add Vignette to the profile**
   - Select the Volume Profile asset.
   - Click **Add override** → **Post-processing** → **Vignette**.
   - Leave **Intensity** at 0; the script will override it at runtime.

3. **Camera**
   - Select the camera that renders the game (e.g. **CameraFx** or **CameraUI**).
   - Enable **Post Processing** on the Camera component.
   - Ensure the camera’s **Volume Mask** includes the layer of the Volume (default layer is fine for a global volume).

4. **Gameplay Post Process Driver**
   - Add **GameplayPostProcessDriver** to a GameObject in the scene (e.g. on the same object as **GameplayLoopController** or a dedicated “GameplayFX” object).
   - Assign the **Volume** reference to the driver’s **Volume** field.
   - Assign **Player Lifebar Manager** (the player’s LifebarManager) so the driver can read current/max life for low-life vignette.
   - Optionally tune **Vignette Intensity On Low Life**, **Low Life Threshold**, **Vignette Pulse On Miss**, and **Vignette Recovery Speed**.

5. **Wire player damage**
   - **GameplayLoopController** already calls `NotifyPlayerDamaged()` on the driver when the player misses a parry, as long as **Gameplay Post Process Driver** is assigned in the **GameplayLoopController** Inspector.

---

## 7. Floating feedback text

1. **Create the prefab**
   - Create a UI **Text - TextMeshPro** (or GameObject with RectTransform + TMP_Text).
   - Set anchor to center, style the font/size/color as desired.
   - Save as a prefab (e.g. `FloatingFeedbackTextPrefab`).

2. **Floating Feedback Text component**
   - In the scene, add an empty GameObject under the HUD Canvas (or wherever the fight UI lives).
   - Add the **FloatingFeedbackText** component.
   - Assign **Text Prefab** to the TMP prefab.
   - Assign **Spawn Parent** to the RectTransform under which instances should be parented (e.g. the same Canvas content or a dedicated child).
   - Optionally set **World Camera** (if null, Camera.main is used).
   - Adjust **Display Duration**, **Scale Start**, **Scale Peak**, **Scale End** if needed.

3. **GameplayLoopController**
   - Assign **Floating Feedback** to the **FloatingFeedbackText** component above.

---

## 10. Haptics

- On **GameplayLoopController**, ensure **Enable Haptics** is checked if you want vibration on handheld devices.
- Haptics run only when `SystemInfo.deviceType == DeviceType.Handheld`; no extra scene setup.

---

## 11. Swipe trail

1. **UI for the line**
   - Under the same Canvas used for the fight (e.g. HUD Canvas), create an empty GameObject and add **SwipeTrailUI**.
   - As a child, add a **UI Image** (thin horizontal bar): e.g. width 200, height 2–4, anchor center, pivot (0, 0.5) or (0.5, 0.5) so the line stretches correctly.

2. **SwipeTrailUI component**
   - Assign **Slide Detection** to the scene’s SlideDetection component.
   - Assign **Line Rect** to the RectTransform of the thin Image (the line).

3. **Canvas**
   - The Canvas should be in **Screen Space - Overlay** or **Screen Space - Camera** so screen positions convert correctly; **SwipeTrailUI** uses the Canvas’s root for conversion.

---

## Checklist

- [ ] **CharacterAttaqueSequence:** Wind Up / Wind Down curves (optional).
- [ ] **GameplayLoopController:** Hit stop values; Player / Enemy Squash Stretch; Floating Feedback; Enable Haptics; Gameplay Post Process Driver.
- [ ] **Post-processing:** Global Volume + Vignette override; camera Post Processing on; GameplayPostProcessDriver with Volume and Player Lifebar Manager.
- [ ] **Floating text:** Prefab + FloatingFeedbackText in scene; spawn parent and prefab assigned; Floating Feedback assigned on GameplayLoopController.
- [ ] **Squash/stretch:** SquashStretchAnimator on player (target = player Image or parent) and on enemy (target = parent of enemy Image); both assigned on GameplayLoopController.
- [ ] **Swipe trail:** SwipeTrailUI under Canvas with thin Image child; Slide Detection and Line Rect assigned.
