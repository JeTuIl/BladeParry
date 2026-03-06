# Page Category Flow – Unity setup

This describes how to set up the category selection + page reader flow with UiFaders.

## 1. Hierarchy

Under your Canvas (or the parent where this flow lives):

- **CategorySelectionPanel** – root for the category buttons/tabs
- **PagesPanel** – root for the page reader (image, text, next/prev, counter, Back button)

## 2. CanvasGroup + UiFader

- On **CategorySelectionPanel**: add **CanvasGroup** and **UiFader** (ReGolithSystems/UI/UiFader).
  - In UiFader: set **Default State** to **Enabled** (so the category panel is visible at start).
- On **PagesPanel**: add **CanvasGroup** and **UiFader**.
  - In UiFader: set **Default State** to **Disabled** (so the page reader is hidden at start).

## 3. Page reader under PagesPanel

- Place your existing page UI under **PagesPanel** (Image, TMP_Text, optional counter, Next/Previous buttons).
- Add **PageUiManager** to a GameObject under PagesPanel (or on PagesPanel itself). Assign:
  - Localization table name, Page Image, Page Text, optional counter and Next/Previous buttons.
  - In **Pages**: leave **Categories** empty to use the legacy single **pages** list; or add **Category** entries, each with its own **Pages** list.

## 4. PageCategoryFlowManager

- Add **PageCategoryFlowManager** to a parent GameObject (e.g. same as the Canvas or a dedicated “Flow” object). Assign:
  - **Category Selection Fader** → the UiFader on CategorySelectionPanel
  - **Pages Fader** → the UiFader on PagesPanel
  - **Page Ui Manager** → the PageUiManager component

## 5. Button wiring

- **Category buttons/tabs**: for each button, in **On Click ()**, add a call to **PageCategoryFlowManager.SelectCategory (int)**. Set the integer argument to the category index (0, 1, 2, …).
- **Back button** (on the page reader): in **On Click ()**, call **PageCategoryFlowManager.BackToCategories ()**.

## 6. Using categories in PageUiManager

- In **PageUiManager**, if you use **Categories**:
  - Add one **Element** per category.
  - For each element set **Category Id** and **Category Name Localization Key** (optional) and add **Page Data** entries (sprite + localization key per page).
- Category indices match the order of elements: first category = 0, second = 1, etc. Use the same index in **SelectCategory (int)** on the corresponding button.
