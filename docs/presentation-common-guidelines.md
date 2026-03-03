# Presentation Common Guidelines

## Purpose
- Keep WinForms View code thin and avoid duplicated UI logic.
- Reuse common utilities for error handling, information messages, and grid header mapping.

## Common Components
- `Presentation/Common/UiMessageService.cs`
  - Show info/warning/error dialogs and confirmation prompts.
- `Presentation/Common/UiActionExecutor.cs`
  - Wrap UI actions with standardized exception handling.
- `Presentation/Common/DataGridHeaderMapper.cs`
  - Apply Japanese headers to `DataGridView` columns.

## Usage Rules
- Do not call `MessageBox.Show` directly in tab implementations. Use `UiMessageService`.
- Execute event-driven operations through `UiActionExecutor.Execute`.
- For list grid headers, define mapping dictionaries in the View and apply via `DataGridHeaderMapper`.
- Keep business decisions in Service/Controller classes and leave View code for input/output.
