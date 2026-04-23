# OpenUpdater 개발 지침 (DEVELOPMENT Guide)

이 문서는 에이전트와 개발자가 OpenUpdater 프로젝트를 유지보수하고 확장할 때 준수해야 할 기술 가이드라인입니다.

## 🛠 기술 스택 및 표준
- **Framework**: .NET 9.0 (WPF)
- **C# Version**: 12.0 이상
- **UI Style**: Modern, Rounded Corners, Blue Theme
- **Logging**: `AutoUpdate.Shared.Logger`를 사용한 실시간 파일 로그 기록

## 🏗 프로젝트 구조
- `OpenUpdater/`: WPF UI 애플리케이션 (MVVM 패턴 권장)
- `AutoUpdate.Shared/`: 핵심 비즈니스 로직 및 공통 유틸리티 (라이브러리)
- `.github/workflows/`: CI/CD (GitHub Actions)

## 📜 코딩 가이드라인
1. **비동기 처리**: 네트워크 요청 및 파일 I/O는 반드시 `async/await`를 사용하며, UI 스레드 프리징을 방지합니다.
2. **취소 토큰**: 긴 작업에는 `CancellationToken`을 지원하여 사용자가 작업을 중단할 수 있도록 합니다.
3. **INI 관리**: 설정 값은 `IniFile` 클래스를 통해 관리하며, 기본값을 반드시 제공합니다.
4. **로깅**: 주요 단계(시작, 성공, 실패)마다 로그를 남겨 디버깅이 용이하게 합니다.

## 🚀 빌드 및 배포 절차
1. **버전 관리**: 릴리즈 시 `.csproj`의 `<Version>` 태그를 업데이트합니다.
2. **배포**: GitHub Tag(`v*.*.*.*`)를 푸시하면 자동으로 단일 파일 빌드 및 릴리즈 자산 업로드가 수행됩니다.
3. **최적화**: 배포용 빌드 시 `PublishTrimmed`, `EnableCompressionInSingleFile`, `PublishReadyToRun` 설정을 유지합니다.

---
*본 지침은 프로젝트 발전과 함께 지속적으로 업데이트됩니다.*
