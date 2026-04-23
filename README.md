# WPF Auto Updater

오토 업데이트 프로그램을 WPF로 재구성한 프로젝트입니다.

## 기능

- **다양한 다운로드 소스 지원**
  - FTP (인증/미인증)
  - GitHub Releases
  - 일반 URL

- **안정적인 비동기 처리**
  - async/await 기반 스레드
  - 취소 가능 작업support
  - 진행률 표시

- **현대적인 UI**
  - 둥근 모서리 윈도우
  - 블루 컬러 테마
  - 드래그 이동 가능

## 빌드

```bash
# Debug 빌드
dotnet build

# Release 빌드
dotnet build -c Release

# 发布
dotnet publish -c Release -o ./publish
```

## 사용법

### 명령줄 인수

- `WpfAutoUpdater.exe` - 일반 실행
- `WpfAutoUpdater.exe 1` - 자동 업데이트 실행

### 설정 파일 (UpdatePath.ini)

```ini
[업데이트정보]
FTP서버=update.wavepos.co.kr
FTP포트=21
FTP사용자=devel
FTP암호=dev1234
업데이트경로=POPs_Renewal
```

## 프로젝트 구조

```
WpfAutoUpdater/
├── .github/workflows/     # GitHub Actions
├── AutoUpdate.Shared/    # 공유 라이브러리
│   ├── Logger.cs
│   ├── IniFile.cs
│   └── Services/
│       └── DownloadServices.cs
└── WpfAutoUpdater/       # WPF 애플리케이션
    ├── MainWindow.xaml(.cs)
    └── SettingsWindow.xaml(.cs)
```

## 라이선스

MIT License