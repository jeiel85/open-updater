# WPF AutoUpdater

<p align="center">
  <img src="https://raw.githubusercontent.com/jeiel85/open-updater/master/docs/icon.png" width="128" height="128" alt="WPF AutoUpdater Logo">
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
    <img src="https://img.shields.io/github/actions/workflow/status/jeiel85/open-updater/build?label=Build" alt="Build">
  </a>
</p>

现代化 오토 업데이트 프로그램을 WPF로 재구성한 프로젝트입니다.

## 주요 기능

### 다양한 다운로드 소스 지원

| 소스 | 인증 | 설명 |
|------|------|------|
| **FTP** | ✅ / ❌ | FTP 서버 (인증/미인증) |
| **GitHub Releases** | OAuth | GitHub Release Asset |
| **Direct URL** | - | 일반 HTTP/HTTPS |

### 안정적인 비동기 처리

- `async/await` 기반 비동기 스레드
- `CancellationToken` 지원으로 취소 가능
- 실시간 진행률 표시

### 현대적인 UI

- 둥근 모서리 윈도우
- 블루 컬러 테마
- 드래그 이동 가능

## 사용법

### 명령줄 인수

```bash
# 일반 실행
WpfAutoUpdater.exe

# 자동 업데이트 실행
WpfAutoUpdater.exe 1
```

### 설정 파일 (UpdatePath.ini)

```ini
[업데이트정보]
FTP서버=update.example.com
FTP포트=21
FTP사용자=username
FTP암호=password
업데이트경로=/path/to/update
```

## 프로젝트 구조

```
WpfAutoUpdater/
├── .github/workflows/     # GitHub Actions CI/CD
├── AutoUpdate.Shared/    # 공유 라이브러리
│   ├── Logger.cs        # 로그 기록
│   ├── IniFile.cs      # INI 파일 처리
│   └── Services/
│       └── DownloadServices.cs  # 다운로드 서비스
└── WpfAutoUpdater/    # WPF 애플리케이션
    ├── MainWindow.xaml(.cs)    # 메인 윈도우
    └── SettingsWindow.xaml(.cs) # 설정 윈도우
```

## 빌드

```bash
# Debug 빌드
dotnet build

# Release 빌드
dotnet build -c Release

# 배포
dotnet publish -c Release -o ./publish
```

## 기술 스택

- **.NET 9.0** - 런타임
- **WPF** - UI 프레임워크
- **C# 12** - 프로그래밍 언어

## 라이선스

MIT License - See [LICENSE](LICENSE)

---

Made with ❤️ by [jeiel85](https://github.com/jeiel85)