# WPF AutoUpdater

<p align="center">
  <!-- 아이콘 파일이 확보되면 아래 경로를 수정하여 활성화할 수 있습니다. -->
  <!-- <img src="docs/icon.png" width="128" height="128" alt="WPF AutoUpdater Logo"> -->
  <h1 align="center">WPF AutoUpdater</h1>
</p>

<p align="center">
  <a href="https://github.com/jeiel85/open-updater/releases/latest">
    <img src="https://img.shields.io/github/v/release/jeiel85/open-updater?include_prereleases&label=Release" alt="Release">
  </a>
  <a href="https://github.com/jeiel85/open-updater/blob/master/LICENSE">
    <img src="https://img.shields.io/github/license/jeiel85/open-updater?label=License" alt="License">
  </a>
  <a href="https://dotnet.microsoft.com/">
    <img src="https://img.shields.io/badge/.NET-9.0-blue?logo=.NET" alt=".NET">
  </a>
  <a href="https://github.com/jeiel85/open-updater/actions/workflows/build.yml">
    <img src="https://img.shields.io/github/actions/workflow/status/jeiel85/open-updater/build.yml?label=Build" alt="Build">
  </a>
</p>

**WPF AutoUpdater**는 기존 오토 업데이트 프로그램을 현대적인 WPF 환경으로 재구성한 오픈소스 프로젝트입니다. 
강력한 비동기 다운로드 엔진과 직관적인 UI를 제공합니다.

---

## 🚀 주요 기능

### 📁 다양한 다운로드 소스 지원

| 소스 | 인증 | 상세 설명 |
|:---:|:---:|---|
| **FTP** | ✅ / ❌ | FTP 서버를 통한 파일 다운로드 (익명/계정 지원) |
| **GitHub Releases** | OAuth | GitHub 저장소의 릴리즈 자산(Asset) 직접 다운로드 |
| **Direct URL** | - | 일반적인 HTTP/HTTPS 경로의 파일 다운로드 |

### ⚡ 안정적인 비동기 엔진

- **Async/Await 기반**: UI 프리징 없이 부드러운 다운로드 환경을 제공합니다.
- **취소(Cancel) 지원**: `CancellationToken`을 통해 언제든지 안전하게 중단 가능합니다.
- **실시간 프로그레스**: 다운로드 진행률을 실시간으로 확인하고 로그를 남깁니다.

### ✨ 현대적인 디자인 (Modern UI)

- **Fluent Design**: 둥근 모서리와 세련된 블루 컬러 테마를 적용했습니다.
- **다크/라이트 모드 대응**: 시스템 설정에 맞춘 깔끔한 UI를 제공합니다.
- **컴팩트 모드**: 화면 어디서든 편리하게 확인할 수 있는 간결한 인터페이스를 제공합니다.

---

## 🛠 사용법

### 💻 명령줄 인수 (Command Line Arguments)

```bash
# 기본 실행 (UI 모드)
WpfAutoUpdater.exe

# 자동 업데이트 실행 (인수가 1인 경우 즉시 확인)
WpfAutoUpdater.exe 1
```

### ⚙️ 설정 파일 (UpdatePath.ini)

설정 파일은 프로그램과 같은 경로에 위치해야 합니다.

```ini
[업데이트정보]
FTP서버=update.example.com
FTP포트=21
FTP사용자=username
FTP암호=password
업데이트경로=/path/to/update
```

---

## 📂 프로젝트 구조

```text
WpfAutoUpdater/
├── .github/workflows/    # CI/CD 자동화 빌드 스크립트
├── AutoUpdate.Shared/    # 핵심 비즈니스 로직 라이브러리
│   ├── Logger.cs         # 상세 로그 기록 및 관리
│   ├── IniFile.cs        # 환경 설정(INI) 처리
│   └── Services/
│       └── DownloadServices.cs  # 비동기 파일 다운로드 서비스
└── WpfAutoUpdater/       # WPF UI 애플리케이션
    ├── MainWindow.xaml   # 메인 다운로드 창
    └── SettingsWindow.xaml # 설정 관리 인터페이스
```

---

## 🔨 빌드 방법 (Build Guide)

.NET 9.0 SDK가 설치되어 있어야 합니다.

```bash
# 디버그 빌드
dotnet build

# 릴리즈 빌드 (최적화)
dotnet build -c Release

# 단일 실행 파일 배포 (.exe 하나만 생성)
dotnet publish WpfAutoUpdater/WpfAutoUpdater.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish
```

---

## 🛡 기술 스택

- **Runtime**: .NET 9.0 (WPF)
- **Language**: C# 12.0
- **Style**: Vanilla CSS-like XAML Styling
- **CI/CD**: GitHub Actions (Windows Runner)

---

## 📄 라이선스

본 프로젝트는 **MIT License**에 따라 자유롭게 사용 및 수정이 가능합니다. 상세 내용은 [LICENSE](LICENSE) 파일을 확인하세요.

---

Made with ❤️ by [jeiel85](https://github.com/jeiel85)
