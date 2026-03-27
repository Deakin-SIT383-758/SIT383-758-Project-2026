# SIT383-758-Project-2026 - OUTBACK ENGINEERS

The official project repository for SIT383-SIT758 2026 T1 Trimester Projects for Assembling Virtual And Augmented Reality Experiences

---

## Table of Contents

- [Repository Structure](#repository-structure)
- [Getting Started](#getting-started)
- [Commit Message Convention](#commit-message-convention)
- [Branching Strategy](#branching-strategy)
- [Development Guidelines](#development-guidelines)
- [Assessment Information](#assessment-information)

---

## Repository Structure

The repository is organised so that each team has a dedicated Unity project folder. This reduces the frequency of merge conflicts and provides a clear record of individual contributions for assessment purposes.

```
/
├── README.md
├── .gitignore
│
├── Team A/                    
│   ├── Assets/
│   │   ├── Scripts/
│   │   ├── Scenes/
│   │   ├── Prefabs/
│   │   ├── Textures/
│   │   └── Materials/
│   ├── ProjectSettings/
│   ├── Packages/
│   └── README.md               # Member-specific notes (optional)
│
├── Team B/                
│   ├── Assets/
│   └── ...
│
```


### Unity Project Notes

- Each folder is a **standalone Unity project**. Open it via Unity Hub by pointing to the subfolder directly.
- Unity version used across all projects: **`6000.3 LTS`**
- Do **not** commit the `Library/`, `Temp/`, `obj/`, or `Build/` directories — these are covered by the `.gitignore`.
- Large binary assets (e.g. `.fbx`, `.wav`, `.psd`, `.mp4`) should not be committed unless absolustely necessary.

---


## Getting Started

### Prerequisites

- [Unity Hub](https://unity.com/download) with Unity `6000.3` installed
- [Git](https://git-scm.com/) (version 2.53 or later recommended [2026])


## Commit Message Convention

Consistent commit messages make it easier to review individual contributions and understand the history of each feature. All team members are expected to follow the format below.

### Format

```
<type>(<scope>): <short summary>

[Optional body — explain what changed and why, not how]

[Optional footer — references, breaking changes]
```

### Types

| Type | When to use |
|------|-------------|
| `feat` | A new feature or mechanic has been implemented |
| `fix` | A bug or unintended behaviour has been corrected |
| `art` | New or updated art, audio, or visual assets |
| `refactor` | Code restructured without changing external behaviour |
| `docs` | Changes to documentation or comments only |
| `test` | Adding or updating test scenes or scripts |
| `chore` | Project settings, package updates, `.gitignore` changes |

### Scope

The scope should refer to the area of the project affected, for example: `player`, `ui`, `audio`, `camera`, `level-01`, `build`, etc.

### Examples

```
feat(player): add capture mechanic with movement smoothing

Implemented a capture system that also utilizes a movement smoothing algorithm. Tested in Scene_PlayerTest.

fix(ui): resolve player action bar not updating on new collaborator joining

The current collaborator window was subscribing to the wrong event. Corrected the listener in CollaboratorController.cs.

art(environment): add modular wall and floor tile prefabs

chore: update .gitignore to exclude JetBrains Rider temp files
```

### Guidelines

- Write the summary in the **imperative mood**: *"add feature"*, not *"added feature"* or *"adding feature"*.
- Keep the summary line under **72 characters**.
- Reference relevant scene names, scripts, or issue numbers where applicable.
- Commit **often and in small units** — a commit should represent one logical change.

---

## Branching Strategy

To keep the `main` branch stable and assessable at all times, the following workflow applies.

```
main                  ← stable, always working
└── dev               ← integration branch for testing combined work
    ├── jane/feature-name
    ├── alex/ui-overhaul
    └── jordan/save-system
```

- `main` — Protected. Only merged into from `dev` when the build is stable. This is the branch assessors should review.
- `dev` — The active working branch. All feature branches are merged here first.
- `<name>/<feature>` — Individual feature branches created off `dev`. Name them descriptively (e.g. `jane/weather-api-integration`, `sam/spatial-mapping`).

**Do not push directly to `main`.** Create a pull request from `dev` → `main` and have at least one other team member review it before merging.

---

## Development Guidelines

- **Scene ownership**: To avoid merge conflicts, only one team member should have a scene open and modified at a time. Communicate in your team channel before editing shared scenes.
- **Prefabs**: Store reusable prefabs in the `Assets/Prefabs/` folder within your project. If sharing a prefab across projects, coordinate with the relevant team member.
- **Scripts**: Use clear, descriptive names for all scripts and classes. Each script should have a brief comment at the top describing its purpose.
- **Asset naming**: Follow a consistent naming convention, for example `T_` for textures, `M_` for materials, `SM_` for static meshes, `SFX_` for audio clips.
- **Testing before committing**: Ensure the project opens and enters Play Mode without errors before pushing to a shared branch.

---

## Assessment Information

| Item | Detail |
|------|---------|
| Unit | SIT383-SIT758 |
| Institution | Deakin University |
| Trimester | Trimester 1, 2026 |
| Submission deadline | May 2026 |
| Assessed branch | `main` |

> **Note to assessors:** Each team member's work is located in their named subfolder. Commit history and authorship can be reviewed via `git log --author="Name"` or through the GitHub contributor graphs. Individual `README.md` files within each subfolder may provide additional context about the work contained therein.

---

*Last updated: March 2026*
