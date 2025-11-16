# KYJ Reminder WOW

World of Warcraft 보스 레이드를 위한 타임라인 기반 리마인더 애플리케이션

## 📋 프로젝트 소개

WoW 게임 외부에서 실행되는 독립형 프로그램으로, 보스 전투 타임라인에 따라 특정 시간에 알림을 표시합니다.
Liquid Reminder 애드온에서 영감을 받아 제작되었습니다.

## ✨ 주요 기능

- **타임라인 기반 리마인더**: 3분 타임라인에 자유롭게 리마인더 추가
- **정밀한 시간 측정**: Stopwatch 기반 정확한 시간 계산 (소수점 1자리 표시)
- **부드러운 재생 헤드**: 0.1초 단위로 업데이트되는 실시간 타임라인
- **토스트 알림**: 우측 하단에 표시되는 비침투적 알림 시스템
- **재생 컨트롤**: Play/Pause/Reset 기능

## 🖼️ 스크린샷

<!-- 여기에 UI 이미지 추가 -->
![메인 화면](./screenshots/main.png)
![타임라인](./screenshots/timeline.png)
![알림 예시](./screenshots/notification.png)

## 🛠️ 기술 스택

- **Framework**: .NET Framework 4.5.2
- **UI**: WPF (Windows Presentation Foundation)
- **Language**: C# 6.0
- **IDE**: Visual Studio 2015
- **Architecture**: MVVM 패턴, Observer 패턴

## 📦 설치 방법

### 실행 파일 다운로드

1. [Releases](https://github.com/yoyoyoun18/KYJReminderWOW/releases) 페이지에서 최신 버전 다운로드
2. `KimYoungJoReminder.exe` 실행

### 소스 코드 빌드

```bash
# 저장소 클론
git clone https://github.com/yoyoyoun18/KYJReminderWOW.git

# Visual Studio로 솔루션 열기
KimYoungJoReminder.sln

# F5 또는 Ctrl+F5로 빌드 및 실행
```

## 🚀 실행 방법

### 실행 파일
```
KimYoungJoReminder\bin\Debug\KimYoungJoReminder.exe
```

### Visual Studio
1. 솔루션 열기 (`KimYoungJoReminder.sln`)
2. F5 키 또는 "디버그 시작" 클릭
3. 또는 Ctrl+F5 (디버깅 없이 실행)

## 📖 사용 방법

### 1. 리마인더 추가
1. 타임라인 위에서 원하는 시간 지점을 **더블클릭**
2. 입력창에 알림 메시지 작성
3. OK 버튼 클릭

### 2. 타임라인 재생
1. **Play** 버튼 클릭으로 시작
2. **Pause** 버튼으로 일시정지 (이어서 재생 가능)
3. **Reset** 버튼으로 초기화

### 3. 알림 확인
- 지정한 시간에 도달하면 우측 하단에 토스트 알림 표시
- 알림은 5초 후 자동으로 사라짐
- 알림 클릭 시 즉시 닫힘

## 📁 프로젝트 구조

```
KimYoungJoReminder/
├── Models/
│   ├── ReminderItem.cs          # 리마인더 데이터 모델
│   ├── TimelineManager.cs       # 타임라인 로직 및 상태 관리
│   └── TimelineState.cs         # 재생 상태 Enum
├── Views/
│   └── ToastNotification.xaml   # 토스트 알림 UI
├── MainWindow.xaml              # 메인 윈도우 UI
├── MainWindow.xaml.cs           # 메인 윈도우 로직
└── App.xaml                     # 애플리케이션 진입점
```

## 🔧 개발 환경

- **OS**: Windows 7 이상
- **IDE**: Visual Studio 2015 이상
- **.NET**: .NET Framework 4.5.2 이상
- **MSBuild**: 14.0

## 📝 주요 기술 특징

### Stopwatch 기반 정확한 타이머
- `DispatcherTimer`의 부정확성 문제 해결
- `Stopwatch`로 실제 경과 시간 측정
- Pause/Resume 시에도 정확한 시간 유지

### 이벤트 기반 아키텍처
- Observer Pattern 구현
- UI와 비즈니스 로직 분리
- 느슨한 결합(Loose Coupling)

### 소수점 시간 표시
- 0.1초 단위 실시간 업데이트
- `MM:SS.F` 형식 (예: `01:23.7`)

## 🐛 알려진 제한사항

- 타임라인 최대 길이: 3분 고정
- 초당 1개 리마인더만 등록 가능
- 프로필 저장/불러오기 미구현 (프로그램 종료 시 데이터 손실)
- 리마인더 수정/삭제 기능 미구현

## 🗺️ 향후 계획

- [ ] 리마인더 우클릭 메뉴 (수정/삭제)
- [ ] JSON 기반 프로필 저장/불러오기
- [ ] 보스별 프로필 관리
- [ ] 타임라인 길이 사용자 설정
- [ ] 재생 속도 조절 (0.5x, 1x, 2x)
- [ ] 여러 토스트 알림 동시 표시 시 쌓기

## 📄 라이선스

이 프로젝트는 MIT 라이선스 하에 배포됩니다.

## 👤 제작자

**yoyounn18**

- GitHub: [@yoyoyoun18](https://github.com/yoyoyoun18)
- Email: yoyounn88@gmail.com

## 🙏 감사의 글

- [Liquid Reminder](https://www.curseforge.com/wow/addons/liquid-reminder) - UI/UX 영감

---

**⭐ 이 프로젝트가 도움이 되셨다면 Star를 눌러주세요!**
