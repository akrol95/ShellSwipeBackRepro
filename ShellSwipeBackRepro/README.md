# ShellSwipeBackRepro

Minimal reproduction of a **white / blank screen after interactive swipe-back inside a
`Shell` on iOS 26**.

On iOS 26, performing the interactive edge swipe-back gesture on a page pushed onto the
`Shell` navigation stack can leave the app on a **white, empty screen**, even though the
managed `NavigationStack` still reports the expected pages. The **hardware/software back
button works correctly** — the problem only happens with the swipe gesture.

## Versions

| Component        | Version                         |
| ---------------- | ------------------------------- |
| .NET MAUI        | 10.0.80 (`Microsoft.Maui.Controls`) |
| .NET SDK         | 10.0.302 (pinned via `global.json`, `rollForward: latestPatch`) |
| Target Framework | `net10.0-ios` only              |
| Device           | iOS 26 simulator                |

## What the repro does

- Default `Shell` + `AppShell` from the template — structure unchanged.
- `MainPage`: a single **Push** button that calls
  `Shell.Current.Navigation.PushAsync(new DetailPage())`.
- `DetailPage`: a big `DETAIL PAGE` label plus a label showing the current
  `NavigationStack.Count`.
- **No** `Routing.RegisterRoute` / `GoToAsync` — plain `Navigation.PushAsync`.
- No extra NuGet packages, no MVVM, no custom styles.

### Diagnostics

Both pages log through `System.Diagnostics.Debug.WriteLine` with a `[REPRO]` prefix:

- `OnAppearing` / `OnDisappearing` on each page, including
  `Shell.Current.Navigation.NavigationStack.Count`.
- `DetailPage` constructor **and finalizer** (`~DetailPage`).

Watch for the **mismatch between the managed navigation stack and what is actually on
screen**: after a broken swipe-back the log shows the stack back at the root (or the
`DetailPage` never finalizing) while the screen is blank.

## How to reproduce

Deploy to an **iOS 26 simulator**, tap **Push** to reach `DetailPage`, then:

### Variant A — completed swipe, immediate re-push

1. Complete a swipe-back from the left edge (let the page fully pop).
2. Immediately tap **Push** again.
3. Repeat this push / swipe-back cycle quickly several times.

### Variant B — started-and-cancelled swipe

1. Begin a swipe-back from the left edge, then **release in the middle so the gesture is
   cancelled** and the page snaps back.
2. Repeat a few times.

Either variant can land the app on a blank white screen. Variant B (the cancelled /
interrupted gesture) most directly matches the suspected root cause below.

### Note

The **back button always works** — pop it that way and the screen is fine. The white
screen only appears via the **interactive swipe gesture**.

## Expected vs Actual

**Expected:** After a completed swipe-back the previous page (`MainPage`) is shown; after a
cancelled swipe-back the current page (`DetailPage`) remains fully visible. In all cases the
visible UI stays consistent with `NavigationStack`.

**Actual:** On iOS 26 the swipe gesture can leave a **white / empty screen** with no content,
while `NavigationStack.Count` reports a normal stack — the native view hierarchy has been torn
down out from under the still-referenced page.

## Related / suspected root cause

This looks like the `Shell` equivalent of
[dotnet/maui#34595](https://github.com/dotnet/maui/issues/34595).

In #34595 the root cause was: an **interrupted/cancelled pop** completes its pending task with
`false` → `Dispose()` is called on a **still-visible `UIViewController`** →
`Disconnect(true)` removes the subviews → **white screen**. That issue was scoped to
`NavigationPage` / `NavigationRenderer`.

Here the same failure appears to occur on the **`Shell` navigation path
(`ShellSectionRenderer`)** rather than `NavigationRenderer`, so the #34595 fix (narrowed to
`NavigationPage`) would not cover it.

## Build

```bash
dotnet build -f net10.0-ios -c Debug
```
